using System;
using System.Collections.Generic;
using System.Globalization;
using SimioAPI;
using SimioAPI.Extensions;

using CalculationSample1Step;
using System.Linq;
using System.Text;
using System.IO;

namespace CalculationSample1Step
{
    class CalculationStep1Definition : IStepDefinition
    {
        #region IStepDefinition Members

        /// <summary>
        /// Property returning the full name for this type of step. The name should contain no spaces. 
        /// </summary>
        public string Name
        {
            get { return "CalculationSample1"; }
        }

        /// <summary>
        /// Property returning a short description of what the step does.  
        /// </summary>
        public string Description
        {
            get { return "The CalculationSample1 step is an example of reading/writing values to a Simio Table. The user defined File Element specifies output path"; }
        }

        /// <summary>
        /// Property returning an icon to display for the step in the UI. 
        /// </summary>
        public System.Drawing.Image Icon
        {
            get { return null; }
        }

        /// <summary>
        /// Property returning a unique static GUID for the step.  
        /// </summary>
        public Guid UniqueID
        {
            get { return MY_ID; }
        }

        // {Changed Apr2020}
        static readonly Guid MY_ID = new Guid("{CBEA8AD0-735A-41C4-968C-B505C3766786}");

        /// <summary>
        /// Property returning the number of exits out of the step. Can return either 1 or 2. 
        /// </summary>
        public int NumberOfExits
        {
            get { return 1; }
        }

        /// <summary>
        /// Method called that defines the property schema for the step.
        /// </summary>
        public void DefineSchema(IPropertyDefinitions schema)
        {
            IPropertyDefinition pd;

            // Reference to the file to read from
            pd = schema.AddElementProperty("CalculationElement", CalculationElementDefinition.MY_ID);

            pd = schema.AddStateProperty("MyStateTableIndex");
            pd.Description = "Reference the State Property that is used in the mapping to the SimioTable";
            pd.Required = false;

            // A repeat group of states to read into
            IRepeatGroupPropertyDefinition fields = schema.AddRepeatGroupProperty("MyFieldValues");
            fields.Description = "The RepeatGroup holding the state values to read the values into";

            pd = fields.PropertyDefinitions.AddStateProperty("Field");
            pd.Description = "A state/field to map into a Table";


        }

        /// <summary>
        /// Method called to create a new instance of this step type to place in a process. 
        /// Returns an instance of the class implementing the IStep interface.
        /// </summary>
        public IStep CreateStep(IPropertyReaders properties)
        {
            return new CalculationSample1Step(properties);
        }

        #endregion
    }

    class CalculationSample1Step : IStep
    {
        IPropertyReaders _props;

        IElementProperty prElement;
        IRepeatingPropertyReader prFields;
        IStateProperty prStateTableIndex;

        Random RandomGenerator;

        List<CalculationRow> CalcDataList;

        int NbrTableRows { get; set; }

        /// <summary>
        /// Constructor. Initialize data.
        /// </summary>
        /// <param name="properties"></param>
        public CalculationSample1Step(IPropertyReaders properties)
        {
            _props = properties;

            prElement = (IElementProperty)_props.GetProperty("CalculationElement");
            prFields = (IRepeatingPropertyReader)_props.GetProperty("MyFieldValues");
            prStateTableIndex = (IStateProperty)_props.GetProperty("MyStateTableIndex");

            RandomGenerator = new Random();
            NbrTableRows = 5;


            CalcDataList = new List<CalculationRow>();

            for (int tr = 1; tr <= NbrTableRows; tr++)
            {
                CalculationRow cr = new CalculationRow();
                cr.MyKey = tr;
                CalcDataList.Add(cr);
            }


        }

        #region IStep Members

        /// <summary>
        /// Method called when a process token executes the step.
        /// </summary>
        public ExitType Execute(IStepExecutionContext context)
        {
            //IStateProperty stateIndex;

            // Set the row by changing the index value
            //stateIndex = (IStateProperty)_props.GetProperty("MyStateTableIndex");
            IState IndexState = prStateTableIndex.GetState(context);

            CalculationElement calcElement = (CalculationElement) prElement.GetElement(context);

            string filepath = calcElement.FilePath;
            bool outputToFile = calcElement.OutputToFile;

            foreach (CalculationRow cr in CalcDataList)
            {
                // Set the Simio Table row by using its key value         
                IndexState.StateValue = cr.MyKey;
                PutSimioValuesToCalculationRow(context, RandomGenerator, prFields, cr);
            } // next table row

            //======= Run the calculator/optimizer/whatever ==============
            if (!RunCalculator(CalcDataList, out string explanation))
            {
                Logit(context, $"Calculation Err={explanation}");
                return ExitType.AlternateExit;
            }
            else
            {
                foreach (CalculationRow cr in CalcDataList)
                {
                    IndexState.StateValue = cr.MyKey;
                    PutCalculationRowToSimioValues(context, prFields, cr);
                }

                if ( outputToFile )
                    AppendDataToFile(filepath, CalcDataList);

                return ExitType.FirstExit;
            }

        }


        private static void AppendDataToFile(string filepath, List<CalculationRow> calcDataList)
        {
            try
            {
                if (!File.Exists(filepath))
                {
                    File.Create(filepath);
                    System.Threading.Thread.Sleep(100);
                }

                StringBuilder sb = new StringBuilder();
                foreach (CalculationRow cr in calcDataList)
                    sb.AppendLine($"{cr}");
                sb.AppendLine($"{DateTime.Now:HH:mm:ss.ffff} ===========================");

                StringBuilder all = new StringBuilder(File.ReadAllText(filepath));
                all.Append(sb);

                File.WriteAllText(filepath, all.ToString());

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"File={filepath} Err={ex}");
            }
        }



        private static bool PutSimioValuesToCalculationRow(IStepExecutionContext context, Random rand, IRepeatingPropertyReader prFields, CalculationRow calcRow)
        {
            try
            {
                int fieldCount = prFields.GetCount(context);

                // For each field in our Simio Repeating Group
                for (int ii = 0; ii < fieldCount; ii++)
                {
                    // The thing returned from GetRow is IDisposable, so we use the using() pattern here
                    using (IPropertyReaders row = prFields.GetRow(ii, context))
                    {
                        // Get the state property out of the i-th tuple of the repeat group
                        IStateProperty statePropreader = (IStateProperty)row.GetProperty("Field");

                        // Resolve that stateprop reader to get the runtime state value
                        IState state = statePropreader.GetState(context);

                        switch (ii)
                        {
                            case 0:
                                state.StateValue = rand.Next(); // Assign some value
                                calcRow.MyReal1 = state.StateValue;
                                break;
                            case 1:
                                state.StateValue = rand.Next(); // Assign some value
                                calcRow.MyReal2 = state.StateValue;
                                break;
                            case 2:
                                //calcRow.MyReal1Sqrt = 0;
                                break;
                            case 3:
                                //calcRow.MyReal2Doubled = 0;
                                break;
                            default:
                                break;
                        }
                    } // using

                } // for each field in repeating group
                return true;
            }
            catch (Exception ex)
            {
                Logit(context, $"Err={ex.Message}");
                return false;
            }
        }
        private static bool PutCalculationRowToSimioValues(IStepExecutionContext context, IRepeatingPropertyReader prFields, CalculationRow calcRow)
        {
            try
            {
                int fieldCount = prFields.GetCount(context);

                // For each field in our Simio Repeating Group
                for (int ii = 0; ii < fieldCount; ii++)
                {
                    // The thing returned from GetRow is IDisposable, so we use the using() pattern here
                    using (IPropertyReaders row = prFields.GetRow(ii, context))
                    {
                        // Get the state property out of the i-th tuple of the repeat group
                        IStateProperty statePropreader = (IStateProperty)row.GetProperty("Field");

                        // Resolve that stateprop reader to get the runtime state value
                        IState state = statePropreader.GetState(context);

                        switch (ii)
                        {
                            case 0:
                                state.StateValue = calcRow.MyReal1;
                                break;
                            case 1:
                                state.StateValue = calcRow.MyReal2;
                                break;
                            case 2:
                                state.StateValue = calcRow.MyReal1Sqrt;
                                break;
                            case 3:
                                state.StateValue = calcRow.MyReal2Doubled;
                                break;
                            default:
                                break;
                        }
                    } // using

                } // for each field in repeating group
                return true;
            }
            catch (Exception ex)
            {
                Logit(context, $"Err={ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// This mock optimizer will receive a list of rows, and each row is a class whose
        /// construction must mimic the definition of the Step's repeating group.
        /// 
        /// </summary>
        /// <returns></returns>
        private bool RunCalculator(List<CalculationRow> calcDataList, out string explanation)
        {
            explanation = "";
            try
            {
                foreach (CalculationRow cr in calcDataList)
                {
                    cr.MyReal1Sqrt = Math.Sqrt(cr.MyReal1);
                    cr.MyReal2Doubled = Math.Pow(cr.MyReal2, 2);
                }
                return true;
            }
            catch (Exception ex)
            {
                explanation = $"Err={ex}";
                return false;
            }
        }


        public static void Logit(IStepExecutionContext context, string msg)
        {
            context.ExecutionInformation.TraceInformation($"{msg}");
        }

        public static void Alert(IStepExecutionContext context, string msg)
        {
            context.ExecutionInformation.ReportError(msg);
        }


    }

    #endregion


    /// <summary>
    /// This must match the definition for the Repeating Group
    /// that is defined in the RunCalculator Step
    /// </summary>
    public class CalculationRow
    {
        /// <summary>
        /// The key will set the index to the Table.
        /// </summary>
        public int MyKey { get; set; }

        /// <summary>
        /// The index into the Simio table.
        /// </summary>
        public int MyIndex { get { return MyKey - 1; } }

        public double MyReal1 { get; set; }
        public double MyReal2 { get; set; }
        public double MyReal1Sqrt { get; set; }
        public double MyReal2Doubled { get; set; }


        public override string ToString()
        {
            return $"{MyKey} R1={MyReal1:0.00}  R2={MyReal2:0.00} Sqrt={MyReal1Sqrt:0.00} Double={MyReal2Doubled:0.00}";
        }


    }
}

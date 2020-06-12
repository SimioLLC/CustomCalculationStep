using System;
using System.Collections.Generic;
using System.Globalization;
using SimioAPI;
using SimioAPI.Extensions;

using SimioTableInterfaceStep;
using System.Linq;
using System.Text;
using System.IO;

namespace SimioTableInterfaceStep
{
    class SimioTableInterfaceStepDefinition : IStepDefinition
    {
        #region IStepDefinition Members

        /// <summary>
        /// Property returning the full name for this type of step. The name should contain no spaces. 
        /// </summary>
        public string Name
        {
            get { return "SimioTableInterface"; }
        }

        /// <summary>
        /// Property returning a short description of what the step does.  
        /// </summary>
        public string Description
        {
            get { return "The SimioTableInterface step is an example of putting/getting values to a Simio Table. The user defined File Element specifies output path"; }
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

        // {Changed Jun2020}
        static readonly Guid MY_ID = new Guid("{EB924E05-4214-4822-A1FB-7F5FCEC295D9}");

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

            // A repeat group of states to read into
            IRepeatGroupPropertyDefinition fields = schema.AddRepeatGroupProperty("MyMappedFields");
            fields.Description = "The RepeatGroup that will reference a Simio Table";

            pd = fields.PropertyDefinitions.AddStateProperty("StateField1");
            pd.Description = "A state/field to map into RepeatingGroup";

            pd = fields.PropertyDefinitions.AddStateProperty("StateField2");
            pd.Description = "A state/field to map into RepeatingGroup";

            pd = fields.PropertyDefinitions.AddStateProperty("StateField3");
            pd.Description = "A state/field to map into RepeatingGroup";

            pd = fields.PropertyDefinitions.AddStateProperty("StateField4");
            pd.Description = "A state/field to map into RepeatingGroup";

            pd = fields.PropertyDefinitions.AddRealProperty("PropertyField1", 9.9);
            pd.Description = "A property/field to map into RepeatingGroup";

            //pd = fields.PropertyDefinitions.AddExpressionProperty("PropertyField1", "Table1.MyRealProp1");
            //pd.Description = "A property/field to map into RepeatingGroup";

        }

        /// <summary>
        /// Method called to create a new instance of this step type to place in a process. 
        /// Returns an instance of the class implementing the IStep interface.
        /// </summary>
        public IStep CreateStep(IPropertyReaders properties)
        {
            return new SimioTableInterfaceStep(properties);
        }

        #endregion
    }

    class SimioTableInterfaceStep : IStep
    {
        IPropertyReaders _props;

        IRepeatingPropertyReader rgReader;

        List<CalculationRow> CalcDataList;

        Random RandomGenerator { get; set; }


        /// <summary>
        /// Constructor. Initialize data. Called as a model with this Step loads.
        /// </summary>
        /// <param name="properties"></param>
        public SimioTableInterfaceStep(IPropertyReaders properties)
        {
            _props = properties;

            RandomGenerator = new Random();

            CalcDataList = null; // new List<CalculationRow>();
        }

        #region IStep Members

        /// <summary>
        /// Method called when a process token executes the step.
        /// </summary>
        public ExitType Execute(IStepExecutionContext context)
        {
            rgReader = (IRepeatingPropertyReader) _props.GetProperty("MyMappedFields");

            // If not set up (e.g. this is the first time), create the CalcDataList
            // Since the RG references the table the count is the number of rows.
            if (CalcDataList == null)
            {
                CalcDataList = new List<CalculationRow>();

                int tableRowCount = rgReader.GetCount(context);
                for (int ri = 0; ri < tableRowCount; ri++)
                {
                    CalculationRow cr = new CalculationRow();
                    cr.MyKey = ri;
                    CalcDataList.Add(cr);
                }
            }

            //// Create values and put them in the CalculationRow objects
            foreach (CalculationRow cr in CalcDataList)
            {
                using (IPropertyReaders rowReader2 = rgReader.GetRow(cr.MyKey, context))
                {
                    PutSimioValuesToCalculationRowObject(context, RandomGenerator, rowReader2, cr);
                }
            } // next table row

            //======= Run a mock calculator/optimizer/whatever ==============
            if (RunMockCalculator(CalcDataList, out string explanation))
            {
                // Put the data back into Simio
                foreach (CalculationRow cr in CalcDataList)
                {
                    using (IPropertyReaders rowReader2 = rgReader.GetRow(cr.MyKey, context))
                    {
                        PutCalculationRowObjectToSimioValues(context, rowReader2, cr);
                    }
                }
                return ExitType.FirstExit;

            }
            else
            {
                Logit(context, $"Calculation Err={explanation}");
                return ExitType.AlternateExit;
            }

            ////}

        }


        private static bool PutSimioValuesToCalculationRowObject(IStepExecutionContext context, Random rand, IPropertyReaders rowReader, CalculationRow calcRow)
        {
            try
            {

                // Use the row reader to get a state reader
                IStateProperty stateReader = (IStateProperty)rowReader.GetProperty("StateField1");
                IState state = stateReader.GetState(context);
                state.StateValue = (10 * calcRow.MyKey) + 1; // Mock to create a value for the state
                calcRow.MyReal1 = state.StateValue;

                stateReader = (IStateProperty)rowReader.GetProperty("StateField2");
                state = stateReader.GetState(context);
                state.StateValue = (10 * calcRow.MyKey) + 2; // Mock to create a value for the State
                calcRow.MyReal2 = state.StateValue;

                stateReader = (IStateProperty)rowReader.GetProperty("StateField3");
                state = stateReader.GetState(context);
                state.StateValue = (10 * calcRow.MyKey) + 3; // Mock to create a value for the State
                calcRow.MyReal3 = state.StateValue;

                stateReader = (IStateProperty)rowReader.GetProperty("StateField4");
                state = stateReader.GetState(context);
                state.StateValue = (10 * calcRow.MyKey) + 4; // Mock to create a value for the State
                calcRow.MyReal4 = state.StateValue;

                return true;
            }
            catch (Exception ex)
            {
                Logit(context, $"Err={ex.Message}");
                return false;
            }
        }


        private static bool PutCalculationRowObjectToSimioValues(IStepExecutionContext context, IPropertyReaders rowReader, CalculationRow calcRow)
        {
            try
            {
                IStateProperty stateReader = (IStateProperty)rowReader.GetProperty("StateField1");
                IState state = stateReader.GetState(context);
                state.StateValue = calcRow.MyReal1;

                stateReader = (IStateProperty)rowReader.GetProperty("StateField2");
                state = stateReader.GetState(context);
                state.StateValue = calcRow.MyReal2;

                stateReader = (IStateProperty)rowReader.GetProperty("StateField3");
                state = stateReader.GetState(context);
                state.StateValue = calcRow.MyReal3;

                stateReader = (IStateProperty)rowReader.GetProperty("StateField4");
                state = stateReader.GetState(context);
                state.StateValue = calcRow.MyReal4;

                return true;
            }
            catch (Exception ex)
            {
                Logit(context, $"Err={ex.Message}");
                return false;
            }
        }


        /// <summary>
        ///  Add data to a file.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="calcDataList"></param>
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


        /// <summary>
        /// This mock optimizer changes some data in each row object of calcDataList.
        /// </summary>
        /// <returns></returns>
        private bool RunMockCalculator(List<CalculationRow> calcDataList, out string explanation)
        {
            explanation = "";
            try
            {
                foreach (CalculationRow cr in calcDataList)
                {
                    cr.MyReal3 = cr.MyReal1 + 1000;
                    cr.MyReal4 = cr.MyReal2 + 1000;
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
        public double MyReal3 { get; set; }
        public double MyReal4 { get; set; }


        public override string ToString()
        {
            return $"{MyKey} R1={MyReal1:0.00}  R2={MyReal2:0.00} R3={MyReal3:0.00} R4={MyReal4:0.00} ";
        }


    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using SimioAPI;
using SimioAPI.Extensions;

using CalculationStep;


namespace CalculationStep
{
    class CalculationStepDefinition : IStepDefinition
    {
        #region IStepDefinition Members

        /// <summary>
        /// Property returning the full name for this type of step. The name should contain no spaces. 
        /// </summary>
        public string Name
        {
            get { return "Calculation"; }
        }

        /// <summary>
        /// Property returning a short description of what the step does.  
        /// </summary>
        public string Description
        {
            get { return "The Calculation step is an example of reading/writing values to a Simio Table. The user defined File Element specifies output path"; }
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
            return new CalculationStep(properties);
        }

        #endregion
    }

    class CalculationStep : IStep
    {
        IPropertyReaders _props;
        IElementProperty prElement;
        IRepeatingPropertyReader prFields;

        CalculationRow CalcRow = new CalculationRow();


        public CalculationStep(IPropertyReaders properties)
        {
            _props = properties;

            prElement = (IElementProperty)_props.GetProperty("CalculationElement");
            prFields = (IRepeatingPropertyReader)_props.GetProperty("MyFieldValues");

        }

        #region IStep Members

        /// <summary>
        /// Method called when a process token executes the step.
        /// </summary>
        public ExitType Execute(IStepExecutionContext context)
        {
            IStateProperty sprVar1;
            IStateProperty sprTableIndex;

            // Set the row by changing the index value
            sprVar1 = (IStateProperty)_props.GetProperty("MyStateTableIndex");

            IState IndexState = sprVar1.GetState(context);
            var vv = IndexState.StateValue;
            IndexState.StateValue = DateTime.Now.Second / 20 + 1; // Just to generate a legitimate row index

            CalcRow.MyKey = (int) IndexState.StateValue; // Put the index in the row class in case the calculation method might need it.

            // Get the file
            CalculationElement calcElement = (CalculationElement)prElement.GetElement(context);
            if (calcElement == null)
            {
                context.ExecutionInformation.ReportError("CalculationElement is null.  Is it defined correctly?");
            }


            int fieldCount = prFields.GetCount(context);

            // Run through each field in our Repeating group and get a corresponding value from our CalculatorRow class.

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
                    state.StateValue = DateTime.Now.Millisecond; // Assign some value

                    switch (ii)
                    {
                        case 0:
                            CalcRow.MyReal1 = state.StateValue;
                            break;
                        case 1:
                            CalcRow.MyReal2 = state.StateValue;
                            break;
                        case 2:
                            break;
                        default:
                            break;
                    }
                } // using
                
            } // for each field in repeating group

            //======= Run the calculator/optimizer/whatever ==============
            if ( !RunCalculator(CalcRow, out string explanation) )
            {
                context.ExecutionInformation.TraceInformation($"Calculation Err={explanation}");
            }
            else
            {
                // Put values back into the table.


                
            }

            return ExitType.FirstExit;
        }


        /// <summary>
        /// This mock optimizer will receive a list of rows, and each row is a class whose
        /// construction must mimic the definition of the Step's repeating group.
        /// 
        /// </summary>
        /// <returns></returns>
        private bool RunCalculator(CalculationRow calcRow, out string explanation )
        {
            explanation = "";
            try
            {


                return true;
            }
            catch (Exception ex)
            {
                explanation = $"Err={ex}";
                return false;
            }
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

        public double MyReal1 { get; set; }
        public double MyReal2 { get; set; }


    }
}

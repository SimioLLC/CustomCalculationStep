using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SimioAPI;
using SimioAPI.Extensions;

namespace CustomCalculationSample2Step
{
    class CalculationSample2StepDefinition : IStepDefinition
    {
        #region IStepDefinition Members

        /// <summary>
        /// Property returning the full name for this type of step. The name should contain no spaces.
        /// </summary>
        public string Name
        {
            get { return "CalculationSample2"; }
        }

        /// <summary>
        /// Property returning a short description of what the step does.
        /// </summary>
        public string Description
        {
            get { return "Sample Calculation Step communicating with external (SQL Server) database"; }
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
        //{Changed 5May2020/Dan}
        static readonly Guid MY_ID = new Guid("{91E2E1A6-BB33-46FE-B030-086C6E7B9CEB}");

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
            // Example of how to add a property definition to the step.
            IPropertyDefinition pd;
            pd = schema.AddExpressionProperty("MyExpression", "0.0");
            pd.DisplayName = "My Expression";
            pd.Description = "An expression property for this step.";
            pd.Required = true;

            // Example of how to add an element property definition to the step.
            pd = schema.AddElementProperty("UserElementName", UserElementDefinition.MY_ID);
            pd.DisplayName = "UserElement Name";
            pd.Description = "The name of a UserElement element referenced by this step.";
            pd.Required = true;
        }

        /// <summary>
        /// Method called to create a new instance of this step type to place in a process.
        /// Returns an instance of the class implementing the IStep interface.
        /// </summary>
        public IStep CreateStep(IPropertyReaders properties)
        {
            return new CalculationSample2Step(properties);
        }

        #endregion
    }

    class CalculationSample2Step : IStep
    {
        IPropertyReaders _properties;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="properties"></param>
        public CalculationSample2Step(IPropertyReaders properties)
        {
            _properties = properties;
        }

        #region IStep Members

        /// <summary>
        /// Method called when a process token executes the step.
        /// </summary>
        public ExitType Execute(IStepExecutionContext context)
        {


            // Example of how to get the value of a step property.
            IPropertyReader myExpressionProp = _properties.GetProperty("MyExpression") as IPropertyReader;
            string myExpressionPropStringValue = myExpressionProp.GetStringValue(context);
            double myExpressionPropDoubleValue = myExpressionProp.GetDoubleValue(context);

            // Example of how to get an element reference specified in an element property of the step.
            IElementProperty myElementProp = (IElementProperty)_properties.GetProperty("UserElementName");
            CalculationSample2Element myElement = (CalculationSample2Element)myElementProp.GetElement(context);

            // Example of how to display a trace line for the step.
            context.ExecutionInformation.TraceInformation($"The value of expression '{myExpressionPropStringValue}' is '{myExpressionPropDoubleValue}'.");

            return ExitType.FirstExit;
        }


        public static void Logit(IStepExecutionContext context, string msg)
        {
            context.ExecutionInformation.TraceInformation($"CalculationSample2Step::{msg}");
        }

        public static void Alert(IStepExecutionContext context, string msg)
        {
            context.ExecutionInformation.ReportError($"CalculationSample2Step::{msg}");
        }

        #endregion
    }
}

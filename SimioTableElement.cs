using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Text;
using SimioAPI;
using SimioAPI.Extensions;

namespace SimioTableInterfaceStep
{

    internal static class MyStrings
    {
        internal const string TableColumnMappingsName = "TableColumnMappings";
        internal const string ColumnMapNameName = "ColumnName";
        internal const string ColumnMapStateName = "ColumnStateName";
        internal const string ColumnMapExpressionName = "ColumnExpressionName";
        internal const string ColumnMapPropertyName = "ColumnPropertyName";

        internal const string TableIndexName = "Table1Index";
        internal const string TableRowCountName = "TableRowCount";

    }

    class SimioTableElementDefinition : IElementDefinition
    {
        #region IElementDefinition Members

        /// <summary>
        /// Property returning the full name for this type of element. The name should contain no spaces. 
        /// </summary>
        public string Name
        {
            get { return "SimioTableElement"; }
        }

        /// <summary>
        /// Property returning a short description of what the element does.  
        /// </summary>
        public string Description
        {
            get { return "Helper for using a SimioTable during run-time."; }
        }

        /// <summary>
        /// Property returning an icon to display for the element in the UI. 
        /// </summary>
        public System.Drawing.Image Icon
        {
            get { return null; }
        }

        /// <summary>
        /// Property returning a unique static GUID for the element.  
        /// </summary>
        public Guid UniqueID
        {
            get { return MY_ID; }
        }

        /// <summary>
        /// We need to use this ID in the element reference property of the Read/Write steps, so we make it public.
        /// Changed 12Jun2020
        /// </summary>
        public static readonly Guid MY_ID = new Guid("{E5AB7FD0-655A-45C9-AB31-BC55F36F13C9}");

        /// <summary>
        /// Method called that defines the property, state, and event schema for the element.
        /// </summary>
        public void DefineSchema(IElementSchema schema)
        {
            IPropertyDefinitions _propDefs = schema.PropertyDefinitions;

            IPropertyDefinition pd;

            pd = _propDefs.AddStateProperty(MyStrings.TableIndexName);
            pd.Description = "References the State Property used to select the SimioTable row";
            pd.Required = false;

            pd = _propDefs.AddExpressionProperty(MyStrings.TableRowCountName, "0");
            pd.Description = "An Expression containing # of rows in the SimioTable";
            pd.Required = false;

            // A repeat group of states to read into
            IRepeatGroupPropertyDefinition columns = _propDefs.AddRepeatGroupProperty(MyStrings.TableColumnMappingsName);
            columns.Description = "The RepeatGroup mapping expressions to the Table columns";

            pd = columns.PropertyDefinitions.AddStringProperty(MyStrings.ColumnMapNameName, "");
            pd.Description = "Column Name Map";

            pd = columns.PropertyDefinitions.AddStateProperty(MyStrings.ColumnMapStateName);
            pd.Description = "Set if the column is a State";

            pd = columns.PropertyDefinitions.AddExpressionProperty(MyStrings.ColumnMapExpressionName, "");
            pd.Description = "Set if the column is an Expression";

            pd = columns.PropertyDefinitions.AddExpressionProperty(MyStrings.ColumnMapPropertyName, "");
            pd.Description = "Set if the column is a Property";

        }

        /// <summary>
        /// Method called to add a new instance of this element type to a model. 
        /// Returns an instance of the class implementing the IElement interface.
        /// </summary>
        public IElement CreateElement(IElementData data)
        {
            return new SimioTableElement(data);
        }

        #endregion
    }

    class SimioTableElement : IElement, IDisposable
    {
        readonly IElementData _Data;
        readonly IPropertyReaders _Props;

        readonly IExecutionContext _Context;

        readonly IStateProperty prTableRowIndex;
        readonly IRepeatingPropertyReader rprTableFields;


        public string FilePath { get; private set; }

        public bool OutputToFile { get; private set; }



        /// <summary>
        /// Constructor called as the run begins.
        /// </summary>
        /// <param name="data"></param>
        public SimioTableElement(IElementData data)
        {
            _Data = data;
            _Props = _Data.Properties; // Property readers
            _Context = _Data.ExecutionContext; // run-time execution context

            rprTableFields = (IRepeatingPropertyReader)_Props.GetProperty(MyStrings.TableColumnMappingsName);
            prTableRowIndex = (IStateProperty)_Props.GetProperty(MyStrings.TableIndexName);

            IExpressionPropertyReader prExpression = (IExpressionPropertyReader)_Props.GetProperty(MyStrings.TableRowCountName);
            double tableRowCount = (double) prExpression.GetExpressionValue(data.ExecutionContext);

            // Build a structure to hold data??
            ////CalcDataList = new List<CalculationRow>();

            ////for (int tr = 1; tr <= NbrTableRows; tr++)
            ////{
            ////    CalculationRow cr = new CalculationRow();
            ////    cr.MyKey = tr;
            ////    CalcDataList.Add(cr);
            ////}

        }



        private static void Logit(IExecutionContext context, string msg)
        {
            context.ExecutionInformation.ReportError($"Err={msg}");
        }

        #region IElement Members

        /// <summary>
        /// Method called when the simulation run is initialized.
        /// </summary>
        public void Initialize()
        {
            
        }

        /// <summary>
        /// Method called when the simulation run is terminating.
        /// </summary>
        public void Shutdown()
        {
            
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Shutdown();
        }

        #endregion
    }
}


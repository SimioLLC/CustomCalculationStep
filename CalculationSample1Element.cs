using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Text;
using SimioAPI;
using SimioAPI.Extensions;

namespace CalculationSample1Step
{
    class CalculationElementDefinition : IElementDefinition
    {
        #region IElementDefinition Members

        /// <summary>
        /// Property returning the full name for this type of element. The name should contain no spaces. 
        /// </summary>
        public string Name
        {
            get { return "CalculationElement"; }
        }

        /// <summary>
        /// Property returning a short description of what the element does.  
        /// </summary>
        public string Description
        {
            get { return "Used with the CalculationStep, holds the path to the file that will hold the table upon output."; }
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
        // We need to use this ID in the element reference property of the Read/Write steps, so we make it public
        // {Changed Apr2020}

        public static readonly Guid MY_ID = new Guid("{D6F34FEC-4C60-4D23-BDB0-1221934F76C0}");

        /// <summary>
        /// Method called that defines the property, state, and event schema for the element.
        /// </summary>
        public void DefineSchema(IElementSchema schema)
        {
            IPropertyDefinition pd = schema.PropertyDefinitions.AddStringProperty("FilePath", String.Empty);
            pd.Description = "The name of the file path for output.";

            pd = schema.PropertyDefinitions.AddBooleanProperty("OutputToFile");
            pd.Description = "If true, append table information to file each time step is executed";
        }

        /// <summary>
        /// Method called to add a new instance of this element type to a model. 
        /// Returns an instance of the class implementing the IElement interface.
        /// </summary>
        public IElement CreateElement(IElementData data)
        {
            return new CalculationElement(data);
        }

        #endregion
    }

    class CalculationElement : IElement, IDisposable
    {
        IElementData _data;
        string _Filepath;
        bool _OutputToFile;

        public string FilePath { get; private set; }

        public bool OutputToFile { get; private set; }

        public CalculationElement(IElementData data)
        {
            _data = data;
            IPropertyReader prFileName = _data.Properties.GetProperty("FilePath");
            IPropertyReader prOutputToFile = _data.Properties.GetProperty("OutputToFile");

            FilePath = prFileName.GetStringValue(_data.ExecutionContext);
            OutputToFile = bool.Parse(prOutputToFile.GetStringValue(_data.ExecutionContext));

        }


        public void LogIt(IElementData data, string msg)
        {
            data.ExecutionContext.ExecutionInformation.ReportError($"Err={msg}");
        }

        #region IElement Members

        /// <summary>
        /// Method called when the simulation run is initialized.
        /// </summary>
        public void Initialize()
        {
            // Initialize the Table??
        }

        /// <summary>
        /// Method called when the simulation run is terminating.
        /// </summary>
        public void Shutdown()
        {
            // On shutdown, what? Write table??
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


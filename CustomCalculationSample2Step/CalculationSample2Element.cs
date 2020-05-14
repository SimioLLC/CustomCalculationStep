using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SimioAPI;
using SimioAPI.Extensions;

namespace CustomCalculationSample2Step
{
    class UserElementDefinition : IElementDefinition
    {
        #region IElementDefinition Members

        /// <summary>
        /// Property returning the full name for this type of element. The name should contain no spaces.
        /// </summary>
        public string Name
        {
            get { return "CalculationSample2Element"; }
        }

        /// <summary>
        /// Property returning a short description of what the element does.
        /// </summary>
        public string Description
        {
            get { return "The element will hold our connect string"; }
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
        //{Changed 5May2020/dan}
        public static readonly Guid MY_ID = new Guid("{BAC6C715-6952-4B5B-8F4B-1BAD236B9F9D}");

        /// <summary>
        /// Method called that defines the property, state, and event schema for the element.
        /// </summary>
        public void DefineSchema(IElementSchema schema)
        {

            // Example of how to add a state definition to the element.
            IStateDefinition sd;
            sd = schema.StateDefinitions.AddState("MyState");
            sd.Description = "A state owned by this element";

            // Example of how to add an event definition to the element.
            IEventDefinition ed;
            ed = schema.EventDefinitions.AddEvent("MyEvent");
            ed.Description = "An event owned by this element";
        }

        /// <summary>
        /// Method called to add a new instance of this element type to a model.
        /// Returns an instance of the class implementing the IElement interface.
        /// </summary>
        public IElement CreateElement(IElementData data)
        {
            return new CalculationSample2Element(data);
        }

        #endregion
    }

    class CalculationSample2Element : IElement
    {
        IElementData _data;

        public CalculationSample2Element(IElementData data)
        {
            _data = data;
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
    }
}

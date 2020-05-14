using SimioAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationSample2Step
{
    public class SimioPropertyHelper
    {
        private IPropertyReaders _PropReaders { get; set; }

        public SimioPropertyHelper(IPropertyReaders propReaders)
        {
            this._PropReaders = propReaders;

        }

        /// <summary>
        /// Dictionary of readers, keyed by Propertyname (insensitive)
        /// </summary>
        private Dictionary<string, IPropertyReader> ReaderDict = new Dictionary<string, IPropertyReader>();

        public string GetString(IStepExecutionContext context, string propertyName)
        {
            try
            {
                IPropertyReader pr = _PropReaders.GetProperty(propertyName);
                return pr.GetStringValue(context);
            }
            catch (Exception ex)
            {
                Logit(context, $"Error getting Property={propertyName}. Err={ex}");
                return string.Empty;
            }
        }


        /// <summary>
        /// Displays a Trace line when running Simio in Trace mode
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        public static void Logit(IStepExecutionContext context, string msg)
        {
            context.ExecutionInformation.TraceInformation($"CalculationSample2Step::{msg}");
        }

    }
}

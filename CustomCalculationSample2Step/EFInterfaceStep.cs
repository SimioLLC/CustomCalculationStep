using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using EFInterfaceStep;
using SimioAPI;
using SimioAPI.Extensions;

namespace CustomEFInterfaceStep
{
    /// <summary>
    /// Demonstrates using an execution step communicating with SQL Server using EF.
    /// In this example, the step is used to record Entity actions
    /// Each time an Entity enters the step the following values are set or updated in the database:
    /// 
    /// </summary>
    class EFInterfaceStepDefinition : IStepDefinition
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
            IPropertyDefinition pd = null;

            pd = schema.AddStringProperty("SqlInstance", "");
            pd.DisplayName = "SQL Instance";
            pd.Description = @"The SQL Instance name, aka DataSource. E.g. (localhost)\SqlExress01";

            pd = schema.AddStringProperty("SqlDbName", "");
            pd.DisplayName = @"SQL DB Name";
            pd.Description = "The database name, or 'Initial Catalog'";

            pd = schema.AddDateTimeProperty("StartDateTime", DateTime.Now);
            pd.DisplayName = @"Start Date/Time";
            pd.Description = "When the simulation starts. Defaults to the current time.";
        }


        /// <summary>
        /// Method called to create a new instance of this step type to place in a process.
        /// Returns an instance of the class implementing the IStep interface.
        /// </summary>
        public IStep CreateStep(IPropertyReaders properties)
        {
            return new EFInterfaceStep(properties);
        }

        #endregion
    }

    class EFInterfaceStep : IStep
    {
        IPropertyReaders _properties;

        string _efConnectString;

        DateTime _dtStart = DateTime.MinValue;

        private SimioPropertyHelper PropertyHelper;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="properties"></param>
        public EFInterfaceStep(IPropertyReaders properties)
        {
            _properties = properties;

            PropertyHelper = new SimioPropertyHelper(properties);

            //var prSqlConnectionString = _properties.GetProperty("SqlConnectionString");
             

        }

        #region IStep Members




        /// <summary>
        /// Method called when a process token executes the step.
        /// </summary>
        public ExitType Execute(IStepExecutionContext context)
        {
            if ( string.IsNullOrEmpty(_efConnectString))
            {
                string sqlInstance = PropertyHelper.GetString(context, "SqlInstance");
                string sqlDbName = PropertyHelper.GetString(context, "SqlDbName");

                // The name of the Entity Framework Model that we created.
                string efModel = "SimioServerModel";

                _efConnectString = EntityFrameworkHelpers.BuildEfConnectionString(sqlInstance, sqlDbName, efModel);

                string startDt = PropertyHelper.GetString(context, "StartDateTime");
                _dtStart = DateTime.Parse(startDt);

                string sqlConnectString = EntityFrameworkHelpers.BuildDbConnectionString(sqlInstance, sqlDbName);
                using ( SqlConnection sqlConn = new SqlConnection(sqlConnectString))
                {
                    sqlConn.Open();
                    using (SqlCommand cmd = new SqlCommand("DELETE * FROM EntityVisits WHERE 1=1"))
                    {
                        int nn = cmd.ExecuteNonQuery();
                    }
                }

            }

            var entity = context.AssociatedObject;
           
            
            using ( SimioCalculationStepSample2Entities efContext = new SimioCalculationStepSample2Entities(_efConnectString) )
            {
                var server = efContext.Servers.SingleOrDefault(rr => rr.Name == "");

                // First see if this Enity exists
                EntityVisit ev = new EntityVisit();
                ev.EntityId = EntityFrameworkHelpers.BuildEntityKey(entity.HierarchicalDisplayName); 
                ev.ArrivalTime = _dtStart.AddSeconds( context.Calendar.TimeNow );
                ev.DepartureTime = null;

                efContext.EntityVisits.Add(ev);

                efContext.SaveChanges();
            }

            return ExitType.FirstExit;
        }


        /// <summary>
        /// Displays a Trace line when running Simio in Trace mode
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        public static void Logit(IStepExecutionContext context, string msg)
        {
            context.ExecutionInformation.TraceInformation($"EFInterfaceStep::{msg}");
        }

        /// <summary>
        /// Displays a modal Simio alert box
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        public static void Alert(IStepExecutionContext context, string msg)
        {
            context.ExecutionInformation.ReportError($"EFInterfaceStep::{msg}");
        }

        #endregion
    }
}

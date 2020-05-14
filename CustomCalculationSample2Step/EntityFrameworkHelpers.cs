using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationSample2Step
{
    public static class EntityFrameworkHelpers
    {

        public static string BuildEntityKey(string entityName)
        {
            string key = $"{DateTime.Now:yymmdd-HHmmss}-{entityName}";
            return key;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlInstanceName"></param>
        /// <param name="sqlDbName"></param>
        /// <param name="efModelName"></param>
        /// <returns></returns>
        public static string BuildDbConnectionString(string sqlInstanceName, string sqlDbName)
        {
            try
            {
                SqlConnectionStringBuilder sqlConnectionBuilder = new SqlConnectionStringBuilder()
                {
                    DataSource = sqlInstanceName,
                    InitialCatalog = sqlDbName,
                    IntegratedSecurity = true,
                    MultipleActiveResultSets = true,
                };

                string dbConnectString = sqlConnectionBuilder.ConnectionString;
                return dbConnectString;

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"SqlServer: Instance={sqlInstanceName} DB={sqlDbName} Err={ex.Message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlInstanceName"></param>
        /// <param name="sqlDbName"></param>
        /// <param name="efModelName"></param>
        /// <returns></returns>
        public static string BuildEfConnectionString(string sqlInstanceName, string sqlDbName, string efModelName)
        {
            try
            {
                string dbConnectionString = BuildDbConnectionString(sqlInstanceName, sqlDbName);

                var entityBuilder = new EntityConnectionStringBuilder();

                string efModel = "SimioServerModel";
                entityBuilder.Metadata = $@"res://*/{efModel}.csdl|res://*/{efModel}.ssdl|res://*/{efModel}.msl";
                entityBuilder.Provider = "System.Data.SqlClient";
                entityBuilder.ProviderConnectionString = dbConnectionString;
                string ecs = entityBuilder.ConnectionString;

                return ecs;

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"SqlServer: Instance={sqlInstanceName} DB={sqlDbName} EFModel={efModelName} Err={ex.Message}");
            }
        }
    }
}

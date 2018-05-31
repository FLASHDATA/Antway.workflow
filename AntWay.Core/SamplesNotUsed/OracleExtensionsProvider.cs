using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using Oracle.ManagedDataAccess.Client;

//NO SE UTILIZA DE MOMENTO
//PRUEBAS MÉTODOS ORACLE PROVIDRE
namespace OptimaJet.Workflow.Oracle
{
    public class OracleExtensionsProvider 
    {
        public string ConnectionString { get; set; }
        public string Schema { get; set; }
        private WorkflowRuntime _runtime;
        public void Init(WorkflowRuntime runtime)
        {
            _runtime = runtime;
        }

        public OracleExtensionsProvider(string connectionString, string schema = null)
        {
            DbObject.SchemaName = schema;
            ConnectionString = connectionString;
            Schema = schema;
        }

        public List<ProcessInstance> GetProcessRunning()
        {
            using (OracleConnection connection = new OracleConnection(ConnectionString))
            {
                WorkflowProcessInstanceStatus
                    .Select(connection, "SELECT * FROM WORKFLOWPROCESSINSTANCES WHERE Status in(0,1)");
            }

            return new List<ProcessInstance>();
        }

    }
}

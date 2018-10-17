using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Antway.Core.Persistence;
using AntWay.Core.Activity;
using AntWay.Core.Mapping;
using AntWay.Core.Model;
using AntWay.Core.Runtime;
using AntWay.Persistence.Provider.Model;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core.Model;
using Sample.Model.Expedientes;
using Temp.Factory;

namespace Client.Winforms.Demos
{
    public partial class Form2 : Form
    {
        private string NewBlock => $"{Environment.NewLine}---------------------------------------------------------{Environment.NewLine}";

        private static Guid? ProcessId;
        private string Localizador;


        public Form2()
        {
            InitializeComponent();
        }



        private void LoadLocalizadores(string schemeCode)
        {
            var locatorPersistence = new LocatorPersistence
            {
                IDALocators = PersistenceObjectsFactory.GetIDALLocatorsObject(),
            };

            var locators = locatorPersistence.GetLocatorsFromScheme(schemeCode);

            lbLocalizadores.Items.Clear();
            foreach (var l in locators)
            {
                lbLocalizadores.Items.Add(l);
            }
            lbLocalizadores.DisplayMember = "LocatorValue";
            lbLocalizadores.ValueMember = "WFProcessGuid";

            //select current item
            foreach(var item in lbLocalizadores.Items)
            {
                if (((ProcessPersistenceView) item).LocatorValue == tbLocalizador.Text)
                {
                    lbLocalizadores.SelectedItem = item;
                    break;
                }
            }
        }

        protected bool CheckTimersExpired(ProcessInstance processInstance = null)
        {
            processInstance = processInstance
                                ?? WorkflowClient.AntWayRunTime
                                    .GetProcessInstance(ProcessId.Value);

            string transitionExpiredStateTo = WorkflowClient.AntWayRunTime
                                .GetTransitionExpiredState(processInstance);

            if (transitionExpiredStateTo != null)
            {
                WorkflowClient.AntWayRunTime
                    .SkipToState(ProcessId.Value, transitionExpiredStateTo);

                RefreshWorkflow($"Salto a {transitionExpiredStateTo} por caducidad de periodo {Environment.NewLine}");
                return true;
            }
            return false;
        }


        private void btTest_Click(object sender, EventArgs e)
        {
            var processInstance = WorkflowClient.AntWayRunTime
                                    .GetProcessInstance(ProcessId.Value);

            CheckTimersExpired(processInstance);
        }

        private void LoadActivities()
        {
            if (ProcessId == null) return;

            var processInstance = WorkflowClient.AntWayRunTime
                     .GetProcessInstance(ProcessId.Value);

            var activities = WorkflowClient.AntWayRunTime
                            .GetActivities(processInstance)
                            .Select(a => new KeyValuePair<string, string>(a.Id, $"{a.Id.Substring(0, 10)}/{a.Name}"))
                            .ToList();


            lbActivities.Items.Clear();
            lbActivities.DisplayMember = "Value";
            lbActivities.ValueMember = "Key";

            foreach(var a in activities)
            {
                lbActivities.Items.Add(a);
            }
        }


        private void btRun_Click(object sender, EventArgs e)
        {
            btSimulateBDChange.Enabled = false;

            if (tbLocalizador.Text == "" || cmbSchemeCodes.SelectedItem?.ToString() == null)
            {
                MessageBox.Show("Elegeix un esquema i localitzador");
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            ProcessId = null;

            Localizador = !String.IsNullOrEmpty(tbLocalizador.Text)
                        ? $"{cmbSchemeCodes.SelectedItem.ToString()}/{tbLocalizador.Text}"
                        : Guid.NewGuid().ToString();

            var locatorPersistence = new LocatorPersistence
                { IDALocators = PersistenceObjectsFactory.GetIDALLocatorsObject() };
            var wfInstance = locatorPersistence.GetWorkflowByLocator(Localizador);
            
            ProcessId = (wfInstance?.WFProcessGuid == null)
                                ? (Guid?) null 
                                : wfInstance.WFProcessGuid;

            string log = "";

            log += wfInstance?.WFProcessGuid != null
                      ? $"Recuperado Workflow con localizador {Localizador}{Environment.NewLine}"
                      : $"Nuevo Workflow con localizador {Localizador}{Environment.NewLine}";


            StartWF(cmbSchemeCodes.SelectedItem.ToString());
            LoadLocalizadores(cmbSchemeCodes.SelectedItem.ToString());

            RefreshWorkflow(log);

            ProcessInstance pi = GetAntWayProcessInstance(Localizador);
            ActivityDefinition activity = pi.CurrentActivity;
            List<TransitionDefinition>
                transitions = pi
                              .ProcessScheme
                              .GetPossibleTransitionsForActivity(activity)
                              .ToList();

            if (transitions.Any(t => t.IsConditionTransition))
            {
                WorkflowClient.AntWayRunTime
                                .ExecuteTriggeredConditionalTransitions(pi, transitions);
                RefreshWorkflow("");
            }

            tbActivityExecutionDetail.Clear();
            LoadActivities();

            Cursor.Current = Cursors.Default;
        }


        private void RefreshWorkflow(string log)
        {

           var availableCommands = WorkflowClient.AntWayRunTime
                                    .GetAvailableCommands(ProcessId.Value);

            btSiguiente.Enabled = availableCommands
                            .FirstOrDefault(c => c.CommandName.ToLower() == SchemeCommandNames.Single.Siguiente.ToLower()) != null;

            btAnterior.Enabled = availableCommands
                            .FirstOrDefault(c => c.CommandName.ToLower() == SchemeCommandNames.Single.Anterior.ToLower()) != null;

            btnFirmarDoc.Enabled = availableCommands
                                .FirstOrDefault(c => c.CommandName.ToLower() == SchemeCommandNames.Single.Firmar.ToLower()) != null;

            string currentActivityName = WorkflowClient.AntWayRunTime
                                         .GetCurrentActivityName(ProcessId.Value);

            string currentStateName = WorkflowClient.AntWayRunTime
                                         .GetCurrentStateName(ProcessId.Value);

                    
            tbLog.Text = $"{log}{Environment.NewLine}" +
                         //$"Parámetros :{String.Join(" | ", parameters)}" +
                         //$"{Environment.NewLine}" +
                         $"Actividad: {currentActivityName}" +
                         $"{Environment.NewLine}" +
                         $"Tag: {currentStateName}" +
                         NewBlock +
                         $"{Environment.NewLine}" +
                         $"{tbLog.Text}";
        }


        private void StartWF(string schemeCode)
        {
            IAssemblies assemblies = AssemblyFactory.GetAssemblyObject(schemeCode);
            WorkflowClient.WithAssemblies(assemblies);
            //WorkflowClient.WithTimeManager(new TimerManager());
            //IWorkflowActionProvider actionProvider = new ExpedientesActionProvider();
            //WorkflowClient.WithActionProvider(actionProvider);
            WorkflowClient.GetAntWayRunTime(schemeCode);

            if (ProcessId == null)
            {
                var processPersistenceViewNew = new ProcessPersistenceView
                {
                    WFProcessGuid = Guid.NewGuid(),
                    LocatorFieldName = "Table.Field",
                    LocatorValue = Localizador,
                    SchemeCode = schemeCode,
                };

                ProcessId = WorkflowClient.GetAntWayRunTime(schemeCode)
                            .CreateInstanceAndPersist(processPersistenceViewNew);
            }

            CheckTimersExpired();
        }

        private void btSiguiente_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            bool caducado =  CheckTimersExpired();
            if (caducado)
            {
                Cursor.Current = Cursors.Default;
                return;
            }
            //var availableCommands = WorkflowClient.AntWayRunTime
            //             .GetAvailableCommands(ProcessId.Value);

            //bool commandAvailable = availableCommands

            WorkflowClient.AntWayRunTime
                .ExecuteCommand(ProcessId.Value, SchemeCommandNames.Single.Siguiente);

            RefreshWorkflow($"Workflow con localizador {Localizador}");
            Cursor.Current = Cursors.Default;
        }

        private void btAnterior_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            bool caducado = CheckTimersExpired();
            if (caducado)
            {
                Cursor.Current = Cursors.Default;
                return;
            }

            WorkflowClient.AntWayRunTime
                .ExecuteCommand(ProcessId.Value, SchemeCommandNames.Single.Anterior);


            RefreshWorkflow($"Workflow con localizador {Localizador}");
            Cursor.Current = Cursors.Default;
        }

        private void btnDocSignat_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            bool caducado = CheckTimersExpired();
            if (caducado)
            {
                Cursor.Current = Cursors.Default;
                return;
            }

            WorkflowClient.AntWayRunTime
                    .ExecuteCommand(ProcessId.Value, SchemeCommandNames.Single.Firmar);
            RefreshWorkflow($"Documento de localizador {Localizador} firmado");

            Cursor.Current = Cursors.Default;
        }

     
        private ProcessInstance GetAntWayProcessInstance(string localizador)
        {
            if (String.IsNullOrEmpty(localizador))
            {
                return null;
            }

            var locatorPersistence = new LocatorPersistence
            {
                IDALocators = PersistenceObjectsFactory.GetIDALLocatorsObject(),
            };
            var wfInstance = locatorPersistence.GetWorkflowByLocator(localizador);

            if (wfInstance?.WFProcessGuid == null)
            {
                return null;
            }

            var processInstance = WorkflowClient.AntWayRunTime
                    .GetProcessInstance(wfInstance.WFProcessGuid);

            return processInstance;
        }

        private void cmbSchemeCodes_SelectedValueChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            string schemeCode = cmbSchemeCodes.SelectedItem.ToString();

            var schemesPersistence = new SchemesPersistence
            {
                IDALSchemes = PersistenceObjectsFactory.GetIDALWFSchemaObject(),
                IDALSchemeParameters = PersistenceObjectsFactory.GetIDALWFSchemeParameters(),
            };

            var scheme = schemesPersistence.GetScheme(schemeCode);
            if (scheme == null)
            {
                throw new NotImplementedException("Esquema inexistente");
            }

            WorkflowClient.DataBaseScheme = scheme?.DBSchemeName;

            lbActivities.Items.Clear();
            tbLocalizador.Clear();
            LoadLocalizadores(schemeCode);

            btSimulateBDChange.Enabled = false;
            btAnterior.Enabled = false;
            btSiguiente.Enabled = false;
            btnFirmarDoc.Enabled = false;

            Cursor.Current = Cursors.Default;
        }


        private void lbLocalizadores_SelectedValueChanged(object sender, EventArgs e)
        {
            if (lbLocalizadores.Text.IndexOf("/") == -1) return;

            tbLocalizador.Text = lbLocalizadores.Text.Split('/')[1];
            tbActivityExecutionDetail.Clear();
        }

        private void lbActivities_SelectedValueChanged(object sender, EventArgs e)
        {
            if (ProcessId == null) return;
            string activityId = lbActivities.SelectedItem != null
                                    ? ((KeyValuePair<string, string>)lbActivities.SelectedItem).Key
                                    : null;
            if (activityId == null) return;

            var processInstance = WorkflowClient.AntWayRunTime
                                    .GetProcessInstance(ProcessId.Value);

            string jsonObjectString = WorkflowClient.AntWayRunTime
                                       .GetActivityExecutionJsonObject(processInstance, 
                                                                      activityId);

            tbActivityExecutionDetail.Text = "";
            if (jsonObjectString == null) return;

            btSimulateBDChange.Enabled = (cmbSchemeCodes.SelectedItem.ToString() == "EXPEDIENTES");

            //TODO: Move to core
            var activityExecution = WorkflowClient.AntWayRunTime
                                    .GetActivityExecutionObject<List<ActivityExecution>>
                                            (processInstance, activityId)
                                    .LastOrDefault();
            
            IAntWayRuntimeActivity activityInstance = WorkflowClient.AntWayRunTime
                                                       .GetActivityExecutionObject<List<ActivityEnviarASignar>>
                                                            (processInstance, activityId)
                                                       .LastOrDefault();
            if (activityInstance == null) return;
            activityInstance.ParametersBind = MappingReflection
                                              .GetParametersBind(ProcessId.Value.ToString(),
                                                  activityInstance,
                                                  JsonConvert
                                                  .DeserializeObject<ActivityEnviarASignarParametersOutput>
                                                    (activityExecution.ParametersOut.ToString()));
            //

            var parametersBind = (ActivityEnviarASignarParametersOutput)activityInstance.ParametersBind;
            var parameterBindSignatura = parametersBind != null
                                            ? $"ParametersBind: Parámetro PARAMETER_SIGNATURA = {parametersBind.PARAMETER_SIGNATURA}"
                                            : "";

            tbActivityExecutionDetail.Text = $"{parameterBindSignatura} {Environment.NewLine} {Environment.NewLine} {jsonObjectString}";
        }


        private void btSimulateBDChange_Click(object sender, EventArgs e)
        {
            if (ProcessId == null)
            {
                MessageBox.Show("Debe especificar localizador");
                return;
            }

            ActivityEnviarASignar.SetParameterSignaturaSignat(ProcessId.Value.ToString());

            lbActivities.SelectedIndex = 0;
            lbActivities_SelectedValueChanged(null, null);
        }

    }
}

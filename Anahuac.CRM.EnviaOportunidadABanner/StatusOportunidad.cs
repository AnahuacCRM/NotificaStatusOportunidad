using Anahuac.CRM.EnviaOportunidadABanner.CRM;
using Anahuac.CRM.EnviaOportunidadABanner.Cross;
using Anahuac.CRM.EnviaOportunidadABanner.Cross.Extensiones;
using Anahuac.CRM.EnviaOportunidadABanner.DataLayer;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using Rhino.RetrieveBearerToken;
using Rhino.RetrieveBearerToken.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using XRM;
using System.ServiceModel;


namespace Anahuac.CRM.EnviaOportunidadABanner
{
    public class StatusOportunidad : IPlugin
    {
        private readonly string postImageUpdate = "PostImage";
        private readonly string PreImageUpdate = "PreImage";
        public void Execute(IServiceProvider serviceProvider)
        {

            ServerConnection _cnx;
            _cnx = new ServerConnection(serviceProvider);
            _cnx.context.SharedVariables.Clear();

            Guid opportunityId = Guid.Empty;

            #region " Validaciones de Plugin "


            //if (_cnx.context.MessageName != "Create" && _cnx.context.MessageName != "Update")
            //{
            //    _cnx.context.SharedVariables.Add("AbortProcess", "Is not Update or Create");
            //    _cnx.trace.Trace("Is not Update or Create");
            //    return;
            //}

            if (_cnx.context.MessageName != "Lose" && _cnx.context.MessageName != "Win" && _cnx.context.MessageName != "SetStateDynamicEntity")
            {
                _cnx.context.SharedVariables.Add("AbortProcess", "Is not Lose or Win");
                _cnx.trace.Trace("Is not Lose or Win or SetStateDynamicEntity");
                return;
            }

            if (_cnx.context.Stage != 40)
            {
                _cnx.context.SharedVariables.Add("AbortProcess", "Invalid Stage");
                _cnx.trace.Trace("Invalid Stage");
                return;
            }

            if (_cnx.context.Depth > 4)
            {
                _cnx.context.SharedVariables.Add("AbortProcess", "Deepth has exceded");
                _cnx.trace.Trace("Deepth has exceded");
                return;
            }

            //if (!_cnx.context.InputParameters.Contains("Target"))
            //{
            //    _cnx.context.SharedVariables.Add("AbortProcess", "Do not Contains Target");
            //    _cnx.trace.Trace("Do not Contains Target");
            //    return;
            //}

            //if (!(_cnx.context.InputParameters["Target"] is Entity))
            //{
            //    _cnx.context.SharedVariables.Add("AbortProcess", "Is Not an Entity");
            //    _cnx.trace.Trace("Is Not an Entity");
            //    return;
            //}

            //if (_cnx.context.PrimaryEntityName != "opportunity")
            //{
            //    _cnx.context.SharedVariables.Add("AbortProcess", "Is not a opportunity");
            //    _cnx.trace.Trace("Is not a opportunity");
            //    return;
            //}


            #endregion

            try
            {

                IPluginExecutionContext context = _cnx.context;






                int StatusOportundiad = 0;
                string idOportundiad = "";
                _cnx.trace.Trace("Validando opportunityclose");

                Guid opporid = Guid.Empty;

                if (context.InputParameters.Contains("OpportunityClose") && context.InputParameters["OpportunityClose"] is Entity)
                {
                    _cnx.trace.Trace("Primer valdiacion");

                    Entity entity = (Entity)context.InputParameters["OpportunityClose"];
                    OpportunityClose opc = entity.ToEntity<OpportunityClose>();
                    if (context.InputParameters.Contains("Status"))
                    {
                        _cnx.trace.Trace("Contiene status");
                        OptionSetValue status = (OptionSetValue)context.InputParameters["Status"];
                        _cnx.trace.Trace(" status de la oportundiad " + status.Value);
                        _cnx.trace.Trace(" id de la oportundiad " + opc.OpportunityId.Id);
                        StatusOportundiad = status.Value;
                        idOportundiad = opc.OpportunityId.Id.ToString();


                    }

                    if (entity.Attributes.Contains("opportunityid") && entity.Attributes["opportunityid"] != null)
                    {
                        _cnx.trace.Trace("Segunda Validacion");
                        EntityReference entityRef = (EntityReference)entity.Attributes["opportunityid"];

                        if (entityRef.LogicalName == "opportunity")
                        {
                            opportunityId = entityRef.Id;
                        }
                    }

                    _cnx.trace.Trace("opportunityclose", opportunityId);
                }






                #region para reopen opportunidad

                _cnx.trace.Trace("Validando EntityMoniker");
                int postStatus = -1;
                if (context.InputParameters.Contains("EntityMoniker") && context.InputParameters["EntityMoniker"] is EntityReference
                && ((EntityReference)context.InputParameters["EntityMoniker"]).LogicalName == "opportunity")
                {
                    _cnx.trace.Trace("entro las validaciones EntityMoniker y el message es   " + context.MessageName);
                    switch (context.MessageName)
                    {
                        // Check if entity status is changed
                        case "SetStateDynamicEntity":
                            if (context.InputParameters.Contains("State"))
                            {
                                _cnx.trace.Trace("Entro en el primer validacion del case" + context.Stage);
                                EntityReference OpprtunityEntityRef = (EntityReference)context.InputParameters["EntityMoniker"];


                                idOportundiad = OpprtunityEntityRef.Id.ToString();


                                if (((OptionSetValue)context.InputParameters["State"]).Value == 0)
                                {
                                    postStatus = ((OptionSetValue)context.InputParameters["State"]).Value;

                                }
                            }
                            break;
                    }
                    //throw new InvalidPluginExecutionException(" status after " + postStatus);
                }

                #endregion

                VariablesRepository Serv = new VariablesRepository(_cnx);

                NotificaStatus StatusOP = new NotificaStatus();
                StatusOP.id_Oportunidad = idOportundiad;

                switch (StatusOportundiad)
                {
                    case 4:
                        _cnx.trace.Trace(" la oportunidad esta cerrada : ");
                        StatusOP.Estatus = "C";
                        break;
                    case 0:
                        _cnx.trace.Trace(" la oportunidad esta abierta : ");
                        StatusOP.Estatus = "A";
                        break;
                    case 1:
                        StatusOP.Estatus = "A";
                        _cnx.trace.Trace(" la oportunidad esta  en curso : ");
                        break;
                    case 3:
                        StatusOP.Estatus = "C";
                        _cnx.trace.Trace(" la oportunidad esta  ganada : ");
                        break;
                }

                _cnx.trace.Trace(" Mandamos consumir el servicio ");

                _cnx.trace.Trace(" status del objeto a mandar  " + StatusOP.Estatus);
                _cnx.trace.Trace(" id oportunidad del objeto a mandar  " + StatusOP.id_Oportunidad);

                Serv.NotificaStatusOportunidad(StatusOP);





            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                _cnx.trace.Trace("The application terminated with an error.");
                _cnx.trace.Trace("Timestamp: {0}", ex.Detail.Timestamp);
                _cnx.trace.Trace("Code: {0}", ex.Detail.ErrorCode);
                _cnx.trace.Trace("Message: {0}", ex.Detail.Message);
                _cnx.trace.Trace("Inner Fault: {0}",
                    null == ex.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
            }
            catch (System.TimeoutException ex)
            {
                _cnx.trace.Trace("The application terminated with an error.");
                _cnx.trace.Trace("Message: {0}", ex.Message);
                _cnx.trace.Trace("Stack Trace: {0}", ex.StackTrace);
                _cnx.trace.Trace("Inner Fault: {0}",
                    null == ex.InnerException.Message ? "No Inner Fault" : ex.InnerException.Message);
            }
            catch (System.Exception ex)
            {
                _cnx.trace.Trace("The application terminated with an error.");
                _cnx.trace.Trace(ex.Message);

                // Display the details of the inner exception.
                if (ex.InnerException != null)
                {
                    _cnx.trace.Trace(ex.InnerException.Message);

                    FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> fe = ex.InnerException
                        as FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>;
                    if (fe != null)
                    {
                        _cnx.trace.Trace("Timestamp: {0}", fe.Detail.Timestamp);
                        _cnx.trace.Trace("Code: {0}", fe.Detail.ErrorCode);
                        _cnx.trace.Trace("Message: {0}", fe.Detail.Message);
                        _cnx.trace.Trace("Trace: {0}", fe.Detail.TraceText);
                        _cnx.trace.Trace("Inner Fault: {0}",
                            null == fe.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
                    }
                }
            }

        }
    }
}

using Anahuac.CRM.EnviaOportunidadABanner.CRM;
using Anahuac.CRM.EnviaOportunidadABanner.Cross;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
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

namespace Anahuac.CRM.EnviaOportunidadABanner.DataLayer
{
    public class VariablesRepository : IVariablesRepository
    {
        private readonly IServerConnection _cnx;

        public VariablesRepository(IServerConnection cnx)
        {
            _cnx = cnx;
        }

        public void NotificaStatusOportunidad(NotificaStatus Status)
        {
           

            _cnx.trace.Trace("****Iniciciando el consumo*****");
            #region Envio de informacion
            #region Seguridad
            _cnx.trace.Trace("Obteniendo variables de seguridad");
             var uriToken = ObtenerVariableSistema(ua_variablesistema.EntityLogicalName, "UrlToken");
            // var uriToken = @"https://rua-integ-dev.ec.lcred.net/wsBannerCRMP/o/Server";
            _cnx.trace.Trace("uriToken : " + uriToken);

            var aplicacion = ObtenerVariableSistema(ua_variablesistema.EntityLogicalName, "AplicacionSeguridad");
            //var aplicacion = "Banner";
            _cnx.trace.Trace("aplicacion : " + aplicacion);

            var secret = ObtenerVariableSistema(ua_variablesistema.EntityLogicalName, "SecretSeguridad");
            //var secret = "ygCz85Z7SBA5HhXCLEjp/Vfb9j1oRzesmBVPNal8u+2ggb0TQO662/xNfyPf2NCRvwA/ppY/OYVn38Eu6w9Sgg==";
            _cnx.trace.Trace("secret : " + secret);


            var usuario = ObtenerVariableSistema(ua_variablesistema.EntityLogicalName, "UsuarioSeguridad");
            //var usuario = "Banner";
            SecurityToken st = new SecurityToken();
            Token token = st.RetrieveBearerToken(uriToken, aplicacion, secret, usuario);
            #endregion

            var url = ObtenerVariableSistema("ua_variablesistema", "url Notificastatus");//URL wsBaner 
            //var url = @"https://rua-integ-dev.ec.lcred.net/wsBannerCRMP/api/srvGestionEstatusOportunidades";
            _cnx.trace.Trace("URL WS Banner 'Notifica status oportunidad' : " + url);


            HttpClient proxy = new HttpClient();
            proxy.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var responsePost = proxy.PostAsJsonAsync(url, Status).Result;

            string json = JsonConvert.SerializeObject(Status);
            _cnx.trace.Trace("--------------------");
            _cnx.trace.Trace("--------------------");
            _cnx.trace.Trace("Request Json: {0}", json);

            _cnx.trace.Trace("responsePost.StatusCode: {0}", responsePost.StatusCode);
            if (responsePost.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var mensaje = responsePost.Content.ReadAsStringAsync().Result;
                _cnx.trace.Trace("Ocurrio un error al enviar la informacion a Banner: " + mensaje);
                // OpportunitytoUpdate.Attributes["rs_banderaenviooportunidad"] = new OptionSetValue(0);// No Enviado
                // _cnx.service.Update(OpportunitytoUpdate);
                return;
            }

            var resultPost = responsePost.Content.ReadAsStringAsync().Result;
            _cnx.trace.Trace("Respuesta Json: {0}", resultPost);


            #endregion


            
        }




        public string ObtenerVariableSistema(string EntityLogicalName, string Variable)
        {


            string resultado = string.Empty;

            QueryExpression query = new QueryExpression()
            {
                NoLock = true,
                EntityName = EntityLogicalName,
                ColumnSet = new ColumnSet(ua_variablesistema.Fields.ua_Valortexto),
                Criteria =
                {
                    Conditions = {
                         new ConditionExpression(ua_variablesistema.Fields.ua_name, ConditionOperator.Equal, Variable)
                    }
                },
            };

            EntityCollection ec = _cnx.service.RetrieveMultiple(query);

            if (ec.Entities.Any())
                resultado = ec.Entities[0].GetAttributeValue<string>(ua_variablesistema.Fields.ua_Valortexto);

            return resultado;
        }



        private string ObtenerCodigo(Guid idLookup, string EntityLogicalName, string campofiltro, string campoRecuperar)
        {
            string resultado = string.Empty;
            QueryExpression query = new QueryExpression(EntityLogicalName)
            {
                NoLock = true,
                EntityName = EntityLogicalName,
                ColumnSet = new ColumnSet(campoRecuperar),
                Criteria =
                {
                    Conditions = {
                         new ConditionExpression(campofiltro, ConditionOperator.Equal, idLookup),
                    },
                },
            };

            EntityCollection ec = _cnx.service.RetrieveMultiple(query);

            if (ec.Entities.Any())
                resultado = ec.Entities[0].GetAttributeValue<string>(campoRecuperar);

            return resultado;
        }
    }
}

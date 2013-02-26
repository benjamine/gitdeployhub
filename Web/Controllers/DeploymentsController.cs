using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using GitDeployHub.Web.Engine;
using Newtonsoft.Json;

namespace GitDeployHub.Web.Controllers
{
    public class DeploymentsController : ApiController
    {
        public class BodyData
        {
            public string payload { get; set; }
        }
        // PUT api/deployments/instance2
        public string Put(string id)
        {
            return Post(id);
        }

        // POST api/deployments/instance2
        public string Post(string id)
        {
            var httpRequest = HttpContext.Current.Request;
            var parameters = Request.GetQueryNameValuePairs().ToDictionary(kv => kv.Key, kv => kv.Value);
            parameters["Address"] = httpRequest.UserHostAddress;
            parameters["UserAgent"] = httpRequest.UserAgent;

            var deployment = Hub.Instance.CreateDeployment(id, parameters);
            if (!deployment.IsAllowed(HttpContext.Current))
            {
                throw new HttpException(403, "Not Allowed");
            }
            deployment.ExecuteAsync();
            return "OK";
        }
    }
}

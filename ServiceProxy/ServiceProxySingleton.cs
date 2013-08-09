using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using CommonClasses;
using CommonClasses.Helpers;
using CommonClasses.MethodResults;

namespace ServiceProxy
{
    public partial class ServiceProxySingleton
    {
        #region Properties and Variables

        public int CompanyId
        {
            get
            {
                if (HttpContext.Current == null) return 1; // for console test application
                return (int)(HttpContext.Current.Session[Constants.SESSION_INSTANCE_ID]);
            }
        }

        public int UserId
        {
            get
            {
                if (HttpContext.Current == null) return 1; // for console test application
                return (int)(HttpContext.Current.Session["UserId"]);
            }
        }

        public string AuthToken
        {
            get
            {
                if (HttpContext.Current == null) return string.Empty;
                return (string) (HttpContext.Current.Session[Constants.SESSION_AUTH_INFO]);
            }
        }

        #endregion

        #region Singleton Methods

        private ServiceProxySingleton()
        {
            System.Net.ServicePointManager.Expect100Continue = false;
        }

        private static ServiceProxySingleton _instance;

        public static ServiceProxySingleton Instance
        {
            get
            {
                return _instance ?? (_instance = new ServiceProxySingleton());
            }
        }

        #endregion

        private string GetFullReqUrl(string operation, string parameters, bool sendToken = true)
        {
            var result = AppConfiguration.RestServiceUrl + operation;
                         //+ (sendToken ? "/" + (AuthToken ?? "_") : string.Empty);
            if (string.IsNullOrEmpty(parameters)) return result;
            if (!parameters.StartsWith("?")) result = result + "/";
            return result + parameters;
        }

        #region Service Requests Methods

        public T SendGetRequest<T>(string operation, string parameters = null, bool sendToken = true)
        {
            string fullReq = GetFullReqUrl(operation, parameters, sendToken);
            var request = WebRequest.Create(fullReq) as HttpWebRequest;
            if (request == null) return default(T);

            request.Method = "GET";
            return SendRequest<T>(request);
        }

        public TReturn SendPostRequest<TReturn, TParam>(string operation, TParam param, bool sendToken = true)
        {
            string fullReq = GetFullReqUrl(operation, null, sendToken);
            var request = WebRequest.Create(fullReq) as HttpWebRequest;
            if (request == null) return default(TReturn);

            request.Method = "POST";
            request.ContentType = "application/json";
            string json = JsonHelper.JsonSerializer(param);
            byte[] data = Encoding.UTF8.GetBytes(json);
            Stream stream = request.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();
            return SendRequest<TReturn>(request);
        }

        public T SendRequest<T>(HttpWebRequest request)
        {
            request.KeepAlive = false;
            request.Headers.Add("authToken", AuthToken);
            var response = request.GetResponse() as HttpWebResponse;
            Encoding encoding = Encoding.UTF8;
            var stream = new StreamReader(response.GetResponseStream(), encoding);
            string result = stream.ReadToEnd();
            stream.Close();
            response.Close();

            return JsonHelper.JsonDeserialize<T>(result);
        }

        public BaseResult SendDeleteRequest(string operation, int id)
        {
            try
            {
                return SendDeleteRequest<BaseResult>(operation, id);
            }
            catch (Exception ex)
            {
                return new BaseResult {ErrorMessage = ex.Message};
            }
        }

        public T SendDeleteRequest<T>(string operation, int id)
        {
            string fullReq = GetFullReqUrl(operation, null) + "?id=" + id.ToString();
            var request = WebRequest.Create(fullReq) as HttpWebRequest;
            if (request == null) throw new Exception("AasService request is not valid");

            request.Method = "DELETE";
            return SendRequest<T>(request);
        }

        #endregion
    }
}

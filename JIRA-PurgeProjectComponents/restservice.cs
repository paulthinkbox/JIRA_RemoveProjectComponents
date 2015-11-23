using System;
using System.Text;
using System.Xml;
using System.Net;
using System.Net.Http;
using System.Web;
using System.IO;
using Newtonsoft.Json;

namespace JIRA_PurgeProjectComponents
{

    public class RestService
    {

        public void MakeXmlRequest<T>(string uri, Action<XmlDocument> successAction, Action<Exception> errorAction)
        {
            XmlDocument XMLResponse = new XmlDocument();

            HttpWebRequest request = PrepGetRequest();
            string doc = "";
            MakeRequest(request, response => doc = response,
                                (error) =>
                                {
                                    if (errorAction != null)
                                    {
                                        errorAction(error);
                                    }
                                }
                       );
            XMLResponse.LoadXml(doc);
            successAction(XMLResponse);
        }

        public void MakeJsonRequest<T>(string uri, Action<T> successAction, Action<Exception> errorAction)
        {
            HttpWebRequest request = PrepGetRequest();

            MakeRequest(
               request,
               (response) =>
               {
                   if (successAction != null)
                   {
                       T toReturn;
                       try
                       {
                           toReturn = Deserialize<T>(response);
                       }
                       catch (Exception ex)
                       {
                           errorAction(ex);
                           return;
                       }
                      successAction(toReturn);
                   }
               },
               (error) =>
               {
                   if (errorAction != null)
                   {
                       errorAction(error);
                   }
               }
            );
        }

        private HttpWebRequest PrepGetRequest()
        {
            StringBuilder url = new StringBuilder();
            url.Append(Program._URI);
            url.Append(String.Format("/rest/api/2/project/{0}/components", Program._Project));

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url.ToString());
            string authInfo = Program._UserName + ":" + Program._Password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Timeout = 30000;
            request.KeepAlive = false;

            request.Headers["Authorization"] = "Basic " + authInfo;
            request.ContentType = "application/json";
            request.Method = "GET";

            return request;
        }

        public void MakeDeleteRequest(string uri, string idToDelete)
        {
            XmlDocument XMLResponse = new XmlDocument();

            string authInfo = Program._UserName + ":" + Program._Password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(uri);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authInfo);
                var response = client.DeleteAsync(String.Format("/rest/api/2/component/{0}", idToDelete)).Result;
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine(String.Format("Successfully Removed: {0}", idToDelete));
                }
                else
                    Console.WriteLine(String.Format("Failed to Remove: {0}", idToDelete));
            }
        }

        private void MakeRequest(HttpWebRequest request, Action<string> successAction, Action<Exception> errorAction)
        {
            try
            {
                using (var webResponse = (HttpWebResponse)request.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        var objText = reader.ReadToEnd();
                        successAction(objText);
                    }
                }
            }
            catch (HttpException ex)
            {
                errorAction(ex);
            }
        }
        private T Deserialize<T>(string responseBody)
        {
            try
            {
                var toReturns = JsonConvert.DeserializeObject<T>(responseBody);
                return toReturns;
            }
            catch (Exception ex)
            {
                string errores;
                errores = ex.Message;
            }
            var toReturn = JsonConvert.DeserializeObject<T>(responseBody);
            return toReturn;
        }
    }
}


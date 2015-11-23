using System;
using Newtonsoft.Json.Linq;

namespace JIRA_PurgeProjectComponents
{
    class Program
    {
        public static string _URI;
        public static string _Project;
        public static string _UserName;
        public static string _Password;

        static void Main(string[] args)
        {
            // enable this if you need to break the command-line and attach debugger
            //Console.ReadLine();

            if (args.Length != 4)
            {
                System.Console.WriteLine("Usage: JIRA-ClearAllComponents <UserName> <Password> <JIRA_REST_BASE_URI> <PROJECT>");
                return;
            }
            else
            {
                try
                {
                    _UserName = args[0].ToString();
                    _Password = args[1].ToString();
                    _URI = args[2].ToString();
                    _Project = args[3].ToString();
                }
                catch
                {
                    System.Console.WriteLine("Check the Parameters");
                }
            }

            Action<JObject[]> doc = delegate (JObject[] d) { ProcessXML(d); };
            Action<Exception> err = delegate (Exception e) { DealWithError(e); };

            RestService r = new RestService();
            r.MakeJsonRequest<JObject[]>(_URI, doc, err);
        }

        private static void ProcessXML(JObject[] message)
        {
            RestService r = new RestService();

            foreach (JObject j in message)
            {
                r.MakeDeleteRequest(_URI, (string)j["id"]);
            }
        }

        private static void DealWithError(Exception x)
        {
            Console.WriteLine(x.Message);
        }

    }
}

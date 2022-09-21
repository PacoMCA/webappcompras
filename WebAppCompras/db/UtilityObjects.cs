using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Web;
using System.IO;
using System.Configuration;
using System.Security;

namespace WebAppCompras.db
{
    public class UtilityObjects
    {
        public static string tokenString = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ";

        // Misc. para token
        private static bool TryRetrieveToken(HttpRequestMessage request, out string token)
        {
            token = null;
            IEnumerable<string> authzHeaders;
            if (!request.Headers.TryGetValues("Authorization", out authzHeaders) || authzHeaders.Count() > 1)  // Verifica que se haya enviado un token en la petición (Bearer)
            {
                return false;
            }

            var bearerToken = authzHeaders.ElementAt(0);
            token = bearerToken.StartsWith("Bearer ") ? bearerToken.Substring(7) : bearerToken;
            return true;
        }


        private static void VerifyDir(String path)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                if (!dir.Exists)
                {
                    dir.Create();
                }
            }
            catch { }
        }

        public static void Log(String info)
        {
            String path = ConfigurationManager.AppSettings["logPath"] + "\\";
            VerifyDir(path);
            String appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            string fileName = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("D2") + DateTime.Now.Day.ToString("D2") + "_" + appName + ".log";
            try
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter(path + fileName, true);
                file.WriteLine(DateTime.Now.ToString() + ": " + (info.Length > 2500 ? info.Substring(0, 2500) : info));
                file.Close();
            }
            catch (Exception e)
            {

            }
        }

        public static string XmlEscape(string unescaped)
        {
            return SecurityElement.Escape(unescaped);
        }
    }
}
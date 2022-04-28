using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Text;

namespace BitcoinLibrary
{
    public class KunaRepository
    {
        public static string CallRestMethod(string url)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
            webrequest.Method = "GET";
            webrequest.ContentType = "application/x-www-form-urlencoded";
            HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
            Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader responseStream = new StreamReader(webresponse.GetResponseStream(),
            enc);
            string result = string.Empty;
            result = responseStream.ReadToEnd();
            webresponse.Close();
            return result;
        }
        private float GetKunaValue()
        {
            float kuna;
            string url = "https://api.hnb.hr/tecajn/v2?valuta=USD";
            string json = CallRestMethod(url);
            JArray jsons = JArray.Parse(json);
            kuna = float.Parse((string)jsons.SelectToken("$..srednji_tecaj"));
            return kuna;
        }
        public float IzracunajVrijednostHRK(float usd)
        {
            float vHRK = GetKunaValue();
            float Izracun = vHRK * usd;
            return Izracun;
        }
    }
}

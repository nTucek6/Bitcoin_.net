using BitcoinLibrary;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;


namespace WindowsServiceBitcoinToDatabase
{
    public partial class Service1 : ServiceBase
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString; //Konekcija na bazu
        public Service1()
        {
            InitializeComponent();
        }
        protected override void OnStart(string[] args)
        {
            ScheduleService(); 
        }
        protected override void OnStop()
        {
        }
        public  void ScheduleService()
        {
            // Objekt klase Timer  
            Timer Schedular = new Timer(new TimerCallback(SchedularCallback));
            // Postavljanje vremena 'po defaultu' 
            DateTime scheduledTime = DateTime.MinValue;
            int intervalMinutes = 1;
            // Postavljanje vremena zapisa u trenutno vrijeme + 1 minuta 
            scheduledTime = DateTime.Now.AddMinutes(intervalMinutes);
            if (DateTime.Now > scheduledTime)
            {
                scheduledTime = scheduledTime.AddMinutes(intervalMinutes);
            }
            // Vremenski interval 
            TimeSpan timeSpan = scheduledTime.Subtract(DateTime.Now);
            AddToDatabase(GetBitcoinValue());
            //Razlika između trenutnog vremena i planiranog vremena 
            int dueTime = Convert.ToInt32(timeSpan.TotalMilliseconds);
            // Promjena vremena izvršavanja metode povratnog poziva. 
            Schedular.Change(dueTime, Timeout.Infinite);
        }
        private void SchedularCallback(object e)
        {
            ScheduleService();
        }
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
        private List<Bitcoin> GetBitcoinValue()
        {
            List<Bitcoin> BitcoinREST = new List<Bitcoin>();
            Bitcoin USDValue = new Bitcoin();
            Bitcoin EUROValue = new Bitcoin();
            Bitcoin GBPValue = new Bitcoin();
            /*-------------------------------------------------------------------------------------------------------------------*/
            string url = "https://api.coindesk.com/v1/bpi/currentprice.json/";
            string json = CallRestMethod(url);
            JObject jsons = JObject.Parse(json);
            /*-------------------------------------------------------------------------------------------------------------------*/
            USDValue.Datum = DateTime.Now;
            USDValue.Valuta = (string)jsons.SelectToken("$.bpi.USD.code");
            USDValue.Opis = (string)jsons.SelectToken("$.bpi.USD.description");
            USDValue.Vrijednost = (decimal)jsons.SelectToken("$.bpi.USD.rate_float");
            /*-------------------------------------------------------------------------------------------------------------------*/
            EUROValue.Datum = DateTime.Now;
            EUROValue.Valuta = (string)jsons.SelectToken("$.bpi.EUR.code");
            EUROValue.Opis = (string)jsons.SelectToken("$.bpi.EUR.description");
            EUROValue.Vrijednost = (decimal)jsons.SelectToken("$.bpi.EUR.rate_float");
            /*-------------------------------------------------------------------------------------------------------------------*/
            GBPValue.Datum = DateTime.Now;
            GBPValue.Valuta = (string)jsons.SelectToken("$.bpi.GBP.code");
            GBPValue.Opis = (string)jsons.SelectToken("$.bpi.GBP.description");
            GBPValue.Vrijednost = (decimal)jsons.SelectToken("$.bpi.GBP.rate_float");
            /*-------------------------------------------------------------------------------------------------------------------*/
            BitcoinREST.Add(USDValue);
            BitcoinREST.Add(EUROValue);
            BitcoinREST.Add(GBPValue);
            /*-------------------------------------------------------------------------------------------------------------------*/
            return BitcoinREST;
        }
        private void AddToDatabase(List<Bitcoin> bitcoin)
        {
            foreach(Bitcoin b in bitcoin)
            {
                using (DbConnection oConnection = new SqlConnection(connectionString))
                using (DbCommand oCommand = oConnection.CreateCommand())
                {
                    oCommand.CommandText = "INSERT INTO Tucek_Bitcoin(Vrijednost,Datum,Valuta,Opis) Values(('" + b.Vrijednost.ToString() + "'), ('" + b.Datum.ToString("yyyy-MM-dd HH:mm:ss") + "'),'" + b.Valuta + "','" + b.Opis + "');";
                    oConnection.Open();
                    using (DbDataReader oReader = oCommand.ExecuteReader())
                    {
                    }
                }
            }
        }
    }
}

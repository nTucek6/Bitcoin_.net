using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;

namespace BitcoinLibrary
{
    public class BitcoinRepository
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
        KunaRepository getKuna =new KunaRepository();
        public List<Bitcoin> GetBitcoin()
        {
            //Ucitavanje zadnjeg upisa iz baze, trenutno stanje bitcoina
            List<Bitcoin> bitcoin = new List<Bitcoin>(); 
            using (DbConnection connection = new SqlConnection(connectionString))
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT TOP 3 * FROM Tucek_Bitcoin Order By Datum DESC"; //Tri najnovija upisa po datumu
                connection.Open();
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bitcoin.Add(new Bitcoin()
                        {
                            Vrijednost = Convert.ToDecimal(reader["Vrijednost"].ToString().Replace(".",",")),
                            Datum = (DateTime)reader["Datum"],
                            Valuta = (string)reader["Valuta"],
                            Opis = (string)reader["Opis"] 
                        });
                    }
                }
            }
            return bitcoin;
        }
        public List<BitcoinHistory> QueryHistory(DateTime DatumOd, DateTime DatumDo,string valuta)
        {
            //Metoda za prikaz povijesti na odabrani interval
            List<BitcoinHistory> DateHistory = new List<BitcoinHistory>();
           // List<BitcoinHistory> Query = new List<BitcoinHistory>();
            string Do;

            //Ako je odabrani datum do jednak današnjem datumu vrijeme se postavlja na trenutno vrijeme
            if(DatumDo.ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd"))
            {
                Do = DatumDo.ToString("yyyy-MM-dd") + DateTime.Now.ToString(" HH:mm:ss");
            }
            else
            {
                Do = DatumDo.ToString("yyyy-MM-dd") + " 23:59:59";
            }

            using (DbConnection connection = new SqlConnection(connectionString))
            using (DbCommand command = connection.CreateCommand())
            {
               // command.CommandText = "SELECT * FROM Tucek_Bitcoin Where Datum Between'" + DatumOd.ToString("yyyy-MM-dd HH:mm:ss") + "'AND'" + DatumDo.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Valuta ='" + valuta + "'  Order By Datum ASC;";
                command.CommandText = "SELECT * FROM Tucek_Bitcoin Where Datum Between'" + DatumOd.ToString("yyyy-MM-dd HH:mm:ss") + "'AND'" +Do + "' AND Valuta ='" + valuta + "'  Order By Datum ASC;";
                //jost treba testirat ako upit radi
                connection.Open();
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateHistory.Add(new BitcoinHistory()
                        {
                            Vrijednost = Convert.ToDecimal(reader["Vrijednost"]),
                            Datum = (DateTime)reader["Datum"]
                        });
                    }
                }
            }
            return DateHistory;
        }
        private bool NotEmptyList()
        {
            //Metoda provjerava ako ima danas kakvih upisa u bazu !vazno je za neke druge metode
            int c = 0;
            using (DbConnection connection = new SqlConnection(connectionString))
            using (DbCommand command = connection.CreateCommand())
            {
                // command.CommandText = "SELECT Top (1) [Vrijednost] FROM Tucek_Bitcoin Where Datum Between'" + DateTime.Now.Date.ToString("yyyy-MM-dd HH:mm:ss") + "'AND'" + (DateTime.Now.Date + new TimeSpan(23, 59, 59)).ToString("yyyy-MM-dd HH:mm:ss") + "'AND Valuta ='" + valuta + "'  Order By Datum ASC; ";
                command.CommandText = "SELECT * FROM Tucek_Bitcoin Where Datum Between'" + DateTime.Now.Date.ToString("yyyy-MM-dd HH:mm:ss") + "'AND'" + (DateTime.Now.Date + new TimeSpan(23, 59, 59)).ToString("yyyy-MM-dd HH:mm:ss") + "';";
                connection.Open();
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        c++;
                    }
                }
            }
            if(c == 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
        public int GetListNumber(string valuta) //Koristi se za racunanje postotka promjene cijene ako je samo jedan unos u bazi za danasnji dan za neku valutu da se onda nista ne racuna
        {

            List<BitcoinHistory> bitcoin = new List<BitcoinHistory>();
          
                using (DbConnection connection = new SqlConnection(connectionString))
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Top 2 * FROM Tucek_Bitcoin Where Datum Between'" + DateTime.Now.Date.ToString("yyyy-MM-dd HH:mm:ss") + "'AND'" + (DateTime.Now.Date + new TimeSpan(23, 59, 59)).ToString("yyyy-MM-dd HH:mm:ss") + "'AND Valuta = '" + valuta + "' Order By Datum DESC;";
                    connection.Open();
                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bitcoin.Add(new BitcoinHistory()
                            {
                                Vrijednost = Convert.ToDecimal(reader["Vrijednost"]),
                                Datum = (DateTime)reader["Datum"]
                            });


                        }
                    }
                }
            return bitcoin.Count;
            

        }
        public decimal PriceLow(string valuta)
        {
            //Metoda vraca najmanju vrijednost Bitcoina u tome danu
            decimal price = 0;
             
            if (NotEmptyList() == true)
            {
                bool HRKValue = false;
                if (valuta == "HRK")
                {
                    valuta = "USD";
                    HRKValue = true;
                }
                using (DbConnection connection = new SqlConnection(connectionString))
                using (DbCommand command = connection.CreateCommand())
                {
                    // command.CommandText = "SELECT Top (1) [Vrijednost] FROM Tucek_Bitcoin Where Datum Between'" + DateTime.Now.Date.ToString("yyyy-MM-dd HH:mm:ss") + "'AND'" + (DateTime.Now.Date + new TimeSpan(23, 59, 59)).ToString("yyyy-MM-dd HH:mm:ss") + "'AND Valuta ='" + valuta + "'  Order By Datum ASC; ";
                    command.CommandText = "SELECT Min(Vrijednost) AS Vrijednost FROM Tucek_Bitcoin Where Datum Between'" + DateTime.Now.Date.ToString("yyyy-MM-dd HH:mm:ss") + "'AND'" + (DateTime.Now.Date + new TimeSpan(23, 59, 59)).ToString("yyyy-MM-dd HH:mm:ss") + "'AND Valuta ='" + valuta + "';";
                    //jost treba testirat ako upit radi
                    connection.Open();
                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            price = Convert.ToDecimal(reader["Vrijednost"]);
                        }
                    }
                }
                if(HRKValue == true)
                {
                    price = (decimal)getKuna.IzracunajVrijednostHRK(Convert.ToSingle(price));
                }
            }
          
            return price;
        }
        public decimal PriceHigh(string valuta)
        {
            //Metoda vraca najveću vrijednost Bitcoina u tome danu
            decimal price = 0;
            if(NotEmptyList() == true)
            {
                bool HRKValue = false;
                if (valuta == "HRK")
                {
                    valuta = "USD";
                    HRKValue = true;
                }
                using (DbConnection connection = new SqlConnection(connectionString))
                using (DbCommand command = connection.CreateCommand())
                {
                     command.CommandText = "SELECT MAX(Vrijednost) AS Vrijednost FROM Tucek_Bitcoin Where Datum Between'" + DateTime.Now.Date.ToString("yyyy-MM-dd HH:mm:ss") + "'AND'" + (DateTime.Now.Date + new TimeSpan(23, 59, 59)).ToString("yyyy-MM-dd HH:mm:ss") + "'AND Valuta ='" + valuta + "' ; "; //Order By Datum DESC
                   // command.CommandText = "SELECT Top (1) [Vrijednost] FROM Tucek_Bitcoin Where Datum Between'" + DateTime.Now.Date.ToString("yyyy-MM-dd HH:mm:ss") + "'AND'" + (DateTime.Now.Date + new TimeSpan(23, 59, 59)).ToString("yyyy-MM-dd HH:mm:ss") + "'AND Valuta ='" + valuta + "'  Order By Datum DESC; ";                                                                                                                                                                                                                                                                          //SELECT Top (1) [Vrijednost] FROM Tucek_Bitcoin Where Datum = 2022/1/7 Order by Datum DESC
                    connection.Open();
                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            price = Convert.ToDecimal(reader["Vrijednost"]);
                        }
                    }
                }
                if (HRKValue == true)
                {
                    price = (decimal)getKuna.IzracunajVrijednostHRK(Convert.ToSingle(price));
                }
            }
        
            return price;
        }
        public decimal GetNewValue(string valuta)
        {
            //Metoda vraća najnoviju vrijednost odabrane valute
            decimal value = 0;
           
            if (NotEmptyList() == true)
            {
                bool HRKValue = false;
                if (valuta == "HRK")
                {
                    valuta = "USD";
                    HRKValue = true;
                }
                using (DbConnection connection = new SqlConnection(connectionString))
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Top 1 Vrijednost FROM Tucek_Bitcoin Where Datum Between'" + DateTime.Now.Date.ToString("yyyy-MM-dd HH:mm:ss") + "'AND'" + (DateTime.Now.Date + new TimeSpan(23, 59, 59)).ToString("yyyy-MM-dd HH:mm:ss") + "'AND Valuta = '" + valuta + "' Order By Datum DESC; ";
                    //jost treba testirat ako upit radi
                    connection.Open();
                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            value = Convert.ToDecimal(reader["Vrijednost"]);
                        }
                    }
                }
                if (HRKValue == true)
                {
                    value = (decimal)getKuna.IzracunajVrijednostHRK(Convert.ToSingle(value));
                }

            }

        
            return value;

        }
        public decimal GetValueBefore(string valuta)
        {
            //Metoda vraća vrijednost bitcoina za korak manje od nove
            decimal value = 0;
            List<BitcoinHistory> bitcoin = new List<BitcoinHistory>();
            if (NotEmptyList() == true)
            {
                bool HRKValue = false;
                if (valuta == "HRK")
                {
                    valuta = "USD";
                    HRKValue = true;
                }
                using (DbConnection connection = new SqlConnection(connectionString))
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Top 2 * FROM Tucek_Bitcoin Where Datum Between'" + DateTime.Now.Date.ToString("yyyy-MM-dd HH:mm:ss") + "'AND'" + (DateTime.Now.Date + new TimeSpan(23, 59, 59)).ToString("yyyy-MM-dd HH:mm:ss") + "'AND Valuta = '" + valuta + "' Order By Datum DESC;";
                    connection.Open();
                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bitcoin.Add(new BitcoinHistory()
                            {
                                Vrijednost = Convert.ToDecimal(reader["Vrijednost"]),
                                Datum = (DateTime)reader["Datum"]
                            });

                          
                        }
                    }
                }
               
               if(bitcoin.Count >1)
                {
                    var result = bitcoin.OrderByDescending(i => i.Datum).Skip(1).First();
                    value = result.Vrijednost;
                    if (HRKValue == true)
                    {
                        value = (decimal)getKuna.IzracunajVrijednostHRK(Convert.ToSingle(value));
                    }
                }

            }
            return value;

        }
        public float PriceChangeP(string valuta)
        {
            //Racunanje razlike  u postotcima između nove i vrijednosti prije
            //Pracenje mijenjanja cijene u realtime
            float percentage;
            if (GetListNumber("USD") > 1)//(PriceLow("USD") != 0) Ako ima više od jednog upisa u bazi
            {
                if (valuta == "HRK")
                {
                    float low = getKuna.IzracunajVrijednostHRK(Convert.ToSingle(GetValueBefore("USD")));
                    float high = getKuna.IzracunajVrijednostHRK(Convert.ToSingle(GetNewValue("USD")));

                    percentage = ((high - low) / low) * 100;
                }
                else
                {
                   percentage = Convert.ToSingle(((GetNewValue(valuta) - GetValueBefore(valuta)) / GetValueBefore(valuta)) * 100);  
                }
            }
            else
            {
                percentage = 0;
            }


            return percentage;
        }


    }
}

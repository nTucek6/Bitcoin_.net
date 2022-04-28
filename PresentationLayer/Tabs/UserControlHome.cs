using BitcoinLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace PresentationLayer.Tabs
{
    public partial class UserControlHome : UserControl
    {
        public UserControlHome()
        {
            InitializeComponent();
        }
        BitcoinRepository getBitcoin = new BitcoinRepository();
        KunaRepository getKuna = new KunaRepository();
        static int PromjenaConverter = 0; //0 - BTC - Valuta ----- 1 - Valuta - BTC

        private List<Bitcoin> GetBitcoin()//Metoda vraća listu vrijednosti bitcoina
        {
            List<Bitcoin> Bitcoin = new List<Bitcoin>(getBitcoin.GetBitcoin());
            return Bitcoin;
        }
        private void UserControlHome_Load(object sender, EventArgs e)
        {
            comboBoxValuta.Items.Add("HRK");
            foreach (Bitcoin b in GetBitcoin())
            {
                comboBoxValuta.Items.Add(b.Valuta);
            }
            comboBoxValuta.SelectedIndex = 0;
            comboBoxValuta.DropDownStyle = ComboBoxStyle.DropDownList;
            dateTimePickerFrom.Value = DateTime.Today;
            dateTimePickerTo.Value = DateTime.Now;
            dateTimePickerTo.MaxDate = DateTime.Now.Date+new TimeSpan(23,59,59); //Daje vrijeme do kraja dana kao max Time
            dateTimePickerFrom.MaxDate = DateTime.Now;
            ScheduleService(); 
        }
        private void comboBoxValuta_SelectedIndexChanged(object sender, EventArgs e)
        {
            //kada se promjeni vrijednost comboboxa poziva se funkcija ispod
            UpdateComboBox();
            GenerirajChart(dateTimePickerFrom.Value.Date, dateTimePickerTo.Value, comboBoxValuta.Text);
            lblPosto.Text = getBitcoin.PriceChangeP(comboBoxValuta.Text).ToString() + " %";
            
            if (PromjenaConverter == 0)
            {
                labelValuta1.Text = "BTC";
                labelValuta2.Text = comboBoxValuta.Text;
            }
            else
            {
                labelValuta1.Text = comboBoxValuta.Text;
                labelValuta2.Text = "BTC";
            }
            
            textBoxShowC.Text = "";
        }
        public void UpdateComboBox()
        {
            // Prolazi kroz listu bitcoina i trazi valutu koja je odabrana, onda se formatira ispis valute ovisno koja je odabrana
            foreach (Bitcoin b in GetBitcoin())
            {
                if (comboBoxValuta.Text == "HRK")
                {
                    if (b.Valuta == "USD")
                    {
                        lblBitcoinValue.Text = getKuna.IzracunajVrijednostHRK((float)b.Vrijednost).ToString("C1"); //Format za ispis valute
                        lblPriceLow.Text = getBitcoin.PriceLow(comboBoxValuta.Text).ToString("C1");
                        lblPriceHigh.Text = getBitcoin.PriceHigh(comboBoxValuta.Text).ToString("C1");
                    }
                       // lblBitcoinValue.Text = b.Vrijednost.ToString("C4", System.Globalization.CultureInfo.GetCultureInfo("en-au")); // za prikaz dolara
                    }
                else if (b.Valuta == comboBoxValuta.Text)
                {
                    //lblBitcoinValue.Text = GetBitcoin().Vrijednost.ToString("N4");
                    //lblBitcoinValue.Text = b.Vrijednost.ToString("N4");
                   
                     if (comboBoxValuta.Text == "USD")
                    {
                        lblBitcoinValue.Text = b.Vrijednost.ToString("C4", System.Globalization.CultureInfo.GetCultureInfo("en-au"));
                        lblPriceLow.Text = getBitcoin.PriceLow(comboBoxValuta.Text).ToString("C4", System.Globalization.CultureInfo.GetCultureInfo("en-au"));
                        lblPriceHigh.Text = getBitcoin.PriceHigh(comboBoxValuta.Text).ToString("C4", System.Globalization.CultureInfo.GetCultureInfo("en-au"));// za prikaz dolara
                    }
                    else if (comboBoxValuta.Text == "EUR")
                    {
                        lblBitcoinValue.Text = b.Vrijednost.ToString("C4", System.Globalization.CultureInfo.GetCultureInfo("en-ie")); // za prikaz eura
                        lblPriceLow.Text = getBitcoin.PriceLow(comboBoxValuta.Text).ToString("C4", System.Globalization.CultureInfo.GetCultureInfo("en-ie"));
                        lblPriceHigh.Text = getBitcoin.PriceHigh(comboBoxValuta.Text).ToString("C4", System.Globalization.CultureInfo.GetCultureInfo("en-ie"));
                    }
                    else if (comboBoxValuta.Text == "GBP")
                    {
                        lblBitcoinValue.Text = b.Vrijednost.ToString("C4", System.Globalization.CultureInfo.GetCultureInfo("en-gb")); //za prikaz pounds
                        lblPriceLow.Text = getBitcoin.PriceLow(comboBoxValuta.Text).ToString("C4", System.Globalization.CultureInfo.GetCultureInfo("en-gb"));
                        lblPriceHigh.Text = getBitcoin.PriceHigh(comboBoxValuta.Text).ToString("C4", System.Globalization.CultureInfo.GetCultureInfo("en-gb"));
                    }
                }
            }
        }
        private void GenerirajChart(DateTime DatumMin, DateTime DatumMax,string valuta) //Generiranje dijagrama
        {
            List<BitcoinHistory> BitcoinList;
            //Treba sve dobro formirati jos
            if (valuta == "HRK")
            {
                BitcoinList = new List<BitcoinHistory>(getBitcoin.QueryHistory(DatumMin, DatumMax, "USD"));
                for (int i = 0; i < BitcoinList.Count; i++)
                {
                    BitcoinList[i].Vrijednost = Convert.ToDecimal(getKuna.IzracunajVrijednostHRK(Convert.ToSingle(BitcoinList[i].Vrijednost)));
                }
            }
            else
            {
                BitcoinList = new List<BitcoinHistory>(getBitcoin.QueryHistory(DatumMin, DatumMax, valuta));
            }
            if(BitcoinList.Count > 1)
            {
                double minVrijednost = BitcoinList.Min(b => Convert.ToDouble(b.Vrijednost)); //dobivanje najmanjeg iznosa za graf, y os
                double MaxVrijednost = BitcoinList.Max(b => Convert.ToDouble(b.Vrijednost)); //dobivanje najvećeg iznosa za graf, y os

                DateTime minDatum = BitcoinList.Min(b => b.Datum);
                DateTime maxDatum = BitcoinList.Max(b => b.Datum);

                //MessageBox.Show(BitcoinList.Count.ToString());
                var objChart = chartBitcoin.ChartAreas[0];

                objChart.AxisX.IntervalType = DateTimeIntervalType.Number;
                //x os u satima
                objChart.AxisX.Minimum = 1;
                objChart.AxisX.Maximum = BitcoinList.Count;
                //y os za vrijednost
                objChart.AxisY.IntervalType = DateTimeIntervalType.Number;
                objChart.AxisY.Minimum = minVrijednost;//-500;
                objChart.AxisY.Maximum = MaxVrijednost;//+500;
                chartBitcoin.Series.Clear();

                chartBitcoin.Series.Add("A");

                chartBitcoin.Series["A"].Color = Color.FromArgb(0, 0, 0);
                chartBitcoin.Series["A"].Legend = "Legend1";
                chartBitcoin.Series["A"].ChartArea = "ChartArea1";
                chartBitcoin.Series["A"].ChartType = SeriesChartType.Spline;

                if(minDatum.Date == maxDatum.Date)
                {
                    //Ako su datumi jednaki dan
                    for (int i = 0; i < BitcoinList.Count; i++)
                    {
                        chartBitcoin.Series["A"].Points.AddXY(BitcoinList[i].Datum.Hour+":"+BitcoinList[i].Datum.Minute, BitcoinList[i].Vrijednost);
                    }
                }
                else
                {
                    int lastDayOfMonth = DateTime.DaysInMonth(minDatum.Year, minDatum.Month);
                    if (maxDatum.Month == minDatum.Month && maxDatum.Year == minDatum.Year && (maxDatum.Day - minDatum.Day) == 1)
                    {
                        //Ako je razlika datuma samo jedan dan 
                        int presjek = 0;
                        for (int i = 0; i < BitcoinList.Count; i++)
                        {
                            if(BitcoinList[i].Datum.Date == maxDatum.Date)
                            {
                                presjek += i;
                                
                                break;
                            }
                        }
                       
                        for (int i = 0; i < BitcoinList.Count; i++)
                        {
                            //Ne radi dobro
                            if (i == 0)
                            {
                                chartBitcoin.Series["A"].Points.AddXY(BitcoinList[i].Datum.Day + "." + BitcoinList[i].Datum.Month + "." + BitcoinList[i].Datum.Year + " "+BitcoinList[i].Datum.Hour + ":" + BitcoinList[i].Datum.Minute, BitcoinList[i].Vrijednost);
                            }
                            else if(i == presjek)
                            {
                                chartBitcoin.Series["A"].Points.AddXY(BitcoinList[i].Datum.Day + "." + BitcoinList[i].Datum.Month + "." + BitcoinList[i].Datum.Year + " " + BitcoinList[i].Datum.Hour + ":" + BitcoinList[i].Datum.Minute, BitcoinList[i].Vrijednost);
                            }
                            else 
                            {
                                chartBitcoin.Series["A"].Points.AddXY(BitcoinList[i].Datum.Hour + ":" + BitcoinList[i].Datum.Minute, BitcoinList[i].Vrijednost);
                            }
                        }
                    }
                    else if((maxDatum.Month - minDatum.Month) == 1 && lastDayOfMonth == minDatum.Day && maxDatum.Day == 1) // provjera ako je zadnji dan mjeseca i prvi dan novog mjeseca
                    {
                        //Ako je razlika datuma samo jedan dan 
                        int presjek = 0;
                        for (int i = 0; i < BitcoinList.Count; i++)
                        {
                            if (BitcoinList[i].Datum.Date == maxDatum.Date)
                            {
                                presjek += i;

                                break;
                            }
                        }

                        for (int i = 0; i < BitcoinList.Count; i++)
                        {
                            //Ne radi dobro
                            if (i == 0)
                            {
                                chartBitcoin.Series["A"].Points.AddXY(BitcoinList[i].Datum.Day + "." + BitcoinList[i].Datum.Month + "." + BitcoinList[i].Datum.Year + " " + BitcoinList[i].Datum.Hour + ":" + BitcoinList[i].Datum.Minute, BitcoinList[i].Vrijednost);
                            }
                            else if (i == presjek)
                            {
                                chartBitcoin.Series["A"].Points.AddXY(BitcoinList[i].Datum.Day + "." + BitcoinList[i].Datum.Month + "." + BitcoinList[i].Datum.Year + " " + BitcoinList[i].Datum.Hour + ":" + BitcoinList[i].Datum.Minute, BitcoinList[i].Vrijednost);
                            }
                            else
                            {
                                chartBitcoin.Series["A"].Points.AddXY(BitcoinList[i].Datum.Hour + ":" + BitcoinList[i].Datum.Minute, BitcoinList[i].Vrijednost);
                            }
                        }

                    }
                    else
                    {
                        //Ako je razlika dana više od jedan
                        for (int i = 0; i < BitcoinList.Count; i++)
                        {
                            //MessageBox.Show(BitcoinList[i].Datum.ToString());
                            chartBitcoin.Series["A"].Points.AddXY(BitcoinList[i].Datum.Day + "." + BitcoinList[i].Datum.Month + "." + BitcoinList[i].Datum.Year, BitcoinList[i].Vrijednost);
                        }
                    }
                }
            }
            else
            {
                //Ako nema zapisa za odabrani datum dijagram se čisti
                chartBitcoin.Series.Clear();
            }
        }
        public void ScheduleService()
        {
            //Timer koji svakih 30 sekundi updatea vrijednost bitcoina
            // Objekt klase Timer  
            System.Threading.Timer Schedular = new System.Threading.Timer(new TimerCallback(SchedularCallback));
            // Postavljanje vremena 'po defaultu' 
            DateTime scheduledTime = DateTime.MinValue;
            float intervalMinutes = (float)0.30;
            // Postavljanje vremena zapisa u trenutno vrijeme + 1 minuta 
            scheduledTime = DateTime.Now.AddMinutes(intervalMinutes);
            if (DateTime.Now > scheduledTime)
            {
                scheduledTime = scheduledTime.AddMinutes(intervalMinutes);
            }
            // Vremenski interval 
            TimeSpan timeSpan = scheduledTime.Subtract(DateTime.Now);
            GetBitcoin();
            Invoke((MethodInvoker)(() => UpdateComboBox()));
            Invoke((MethodInvoker)(() => lblPosto.Text = getBitcoin.PriceChangeP(comboBoxValuta.Text).ToString() + " %"));
            if (dateTimePickerTo.Value.Date == DateTime.Now.Date) //Ako je datum do današnji onda će se s timer-om dodati vrijdnost bitcoina
            {
                //Promjena na chartu
                Invoke((MethodInvoker)(() => dateTimePickerFrom.Value = dateTimePickerFrom.Value.Date));
                Invoke((MethodInvoker)(() => dateTimePickerTo.Value = DateTime.Now));
            }
            
            //Razlika između trenutnog vremena i planiranog vremena 
            int dueTime = Convert.ToInt32(timeSpan.TotalMilliseconds);
            // Promjena vremena izvršavanja metode povratnog poziva. 
            Schedular.Change(dueTime, Timeout.Infinite);
        }
        private void SchedularCallback(object e)
        {
            ScheduleService();
        }
        private void dateTimePickerFrom_ValueChanged(object sender, EventArgs e)
        {
            if(dateTimePickerFrom.Value.Date <= dateTimePickerTo.Value.Date)
            {
                GenerirajChart(dateTimePickerFrom.Value, dateTimePickerTo.Value, comboBoxValuta.Text);
            }
            else
            {
                MessageBox.Show("Datum početka ne može biti veći od datuma kraja!\nPonovite unos");
            }
        }
        private void dateTimePickerTo_ValueChanged(object sender, EventArgs e)
        {
            if (dateTimePickerFrom.Value.Date <= dateTimePickerTo.Value.Date)
            {
                GenerirajChart(dateTimePickerFrom.Value, dateTimePickerTo.Value, comboBoxValuta.Text);
            }
            else
            {
                MessageBox.Show("Datum početka ne može biti veći od datuma kraja!\nPonovite unos");
            }
        }
        private void btnConvert_Click(object sender, EventArgs e)
        {
            //Button koji pretvara ili iz valute u bitcoin ili obrnuto
            decimal vrijednost = 0;
            List<Bitcoin> btc;
           
                if (PromjenaConverter == 0)
                {
                    if (comboBoxValuta.Text == "HRK")
                    {
                        btc = new List<Bitcoin>(GetBitcoin().Where(b => b.Valuta == "USD"));
                        vrijednost = Convert.ToDecimal(numericUpDownBTCValue.Value) * Convert.ToDecimal(getKuna.IzracunajVrijednostHRK(Convert.ToSingle(btc[0].Vrijednost)));
                    }
                    else
                    {
                        btc = new List<Bitcoin>(GetBitcoin().Where(b => b.Valuta == comboBoxValuta.Text));
                        vrijednost = Convert.ToDecimal(numericUpDownBTCValue.Value) * btc[0].Vrijednost;
                    }
                    textBoxShowC.Text = vrijednost.ToString();
                }
                else
                {
                    if (comboBoxValuta.Text == "HRK")
                    {
                        btc = new List<Bitcoin>(GetBitcoin().Where(b => b.Valuta == "USD"));
                        vrijednost = Convert.ToDecimal(numericUpDownBTCValue.Value) / Convert.ToDecimal(getKuna.IzracunajVrijednostHRK(Convert.ToSingle(btc[0].Vrijednost)));
                    }
                    else
                    {
                        btc = new List<Bitcoin>(GetBitcoin().Where(b => b.Valuta == comboBoxValuta.Text));
                        vrijednost = Convert.ToDecimal(numericUpDownBTCValue.Value) / btc[0].Vrijednost;
                    }
                    textBoxShowC.Text = string.Format("{0:0.0000000000000000}", vrijednost).ToString();
                }
        }
        private void btnSwitchC_Click(object sender, EventArgs e)
        {
            //Zamjena što se pretvara (btc-usd,usd-btc)
            if(PromjenaConverter == 0)
            {
                PromjenaConverter = 1;
                labelValuta1.Text = comboBoxValuta.Text;
                labelValuta2.Text = "BTC";
            }
            else
            {
                PromjenaConverter = 0;
                labelValuta1.Text = "BTC";
                labelValuta2.Text = comboBoxValuta.Text;
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using BitcoinLibrary;
using ConsoleTables;

namespace PresentationLayer.Tabs
{
    public partial class UserControlHistory : UserControl
    {
        BindingSource _dataHistory = new BindingSource();
        BitcoinRepository _bitcoinLibrary = new BitcoinRepository();
        KunaRepository _kunaRepository = new KunaRepository();
        public UserControlHistory()
        {
            InitializeComponent();
        }
        private void UserControlHistory_Load(object sender, EventArgs e)
        {
            dateTimePickerFrom.Value = DateTime.Today;
            dateTimePickerTo.Value = DateTime.Now;         //postavljanje 
            dateTimePickerTo.MaxDate = DateTime.Now;
            dateTimePickerFrom.MaxDate = DateTime.Now;

            comboBoxHValuta.Items.Add("HRK");
            foreach (Bitcoin b in _bitcoinLibrary.GetBitcoin())
            {
                comboBoxHValuta.Items.Add(b.Valuta);
            }
            comboBoxHValuta.SelectedIndex = 0;
            comboBoxHValuta.DropDownStyle = ComboBoxStyle.DropDownList;
        }
        private void btnShow_Click(object sender, EventArgs e) //Prikaz u dataGridView za odabrani interval
        {
            //Prikaz povijesti bitoina za odabrani interval
            List<BitcoinHistory> btcH;
            if (dateTimePickerFrom.Value.Date <= dateTimePickerTo.Value.Date)
            {
                if (comboBoxHValuta.Text == "HRK")
                {
                    btcH = _bitcoinLibrary.QueryHistory(dateTimePickerFrom.Value, dateTimePickerTo.Value, "USD");
                    for (int i = 0; i < btcH.Count; i++)
                    {
                        btcH[i].Vrijednost = (decimal)_kunaRepository.IzracunajVrijednostHRK(Convert.ToSingle(btcH[i].Vrijednost));
                    }
                }
                else
                {
                    btcH = _bitcoinLibrary.QueryHistory(dateTimePickerFrom.Value, dateTimePickerTo.Value, comboBoxHValuta.Text);
                }
                if (btcH.Count != 0)
                {
                    dataGridViewHistory.DataSource = _dataHistory.DataSource = btcH;//_bitcoinLibrary.QueryHistory(dateTimePickerFrom.Value, dateTimePickerTo.Value,comboBoxHValuta.Text);
                }
                else
                {
                    MessageBox.Show("Odabrani interval nema unosa!");
                }
            }
            else
            {
                MessageBox.Show("Datum početka ne može biti veći od datuma kraja!\nPonovite unos");
            }
          
        }
        private void btnExport_Click(object sender, EventArgs e) //Spremanje datagridview prikaza u .txt file
        {
           // MessageBox.Show(dateTimePickerFrom.Value.ToString());
           // MessageBox.Show(dateTimePickerTo.Value.ToString());
            List<BitcoinHistory> bitcoin; //= new List<BitcoinHistory>(_bitcoinLibrary.QueryHistory(dateTimePickerFrom.Value, dateTimePickerTo.Value, comboBoxHValuta.Text));
            if (comboBoxHValuta.Text == "HRK")
            {
                bitcoin = _bitcoinLibrary.QueryHistory(dateTimePickerFrom.Value, dateTimePickerTo.Value, "USD");
                for (int i = 0; i < bitcoin.Count; i++)
                {
                    bitcoin[i].Vrijednost = (decimal)_kunaRepository.IzracunajVrijednostHRK(Convert.ToSingle(bitcoin[i].Vrijednost));

                }
            }
            else
            {
                bitcoin = _bitcoinLibrary.QueryHistory(dateTimePickerFrom.Value, dateTimePickerTo.Value, comboBoxHValuta.Text);
            }
            if (bitcoin.Count > 0) // Ako je lista prazna export se ne moze izvrsiti
            {
                ConsoleTable table = new ConsoleTable("Datum","Iznos");

                string save = String.Empty;

                save += "TXT ispis vrijednosti bitcoina za odbarani interval " +dateTimePickerFrom.Value.ToString("dd.MM.yyyy") + " - " + dateTimePickerTo.Value.ToString("dd.MM.yyyy") + "\n";
                save += "Bitcoin -" + comboBoxHValuta.Text + "\n\n";
                
                foreach (BitcoinHistory b in bitcoin)
                {
                    //save += b.Vrijednost + " " + comboBoxHValuta.Text + " - " + b.Datum + "\n";
                    table.AddRow(b.Datum, b.Vrijednost + " " + comboBoxHValuta.Text);
                }
                saveFileBTC.FileName = "BitcoinHistory.txt";
                if (saveFileBTC.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveFileBTC.FileName, save);
                    File.AppendAllText(saveFileBTC.FileName, table.ToStringAlternative()); 
                }
            }
            else
            {
                MessageBox.Show("Odabrani interval nema unosa");
            }
        }

    }
}

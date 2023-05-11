using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Replikacija
{
    public partial class frmReplikacija : Form
    {
        readonly PoziviProcedura poziv = new PoziviProcedura();//instanca PoziviProcedura klase gdje su procedure

        string ConNaCentralni = "Data Source=192.168.234.100;Database=A2D2udb-Frukta-2023;Persist Security Info=True;uid=sa;pwd=Fructa2014pa$$";
        public frmReplikacija()
        {
            InitializeComponent();
        }

        internal List<KeyValuePair<string, string>> getServerTableList()
        {
            var ServerTableList = new List<KeyValuePair<string, string>>();

            foreach (DataGridViewRow row in dgvReplikStatusServera.Rows)
            {

                if (!dgvReplikStatusServera.Rows[row.Index].IsNewRow)
                {               
                    ServerTableList.Add(new KeyValuePair<string, string>(row.Cells[0].Value.ToString(), row.Cells[2].Value.ToString()));//lista svih servera i tabela iz datagridview               
                }
            }
            return ServerTableList;
        }

        internal async Task LoadServTblChkDelUpdIns()//salje listu tabela i servera za proveru u stored procedure koja proverava
        {    
            List<bool> UpdatedTStamp= new List<bool>();
            await Task.Run(()=>
           {
               var ServerTableList = getServerTableList();//new List<KeyValuePair<string, string>>();

               foreach (var item in ServerTableList)//provera da li je bilo DELETE operacija na centralnom koje nisu replicirane za dati lokalni server i tabelu
               {
                   string ServerAdress = item.Key;
                   string Table = item.Value;

                   if (poziv.CheckDelete(ServerAdress, Table) == true)//poziva stored procedure za svaki par server-tabela da proveri ima li sta za brisanje
                   {

                       BeginInvoke(new Action(() =>
                       {
                           dgvStatus.Rows.Clear();
                           LoadIntoDgvStatus(ServerAdress, Table, "DELETE operacija u toku", "");
                       }));

                       string StatusDelete = "";
                       string Greska = "";
                       if (poziv.DelFromLocServ(ServerAdress, Table) == true)//brisanje upspjesno
                       {
                           poziv.SetNEzaBrisanje(ServerAdress, Table);// na centralnom u tbl_ReplikStatusServera upisuje NE u kolonu fld_ImaZaBrisanje
                          // poziv.IzbrisiIzReplikDeleted(Table);//brise listu obrisanih identa u tbl_ReplikDeleted koje triger upisuje prilikom brisanja
                           StatusDelete = "Delete uspesan";
                           Greska = "";
                       }
                       else
                       {
                           //string ErrorMessage = poziv.GetErrorMessage();
                           StatusDelete = "Delete neuspesan";// LoadIntoDgvStatus(ServerAdress, Table, "UPDATE NEUSPESAN", "Tekst greske \n" + ErrorMessage);
                           Greska = poziv.GetErrorMessage();
                       }
                       BeginInvoke(new Action(() =>
                       {
                           LoadIntoDgvStatus(ServerAdress, Table, StatusDelete, Greska);
                       }));
                     //  poziv.SetTimeStamp(ServerAdress, Table);
                   }

               }

               foreach (var item in ServerTableList)//provera da li je bilo UPDATE operacija na centralnom koje nisu replicirane za dati lokalni server i tabelu
               {
                  // string TStampType = "fld_OldUpdateTStamp";
                   string UpdateStorProc = "stp_IUpdateReplikacija";
                   string UpdateTStamp = "fld_OldUpdateTStamp";
                   string sinhro = "sinhro=1"; //redovi imaju 1 u sinhro koloni posle update op. i ostaju 1 jer je null samo prvi insert, posle moze samo update na tom redu da se radi
                   string ServerAdress = item.Key;
                   string Table = item.Value;
                  
                   if (poziv.TimestampsChanged(ServerAdress, Table, sinhro, UpdateTStamp) == true)//poziva stored procedure za svaki par server-tabela da proveri timestamp
                   {
                       BeginInvoke(new Action(() =>
                       {
                           dgvStatus.Rows.Clear();
                           LoadIntoDgvStatus(ServerAdress, Table, "UPDATE operacija u toku", "");
                       }));

                       string StatusUpdate = "";
                       string Greska = "";
                       if (poziv.UpisiInsUpdReplikaciju(ServerAdress, Table, UpdateStorProc) == true)
                       {
                           StatusUpdate = "Update uspesan";
                            Greska = "";                          
                       }
                       else
                       {
                           //string ErrorMessage = poziv.GetErrorMessage();
                           StatusUpdate = "Update neuspesan";// LoadIntoDgvStatus(ServerAdress, Table, "UPDATE NEUSPESAN", "Tekst greske \n" + ErrorMessage);
                           Greska=poziv.GetErrorMessage();
                       }
                       BeginInvoke(new Action(() =>
                       {
                           LoadIntoDgvStatus(ServerAdress, Table, StatusUpdate, Greska);
                       }));
                       poziv.SetTimeStamp(ServerAdress, Table, UpdateTStamp, sinhro);
                   }
                  
               }

               foreach (var item in ServerTableList)//Provera da li je bilo INSERT operacija koje nisu replicirane za dati lokalni server i tabelu
               {
                   //string TStampType = "fld_OldInsertTStamp";
                   string InsertStorProc = "stp_InsertReplikacija";
                   string InsertTStamp = "fld_OldInsertTStamp";
                   string sinhro = "sinhro IS NULL";//redovi imaju upisano NULL u sinhro kolonu nakon INSERT operacije
                   string ServerAdress = item.Key;
                   string Table = item.Value;
                  
                   if (poziv.TimestampsChanged(ServerAdress, Table, sinhro, InsertTStamp) == true)
                   {
                       BeginInvoke(new Action(() =>
                       {
                           dgvStatus.Rows.Clear();
                           LoadIntoDgvStatus(ServerAdress, Table, "INSERT operacija u toku", "");
                       }));//upis u dgvStatus da je zapoceta operacjia
                      
                       string StatusInsert = "";
                       string Greska = "";
                       if (poziv.UpisiInsUpdReplikaciju(ServerAdress, Table, InsertStorProc) == true)
                       {
                           StatusInsert = "INSERT uspesan";
                           Greska = "";
                       }
                       else
                       {
                           StatusInsert = "INSERT neuspesan";// LoadIntoDgvStatus(ServerAdress, Table, "UPDATE NEUSPESAN", "Tekst greske \n" + ErrorMessage);
                           Greska = poziv.GetErrorMessage();
                       }
                       BeginInvoke(new Action(() =>
                       {
                           LoadIntoDgvStatus(ServerAdress, Table, StatusInsert, Greska);
                       }));
                       poziv.SetTimeStamp(ServerAdress, Table, InsertTStamp, sinhro);
                   }
                   else
                   {                      
                       BeginInvoke(new Action(() =>
                       {
                           LoadIntoDgvStatus(ServerAdress, Table, "Nema izmjena", "");
                       }));
                   }
               }
           });
        }
  
        public void LoadIntoDgvStatus(string AdresaServera,string Tabela,string Status,string TekstGreske)//upis servera, statusa i gresaka u dgvStatus datagridview
        {
            string ImeServera ="";
            try
            {
                foreach (DataGridViewRow row in dgvReplikStatusServera.Rows)
                {
                    if(row.Cells[0].Value != null)
                    {
                        if (AdresaServera == row.Cells[0].Value.ToString() && AdresaServera != null)
                        {
                            ImeServera = row.Cells[1].Value.ToString();
                            break;
                        }
                    }
                   
                }
                dgvStatus.Rows.Add(AdresaServera, ImeServera, Tabela, Status, TekstGreske);
            }
            catch (Exception ex)
            {               
                frmReplikacija_Load(null,EventArgs.Empty);
            }
        }

        List<KeyValuePair<string, string>> ServerTableList = new List<KeyValuePair<string, string>>();
        private void LoadDgvReplikStatusServera()
        {
            DataSet ds = new DataSet();
            try
            {             
                string TableName = "tbl_ReplikStatusServera";
                SqlDataAdapter da = new SqlDataAdapter($"SELECT fld_IPAdresaServera,fld_ImeServera,fld_ImeTabele" +
                    $" FROM tbl_ReplikStatusServera " +
                    $"WHERE fld_TabelaUReplikaciji='DA' ", $"{ConNaCentralni}");             
                da.Fill(ds, $"{TableName}");
                dgvReplikStatusServera.DataSource = ds.Tables[0];
            }
            catch(Exception ex)
            {
                MessageBox.Show("Greska \n"+ex.Message);
            }
        }
     
        public bool UpisInsTabele = false;
        public bool UpisUpdTabele = false;
        private bool UpisaneTabele = false;

        public async void timer1_Tick(object sender, EventArgs e)
        {      
            timer1.Stop();
            dgvStatus.Rows.Clear();
           await  LoadServTblChkDelUpdIns();
            timer1.Start();
        }

        private void PrikaziStatusKonekcije()
        {
            
            if (UpisaneTabele == true)//ako su ucitane tabele sakriva se labela prekid konekcije
            {
                lblKonekPrekid.ForeColor = Color.Green;
                lblKonekPrekid.Text = "Konekcija na glavni server OK";

            }
            else
            {
                lblKonekPrekid.ForeColor = Color.Red;
                lblKonekPrekid.Text = "Konekcija na glavni server u prekidu";
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if(numericUpDown1.Value < 1)
            {
                numericUpDown1.Value = 1;
            }
            timer1.Stop();
            timer1.Interval = Convert.ToInt32(numericUpDown1.Value)*1000;       //promjena vremena skeniranja timera
            timer1.Start();
        }

        internal async Task ChangeReplikTblInDB(string ServerAdress,  string TableName,string YesOrNo, bool Remove)
        {
            await Task.Run(() =>
            {
                SqlDataAdapter adapter = new SqlDataAdapter();
                string connetionString = ConNaCentralni;
                SqlConnection connection = new SqlConnection(connetionString);
                string sql = $"UPDATE tbl_ReplikStatusServera SET fld_TabelaUReplikaciji='{YesOrNo}' ";

                if (Remove == true)//setuje se kolona fld_TabelaUReplikaciji na 'NE' , zato se dodaje where klauzula
                {
                    sql += $"WHERE fld_IPAdresaServera IN('{ServerAdress}') AND fld_ImeTabele IN('{TableName}')";
                }

                try
                {
                    connection.Open();
                    adapter.UpdateCommand = connection.CreateCommand();
                    adapter.UpdateCommand.CommandText = sql;
                    adapter.UpdateCommand.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            });
        }

        private async void cmbUkloniTabele_Click(object sender, EventArgs e)//brise selektovane redove iz dgvReplikStatusServera i vise nisu u replikaciji dok se ne ucita ponovo sve iz baze
        {
            try
            {
                List<string> Ukloni = new List<string>();
                
                foreach (DataGridViewRow row in this.dgvReplikStatusServera?.SelectedRows)
                {
                    if (row.Cells[0].Value!= null)
                    {
                        string ServerAdress = row.Cells[0].Value.ToString();
                        string TableName = row.Cells[2].Value.ToString();
                        bool Remove=true;
                        await ChangeReplikTblInDB(ServerAdress,TableName,"NE",Remove);
                        Ukloni.Add(row.Cells[0].Value.ToString() +" "+ row.Cells[2].Value.ToString());
                        dgvReplikStatusServera.Rows.Remove(row);                      
                    }
                }
                string Poruka = "";
                Ukloni.ForEach(row => { Poruka += row + "\n"; });
                if (Poruka != "")
                {
                    MessageBox.Show("Serveri i tabele koji su uklonjeni iz replikacije\n" + Poruka);
                }
                else 
                {
                    MessageBox.Show("Nije selektovana nijedna tabela");
                }
                
            }
            catch(System.Exception ex)
            {
                MessageBox.Show("Greska "+ex.Message);
                frmReplikacija_Load(null, EventArgs.Empty);
            }
        }

        private async void cmbVratiTabele_Click(object sender, EventArgs e)
        {
            DataSet ds = new DataSet();
            try
            {
                bool Remove = false;
                await ChangeReplikTblInDB("", "", "DA", Remove);//posto se sve setuju na DA tj idu u replikaciju ne treba ime servera i tabele
                string TableName = "tbl_ReplikStatusServera";
                SqlDataAdapter da = new SqlDataAdapter($"Select fld_IPAdresaServera,fld_ImeServera,fld_ImeTabele from tbl_ReplikStatusServera", $"{ConNaCentralni}");
                da.Fill(ds, $"{TableName}");
                dgvReplikStatusServera.DataSource = ds.Tables[0];
                MessageBox.Show("Sve tabele na svim serverima su vraćene u replikaciju");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greska \n" + ex.Message);
            }

        }

        private void frmReplikacija_Load(object sender, EventArgs e)
        {
            try
            {
                LoadDgvReplikStatusServera();
            DataGridViewColumn col1 = dgvReplikStatusServera.Columns[0];
            col1.Width = 150;
            DataGridViewColumn col2 = dgvReplikStatusServera.Columns[1];
            col2.Width = 180;
            DataGridViewColumn col3 = dgvReplikStatusServera.Columns[2];
            col3.Width = 200;
            dgvReplikStatusServera.ClearSelection();
            ServerTableList = getServerTableList();
            timer1.Stop();
           
                using (SqlConnection con = new SqlConnection(ConNaCentralni))
                {
                    con.Open();
                    lblKonektovano.ForeColor = Color.Green;
                    lblKonektovano.Text = "    Konekcija\n uspostavljena!";
                    UpisaneTabele = true;
                    PrikaziStatusKonekcije();
                    timer1.Start();
                }
            }
            catch (Exception ex)
            {
                lblKonekPrekid.Visible = false;
                lblKonektovano.ForeColor = Color.Red;
                lblKonektovano.Text=" Konekcija\n u prekidu!";
                timer1.Start();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dgvReplikStatusServera.ClearSelection();
        }
    }    
  
}

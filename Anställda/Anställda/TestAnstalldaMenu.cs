using Google.Protobuf.WellKnownTypes;
using MessagePack.Formatters;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Enum = System.Enum;

namespace Anställda
{
    public partial class Anstallda : Form
    {

        MySqlConnection conn;

        TextBox[] txtBoxesAnstallda;
        TextBox[] txtBoxesICEAnstallda;
       
        
        public Anstallda()
        {
            InitializeComponent();

            //Bygger upp MySqlConnection objekt
            string server = "localhost";
            string database = "anstallda";
            string user = "root";
            string password = "Onsala01";

            string connString = $"SERVER={server};DATABASE={database};UID={user};PASSWORD={password};";            
            conn = new MySqlConnection(connString);

           
            //Skapa en Array Ref för input fält
            txtBoxesICEAnstallda = new TextBox[] { txtICEFornamn, txtICEEfternamn, txtICETelefonnummer };         
            txtBoxesAnstallda = new TextBox[] { txtFornamn, txtEfternamn, txtAddress, txtPostnummer, txtEmail, txtTelefonnummer};

        }
     
        //METOD KNAPP INFOGA 
        public void InfogaAnstalld()
        {
            //Validering
            bool valid = true;

            foreach (TextBox txtBox in txtBoxesAnstallda)
            {
                //Trimmar test-innehållet
                txtBox.Text = txtBox.Text.Trim();

                //Kontrollera att txtBox har text
                if (txtBox.Text == "")
                {
                    //Validering har misslyckats
                    valid = false;
                    txtBox.BackColor = Color.IndianRed;
                }
                else
                {
                    txtBox.BackColor = TextBox.DefaultBackColor;
                }
            }

            //Kontrollera valid
            if (!valid)
            {
                MessageBox.Show("Felaktig validering. Kontrollera röda fält.");
                return;
            }
            
            
            //Hämta värden från textfält           
            string fornamn = txtFornamn.Text.ToString();
            string efternamn = txtEfternamn.Text.ToString();
            string address = txtAddress.Text.ToString();
            string postnummer = txtPostnummer.Text.ToString();
            string kon = Convert.ToString(cbKon.SelectedItem);
            string fodelsedatum = dTPfodelsedag.Value.ToShortDateString();
            string email = txtEmail.Text.ToString();
            string telefonnummer = txtTelefonnummer.Text.ToString();
            
            


            //Bygg upp SQL querry
            string sqlQuerry = $"CALL infogaAnstalld('{fornamn}', '{efternamn}', '{address}', '{postnummer}', '{kon}', '{fodelsedatum}',  '{email}', '{telefonnummer}' );";

            //Skapar ett MySqlCOmmand objekt
            MySqlCommand cmd = new MySqlCommand(sqlQuerry, conn);

            //Skapa ett TryCatch Block
            try
            {
                //Öppna Connection
                conn.Open();

                //Exekvera kommando
                cmd.ExecuteReader();

                //Stänga Connection
                conn.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            OppnaAnstalldaFranDB();
            //Bekräftelse till användare
            MessageBox.Show("Anställd tillagd!");
            
            
        }

        //EVENT KNAPP ÖPPNA
        private void btnOppna_Click(object sender, EventArgs e)
        {
            OppnaAnstalldaFranDB();
        }

        //METOD KNAPP ÖPPNA 
        private void OppnaAnstalldaFranDB(string keyword = "")
        {
            //Skapa en SQL Querry
            string sqlQuerry;

            if (keyword == "") sqlQuerry = $"CALL oppnaAnstallda();";
            else sqlQuerry = $"CALL sokAnstalld('{keyword}');";

            //Skapa ett MySQLCommand objekt
            MySqlCommand cmd = new MySqlCommand(sqlQuerry, conn);

            //Exekvera querry mot DB. Få data tillbaka
            try
            {
                //Öppnar koppling till DB
                conn.Open();

                //Exekvera cmd
                MySqlDataReader reader = cmd.ExecuteReader();

                //Placera data i en DataTable objekt
                DataTable dt = new DataTable();
                dt.Load(reader);

                //Koppla TD objekt som DataSource till Grid
                gridAnstalldaOutput.DataSource = dt;

                //Ladda Reader på Nytt
                reader = cmd.ExecuteReader();

                //Tömma anställd lista
                Anstalld.anstalld.Clear();

                //While loop för att spara datan lokalt i en lista
                while (reader.Read())
                {
                    
                    //Hämta och spara data till variabler
                    int anstallningsid = Convert.ToInt32(reader["anstalld_anstallningsid"]);
                    string fornamn = reader["anstalld_fornamn"].ToString();
                    string efternamn = reader["anstalld_efternamn"].ToString();
                    string address = reader["anstalld_address"].ToString();
                    string postnummer = reader["anstalld_postnummer"].ToString();
                    string kon = reader["anstalld_kon"].ToString();
                    string email = reader["anstalld_email"].ToString();
                    string telefonnummer = reader["anstalld_telefonnummer"].ToString();
                    

                    //Skapa ett anställd objekt och spara i statisk lista
                    Anstalld.anstalld.Add(new Anstalld(anstallningsid, fornamn, efternamn, address, 
                        postnummer, kon, (DateTime)reader["anstalld_fodelsedatum"], email, telefonnummer));
                }

                //Stänga koppling till DB
                conn.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            //Enabla knapp för Update och Delete
            btnRadera.Enabled = true;
            btnUppdatera.Enabled = true;
         
        }

      
        //VÄLJ ANSTÄLLD FÖR ATT UPPDATERA
        private void ValjAnstalld()
        {
            //Kontrollera att vi har en markerad rad i grid
            if (gridAnstalldaOutput.SelectedRows.Count != 1) return;

            //Hämta data från grid
            DataGridViewSelectedRowCollection row = gridAnstalldaOutput.SelectedRows;
            int anstallningsid = Convert.ToInt32(row[0].Cells[0].Value);

            //Skriva in data från grid till formulär
            foreach (Anstalld anstalld in Anstalld.anstalld)
            {
                // Kontrollera ID property
                if (anstalld.Anstallningsid == anstallningsid)
                {
                    //Rätt objekt hittat                 
                    txtFornamn.Text = anstalld.Fornamn;
                    txtEfternamn.Text = anstalld.Efternamn;
                    txtAddress.Text = anstalld.Address;
                    txtPostnummer.Text = anstalld.Postnummer;                 
                    txtEmail.Text = anstalld.Email;
                    txtTelefonnummer.Text = anstalld.Telefonnummer;

                    break;

                }
            }

            //Uppdatera  ICE grid via personens id 
            OppnaICEtillAnstalld(anstallningsid);
            OppnaUtlaningtillAnstalld(anstallningsid); 


        }

  

     
        //METOD FÖR DELETE Anstalld
        private void RaderaAnstalld()
        {
            //Kontrollera att vi har en markerad rad i grid
            if (gridAnstalldaOutput.SelectedRows.Count != 1) return;

            //Hämta data från grid
            DataGridViewSelectedRowCollection row = gridAnstalldaOutput.SelectedRows;
            int anstallningsid = Convert.ToInt32(row[0].Cells[0].Value);

            //Skapar en SQL Querry
            string SqlQuerry = $"CALL raderaAnstalld({anstallningsid});";

            //MySqlCommand
            MySqlCommand cmd = new MySqlCommand(SqlQuerry, conn);

            try
            {
                //Öppna koppling till DB
                conn.Open();

                //Exekverar commando
                cmd.ExecuteReader();

                conn.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            //Hämta den nya datan
            OppnaAnstalldaFranDB(); 
        }



        //METOD FÖR ATT UPPDATERA 
        private void UppdateraAnstalldTillDB()
        {
            //Kontrollera att vi har en markerad rad i grid
            if (gridAnstalldaOutput.SelectedRows.Count != 1) return;

            //Hämta data från grid
            DataGridViewSelectedRowCollection row = gridAnstalldaOutput.SelectedRows;
            int anstallningsid = Convert.ToInt32(row[0].Cells[0].Value);


            //hämtar värden från textfält        
            string fornamn = txtFornamn.Text;
            string efternamn = txtEfternamn.Text;
            string address = txtAddress.Text;
            string postnummer = txtPostnummer.Text;
            string email = txtEmail.Text;
            string telefonnummer = txtTelefonnummer.Text;

            //Skapar en SQL Querry
            string SqlQuerry = $"CALL uppdateraAnstalld( {anstallningsid}, '{fornamn}', '{efternamn}', '{address}', '{postnummer}',  '{email}', '{telefonnummer}' );";

            //MySqlCommand
            MySqlCommand cmd = new MySqlCommand(SqlQuerry, conn);

            try
            {
                //Öppna koppling till DB
                conn.Open();

                //Exekverar commando
                cmd.ExecuteReader();

                conn.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            //Hämta den nya datan
            OppnaAnstalldaFranDB();
        }

        //KNAPP
        private void btnUppdatera_Click(object sender, EventArgs e)
        {
            UppdateraAnstalldTillDB();
        }

        //KNAPP 
        private void btnInfoga_Click_1(object sender, EventArgs e)
        {
            InfogaAnstalld();
        }

        //KNAPP 
        private void btnSokAnstalld_Click(object sender, EventArgs e)
        {
            OppnaAnstalldaFranDB(txtSok.Text);
        }

        //GRIDS 
        private void gridAnstalldaOutput_SelectionChanged(object sender, EventArgs e)
        {
            ValjAnstalld();
        }

        //Metod knapp öppan ICE 
        private void OppnaICEAnstallda()
        {
            //Skapa en SQL Querry 
            string sqlQuerry = $"CALL OppnaICEAnstallda();";

            //skapa ett MySqlCommand object
            MySqlCommand cmd = new MySqlCommand(sqlQuerry, conn);

            //exekvera qyerry mot DB. Få data tillbaka 
            try
            {
                //Öppna koppling till DB 
                conn.Open();

                //Exekvera cmd
                MySqlDataReader reader = cmd.ExecuteReader();

                //Placera data i en datatable objekt
                DataTable dt = new DataTable();
                dt.Load(reader);

                //koppla Dt objekt som datasource till grid 
                gridICEAnstalldaOutput.DataSource = dt;

                //Stänga koppling till DB 
                conn.Close();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }

        //METOD  
        private void OppnaICEtillAnstalld(int anstallningsid)
        {
            //sql dquerry 
            string SqlQuerry = $"CALL oppnaICETillAnstalld({anstallningsid});"; 
            
            //command
            MySqlCommand cmd = new MySqlCommand(@SqlQuerry, conn);

            try
            {
                //öppna koppling till DB
                conn.Open(); 

                //Exekvera kommando 
                MySqlDataReader reader = cmd.ExecuteReader();

                //Skapa och fyll upp datatable 
                DataTable dt = new DataTable();
                dt.Load(reader);

                //koppla dt till datasource i grid 
                gridICEAnstalldaOutput.DataSource = dt;

                //stäng koppling till db 
                conn.Close(); 


            } catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        
        //METOD 
        private void InfogaICEtillAnstalld()
        {
            //Kontrollera att vi har en markerad rad i grid 
            if (gridAnstalldaOutput.SelectedRows.Count != 1) return;

            //kontrollera att ICe anstallda formuläret har inmatade värden 
            bool valid = true; 

            foreach(TextBox txtbox in txtBoxesICEAnstallda)
            {
                //trimmar testinnehåller
                txtbox.Text = txtbox.Text.Trim();

                //kontrollera att txtBox har text 
                if(txtbox.Text == "")
                {
                    //validering har misslyckats 
                    valid= false;
                    txtbox.BackColor = Color.IndianRed; 
                } else
                {
                    txtbox.BackColor = TextBox.DefaultBackColor;
                }
            }

            //kontroller valid
            if(!valid)
            {
                MessageBox.Show("Felaktig validering. Kontrollera röda fält");
                return;
            }

            //Hämta data från grid
            DataGridViewSelectedRowCollection row = gridAnstalldaOutput.SelectedRows; 
            int anstallningsid = Convert.ToInt32(row[0].Cells[0].Value);

            //Hämta textvärden
            string iceFornamn = txtICEFornamn.Text;
            string iceEfternamn = txtICEEfternamn.Text;
            string iceTelefonnummer = txtICETelefonnummer.Text;

            //Sqlquerry 
            string SqlQuerry = $"CALL infogaICEtillAnstalld({anstallningsid}, '{iceFornamn}', '{iceEfternamn}', '{iceTelefonnummer}');";

            //sql command
            MySqlCommand cmd = new MySqlCommand(SqlQuerry, conn);

            try
            {
                //öppna koppling till DB 
                conn.Open();

                //exekverar kommando 
                cmd.ExecuteReader(); 

                //stäng koppling till DB 
                conn.Close();
            } catch (Exception e)
            {
                MessageBox.Show(e.Message); 
            }


            //Uppdatera ICE grid 
            OppnaICEtillAnstalld(anstallningsid);

            MessageBox.Show("ICE tillagd!"); 
        }

        //METOD 
        private void InfogaICEtillNyAnstalld()
        {
            bool valid = true; 

            foreach(TextBox txtBox in txtBoxesAnstallda)
            {
                //trimmar test-innehållet
                txtBox.Text = txtBox.Text.Trim();

                //kontrollera att txtBox har text 
                if (txtBox.Text == "")
                {
                    //Validering har misslyckats
                    valid = false;
                    txtBox.BackColor = Color.IndianRed;
                }
                else
                {
                    txtBox.BackColor = TextBox.DefaultBackColor;
                }
            }

            foreach(TextBox txtBox in txtBoxesICEAnstallda)
            {
                //trimmar testinnehållet 
                txtBox.Text = txtBox.Text.Trim();

                //Kontrollera att txtBox har text
                if (txtBox.Text == "")
                {
                    //Validering har misslyckats
                    valid = false;
                    txtBox.BackColor = Color.IndianRed;
                }
                else
                {
                    txtBox.BackColor = TextBox.DefaultBackColor;
                }
            }

            //kontroller valid 
            if (!valid)
            {
                MessageBox.Show("Kontrollera röd fält."); 
                return;
            }

            //Hämta data och exekvera SQL
            string fornamn = txtFornamn.Text.ToString();
            string efternamn = txtEfternamn.Text.ToString();
            string address = txtAddress.Text.ToString();
            string postnummer = txtPostnummer.Text.ToString();
            string kon = Convert.ToString(cbKon.SelectedItem);
            string fodelsedatum = dTPfodelsedag.Value.ToShortDateString();
            string email = txtEmail.Text.ToString();
            string telefonnummer = txtTelefonnummer.Text.ToString();
            string iceFornamn = txtICEFornamn.Text.ToString();
            string iceEfternamn = txtICEEfternamn.Text.ToString();
            string iceTelefonnummer = txtICETelefonnummer.Text.ToString();

            //SKapa en sql querrt 
            string sqlQuerry = $"CALL InfogaICEtillNyAnstalld('{fornamn}', '{efternamn}', '{address}', '{postnummer}', '{kon}', '{fodelsedatum}', '{email}', '{telefonnummer}', '{iceFornamn}', '{iceEfternamn}', '{iceTelefonnummer}');";

            //Skapa command objekt 
            MySqlCommand cmd = new MySqlCommand(sqlQuerry, conn);

            try
            {
                //Öppna koppling, exekvera och stäng koppling 
                conn.Open();
                cmd.ExecuteReader();
                conn.Open(); 
            }catch (Exception e)
            {
                MessageBox.Show (e.Message);
            }

            //Hämta data till person tabellen 
            OppnaAnstalldaFranDB();

            //Markera den nya personen i grid 
            gridAnstalldaOutput.Rows[gridAnstalldaOutput.Rows.Count - 2].Selected = true;

            //Hämta data till Tabellen 
            ValjAnstalld(); 

        }

        private void btnICEInfoga_Click(object sender, EventArgs e)
        {
            InfogaICEtillAnstalld(); 
        }

        private void btnInfogaICEtillNyAnstalld_Click(object sender, EventArgs e)
        {
            InfogaICEtillNyAnstalld(); 
        }

        private void btnICEOppna_Click(object sender, EventArgs e)
        {
            OppnaICEAnstallda();
        }

        //METOD FÖR DELETE ICE
        private void RaderaICE()
        {
            //Kontrollera att vi har en markerad rad i grid
            if (gridICEAnstalldaOutput.SelectedRows.Count != 1) return;

            //Hämta data från grid
            DataGridViewSelectedRowCollection row = gridICEAnstalldaOutput.SelectedRows;
            int iceanstalldid = Convert.ToInt32(row[0].Cells[0].Value);

            //Skapar en SQL Querry
            string SqlQuerry = $"CALL raderaICE({iceanstalldid});";

            //MySqlCommand
            MySqlCommand cmd = new MySqlCommand(SqlQuerry, conn);

            try
            {
                //Öppna koppling till DB
                conn.Open();

                //Exekverar commando
                cmd.ExecuteReader();

                conn.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            //Hämta den nya datan
            OppnaICEAnstallda();
        }

        //METOD FÖR ÖPPNA UTLÅNING 
        private void OppnaUtlaning()
        {
            //Skapa en SQL Querry 
            string sqlQuerry = $"CALL oppnaUtlaning();";

            //skapa ett MySqlCommand object
            MySqlCommand cmd = new MySqlCommand(sqlQuerry, conn);

            //exekvera qyerry mot DB. Få data tillbaka 
            try
            {
                //Öppna koppling till DB 
                conn.Open();

                //Exekvera cmd
                MySqlDataReader reader = cmd.ExecuteReader();

                //Placera data i en datatable objekt
                DataTable dt = new DataTable();
                dt.Load(reader);

                //koppla Dt objekt som datasource till grid 
                gridUtlaningOutput.DataSource = dt;

                //Stänga koppling till DB 
                conn.Close();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        //METOD ÖPPNA UTLÅNING TILL ANSTALLD 
        private void OppnaUtlaningtillAnstalld(int anstallningsid)
        {

            //sql dquerry 
            string SqlQuerry = $"CALL oppnaUtlaningtillAnstalld({anstallningsid});";

            //command
            MySqlCommand cmd = new MySqlCommand(@SqlQuerry, conn);

            try
            {
                //öppna koppling till DB
                conn.Open();

                //Exekvera kommando 
                MySqlDataReader reader = cmd.ExecuteReader();

                //Skapa och fyll upp datatable 
                DataTable dt = new DataTable();
                dt.Load(reader);

                //koppla dt till datasource i grid 
                gridUtlaningOutput.DataSource = dt;

                //stäng koppling till db 
                conn.Close();


            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        
        //INFOGA UTLÅNING TILL ANSTÄLLD 
        private void InfogaUtlaningtillAnstalld()
        {
            //Hämta data från grid
            DataGridViewSelectedRowCollection row = gridAnstalldaOutput.SelectedRows;
            int anstallningsid = Convert.ToInt32(row[0].Cells[0].Value);

            //Hämta textvärden
            string tjansteTelefon = Convert.ToString(cbTjanstetelefon.SelectedItem); 
            string tjansteDator = Convert.ToString(cbTjanstedator.SelectedItem); 
            string tjansteBil = Convert.ToString(cbTjanstebil.SelectedItem); 


            //Sqlquerry 
            string SqlQuerry = $"CALL infogaUtlaningtillAnstalld({anstallningsid}, '{tjansteTelefon}', '{tjansteDator}', '{tjansteBil}');";

            //sql command
            MySqlCommand cmd = new MySqlCommand(SqlQuerry, conn);

            try
            {
                //öppna koppling till DB 
                conn.Open();

                //exekverar kommando 
                cmd.ExecuteReader();

                //stäng koppling till DB 
                conn.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }


            //Uppdatera utlåning grid 
            OppnaUtlaningtillAnstalld(anstallningsid);

            MessageBox.Show("ICE tillagd!");
        }

        //INFOGA UTLÅNING TILL NY ANSTÄLLD 
        public void InfogaUtlaningtillNyAnstalld()
        {
            //Hämta data och exekvera SQL
            string fornamn = txtFornamn.Text.ToString();
            string efternamn = txtEfternamn.Text.ToString();
            string address = txtAddress.Text.ToString();
            string postnummer = txtPostnummer.Text.ToString();
            string kon = Convert.ToString(cbKon.SelectedItem);
            string fodelsedatum = dTPfodelsedag.Value.ToShortDateString();
            string email = txtEmail.Text.ToString();
            string telefonnummer = txtTelefonnummer.Text.ToString();
            string tjansteTelefon = Convert.ToString(cbTjanstetelefon.SelectedItem);
            string tjansteDator = Convert.ToString(cbTjanstedator.SelectedItem);
            string tjansteBil = Convert.ToString(cbTjanstebil.SelectedItem);

            //SKapa en sql querrt 
            string sqlQuerry = $"CALL infogaUtlaningtillNyAnstalld('{fornamn}', '{efternamn}', '{address}', '{postnummer}', '{kon}', '{fodelsedatum}', '{email}', '{telefonnummer}', '{tjansteTelefon}', '{tjansteDator}', '{tjansteBil}');";

            //Skapa command objekt 
            MySqlCommand cmd = new MySqlCommand(sqlQuerry, conn);

            try
            {
                //Öppna koppling, exekvera och stäng koppling 
                conn.Open();
                cmd.ExecuteReader();
                conn.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            //Hämta data till person tabellen 
            OppnaAnstalldaFranDB();

            //Markera den nya personen i grid 
            gridAnstalldaOutput.Rows[gridAnstalldaOutput.Rows.Count - 2].Selected = true;

            //Hämta data till Tabellen 
            ValjAnstalld();
        }

        private void btnUtlaningOppna_Click(object sender, EventArgs e)
        {
            OppnaUtlaning(); 
        }

        private void btnInfogaUtlaningtillAnstalld_Click(object sender, EventArgs e)
        {
            InfogaUtlaningtillAnstalld(); 
        }

        private void btnInfogaNyAnstalldOchNyUtlaning_Click(object sender, EventArgs e)
        {
            InfogaUtlaningtillNyAnstalld(); 
        }

        private void btnRaderaICE_Click(object sender, EventArgs e)
        {
            RaderaICE(); 
        }

        private void RaderaUtlaning()
        {

            //Kontrollera att vi har en markerad rad i grid
            if (gridUtlaningOutput.SelectedRows.Count != 1) return;

            //Hämta data från grid
            DataGridViewSelectedRowCollection row = gridUtlaningOutput.SelectedRows;
            int utlaningid = Convert.ToInt32(row[0].Cells[0].Value);

            //Skapar en SQL Querry
            string SqlQuerry = $"CALL raderaUtlaning({utlaningid});";

            //MySqlCommand
            MySqlCommand cmd = new MySqlCommand(SqlQuerry, conn);

            try
            {
                //Öppna koppling till DB
                conn.Open();

                //Exekverar commando
                cmd.ExecuteReader();

                conn.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            //Hämta den nya datan
            OppnaUtlaning();
        }

        private void btnRaderaUtlaning_Click(object sender, EventArgs e)
        {
            RaderaUtlaning(); 
        }

        private void btnRadera_Click(object sender, EventArgs e)
        {
            RaderaAnstalld(); 
        }

        private void OppnaTelefonlista()
        {
            //Skapa en SQL Querry 
            string sqlQuerry = $"SELECT * FROM telefonlista_view";

            //skapa ett MySqlCommand object
            MySqlCommand cmd = new MySqlCommand(sqlQuerry, conn);

            //exekvera qyerry mot DB. Få data tillbaka 
            try
            {
                //Öppna koppling till DB 
                conn.Open();

                //Exekvera cmd
                MySqlDataReader reader = cmd.ExecuteReader();

                //Placera data i en datatable objekt
                DataTable dt = new DataTable();
                dt.Load(reader);

                //koppla Dt objekt som datasource till grid 
                gridTelefonlistaView.DataSource = dt;

                //Stänga koppling till DB 
                conn.Close();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void btnOppnaTelefonlista_Click(object sender, EventArgs e)
        {
            OppnaTelefonlista();
        }

        private void OppnaUtlaningslista()
        {
            //Skapa en SQL Querry 
            string sqlQuerry = $"CALL sammanfattningAnstalldUtlaning();";

            //skapa ett MySqlCommand object
            MySqlCommand cmd = new MySqlCommand(sqlQuerry, conn);

            //exekvera qyerry mot DB. Få data tillbaka 
            try
            {
                //Öppna koppling till DB 
                conn.Open();

                //Exekvera cmd
                MySqlDataReader reader = cmd.ExecuteReader();

                //Placera data i en datatable objekt
                DataTable dt = new DataTable();
                dt.Load(reader);

                //koppla Dt objekt som datasource till grid 
                gridUtlaningslistaOutput.DataSource = dt;

                //Stänga koppling till DB 
                conn.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }



        }

        private void btnUtlaningslist_Click(object sender, EventArgs e)
        {
            OppnaUtlaningslista(); 
        }
    }
}

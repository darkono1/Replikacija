using DevExpress.Xpo.DB.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Replikacija
{

    internal class PoziviProcedura// : frmReplikacija
    {
        string ExceptErrorMessage="";

        internal bool SetTimeStamp(string ServerAdress, string Table, string TStampType, string sinhro1or0)/// upisuje novi timestamp u tbl_ReplikStatus 
        {
            
            string SqlCon = $"Data Source={ServerAdress};Database=A2D2udb-Frukta-2023;Persist Security Info=True;uid=sa;pwd=Fructa2014pa$$";
            try
            {
                using (SqlConnection con = new SqlConnection(SqlCon))
                {
                    using (SqlCommand cmd = new SqlCommand("stp_ISetTimeStamp", con))//poziv stored procedure za upis najnovijeg timestampa sa centralnog na lokalnim serv
                    {
                        cmd.CommandTimeout = 100;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@table", SqlDbType.VarChar).Value = Table;
                        cmd.Parameters.Add("@TStampType", SqlDbType.VarChar).Value = TStampType;
                        cmd.Parameters.Add("@sinhro1or0", SqlDbType.VarChar).Value = sinhro1or0;

                        con.Open();
                        int i = cmd.ExecuteNonQuery();
                        con.Close();
                        if (i != 0)
                        {
                           return true;
                        }
                        else
                        {
                            return false;
                        }
                        
                    }
                   
                }
               
            }
            catch (Exception ex)
            {
                {
                    ExceptErrorMessage= ex.Message;
                    return false;
                }
            }
            
                                 
        }

        internal bool UpisiInsUpdReplikaciju(string ServerAdress, string Table, string StorProcedure)///metod upisuje izmjene za update i insert pozivom stp_InsertReplikacija ili stp_UpdateReplikacija
        {
            frmReplikacija frm=new frmReplikacija();
            int i = 0;
            string SqlCon = $"Data Source={ServerAdress};Database=A2D2udb-Frukta-2023;Persist Security Info=True;uid=sa;pwd=Fructa2014pa$$";
            try
            {
                using (SqlConnection con = new SqlConnection(SqlCon))
                {
                    using (SqlCommand cmd = new SqlCommand(StorProcedure, con))//poziv stored proc koja upisuje sa centralnog na servere
                    {
                       //   frm.LoadIntoDgvStatus(ServerAdress, Table, "INSERT U TOKU", "");
                        cmd.CommandTimeout = 300;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@table", SqlDbType.VarChar).Value = Table;

                        con.Open();
                        i = cmd.ExecuteNonQuery();
                        con.Close();
                        if (i != 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                            
                    }
                    
                }
            
                
            }
            catch (Exception ex)
            {
                {
                    ExceptErrorMessage=ex.Message;
                    return false;
                }
            }
        }

        internal bool DelFromLocServ(string ServerAdress, string Table)///metod upisuje izmjene za update i insert pozivom stp_InsertReplikacija ili stp_UpdateReplikacija
        {
            frmReplikacija frm = new frmReplikacija();
            int i = 0;
            string SqlCon = $"Data Source={ServerAdress};Database=A2D2udb-Frukta-2023;Persist Security Info=True;uid=sa;pwd=Fructa2014pa$$";
            try
            {
                using (SqlConnection con = new SqlConnection(SqlCon))
                {
                    using (SqlCommand cmd = new SqlCommand("stp_IDeleteReplikacija", con))//poziv stored proc koja upisuje sa centralnog na servere
                    {
                        //   frm.LoadIntoDgvStatus(ServerAdress, Table, "INSERT U TOKU", "");
                        cmd.CommandTimeout = 300;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Table", SqlDbType.VarChar).Value = Table;
                      

                        con.Open();
                        i = cmd.ExecuteNonQuery();
                        con.Close();
                        if (i != 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                {
                    ExceptErrorMessage = ex.Message;
                    return false;
                }
            }
        }

        public string GetErrorMessage()
        {
            return ExceptErrorMessage;
        }
        internal bool TimestampsChanged(string ServerAdress, string Table, string sinhro, string TimeStampType)//provera da li se promenio timestamp
        {
            bool TimeStampChanged = false;
            string SqlCon = $"Data Source={ServerAdress};Database=A2D2udb-Frukta-2023;Persist Security Info=True;uid=sa;pwd=Fructa2014pa$$";
            try
            {
                using (SqlConnection con = new SqlConnection(SqlCon))
                {
                    using (SqlCommand cmd = new SqlCommand("stp_ICheckInsUpdTStamp", con))//
                    {
                        cmd.CommandTimeout = 120;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@TStampChanged", SqlDbType.Bit);
                        cmd.Parameters["@TStampChanged"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@table", SqlDbType.VarChar).Value = Table;
                        cmd.Parameters.Add("@sinhro1or0", SqlDbType.VarChar).Value = sinhro;
                        cmd.Parameters.Add("@TimeStampType", SqlDbType.VarChar).Value = TimeStampType;
                        con.Open();
                        int i = cmd.ExecuteNonQuery();
                        TimeStampChanged = Boolean.Parse(cmd.Parameters["@TStampChanged"].Value.ToString());
                        con.Close();
                    }
                }
            }

            catch (Exception ex)
            {
                frmReplikacija frm = new frmReplikacija();
                frm.LoadIntoDgvStatus(ServerAdress, Table, "neuspjesna provera timestampa", ex.Message);
            }
            return TimeStampChanged;
        }

        internal bool CheckDelete(string ServerAdress,string  Table)//provera da li se brisalo na centralli
        {
            bool RowsDeleted = false;
            string SqlCon = $"Data Source={ServerAdress};Database=A2D2udb-Frukta-2023;Persist Security Info=True;uid=sa;pwd=Fructa2014pa$$";
            try
            {
                using (SqlConnection con = new SqlConnection(SqlCon))
                {
                    using (SqlCommand cmd = new SqlCommand("stp_ICheckDelete", con))//
                    {
                        cmd.CommandTimeout = 120;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ImaZaBrisanje", SqlDbType.Bit);
                        cmd.Parameters["@ImaZaBrisanje"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@Table", SqlDbType.VarChar).Value = Table;
                        cmd.Parameters.Add("@LocServerAdress", SqlDbType.VarChar).Value = ServerAdress;
                        con.Open();
                        int i = cmd.ExecuteNonQuery();
                        RowsDeleted = Boolean.Parse(cmd.Parameters["@ImaZaBrisanje"].Value.ToString());
                         con.Close();
                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                frmReplikacija frm = new frmReplikacija();
                frm.LoadIntoDgvStatus(ServerAdress, Table, "neuspjesna provera delete statusa ", ex.Message);
            }
            
            return RowsDeleted;
        }


        internal void SetNEzaBrisanje(string ServerAdress, string Table)
        {
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter();
                string connetionString = $"Data Source=192.168.234.100;Database=A2D2udb-Frukta-2023;Persist Security Info=True;uid=sa;pwd=Fructa2014pa$$";
                SqlConnection connection = new SqlConnection(connetionString);
                string sqlRemoveDAfromRepStatServ = $"UPDATE tbl_ReplikStatusServera SET fld_ImaZaBrisanje='NE' " +
                           $"WHERE fld_IPAdresaServera='{ServerAdress}' AND fld_ImeTabele='{Table}'";
                try
                {
                    connection.Open();
                    adapter.UpdateCommand = connection.CreateCommand();
                    adapter.UpdateCommand.CommandText = sqlRemoveDAfromRepStatServ;
                    adapter.UpdateCommand.ExecuteNonQuery();
                    connection.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greska \n" + ex.Message);
            }
        }


    }
}

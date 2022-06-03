using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Data.OleDb;

namespace HukukBurosu
{
    public partial class Giris : MetroForm
    {
        public Giris()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 50, 50));
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
        int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse
        );

        private void metroButtonGiris_Click(object sender, EventArgs e)
        {
            String databasePath = "hukukburosu.accdb";
            bool result = false;
            bool isAdmin = false;
            
            String sorguStringi = "Select * from kullanicilar Where kullanici_adi = '" +
            metroTextBoxKullaniciAdi.Text + "' AND sifre = '" + metroTextBoxSifre.Text + "'";
            OleDbConnection baglanti = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + databasePath);
            OleDbCommand komut = new OleDbCommand(sorguStringi, baglanti);
            baglanti.Open();
            OleDbDataReader dataReader = komut.ExecuteReader();
            if (dataReader.Read())
            {
                if (dataReader.GetValue(3).ToString().ToLower() == "true")
                {
                    isAdmin = true;
                }
                
                result = true;
                baglanti.Close();
            }
            if(result){
                Panel form = new Panel(isAdmin);
                form.Visible = true;
                MetroFramework.MetroMessageBox.Show(form, "Giriş başarılı");
                this.Visible = false;
            }
            else
            {
                MetroFramework.MetroMessageBox.Show(this,"Kullanıcı Adı veya Şifre Hatalı");
            }
        }
            

    }
}

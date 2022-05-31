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
using System.Data.OleDb;

namespace HukukBurosu
{
    public partial class Form1 : MetroForm
    {
        public DateTime[] dates = new DateTime[30];
        public Form1()
        {
            
            InitializeComponent();
            groupBoxDurum.Visible = false;
            VeriDoldur();
            anasayfaGrafik();
            metroTileGozetimPaneli_Click(metroTileGozetimPaneli, new EventArgs());

        }
        private DataSet veritabaniSelect(String tabloAdi, String kolon = "*", String where = "", String aranan = "")
        {
            String sorguStringi = "Select " + kolon + " from " + tabloAdi;
            if (!where.Equals("") && !where.Equals(""))
            {
                sorguStringi += " WHERE " + where + "='" + aranan + "'" ;
            }
            OleDbConnection baglanti;
            OleDbDataAdapter sorgu;
            DataSet verikumesi = new DataSet();
            String databasePath = @"C:\Users\MAKAROV\Documents\Visual Studio 2013\Projects\HukukBurosu\HukukBurosu\hukukburosu.accdb";
            baglanti = new OleDbConnection("Provider=Microsoft.ACE.Oledb.12.0; Data Source=" + databasePath);
            sorgu = new OleDbDataAdapter("Select " + kolon + " from " + tabloAdi, baglanti);
            baglanti.Open();
            sorgu.Fill(verikumesi, tabloAdi);
            baglanti.Close();
            return verikumesi;
        }

        private int[] veritabaniDiziyeAktar(String tabloAdi, String kolon = "*")
        {
            int[] result = new int[30];
            String sorguStringi = "Select " + kolon + " from " + tabloAdi;
            OleDbConnection baglanti;
            String databasePath = @"C:\Users\MAKAROV\Documents\Visual Studio 2013\Projects\HukukBurosu\HukukBurosu\hukukburosu.accdb";
            baglanti = new OleDbConnection("Provider=Microsoft.ACE.Oledb.12.0; Data Source=" + databasePath);
            baglanti.Open();
            OleDbCommand komut = new OleDbCommand(sorguStringi, baglanti);
            OleDbDataReader cikti = komut.ExecuteReader();
            int i = 0;
            if (kolon == "tarih")
            {
                while (cikti.Read())
                {
                    if (!cikti[kolon].Equals(DBNull.Value))
                    {
                            dates[i] = (DateTime)cikti[kolon];
                    }
                    i++;
                }
                
            }else{
                while (cikti.Read())
                {

                    if (!cikti[kolon].Equals(DBNull.Value))
                    {
                            result[i] = (int)cikti[kolon];
                    }
                    i++;
                }
                cikti.Close();
                baglanti.Close();
                return result;
            }
            
            cikti.Close();
            baglanti.Close();
            return null;
        }

         void VeriDoldur()
        {
            DataSet veri = veritabaniSelect("muhasebe_defteri");
            metroGrid1.DataSource = veri.Tables["muhasebe_defteri"];
 
        }

         void anasayfaGrafik()
         {
             int[] gelenPara = veritabaniDiziyeAktar("muhasebe_defteri", "tahsil_edilen");
             int[] gidenPara = veritabaniDiziyeAktar("muhasebe_defteri", "gider");
             veritabaniDiziyeAktar("muhasebe_defteri", "tarih");
             double netDurum, gelir = 0, gider = 0;
             for (int i = 0; i < gelenPara.Length; i++)
             {
                 if(gelenPara[i] != 0){
                     double doubleGelenPara = Convert.ToDouble(gelenPara[i]);
                     gelir += doubleGelenPara;
                     chartGozetimPaneli.Series["Bu Ay Tahsil Edilen Miktar"].Points.Add(doubleGelenPara);
                 }
                 
             }
             for (int i = 0; i < gidenPara.Length; i++)
             {
                 if(gelenPara[i] != 0){
                     double doubleGidenPara = Convert.ToDouble(gidenPara[i]);
                     gider += doubleGidenPara;
                     chartGozetimPaneli.Series["Bu Ay Harcanan Miktar"].Points.Add(doubleGidenPara);
                 }
                 
             }

             for (int i = 0; i < this.dates.Length; i++)
             {
                 if (this.dates[i] > new DateTime())
                 {
                     chartGozetimPaneli.Series["Bu Ay Harcanan Miktar"].Points[i].AxisLabel = this.dates[i].ToString("MMMM dd yyyy");
                 }

             }

             netDurum = gelir - gider;
             metroLabelNetDurum.Text = "Net Durum: " + netDurum.ToString() + "₺";

             metroLabelGelir.Text = gelir.ToString() + "₺";
             metroLabelGider.Text = gider.ToString() + "₺";
             
         }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void metroTileGozetimPaneli_Click(object sender, EventArgs e)
        {
            groupBoxDurum.Visible = true;
        }
    }
}

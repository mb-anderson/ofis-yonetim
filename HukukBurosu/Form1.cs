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
        String databasePath = "hukukburosu.accdb";
        public DateTime[] dates = new DateTime[30];
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            groupBoxGozetimPaneli.Visible = false;
            metroGridDoldur(metroGridDurusmalar, "durusmalar", "muvekkil, vekil, tarih");
            anasayfaGrafik();
            metroTileGozetimPaneli_Click(metroTileGozetimPaneli, new EventArgs());
            metroTileGozetimPaneli.UseSelectable = false;
            metroTileCalisanlar.UseSelectable = false;
            metroTileEvrak.UseSelectable = false;
            metroTileMuvekkiller.UseSelectable = false;
            metroTileTebligatlar.UseSelectable = false;
            metroTileBilgi.UseSelectable = false;
            metroTileCikis.UseSelectable = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            dirListBoxEvraklar.Path = System.IO.Path.GetFullPath("evraklar");
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(metroButtonDosyaYukle_DragEnter);
            this.DragDrop += new DragEventHandler(metroButtonDosyaYukle_DragDrop);
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
            baglanti = new OleDbConnection("Provider=Microsoft.ACE.Oledb.12.0; Data Source=" + databasePath);
            sorgu = new OleDbDataAdapter("Select " + kolon + " from " + tabloAdi, baglanti);
            baglanti.Open();
            sorgu.Fill(verikumesi, tabloAdi);
            baglanti.Close();
            return verikumesi;
        }

        private int[] veritabaniDiziyeAktar(String tabloAdi, String kolon = "*")
        {
            try
            {
                int[] result = new int[30];
                String sorguStringi = "Select " + kolon + " from " + tabloAdi;
                OleDbConnection baglanti;
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

                }
                else
                {
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
            }
            catch (OleDbException e)
            {

                MetroFramework.MetroMessageBox.Show(this, "Veritabanına bağlanılamadı HATA KODU: " + e.ToString());
                Environment.Exit(0);
            }
            return null;
        }

         private void metroGridDoldur(MetroFramework.Controls.MetroGrid metroGrid, String tabloAdi, String kolonlar)
        {
            DataSet veri = veritabaniSelect(tabloAdi, kolonlar);
            metroGrid.DataSource = veri.Tables[tabloAdi];
 
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
                 if(gidenPara[i] != 0){
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

         private void menuSec(MetroFramework.Controls.MetroTile metroTile)
         {
             groupBoxGozetimPaneli.Visible = false;
             groupBoxCalisanlar.Visible = false;
             groupBoxEvraklar.Visible = false;

             switch (metroTile.Text)
             {
                 case "Gözetim Paneli":
                     groupBoxGozetimPaneli.Visible = true;
                     this.Text = "Gözetim Paneli";
                     break;
                 case "Çalışanlar":
                     groupBoxCalisanlar.Visible = true;
                     this.Text = "Çalışanlar";
                     break;
                 case "Evraklar":
                     groupBoxEvraklar.Visible = true;
                     this.Text = "Evraklar";
                     dirListBoxEvraklar_SelectedIndexChanged(dirListBoxEvraklar, new EventArgs());
                     break;
                 case "Müvekkiller":
                     //groupBoxCalisanlar.Visible = true;
                     this.Text = "Müvekkiller";
                     break;
                 case "Tebligatlar":
                     //groupBoxCalisanlar.Visible = true;
                     this.Text = "Tebligatlar";
                     break;
                 case "Bilgi":
                     //groupBoxCalisanlar.Visible = true;
                     this.Text = "Bilgi";
                     break;
             }

         }

         private void excelAktar(MetroFramework.Controls.MetroGrid dataGridView)
         {
             Microsoft.Office.Interop.Excel._Application app = new Microsoft.Office.Interop.Excel.Application();
             Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);
             Microsoft.Office.Interop.Excel._Worksheet worksheet = null;
             app.Visible = true;
             worksheet = workbook.Sheets["Sayfa1"];
             worksheet = workbook.ActiveSheet;
             worksheet.Name = "Exported from gridview";
             for (int i = 1; i < dataGridView.Columns.Count + 1; i++)
             {
                 worksheet.Cells[1, i] = dataGridView.Columns[i - 1].HeaderText;
             }
             for (int i = 0; i < dataGridView.Rows.Count - 1; i++)
             {
                 for (int j = 0; j < dataGridView.Columns.Count; j++)
                 {
                     worksheet.Cells[i + 2, j + 1] = dataGridView.Rows[i].Cells[j].Value.ToString();
                 }
             }
             workbook.SaveAs("c:\\tablo.xls", Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
             app.Quit();
             MetroFramework.MetroMessageBox.Show(this, "Tablo Başarıyla Excel Dosyası Olarak Dışarı Aktarıldı");
         }

        

        private void metroTileGozetimPaneli_Click(object sender, EventArgs e)
        {
            menuSec(metroTileGozetimPaneli);
        }

        private void metroTileCalisanlar_Click(object sender, EventArgs e)
        {
            menuSec(metroTileCalisanlar);
            metroGridDoldur(metroGridCalisanlar, "calisanlar", "tc_kimlik, ad_soyad, calisma_sekli, maas, ise_baslama_tarihi");
            int[] maaslar = veritabaniDiziyeAktar("calisanlar", "maas");
            int toplamOdeme = 0, calisanSayisi = 0;
            for (int i = 0; i < maaslar.Length; i++)
            {
                if (maaslar[i] != 0)
                {
                    calisanSayisi++;
                    toplamOdeme += maaslar[i];
                }
            }
            metroLabelToplamOdeme.Text = "Toplam Ödeme: " + toplamOdeme.ToString() + "₺";
            metroLabelCalisanSayisi.Text = " Toplam Çalışan Sayısı: " + calisanSayisi.ToString();
        }

        private void metroTileEvrak_Click(object sender, EventArgs e)
        {
            menuSec(metroTileEvrak);
        }

        private void metroTileMuvekkiller_Click(object sender, EventArgs e)
        {
            menuSec(metroTileMuvekkiller);
        }

        private void metroTileTebligatlar_Click(object sender, EventArgs e)
        {
            menuSec(metroTileTebligatlar);
        }

        private void metroTileBilgi_Click(object sender, EventArgs e)
        {
            menuSec(metroTileBilgi);
        }

        private void metroPanelMenu_MouseHover(object sender, EventArgs e)
        {

        }

        private void metroTileMouseEnter(MetroFramework.Controls.MetroTile metroTile)
        {
            metroTile.Style = MetroFramework.MetroColorStyle.White;
            metroTile.ForeColor = System.Drawing.Color.FromArgb(13701441);//209; 17; 65 - RED decimal değeri
        }

        private void metroTileMouseLeave(MetroFramework.Controls.MetroTile metroTile)
        {
            metroTile.Style = MetroFramework.MetroColorStyle.Red;
            metroTile.ForeColor = System.Drawing.Color.White;
        }
        private void metroTileGozetimPaneli_MouseEnter(object sender, EventArgs e)
        {
            metroTileMouseEnter(metroTileGozetimPaneli);   
        }
        private void metroTileGozetimPaneli_MouseLeave(object sender, EventArgs e)
        {
            metroTileMouseLeave(metroTileGozetimPaneli);
        }

        private void metroTileCalisanlar_MouseEnter(object sender, EventArgs e)
        {
            metroTileMouseEnter(metroTileCalisanlar);
        }
        private void metroTileCalisanlar_MouseLeave(object sender, EventArgs e)
        {
            metroTileMouseLeave(metroTileCalisanlar);
        }

        private void metroTileEvraklar_MouseEnter(object sender, EventArgs e)
        {
            metroTileMouseEnter(metroTileEvrak);
        }
        private void metroTileEvrak_MouseLeave(object sender, EventArgs e)
        {
            metroTileMouseLeave(metroTileEvrak);
        }

        private void metroTileMuvekkiller_MouseEnter(object sender, EventArgs e)
        {
            metroTileMouseEnter(metroTileMuvekkiller);
        }
        private void metroTileMuvekkiller_MouseLeave(object sender, EventArgs e)
        {
            metroTileMouseLeave(metroTileMuvekkiller);
        }

        private void metroTileTebligatlar_MouseEnter(object sender, EventArgs e)
        {
            metroTileMouseEnter(metroTileTebligatlar);
        }
        private void metroTileTebligatlar_MouseLeave(object sender, EventArgs e)
        {
            metroTileMouseLeave(metroTileTebligatlar);
        }

        private void metroTileBilgi_MouseEnter(object sender, EventArgs e)
        {
            metroTileMouseEnter(metroTileBilgi);
        }
        private void metroTileBilgi_MouseLeave(object sender, EventArgs e)
        {
            metroTileMouseLeave(metroTileBilgi);
        }
        private void metroTileCikis_MouseEnter(object sender, EventArgs e)
        {
            metroTileMouseEnter(metroTileCikis);
        }
        private void metroTileCikis_MouseLeave(object sender, EventArgs e)
        {
            metroTileMouseLeave(metroTileCikis);
        }

        private void metroTileCikis_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MetroFramework.MetroMessageBox.Show(this, "Eğer çıkarsanız kaydedilmemiş tüm veriler kaybolacak.", "Çıkmak istediğinize emin misiniz?", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void metroButtonExcelAktar_Click(object sender, EventArgs e)
        {
            excelAktar(metroGridCalisanlar);
        }
        private void dirListBoxEvraklar_DoubleClick(object sender, EventArgs e)
        {
            fileListBoxEvraklar.Path = dirListBoxEvraklar.Path;
            metroLabelDosyaSayisi.Text = "Dosya Sayısı: " + System.IO.Directory.GetFiles(dirListBoxEvraklar.Path, "*.*", System.IO.SearchOption.AllDirectories).Length.ToString();
            metroLabelDosyaBoyutu.Text = "Dosya Seçilmedi";
        }

        private void dirListBoxEvraklar_SelectedIndexChanged(object sender, EventArgs e)
        {
            fileListBoxEvraklar.Path = dirListBoxEvraklar.Path;
            metroLabelDosyaSayisi.Text = "Dosya Sayısı: " + System.IO.Directory.GetFiles(dirListBoxEvraklar.Path, "*.*", System.IO.SearchOption.AllDirectories).Length.ToString();
        }

        private void fileListBoxEvraklar_DoubleClick(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"evraklar\" + fileListBoxEvraklar.SelectedItem);
        }

        private void fileListBoxEvraklar_SelectedIndexChanged(object sender, EventArgs e)
        {
            int size = -1;
            string cikti = "Dosya Seçilmedi";
            try
            {
                string text = System.IO.File.ReadAllText(@"evraklar\" + fileListBoxEvraklar.SelectedItem);
                size = text.Length;
            }
            catch(System.IO.FileLoadException err)
            {
                cikti = err.Message;
            }
            finally
            {
                
                if(size != -1){
                    cikti = "Dosya Boyutu: " + (size / 1024).ToString() + "KB";
                }
                metroLabelDosyaBoyutu.Text =  cikti;
            }
            
        }
        private void metroButtonDosyaYukle_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string filePath = "";
            foreach (string characters in files)
            {
                filePath += characters;
            }
            int size = -1;
            string file = filePath;
            string fileName = System.IO.Path.GetFileName(file);
            DialogResult dialogResult = MetroFramework.MetroMessageBox.Show(this, fileName + " dosyasını yüklemek istediğinize emin misiniz?", "SÜRÜKLE BIRAK DOSYA YÜKLEME", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    string text = System.IO.File.ReadAllText(file);
                    size = text.Length;
                    System.IO.File.Copy(file, @"evraklar\" + fileName, true);
                    MetroFramework.MetroMessageBox.Show(
                        this,
                        size / 1024 + "KB boyutundaki " + fileName + " dosyası başarıyla " + System.IO.Path.GetFullPath(@"evraklar\") + " konumuna kopyalandı."
                        );
                }
                catch (System.IO.IOException)
                {
                }
                finally
                {
                    dirListBoxEvraklar_SelectedIndexChanged(dirListBoxEvraklar, new EventArgs());
                }
            }
        }

        private void metroButtonDosyaYukle_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void metroButtonDosyaYukle_Click(object sender, EventArgs e)
        {
           
            int size = -1;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = openFileDialog1.FileName;
                try
                {
                    string text = System.IO.File.ReadAllText(file);
                    size = text.Length;
                    string fileName = System.IO.Path.GetFileName(file);
                    System.IO.File.Copy(file, @"evraklar\" + fileName, true);
                    MetroFramework.MetroMessageBox.Show(
                        this,
                        size/1024 + "KB boyutundaki " + fileName + " dosyası başarıyla " + System.IO.Path.GetFullPath(@"evraklar\") + " konumuna kopyalandı."
                        );
                }
                catch (System.IO.IOException)
                {
                }
                finally
                {
                    dirListBoxEvraklar_SelectedIndexChanged(dirListBoxEvraklar, new EventArgs());
                }
            }
        }
    }
}

﻿using System;
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
using System.Runtime.InteropServices;

namespace HukukBurosu
{
    public partial class Panel : MetroForm
    {
        private String databasePath = "hukukburosu.accdb";
        public DateTime[] dates = new DateTime[30];
        bool isAdmin = false;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse
            );


        public Panel(bool isAdmin = false)
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 50, 50));
            this.isAdmin = isAdmin;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            metroTileGozetimPaneli_Click(metroTileGozetimPaneli, new EventArgs());
            anasayfaGrafik();
            metroTileGozetimPaneli_Click(metroTileGozetimPaneli, new EventArgs());
            metroTileGozetimPaneli.UseSelectable = false;
            metroTileCalisanlar.UseSelectable = false;
            metroTileEvrak.UseSelectable = false;
            metroTileMuvekkiller.UseSelectable = false;
            metroTileTebligatlar.UseSelectable = false;
            metroTileBilgi.UseSelectable = false;
            metroTileCikis.UseSelectable = false;
            dirListBoxEvraklar.Path = System.IO.Path.GetFullPath("evraklar");
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(metroButtonDosyaYukle_DragEnter);
            this.DragDrop += new DragEventHandler(metroButtonDosyaYukle_DragDrop);
            if (!isAdmin)
            {
                metroTileTebligatlar.Enabled = false;
                metroTileBilgi.Enabled = false;
            }
        }
        private DataSet veritabaniSelectForDataGrid(String tabloAdi, String kolon = "*", String where = "")
        {
            String sorguStringi = "Select " + kolon + " from " + tabloAdi;
            if (!where.Equals(""))
            {
                sorguStringi += " WHERE " + where;
            }
            OleDbConnection baglanti;
            OleDbDataAdapter sorgu;
            DataSet verikumesi = new DataSet();
            baglanti = new OleDbConnection("Provider=Microsoft.ACE.Oledb.12.0; Data Source=" + databasePath);
            sorgu = new OleDbDataAdapter(sorguStringi, baglanti);
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

        private bool veritabaniSelect(String tabloAdi, String kolon = "*", String where = "")
        {
            bool result = false;
            String sorguStringi = "Select " + kolon + " from " + tabloAdi;
            if (!where.Equals(""))
            {
                sorguStringi += " WHERE " + where;
            }
            OleDbConnection baglanti = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + databasePath);
            OleDbCommand komut = new OleDbCommand(sorguStringi, baglanti);
            baglanti.Open();
            OleDbDataReader dataReader = komut.ExecuteReader();
            if (dataReader.Read())
            {
                result = true;
                baglanti.Close();
            }
            return result;
        }

        public void veritabaniInsert(String tabloAdi, String kolonlar, String degerler)
        {
            String sorguStringi = "Insert Into " + tabloAdi + " (" + kolonlar + ") values (" + degerler + ")";
            OleDbConnection baglanti;
            baglanti = new OleDbConnection("Provider=Microsoft.ACE.Oledb.12.0; Data Source=" + databasePath);
            baglanti.Open();
            OleDbCommand komut = new OleDbCommand(sorguStringi,baglanti);
            komut.ExecuteNonQuery();
            baglanti.Close();
        }

        public void veritabaniUpdate(String tabloAdi, String[] kolonlar, String[] degerler, String whereColumn, String whereValue)
        {
            String update = "";
            for (int i = 0; i < kolonlar.Length; i++)
            {
                update += kolonlar[i] + " = '" + degerler[i] + "'";
                if (kolonlar.Length != 1 && i + 1 != kolonlar.Length)
                {
                    update += ",";
                }
            }
            String sorguStringi = "Update " + tabloAdi + " Set " + update +
                " Where " + whereColumn + " = " + whereValue + "";
            OleDbConnection baglanti;
            baglanti = new OleDbConnection("Provider=Microsoft.ACE.Oledb.12.0; Data Source=" + databasePath);
            baglanti.Open();
            OleDbCommand komut = new OleDbCommand(sorguStringi, baglanti);
            komut.ExecuteNonQuery();
            baglanti.Close();
        }

        public void veritabaniDelete(String tabloAdi, String whereColumn, String whereValue)
        {
            String sorguStringi = "Delete From " + tabloAdi + " Where " + whereColumn + "=" + whereValue;
            OleDbConnection baglanti;
            baglanti = new OleDbConnection("Provider=Microsoft.ACE.Oledb.12.0; Data Source=" + databasePath);
            baglanti.Open();
            OleDbCommand komut = new OleDbCommand(sorguStringi, baglanti);
            komut.ExecuteNonQuery();
            baglanti.Close();
        }


        private void metroGridDoldur(MetroFramework.Controls.MetroGrid metroGrid, String tabloAdi, String kolonlar = "*", String where = "")
        {
            DataSet veri = veritabaniSelectForDataGrid(tabloAdi, kolonlar, where);
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
                if (gelenPara[i] != 0)
                {
                    double doubleGelenPara = Convert.ToDouble(gelenPara[i]);
                    gelir += doubleGelenPara;
                    chartGozetimPaneli.Series["Bu Ay Tahsil Edilen Miktar"].Points.Add(doubleGelenPara);
                }

            }
            for (int i = 0; i < gidenPara.Length; i++)
            {
                if (gidenPara[i] != 0)
                {
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
            metroGridDoldur(metroGridCalisanlar, "calisanlar", "tc_kimlik, ad_soyad, calisma_sekli, maas, ise_baslama_tarihi");
            metroLabelGozetimPaneliCalisanSayisi.Text = metroGridCalisanlar.RowCount.ToString() + " Çalışan";
            metroGridDoldur(metroGridMuvekkiller, "muvekkiller", "tc_kimlik, ad, soyad, telefon, adres");
            metroLabelGozetimPaneliMuvekkilSayisi.Text = metroGridMuvekkiller.RowCount.ToString() + " Müvekkil";

        }

        private void menuSec(MetroFramework.Controls.MetroTile metroTile)
        {
            groupBoxGozetimPaneli.Visible = false;
            groupBoxCalisanlar.Visible = false;
            groupBoxEvraklar.Visible = false;
            groupBoxMuvekkiller.Visible = false;
            groupBoxTebligatlar.Visible = false;
            groupBoxBilgi.Visible = false;

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
                    groupBoxMuvekkiller.Visible = true;
                    this.Text = "Müvekkiller";
                    break;
                case "Tebligatlar":
                    if (!groupBoxUYAPPortal.Visible)
                    {
                        groupBoxTebligatlar.Visible = true;
                    }
                    this.Text = "Tebligatlar";
                    break;
                case "Bilgi":
                    groupBoxBilgi.Visible = true;
                    this.Text = "Bilgi";
                    break;
            }

        }

        private void excelAktar(MetroFramework.Controls.MetroGrid dataGridView, String ciktiDosyaAdi)
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
            workbook.SaveAs("c:\\" + ciktiDosyaAdi + ".xls", Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            app.Quit();
            MetroFramework.MetroMessageBox.Show(this, "Tablo Başarıyla Excel Dosyası Olarak Dışarı Aktarıldı");
        }



        private void metroTileGozetimPaneli_Click(object sender, EventArgs e)
        {
            menuSec(metroTileGozetimPaneli);
            metroGridDoldur(metroGridDurusmalar, "durusmalar", "muvekkil, vekil, tarih");
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
            metroGridDoldur(metroGridMuvekkiller, "muvekkiller", "tc_kimlik, ad, soyad, telefon, adres");
        }

        private void metroTileTebligatlar_Click(object sender, EventArgs e)
        {
            menuSec(metroTileTebligatlar);
        }

        private void metroTileBilgi_Click(object sender, EventArgs e)
        {
            menuSec(metroTileBilgi);
            metroGridDoldur(metroGridBilgiRootKullanicilar, "kullanicilar");
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
            excelAktar(metroGridCalisanlar, metroLabelCalisanlar.Text);
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
                string text = System.IO.File.ReadAllText(@"evraklar\" + fileListBoxEvraklar.SelectedItem.ToString());
                size = text.Length;
            }
            catch (System.IO.FileLoadException err)
            {
                cikti = err.Message;
            }
            finally
            {

                if (size != -1)
                {
                    cikti = "Dosya Boyutu: " + (size / 1024).ToString() + "KB";
                }
                metroLabelDosyaBoyutu.Text = cikti;
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

        private void metroGridMuvekkiller_SelectionChanged(object sender, EventArgs e)
        {
            if (metroGridMuvekkiller.SelectedCells.Count > 0)
            {
                metroLabelMuvekkilTC.Text = "TC: " + metroGridMuvekkiller.SelectedCells[0].Value.ToString();
                metroLabelMuvekkilAd.Text = "İsim: " + metroGridMuvekkiller.SelectedCells[1].Value.ToString();
                metroLabelMuvekkilSoyad.Text = "Soyisim: " + metroGridMuvekkiller.SelectedCells[2].Value.ToString();
                metroLabelMuvekkilTelefon.Text = "Telefon: " + metroGridMuvekkiller.SelectedCells[3].Value.ToString();
            }
        }

        private void metroButtonMuvekkillerExcelAktar_Click(object sender, EventArgs e)
        {
            excelAktar(metroGridMuvekkiller, metroLabelMuvekkiller.Text);
        }

        private void metroButtonUyapGiris_Click(object sender, EventArgs e)
        {
            String tcKimlik = metroTextBoxUyapTCKimlik.Text;
            String sifre = metroTextBoxUyapSifre.Text;
            bool uyapKullanicilar = veritabaniSelect(
                "uyap_kullanicilar",
                "tc_kimlik, sifre", "tc_kimlik = '" + tcKimlik + "' AND sifre = '" + sifre + "'");
            if (uyapKullanicilar)
            {
                MetroFramework.MetroMessageBox.Show(this, "GİRİŞ BAŞARILI");
                groupBoxUYAP.Visible = false;
                groupBoxUyapPortalTebligatDetay.Visible = true;
                metroGridDoldur(metroGridTebligatDetay, "uyap_kullanicilar", "*", "tc_kimlik = '" + metroTextBoxUyapTCKimlik.Text + "'");
                metroLabelTebligaDetaytİsim.Text = "İsim: Avukat " + metroGridTebligatDetay.Rows[0].Cells[3].Value.ToString();
                metroLabelTebligAdresi.Text = "Adres: " + metroGridTebligatDetay.Rows[0].Cells[4].Value.ToString();
                metroGridDoldur(metroGridTebligatDetay, "tebligatlar", "tebligat_no, teblig_tarihi, icerik");
                
            }
            else
            {
                MetroFramework.MetroMessageBox.Show(this, "Kullanıcı Adı veya Şifre Hatalı");
            }
        }

        private void metroGridTebligatDetay_SelectionChanged(object sender, EventArgs e)
        {
            if (metroGridTebligatDetay.SelectedCells.Count > 0)
            {
                metroLabelTebligTarihi.Text = "Tebliğ Tarihi: " + metroGridTebligatDetay.SelectedCells[1].Value.ToString().Replace("00:00:00","");
                metroTextBoxTebligatİcerik.Text = metroGridTebligatDetay.SelectedCells[2].Value.ToString();
                
            }
        }

        private void metroButtonKullaniciEkle_Click(object sender, EventArgs e)
        {
            String kullaniciAdi = metroTextBoxBilgiKullaniciAdi.Text;
            String sifre = metroTextBoxBilgiSifre.Text;
            bool admin = metroCheckBoxBilgiAdmin.Checked;
            veritabaniInsert("kullanicilar", "kullanici_adi, sifre, admin", "'" + kullaniciAdi + "','" + sifre + "'," + admin.ToString());
            metroTileBilgi_Click(metroTileBilgi, new EventArgs());
            MetroFramework.MetroMessageBox.Show(this, "Kullanıcı bilgileri başarıyla EKLENDİ");
        }

        private void metroGridBilgiRootKullanicilar_SelectionChanged(object sender, EventArgs e)
        {
            
            if (metroGridBilgiRootKullanicilar.SelectedCells.Count > 0)
            {
                metroTextBoxBilgiKullaniciAdi.Text = metroGridBilgiRootKullanicilar.SelectedCells[1].Value.ToString();
                metroTextBoxBilgiSifre.Text = metroGridBilgiRootKullanicilar.SelectedCells[2].Value.ToString();
                if (metroGridBilgiRootKullanicilar.SelectedCells[3].Value.ToString().ToLower() == "true")
                {
                    metroCheckBoxBilgiAdmin.Checked = true;
                }
                else
                {
                    metroCheckBoxBilgiAdmin.Checked = false;
                }
            }
            
        }

        private void metroButtonKullaniciGuncellestir_Click(object sender, EventArgs e)
        {
            string isAdmin = "Admin Değil";
            if(metroCheckBoxBilgiAdmin.Checked){
                isAdmin = "ADMİN";
            }
            DialogResult dialogResult = MetroFramework.MetroMessageBox.Show(this,
                metroLabelKullaniciAdi.Text + ": " + metroTextBoxBilgiKullaniciAdi.Text + ", " +
                metroLabelSifre.Text + ": " + metroTextBoxBilgiSifre.Text + ", " +
                metroLabelAdminmi.Text + ": " + isAdmin,
                metroGridBilgiRootKullanicilar.SelectedCells[1].Value.ToString() + 
                "kullanıcısını aşağıdaki bilgilerle GÜNCELLEŞTİRMEK istediğinize emin misiniz?",
                MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                String[] kolonlar = new String[2];
                kolonlar[0] = "kullanici_adi";
                kolonlar[1] = "sifre";
                String[] degerler = new String[kolonlar.Length];
                degerler[0] = metroTextBoxBilgiKullaniciAdi.Text;
                degerler[1] = metroTextBoxBilgiSifre.Text;
                veritabaniUpdate("kullanicilar", kolonlar, degerler, "Kimlik", metroGridBilgiRootKullanicilar.SelectedCells[0].Value.ToString());
                metroTileBilgi_Click(metroTileBilgi, new EventArgs());
                MetroFramework.MetroMessageBox.Show(this, "Kullanıcı bilgileri başarıyla GÜNCELLEŞTİRİLDİ");
            }

        }

        
        private void metroButtonKullaniciSil_Click(object sender, EventArgs e)
        {
            String[] degerler = new String[3];
            degerler[0] = metroTextBoxBilgiKullaniciAdi.Text;
            DialogResult dialogResult = MetroFramework.MetroMessageBox.Show(this, "",
                metroGridBilgiRootKullanicilar.SelectedCells[1].Value.ToString() +
                " kullanıcısını SİLMEK istediğinize emin misiniz?",
                MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                veritabaniDelete("kullanicilar", "Kimlik", metroGridBilgiRootKullanicilar.SelectedCells[0].Value.ToString());
                metroTileBilgi_Click(metroTileBilgi, new EventArgs());
                MetroFramework.MetroMessageBox.Show(this, "Kullanıcı bilgileri başarıyla SİLİNDİ");
            }
            

        }

        private void metroButtonDurusmalarExcelAktar_Click(object sender, EventArgs e)
        {
            excelAktar(metroGridDurusmalar, "Duruşmalar");
        }

        private void resimBacgroundDegistir(PictureBox pictureBox, bool mouseUstunde = true, String renk = "red")
        {
            if (mouseUstunde)
            {
                pictureBox.BackColor = System.Drawing.Color.FromArgb(102, 102, 102);
            }
            else
            {
                if (renk.Equals("red"))
                {
                    pictureBox.BackColor = System.Drawing.Color.FromArgb(209, 17, 65);
                }
                else
                {
                    pictureBox.BackColor = System.Drawing.Color.FromArgb(152, 251, 152);
                }
            }
        }

        private void metroButtonKullaniciEkle_MouseEnter(object sender, EventArgs e)
        {
            resimBacgroundDegistir(pictureBoxKullaniciEkle, true, "pale green");
        }

        private void metroButtonKullaniciEkle_MouseLeave(object sender, EventArgs e)
        {
            resimBacgroundDegistir(pictureBoxKullaniciEkle, false, "pale green");
        }

        private void metroButtonKullaniciSil_MouseEnter(object sender, EventArgs e)
        {
            resimBacgroundDegistir(pictureBoxKullaniciSil, true);
        }

        private void metroButtonKullaniciSil_MouseLeave(object sender, EventArgs e)
        {
            resimBacgroundDegistir(pictureBoxKullaniciSil, false);
        }

        private void metroButtonDurusmalarExcelAktar_MouseEnter(object sender, EventArgs e)
        {
            resimBacgroundDegistir(pictureBoxExcelDurusmalar, true, "pale green");
        }

        private void metroButtonDurusmalarExcelAktar_MouseLeave(object sender, EventArgs e)
        {
            resimBacgroundDegistir(pictureBoxExcelDurusmalar, false, "pale green");
        }

        private void metroButtonCalisanlarExcelAktar_MouseEnter(object sender, EventArgs e)
        {
            resimBacgroundDegistir(pictureBoxExcelCalisanlar, true, "pale green");
        }

        private void metroButtonCalisanlarExcelAktar_MouseLeave(object sender, EventArgs e)
        {
            resimBacgroundDegistir(pictureBoxExcelCalisanlar, false, "pale green");
        }

        private void metroButtonDosyaYukle_MouseEnter(object sender, EventArgs e)
        {
            resimBacgroundDegistir(pictureBoxEvraklarDosyaYukle, true, "pale green");
        }

        private void metroButtonDosyaYukle_MouseLeave(object sender, EventArgs e)
        {
            resimBacgroundDegistir(pictureBoxEvraklarDosyaYukle, false, "pale green");
        }

        private void metroButtonMuvekkillerExcelAktar_MouseEnter(object sender, EventArgs e)
        {
            resimBacgroundDegistir(pictureBoxExcelAktarMuvekkiller, true, "pale green");
        }

        private void metroButtonMuvekkillerExcelAktar_MouseLeave(object sender, EventArgs e)
        {
            resimBacgroundDegistir(pictureBoxExcelAktarMuvekkiller, false, "pale green");
        }

        private void metroButtonUyapGiris_MouseEnter(object sender, EventArgs e)
        {
            resimBacgroundDegistir(pictureBoxTebligatlarGirisYap, true);
        }

        private void metroButtonUyapGiris_MouseLeave(object sender, EventArgs e)
        {
            resimBacgroundDegistir(pictureBoxTebligatlarGirisYap, false);
        }
    }
}

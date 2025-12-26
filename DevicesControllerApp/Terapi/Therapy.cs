using DevicesControllerApp.Database;
using Npgsql;
using RehabilitationSystem.Communication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DevicesControllerApp.Terapi
{
    public partial class Therapy : UserControl
    {
        int laguageId = 0;   // Dil ID (0: TR, 1: EN, 2: AR)
        int hastaId;         // Seçilen hasta ID
        int terapi_id;       // Terapi ID
        DateTime bitis;
        DateTime baslangic;
        DeviceCommunication com;
        bool cihazBagli = false;

       

        public Therapy()
        {
            InitializeComponent(); // UserControl başlatılır
            com = DeviceCommunication.Instance; // Singleton iletişim nesnesi
            com.ErrorOccurred += Com_ErrorOccurred;

        }

        // Uygulama dilini değiştirir
        public void ChangeLanguage(string culture)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            this.Controls.Clear();   // Kontroller temizlenir
            InitializeComponent();   // Yeniden yüklenir
        }

       
        private void Therapy_Load(object sender, EventArgs e)
        {
            decimal carpan = DatabaseManager.Instance.GetLengthMultiplier(false);
            string birim = DatabaseManager.Instance.GetLengthUnitLabel();

            // 2. Başlığı Güncelle
            lblAnlikDestekBar.Text = $"Mesafe ({birim})";
            lblAnlikAyak.Text = $"Mesafe ({birim})";

        }

        private void txbArama_Enter(object sender, EventArgs e)
        {
            // Arama textbox odaklandığında
            txbArama.Text = "";
            txbArama.ForeColor = Color.Black;
        }

        private void txbArama_Leave(object sender, EventArgs e)
        {
            // Arama textbox boşsa placeholder göster
            if (txbArama.Text == "")
            {
                txbArama.Text = "🔎︎ Search";
                txbArama.ForeColor = Color.Gray;
            }
        }

        private void trackBarAgirlik_Scroll(object sender, EventArgs e)
        {
            // Ağırlık değişimi
            lblAnlikAgirlik.Text = trackBarAgirlik.Value + " kg";
        }

        private void trackBarDestekBar_Scroll(object sender, EventArgs e)
        {
            // Destek barı yüksekliği değişimi
            lblAnlikDestekBar.Text = trackBarDestekBar.Value + " cm";
        }

        private void trackBarAyakNumarasi_Scroll(object sender, EventArgs e)
        {
            // Ayak numarası değişimi
            lblAnlikAyak.Text = trackBarAyakNumarasi.Value.ToString();
        }

        private void trackBarHiz_Scroll(object sender, EventArgs e)
        {
            // Terapi hızı değişimi
            lblAnlikBilgiHiz.Text = trackBarHiz.Value + " km/h";
        }

        int seconds = 0; // Terapi süresi (saniye)

        private void timer_Tick(object sender, EventArgs e)
        {
            // Süre sayacı
            seconds++;
            int mins = seconds / 60;
            int hrs = mins / 60;
            lblTerapiSuresi.Text = string.Format("{0:D2}:{1:D2}:{2:D2}", hrs, mins % 60, seconds % 60);
        }

        // Terapi başlat
        private void btnTerapiBasla_Click(object sender, EventArgs e)
        {
            if (!cihazBagli)
            {
                //cihazBagli = com.OpenConnection("COM3", 9600);
                if (!cihazBagli)
                {
                    MessageBox.Show("Cihaz bağlantısı kurulamadı");
                    return;
                }
            }

            //com.TurnOnDevice();
            com.SetSpeed((byte)trackBarHiz.Value);

            timer.Start();
            baslangic = DateTime.Now;
            if (laguageId == 0) lblAnlikCihazDurumu.Text = "⬤ Aktif";
            else if (laguageId == 1) lblAnlikCihazDurumu.Text = "⬤ Aktive";
            else if (laguageId == 2) lblAnlikCihazDurumu.Text = "⬤ نشط";

            lblAnlikCihazDurumu.ForeColor = Color.FromArgb(29, 129, 123);
        }

        // Terapi durdur / devam
        private void btnTerapiDurdur_Click(object sender, EventArgs e)
        {
            

            // Aktif durumdaysa durdur
            if (lblAnlikCihazDurumu.Text == "⬤ Aktif" ||
                lblAnlikCihazDurumu.Text == "⬤ Aktive" ||
                lblAnlikCihazDurumu.Text == "⬤ نشط")
            {
                timer.Stop();

                if (laguageId == 0) lblAnlikCihazDurumu.Text = "⬤ Ara";
                else if (laguageId == 1) lblAnlikCihazDurumu.Text = "⬤ Paused";
                else if (laguageId == 2) lblAnlikCihazDurumu.Text = "⬤ متوقف";

                lblAnlikCihazDurumu.ForeColor = Color.Black;
            }
            // Duraklatılmışsa devam et
            else if (lblAnlikCihazDurumu.Text == "⬤ Ara" ||
                     lblAnlikCihazDurumu.Text == "⬤ Paused" ||
                     lblAnlikCihazDurumu.Text == "⬤ متوقف")
            {
                timer.Start();
                lblAnlikCihazDurumu.Text = "⬤ Aktif";
                lblAnlikCihazDurumu.ForeColor = Color.FromArgb(29, 129, 123);
            }
        }

        // Terapi bitir
        private void btnBitir_Click(object sender, EventArgs e)
        {
            //com.TurnOffDevice();
           // com.CloseConnection();
            cihazBagli = false;

            seconds = 0;
            timer.Stop();
            bitis = DateTime.Now;
            lblTerapiSuresi.Text = "00:00:00";

            if (laguageId == 0) lblAnlikCihazDurumu.Text = "⬤ Pasif";
            else if (laguageId == 1) lblAnlikCihazDurumu.Text = "⬤ Passive";
            else if (laguageId == 2) lblAnlikCihazDurumu.Text = "⬤ غير نشط";

            lblAnlikCihazDurumu.ForeColor = Color.Red;
        }

        // Acil stop
        private void btnAcilStop_Click(object sender, EventArgs e)
        {
            com.EmergencyStop();

            timer.Stop();

            if (laguageId == 0) lblAnlikCihazDurumu.Text = "⬤ Ara";
            else if (laguageId == 1) lblAnlikCihazDurumu.Text = "⬤ Paused";
            else lblAnlikCihazDurumu.Text = "⬤ متوقف";

            lblAnlikCihazDurumu.ForeColor = Color.Red;
        }

        // Karakter seçiminde tek seçim
        private void checkedListBoxKarakter_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                for (int i = 0; i < checkedListBoxKarakter.Items.Count; i++)
                    if (i != e.Index) checkedListBoxKarakter.SetItemChecked(i, false);
            }
        }

        // Ortam seçiminde tek seçim
        private void checkedListBoxOrtam_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                for (int i = 0; i < checkedListBoxOrtam.Items.Count; i++)
                    if (i != e.Index) checkedListBoxOrtam.SetItemChecked(i, false);
            }
        }

        // Arama metni değiştiğinde
        private void txbArama_TextChanged(object sender, EventArgs e)
        {
            /*if (txbArama.Text.Trim().Length < 3) return;
            SearchHasta(txbArama.Text.Trim());*/
        }

        // Türkçe seçimi
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            laguageId = 0;
            ChangeLanguage("tr-TR");
        }

        // İngilizce seçimi
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            laguageId = 1;
            ChangeLanguage("en-US");
        }

        // Hasta seçildiğinde
        private void comboBoxHasta_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxHasta.SelectedValue == null) return;
            if (comboBoxHasta.SelectedValue is DataRowView) return;

            int hastaId = Convert.ToInt32(comboBoxHasta.SelectedValue);
          //  LoadHastaDetails(hastaId);
        }

        // Yükle butonu
        private void btnYukle_Click(object sender, EventArgs e)
        {
            label21.Visible = true;
            label22.Visible = true;
            labelSoyad.Visible = true;
            labelAd.Visible = true;
            lblTerapiID.Visible = true;
            lblHastaID.Visible = true;

           // LoadHastaDetails(hastaId);
           // LoadTerapiler(hastaId);
            lblTerapiID.Text = Convert.ToString(terapi_id + 1000);
        }

  

        private void btnKaydet_Click(object sender, EventArgs e)
        {
             //SaveTherapyData(); 
        }

        // Arapça seçimi
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            laguageId = 2;
            ChangeLanguage("ar");
            lblAnlikCihazDurumu.Text = "⬤ غير نشط";
        }
        private void Com_ErrorOccurred(object sender, ErrorEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                   /// lblAnlikCihazDurumu.Text = e.Message;
                }));
            }
            else
            {
                //lblAnlikCihazDurumu.Text = e.Message;
            }
        }

    }
}

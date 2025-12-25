using DevicesControllerApp.Database;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DevicesControllerApp.Ayarlar
{
    public partial class Settings : UserControl
    {
        // MainForm'a haber vermek için bir olay (Event) tanımlıyoruz
        public event EventHandler<string> DilDegisti;

        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            // Veritabanından ayarları çekiyoruz
            DataRow row = DatabaseManager.Instance.GetGeneralSettings();

            if (row != null)
            {
                // 1. DİL AYARI (application_language)
                string dbDil = row["application_language"].ToString();
                if (dbDil == "tr" || dbDil.ToLower().Contains("türk"))
                {
                    comboBox1.SelectedIndex = 0; // Türkçe
                }
                else
                {
                    comboBox1.SelectedIndex = 1; // İngilizce
                }

                // 2. TARİH FORMATI (date_time_format)
                string dbTarih = row["date_time_format"].ToString();
                int tarihIndex = comboBox2.FindStringExact(dbTarih);
                if (tarihIndex != -1)
                    comboBox2.SelectedIndex = tarihIndex;
                else
                    comboBox2.SelectedIndex = 0;

                // 3. UZUNLUK BİRİMİ (length_unit)
                string dbUzunluk = row["length_unit"].ToString();
                AkilliSecimYap(comboBox3, dbUzunluk);

                // 4. AĞIRLIK BİRİMİ (weight_unit)
                string dbAgirlik = row["weight_unit"].ToString();
                AkilliSecimYap(comboBox4, dbAgirlik);

                // 5. TEMA (theme)
                string dbTema = row["theme"].ToString();
                if (dbTema == "Light" || dbTema == "Açık")
                    comboBox5.SelectedIndex = 0;
                else
                    comboBox5.SelectedIndex = 1; // Dark
            }
        }

        // AKILLI SEÇİM METODU
        private void AkilliSecimYap(System.Windows.Forms.ComboBox cb, string aranacakKelime)
        {
            if (string.IsNullOrEmpty(aranacakKelime)) return;

            for (int i = 0; i < cb.Items.Count; i++)
            {
                if (cb.Items[i].ToString().ToLower().Contains(aranacakKelime.ToLower()))
                {
                    cb.SelectedIndex = i;
                    return;
                }
            }
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string secilenDilKodu = "tr";
            string mainFormIcinDilIsmi = "türkçe";

            // Seçilen index'e göre dili belirle
            if (comboBox1.SelectedIndex == 1) // English
            {
                secilenDilKodu = "en";
                mainFormIcinDilIsmi = "english";
            }
            else // Türkçe
            {
                secilenDilKodu = "tr";
                mainFormIcinDilIsmi = "türkçe";
            }

            // Ayarlar sayfasındaki yazıları güncelle
            DiliGuncelle(secilenDilKodu);

            // MainForm'a haber ver
            DilDegisti?.Invoke(this, mainFormIcinDilIsmi);
        }

        public void DiliGuncelle(string lang)
        {
            if (lang == "en")
            {
                // --- İNGİLİZCE ---
                tabPage1.Text = "General Settings";
                tabPage2.Text = "Device Settings";
                tabPage3.Text = "Security Settings";
                tabPage4.Text = "Database Settings";
                tabPage5.Text = "Mobile Permissions";
                tabPage6.Text = "Email Settings";

                if (UygulamaDiliLbl != null) UygulamaDiliLbl.Text = "Application Language";
                if (TarihSaatLbl != null) TarihSaatLbl.Text = "Date/Time Format";
                if (uzunlukBirimiLbl != null) uzunlukBirimiLbl.Text = "Length Unit";
                if (AğrlıkBirimiLbl != null) AğrlıkBirimiLbl.Text = "Weight Unit";
                if (TemaLbl != null) TemaLbl.Text = "Theme";
                if (Kaydet1Lbl != null) Kaydet1Lbl.Text = "Save";

                // Diğer kontroller (kodunun devamı buraya gelecek, eski kodunda ne varsa)
                label6.Text = "Min Speed Limits";
                label8.Text = "Max Speed Limits";
                label7.Text = "Timeout (ms)";
                label9.Text = "Auto-Home";
                button2.Text = "Save";

                label10.Text = "Session Timeout";
                label11.Text = "Max Login Attempts";
                button3.Text = "Save";

                label15.Text = "Backup Folder";
                button5.Text = "Select";
                label14.Text = "Log Cleanup";
                label13.Text = "Auto Backup";
                button4.Text = "Save";

                label21.Text = "Roles";
                label28.Text = "Mobile App Permissions";
                label22.Text = "Admin";
                label23.Text = "Operator";
                label24.Text = "Service";
                label27.Text = "Start Therapy";
                label26.Text = "Stop";
                label25.Text = "Crane Control";
                label30.Text = "Foot Size";
                label29.Text = "Weight Reduction";
                button8.Text = "Save";

                label16.Text = "SMTP Server";
                label17.Text = "Sender Email";
                label18.Text = "Password";
                label19.Text = "Port";
                label20.Text = "SSL Enabled";
                button6.Text = "Test";
                button7.Text = "Save";
            }
            else
            {
                // --- TÜRKÇE ---
                tabPage1.Text = "Genel Ayarlar";
                tabPage2.Text = "Cihaz Ayarları";
                tabPage3.Text = "Güvenlik Ayarları";
                tabPage4.Text = "Veritabanı Ayarları";
                tabPage5.Text = "Mobil İzinler";
                tabPage6.Text = "E-Posta Ayarları";

                if (UygulamaDiliLbl != null) UygulamaDiliLbl.Text = "Uygulama Dili";
                if (TarihSaatLbl != null) TarihSaatLbl.Text = "Tarih/Saat";
                if (uzunlukBirimiLbl != null) uzunlukBirimiLbl.Text = "Uzunluk Birimi";
                if (AğrlıkBirimiLbl != null) AğrlıkBirimiLbl.Text = "Ağırlık Birimi";
                if (TemaLbl != null) TemaLbl.Text = "Tema";
                if (Kaydet1Lbl != null) Kaydet1Lbl.Text = "Kaydet";

                label6.Text = "Minimum Hız Limitleri";
                label8.Text = "Maksimum Hız Limitleri";
                label7.Text = "Timeout Süresi (ms)";
                label9.Text = "Otomatik Home";
                button2.Text = "Kaydet";

                label10.Text = "Oturum Timeout Süreleri";
                label11.Text = "Maksimum Hatalı Giriş";
                button3.Text = "Kaydet";

                label15.Text = "Yedekleme Klasörü";
                button5.Text = "Seç";
                label14.Text = "Log Temizleme Politikası";
                label13.Text = "Oto Yedekleme";
                button4.Text = "Kaydet";

                label21.Text = "Roller";
                label28.Text = "Mobil Uygulama İzinleri";
                label22.Text = "Admin";
                label23.Text = "Operatör";
                label24.Text = "Servis";
                label27.Text = "Terapi Başlat";
                label26.Text = "Durdur";
                label25.Text = "Vinç Kontrol";
                label30.Text = "Ayak Numarası Ayarı";
                label29.Text = "Ağırlık Azaltma";
                button8.Text = "Kaydet";

                label16.Text = "SMTP Sunucusu";
                label17.Text = "Gönderen E-Posta";
                label18.Text = "Şifre";
                label19.Text = "Port";
                label20.Text = "SSL Etkin";
                button6.Text = "Test et";
                button7.Text = "Kaydet";
            }
        }
    }
}
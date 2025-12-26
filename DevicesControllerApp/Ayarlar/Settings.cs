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
        // MainForm'a haber vermek için event
        public event EventHandler<string> DilDegisti;
        public event EventHandler<string> TemaDegisti;

        public Settings()
        {
            InitializeComponent();

            // ÖNEMLİ: Butona tıklama olayını buraya elle ekliyoruz.
            // Designer tarafında çift tıklanmamış olsa bile bu satır sayesinde çalışır.
            this.Kaydet1Lbl.Click += new System.EventHandler(this.Kaydet1Lbl_Click);
        }



        private void Settings_Load(object sender, EventArgs e)
        {
            // Veritabanından mevcut ayarları çekip kutuları dolduruyoruz
            DataRow row = DatabaseManager.Instance.GetGeneralSettings();

            if (row != null)
            {
                // 1. Dil Ayarı
                string dbDil = row["application_language"].ToString();
                if (dbDil == "tr" || dbDil.ToLower().Contains("türk"))
                    comboBox1.SelectedIndex = 0; // Türkçe
                else
                    comboBox1.SelectedIndex = 1; // İngilizce

                // 2. Tarih Formatı
                string dbTarih = row["date_time_format"].ToString();
                int tarihIndex = comboBox2.FindStringExact(dbTarih);
                comboBox2.SelectedIndex = (tarihIndex != -1) ? tarihIndex : 0;

                // 3. Uzunluk Birimi
                string dbUzunluk = row["length_unit"].ToString().ToLower(); // Küçült ki hata olmasın

                // Varsayılan: Santimetre (Index 0)
                comboBox3.SelectedIndex = 0;

                // 1. Önce MİLİMETRE kontrolü (Çünkü içinde 'metre' kelimesi de geçiyor, karışmasın diye ilk buna bakıyoruz)
                if (dbUzunluk.Contains("mili") || dbUzunluk.Contains("mm"))
                {
                    comboBox3.SelectedIndex = 2; // Milimetre (mm)
                }
                // 2. Sonra METRE kontrolü (Santi veya Mili değilse ama Metre ise)
                else if (dbUzunluk.Contains("metre") && !dbUzunluk.Contains("santi"))
                {
                    comboBox3.SelectedIndex = 1; // Metre (m)
                }
                // 3. Zaten varsayılan Santimetre idi, ama yine de kontrol edebiliriz
                else if (dbUzunluk.Contains("santi") || dbUzunluk.Contains("cm"))
                {
                    comboBox3.SelectedIndex = 0; // Santimetre (cm)
                }

                // --- Settings_Load İçine Ekle ---

                // Ağırlık Birimi Ayarı
                string dbAgirlik = row["weight_unit"].ToString().ToLower();

                // Varsayılan: Kilogram (Index 0)
                comboBox4.SelectedIndex = 0;

                if (dbAgirlik.Contains("gram") && !dbAgirlik.Contains("kilogram"))
                {
                    comboBox4.SelectedIndex = 1; // Gram (g)
                }
                else
                {
                    comboBox4.SelectedIndex = 0; // Kilogram (kg)
                }

                // 5. Tema
                string dbTema = row["theme"].ToString();
                if (dbTema == "Light" || dbTema == "Açık")
                    comboBox5.SelectedIndex = 0;
                else
                    comboBox5.SelectedIndex = 1; // Dark
            }
        }

        // --- KAYDET BUTONU TIKLANDIĞINDA ÇALIŞACAK KOD ---
        private void Kaydet1Lbl_Click(object sender, EventArgs e)
        {
            try
            {
                // Değerleri al
                string dilKodu = (comboBox1.SelectedIndex == 1) ? "en" : "tr";

                // Tema Kodu: "Dark" veya "Light"
                string temaKodu = (comboBox5.SelectedIndex == 1) ? "Dark" : "Light";

                string tarihFormat = comboBox2.Text;
                string uzunlukBirim = comboBox3.Text;
                string agirlikBirim = comboBox4.Text;

                // Veritabanını Güncelle
                bool sonuc = DatabaseManager.Instance.UpdateGeneralSettings(dilKodu, tarihFormat, uzunlukBirim, agirlikBirim, temaKodu);

                if (sonuc)
                {
                    MessageBox.Show((dilKodu == "tr") ? "Ayarlar kaydedildi!" : "Settings saved!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // --- KRİTİK NOKTA: MAIN FORM'A HABER VERİYORUZ ---
                    TemaDegisti?.Invoke(this, temaKodu);

                    // Eğer Ayarlar sayfasının rengini de anında değiştirmek istersen:
                    this.BackColor = (temaKodu == "Dark") ? Color.FromArgb(30, 30, 30) : Color.White;
                    this.ForeColor = (temaKodu == "Dark") ? Color.White : Color.Black;
                }
                else
                {
                    MessageBox.Show("Hata oluştu / Error occurred.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        // Combobox içinde kelime arayıp seçen yardımcı metod
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

            // Anlık olarak arayüz dilini değiştir
            DiliGuncelle(secilenDilKodu);

            // MainForm'u uyar
            DilDegisti?.Invoke(this, mainFormIcinDilIsmi);
        }

        public void DiliGuncelle(string lang)
        {
            if (lang == "en")
            {
                // İngilizce metinler
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

                label6.Text = "Min Speed Limits";
                label8.Text = "Max Speed Limits";
                label7.Text = "Timeout (ms)";
                label9.Text = "Auto-Home";
                button2.Text = "Save";

                label10.Text = "Session Timeout";
                label11.Text = "Max Login Attempts";
                button3.Text = "Save";
            }
            else
            {
                // Türkçe metinler
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
            }
        }

        private void Kaydet1Lbl_Click_1(object sender, EventArgs e)
        {

        }
    }
}
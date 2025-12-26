/* using DevicesControllerApp.Database;
using Npgsql;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace DevicesControllerApp.Hasta_kayit
{
    public partial class PatientRegistration : UserControl
    {
        public PatientRegistration()
        {
            InitializeComponent();
        }


        private void seeAll()
        {
            try
            {
                // DatabaseManager'dan bağlantıyı al
                var connection = DatabaseManager.Instance.GetConnection();

                string query = "SELECT * FROM hastalar ORDER BY tc_kimlik_no ASC";
                NpgsqlDataAdapter dt = new NpgsqlDataAdapter(query, connection);
                DataSet ds = new DataSet();
                dt.Fill(ds);

                dataGridView1.DataSource = ds.Tables[0];

                // DataGridView Ayarları
                dataGridView1.ColumnHeadersVisible = true;
                dataGridView1.ScrollBars = ScrollBars.Both;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri çekilirken hata oluştu: " + ex.Message);
            }
        }

        private void clearAll()
        {
            textBox_adress.Clear();
            textBox_teshis.Clear();
            textBox_teshisacikalma.Clear();
            textBox_email.Clear();
            textBox_name.Clear();
            textBox_surname.Clear();
            tc_no.Clear();
            maskedTextBox_phone.Clear();
            numericUpDown_ayak.Value = 40;
            numericUpDown_diz.Value = 1;
            numericUpDown_kalca.Value = 5;
            numericUpDown_boy.Value = 100;
            numericUpDown_kilo.Value = 50;
            radioButton_woman.Checked = false;
            radioButton_man.Checked = false;
            dateTimePicker1.Value = DateTime.Now;
            dateTimePicker_tedaviBas.Value = DateTime.Now;
        }

        private void PatientRegistration_Load(object sender, EventArgs e)
        {
            seeAll();
        }

        private void btn_Create_Click(object sender, EventArgs e)
        {
            // ÖNCE FORM KONTROLÜ
            if (KontrolEt() == false) return;

            var connection = DatabaseManager.Instance.GetConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = @"
                    INSERT INTO hastalar (
                        tc_kimlik_no, ad, soyad, dogum_tarihi, cinsiyet_id,
                        boy_cm, kilo_kg, bacak_boyu_cm, kalca_diz_boyu_cm, diz_bilek_boyu_cm,
                        ayak_numarasi, teshis, teshis_aciklamasi, rahatsizlandigi_tarih,
                        telefon, e_posta, adres, sehir_plaka_kodu, olusturulma_tarihi, son_giris_tarihi
                    )
                    VALUES (
                        @tc, @ad, @soyad, @dogum,
                        @cinsiyet,
                        @boy, @kilo, @bacak, @kalca, @diz,
                        @ayak, @teshis, @aciklama, @rahatsizlikTarihi,
                        @telefon, @mail, @adres, @sehir, @olusmatarihi, @sonGiris
                    )";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@tc", tc_no.Text);
                    cmd.Parameters.AddWithValue("@ad", textBox_name.Text);
                    cmd.Parameters.AddWithValue("@soyad", textBox_surname.Text);
                    cmd.Parameters.AddWithValue("@dogum", dateTimePicker1.Value.Date);
                    cmd.Parameters.AddWithValue("@cinsiyet", radioButton_man.Checked ? 1 : 2);
                    cmd.Parameters.AddWithValue("@boy", numericUpDown_boy.Value);
                    cmd.Parameters.AddWithValue("@kilo", numericUpDown_kilo.Value);
                    cmd.Parameters.AddWithValue("@bacak", numericUpDownbacak.Value);
                    cmd.Parameters.AddWithValue("@kalca", numericUpDown_kalca.Value);
                    cmd.Parameters.AddWithValue("@diz", numericUpDown_diz.Value);
                    cmd.Parameters.AddWithValue("@ayak", numericUpDown_ayak.Value);
                    cmd.Parameters.AddWithValue("@teshis", textBox_teshis.Text);
                    cmd.Parameters.AddWithValue("@aciklama", textBox_teshisacikalma.Text);
                    cmd.Parameters.AddWithValue("@rahatsizlikTarihi", dateTimePicker_tedaviBas.Value.Date);
                    cmd.Parameters.AddWithValue("@telefon", maskedTextBox_phone.Text);
                    cmd.Parameters.AddWithValue("@mail", textBox_email.Text);
                    cmd.Parameters.AddWithValue("@adres", textBox_adress.Text);
                    cmd.Parameters.AddWithValue("@sehir", 1);
                    cmd.Parameters.AddWithValue("@olusmatarihi", DateTime.Now);
                    cmd.Parameters.AddWithValue("@sonGiris", DateTime.Now);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Kayıt başarıyla eklendi!");
                seeAll();
                clearAll();
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                MessageBox.Show("Bu e-posta adresi veya TC zaten kayıtlı!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Beklenmeyen hata: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void button_remove_Click(object sender, EventArgs e)
        {
            if (tc_no.Text.Length != 11 || !tc_no.Text.All(char.IsDigit))
            {
                MessageBox.Show("TC Kimlik numarası 11 haneli ve sadece rakam olmalıdır!",
                                "Hatalı TC", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var connection = DatabaseManager.Instance.GetConnection();
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = "DELETE FROM hastalar WHERE tc_kimlik_no = @tc";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@tc", tc_no.Text);
                    int affected = command.ExecuteNonQuery();

                    if (affected > 0)
                        MessageBox.Show("Kayıt başarıyla silindi!");
                    else
                        MessageBox.Show("Silinecek kayıt bulunamadı.");
                }

                seeAll();
                clearAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Silme hatası: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        // Güncelleme işlemini tek bir metoda topladık
        private void PerformUpdate()
        {
            if (string.IsNullOrWhiteSpace(tc_no.Text))
            {
                MessageBox.Show("Lütfen güncellenecek hastayı seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (KontrolEt() == false) return;

            var connection = DatabaseManager.Instance.GetConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = @"
                    UPDATE hastalar SET
                        ad = @ad,
                        soyad = @soyad,
                        dogum_tarihi = @dogum,
                        cinsiyet_id = @cinsiyet,
                        boy_cm = @boy,
                        kilo_kg = @kilo,
                        bacak_boyu_cm = @bacak,
                        kalca_diz_boyu_cm = @kalca,
                        diz_bilek_boyu_cm = @diz,
                        ayak_numarasi = @ayak,
                        teshis = @teshis,
                        teshis_aciklamasi = @aciklama,
                        rahatsizlandigi_tarih = @rahatsizlikTarihi,
                        telefon = @telefon,
                        e_posta = @mail,
                        adres = @adres,
                        sehir_plaka_kodu = @sehir,
                        son_giris_tarihi = @sonGiris
                    WHERE tc_kimlik_no = @tc";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@tc", tc_no.Text);
                    cmd.Parameters.AddWithValue("@ad", textBox_name.Text);
                    cmd.Parameters.AddWithValue("@soyad", textBox_surname.Text);
                    cmd.Parameters.AddWithValue("@dogum", dateTimePicker1.Value.Date);
                    cmd.Parameters.AddWithValue("@cinsiyet", radioButton_man.Checked ? 1 : 2);
                    cmd.Parameters.AddWithValue("@boy", numericUpDown_boy.Value);
                    cmd.Parameters.AddWithValue("@kilo", numericUpDown_kilo.Value);
                    cmd.Parameters.AddWithValue("@bacak", numericUpDownbacak.Value);
                    cmd.Parameters.AddWithValue("@kalca", numericUpDown_kalca.Value);
                    cmd.Parameters.AddWithValue("@diz", numericUpDown_diz.Value);
                    cmd.Parameters.AddWithValue("@ayak", numericUpDown_ayak.Value);
                    cmd.Parameters.AddWithValue("@teshis", textBox_teshis.Text);
                    cmd.Parameters.AddWithValue("@aciklama", textBox_teshisacikalma.Text);
                    cmd.Parameters.AddWithValue("@rahatsizlikTarihi", dateTimePicker_tedaviBas.Value.Date);
                    cmd.Parameters.AddWithValue("@telefon", maskedTextBox_phone.Text);
                    cmd.Parameters.AddWithValue("@mail", textBox_email.Text);
                    cmd.Parameters.AddWithValue("@adres", textBox_adress.Text);

                    // Şehir combobox'ı null ise varsayılan 1 gönderiyoruz
                    int sehirKodu = 1;
                    if (comboBox_sehir.SelectedItem != null)
                    {
                        int.TryParse(comboBox_sehir.SelectedItem.ToString(), out sehirKodu);
                    }
                    cmd.Parameters.AddWithValue("@sehir", sehirKodu);

                    cmd.Parameters.AddWithValue("@sonGiris", DateTime.Now);

                    int affected = cmd.ExecuteNonQuery();

                    if (affected > 0)
                        MessageBox.Show("Kayıt başarıyla güncellendi!");
                    else
                        MessageBox.Show("Güncellenecek kayıt bulunamadı!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
            finally
            {
                connection.Close();
                seeAll();
                clearAll();
            }
        }

        // Eski kodda iki farklı Update butonu vardı, ikisini de PerformUpdate'e yönlendirdim
        private void button_update_Click(object sender, EventArgs e)
        {
            PerformUpdate();
        }

        private void button_update_Click_1(object sender, EventArgs e)
        {
            PerformUpdate();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // Başlığa tıklanırsa hata vermesin

            try
            {
                // Hücre indekslerinin doğruluğundan emin olmalısın. 
                // Veritabanı sütun sırası değişirse burası kayabilir.
                tc_no.Text = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                textBox_name.Text = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                textBox_surname.Text = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                dateTimePicker1.Text = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();

                string cinsiyetId = dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString();
                if (cinsiyetId == "1")
                    radioButton_man.Checked = true;
                else
                    radioButton_woman.Checked = true;

                numericUpDown_boy.Value = Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells[5].Value);
                numericUpDown_kilo.Value = Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells[6].Value);
                numericUpDownbacak.Value = Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells[7].Value);
                numericUpDown_kalca.Value = Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells[8].Value);
                numericUpDown_diz.Value = Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells[9].Value);
                numericUpDown_ayak.Value = Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells[10].Value);

                textBox_teshis.Text = dataGridView1.Rows[e.RowIndex].Cells[11].Value.ToString();
                textBox_teshisacikalma.Text = dataGridView1.Rows[e.RowIndex].Cells[12].Value.ToString();
                dateTimePicker_tedaviBas.Text = dataGridView1.Rows[e.RowIndex].Cells[13].Value.ToString();
                maskedTextBox_phone.Text = dataGridView1.Rows[e.RowIndex].Cells[14].Value.ToString();
                textBox_email.Text = dataGridView1.Rows[e.RowIndex].Cells[15].Value.ToString();
                textBox_adress.Text = dataGridView1.Rows[e.RowIndex].Cells[16].Value.ToString();
            }
            catch (Exception ex)
            {
                // Hücre dönüşüm hatası olursa program çökmesin
                MessageBox.Show("Seçim sırasında hata: " + ex.Message);
            }
        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            clearAll();
        }

        private void button_search_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tc_no.Text))
            {
                MessageBox.Show("Lütfen TC Kimlik numarası giriniz!");
                return;
            }

            var connection = DatabaseManager.Instance.GetConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = "SELECT * FROM hastalar WHERE tc_kimlik_no = @tc";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@tc", tc_no.Text);

                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("Hasta bulunamadı!");
                        // dataGridView1.DataSource = null; // Tabloyu tamamen boşaltmak yerine olduğu gibi bırakabiliriz
                    }
                    else
                    {
                        dataGridView1.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Arama hatası: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void button_lookAll_Click(object sender, EventArgs e)
        {
            seeAll();
        }

        // --- DİL VE KONTROL İŞLEMLERİ ---

        private string GetErrorMsg(string hataKodu)
        {
            string dil = comboBox1.SelectedItem?.ToString() ?? "TR";
            bool isEng = (dil == "English" || dil == "EN");

            switch (hataKodu)
            {
                case "Required":
                    return isEng ? "This field cannot be empty!" : "Bu alan boş bırakılamaz!";
                case "TC":
                    return isEng ? "ID must be 11 digits!" : "TC Kimlik 11 haneli olmalı!";
                case "Gender":
                    return isEng ? "Please select a gender!" : "Lütfen cinsiyet seçiniz!";
                case "Phone":
                    return isEng ? "Invalid phone number!" : "Geçersiz telefon numarası!";
                default:
                    return "Error";
            }
        }

        private bool KontrolEt()
        {
            bool durum = true;
            errorProvider1.Clear();

            if (string.IsNullOrWhiteSpace(tc_no.Text) || tc_no.Text.Length != 11)
            {
                errorProvider1.SetError(tc_no, GetErrorMsg("TC"));
                durum = false;
            }

            if (string.IsNullOrWhiteSpace(textBox_name.Text))
            {
                errorProvider1.SetError(textBox_name, GetErrorMsg("Required"));
                durum = false;
            }

            if (string.IsNullOrWhiteSpace(textBox_surname.Text))
            {
                errorProvider1.SetError(textBox_surname, GetErrorMsg("Required"));
                durum = false;
            }

            if (!maskedTextBox_phone.MaskCompleted)
            {
                errorProvider1.SetError(maskedTextBox_phone, GetErrorMsg("Phone"));
                durum = false;
            }

            if (radioButton_man.Checked == false && radioButton_woman.Checked == false)
            {
                errorProvider1.SetError(radioButton_woman, GetErrorMsg("Gender"));
                durum = false;
            }

            return durum;
        }

        // Dil Değiştirme Mantığını Tek Fonksiyonda Topladık
        private void ChangeLanguage()
        {
            string secilenDil = "";
            if (comboBox1.SelectedItem != null)
                secilenDil = comboBox1.SelectedItem.ToString();

            if (secilenDil == "English" || secilenDil == "EN")
            {
                // --- İNGİLİZCE ---
                CultureInfo en = new CultureInfo("en-US");
                System.Threading.Thread.CurrentThread.CurrentCulture = en;
                System.Threading.Thread.CurrentThread.CurrentUICulture = en;

                label5.Text = "Select City:";
                label4.Text = "Name:";
                label3.Text = "Surname:";
                label30.Text = "ID / Passport No:";
                label2.Text = "Birth Date:";
                label9.Text = "Gender:";
                label6.Text = "Email:";
                label8.Text = "Address:";
                label7.Text = "Phone Number:";
                label11.Text = "Height (cm):";
                label12.Text = "Weight (kg):";
                label13.Text = "Shoe Size:";
                label20.Text = "Hip-Knee Dist. (cm):";
                label21.Text = "Knee-Heel Dist. (cm):";
                label22.Text = "Diagnosis Description:";
                label23.Text = "Treatment Start:";
                label24.Text = "Diagnosis:";
                label27.Text = "Leg Length:";

                radioButton_man.Text = "Male";
                radioButton_woman.Text = "Female";

                btn_Create.Text = "Save";
                button_remove.Text = "Delete";
                button_update.Text = "Update";
                button_search.Text = "Search";
                button_clear.Text = "Clear";
                button_lookAll.Text = "List All";

                dateTimePicker1.Format = DateTimePickerFormat.Short;
                dateTimePicker_tedaviBas.Format = DateTimePickerFormat.Short;
            }
            else
            {
                // --- TÜRKÇE ---
                CultureInfo tr = new CultureInfo("tr-TR");
                System.Threading.Thread.CurrentThread.CurrentCulture = tr;
                System.Threading.Thread.CurrentThread.CurrentUICulture = tr;

                label4.Text = "Adı:";
                label5.Text = "Şehir Seç:";
                label3.Text = "Soyadı:";
                label30.Text = "Tc Kimlik Numarası:";
                label2.Text = "Doğum Tarihi:";
                label9.Text = "Cinsiyet:";
                label6.Text = "Mail:";
                label8.Text = "Adresi:";
                label7.Text = "Telefon Numarası:";
                label11.Text = "Boy(cm):";
                label12.Text = "Kilo(kg):";
                label13.Text = "Ayak Numarası(cm):";
                label20.Text = "Kalça-Diz Mesafesi(cm):";
                label21.Text = "Diz-Topuk Mesafesi(cm):";
                label22.Text = "Teşhis Açıklaması:";
                label23.Text = "Tedavi Başlangıcı:";
                label24.Text = "Teşhis:";
                label27.Text = "Bacak Boyu:";

                radioButton_man.Text = "Erkek";
                radioButton_woman.Text = "Kadın";

                btn_Create.Text = "Kaydet";
                button_remove.Text = "Sil";
                button_update.Text = "Güncelle";
                button_search.Text = "Ara";
                button_clear.Text = "Temizle";
                button_lookAll.Text = "Hepsine Bak";

                dateTimePicker1.Format = DateTimePickerFormat.Short;
                dateTimePicker_tedaviBas.Format = DateTimePickerFormat.Short;
            }
        }

        // Tasarımcıda yanlışlıkla oluşmuş 3 farklı event varsa hepsi aynı işi yapsın:
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangeLanguage();
        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            ChangeLanguage();
        }

        private void comboBox1_SelectedIndexChanged_2(object sender, EventArgs e)
        {
            ChangeLanguage();
        }

        // Boş eventler (Silinirse designer hata verebilir, içi boş kalsın)
        private void textBox_adress_TextChanged(object sender, EventArgs e) { }
        private void label8_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
    }
} */
using DevicesControllerApp.Database; // DatabaseManager'ı kullanmak için
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DevicesControllerApp.Hasta_kayit
{
    public partial class PatientRegistration : UserControl
    {
        public PatientRegistration()
        {
            InitializeComponent();
        }
        public class Sehir
        {
            public int Plaka { get; set; }
            public string Isim { get; set; }
        }

        private void SehirleriYukle()
        {
            // 81 ilin tam listesi
            string[] iller = {
        "Adana", "Adıyaman", "Afyonkarahisar", "Ağrı", "Amasya", "Ankara", "Antalya", "Artvin", "Aydın", "Balıkesir", "Bilecik", "Bingöl", "Bitlis", "Bolu", "Burdur", "Bursa", "Çanakkale", "Çankırı", "Çorum", "Denizli", "Diyarbakır", "Edirne", "Elazığ", "Erzincan", "Erzurum", "Eskişehir", "Gaziantep", "Giresun", "Gümüşhane", "Hakkari", "Hatay", "Isparta", "Mersin", "İstanbul", "İzmir", "Kars", "Kastamonu", "Kayseri", "Kırklareli", "Kırşehir", "Kocaeli", "Konya", "Kütahya", "Malatya", "Manisa", "Kahramanmaraş", "Mardin", "Muğla", "Muş", "Nevşehir", "Niğde", "Ordu", "Rize", "Sakarya", "Samsun", "Siirt", "Sinop", "Sivas", "Tekirdağ", "Tokat", "Trabzon", "Tunceli", "Şanlıurfa", "Uşak", "Van", "Yozgat", "Zonguldak", "Aksaray", "Bayburt", "Karaman", "Kırıkkale", "Batman", "Şırnak", "Bartın", "Ardahan", "Iğdır", "Yalova", "Karabük", "Kilis", "Osmaniye", "Düzce"
    };

            List<Sehir> sehirListesi = new List<Sehir>();

            for (int i = 0; i < iller.Length; i++)
            {
                // i+1 yaparak plaka kodunu (1, 2, 3...) atıyoruz
                sehirListesi.Add(new Sehir { Plaka = i + 1, Isim = iller[i] });
            }

            // ComboBox ayarları
            comboBox_sehir.DataSource = sehirListesi;
            comboBox_sehir.DisplayMember = "Isim"; // Kullanıcıya gözükecek olan
            comboBox_sehir.ValueMember = "Plaka";   // Arka planda çalışacak olan (Value)
        }


        private void PatientRegistration_Load(object sender, EventArgs e)
        {
            ListeyiYenile();
            SehirleriYukle();
            string format = DatabaseManager.Instance.GetCurrentDateFormat();

            // 2. Doğum Tarihi Seçicisine Uygula
            // (DateTimePicker nesnelerinin adlarını Designer'dan kontrol et)
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = format;

            // Varsa Kayıt Tarihi vb. diğer tarih kutularına da aynısını yap
             dateTimePicker_tedaviBas.Format = DateTimePickerFormat.Custom;
            dateTimePicker_tedaviBas.CustomFormat = format;



          

            // Birim etiketini al (m veya cm)
            string birim = DatabaseManager.Instance.GetLengthUnitLabel();

            // Label'ların sonuna birimi ekle
            // NOT: Kodundaki label isimleri farklıysa (label7, label8 vb.) onları buraya yazmalısın.
            label11.Text = $"Boy ({birim})";
            label27.Text = $"Bacak Boyu ({birim})";
            label20.Text = $"Kalça-Diz ({birim})";
            label21.Text = $"Diz-Bilek ({birim})";
            label13.Text = $"Ayak ({birim})";

            // --- BURASI KRİTİK: NUMERICUPDOWN AYARLARI ---
            if (birim == "m")
            {
                // Metre ise: Virgülden sonra 2 hane olsun (1.80 gibi)
                // Maksimum değeri küçült (3.00 metre yeterli)
                AyarlaNumeric(numericUpDown_boy, 2, 3);
                AyarlaNumeric(numericUpDownbacak, 2, 2);
                AyarlaNumeric(numericUpDown_kalca, 2, 2);
                AyarlaNumeric(numericUpDown_diz, 2, 2);
                AyarlaNumeric(numericUpDown_ayak, 2, 1);
            }
            else
            {
                // CM veya MM ise: Virgül olmasın (180 gibi)
                // Maksimum değer yüksek olsun (300 cm gibi)
                AyarlaNumeric(numericUpDown_boy, 0, 300);
                AyarlaNumeric(numericUpDownbacak, 0, 200);
                AyarlaNumeric(numericUpDown_kalca, 0, 200);
                AyarlaNumeric(numericUpDown_diz, 0, 200);
                AyarlaNumeric(numericUpDown_ayak, 0, 100);
            }

        }


        // Bu yardımcı metodu Class'ın içine bir yere ekle
        private void AyarlaNumeric(NumericUpDown nud, int decimalPlace, int maxVal)
        {
            nud.DecimalPlaces = decimalPlace; // Virgülden sonra kaç basamak?
            nud.Maximum = maxVal;             // En fazla kaç yazılabilsin?
            nud.Increment = (decimalPlace == 0) ? 1 : 0.01m; // Ok tuşuna basınca kaçar kaçar artsın?
        }


        // ===============================================
        //  BUTON İŞLEMLERİ (Sadece DatabaseManager'ı Çağırır)
        // ===============================================

        // 1. KAYDETME
        private void btn_Create_Click(object sender, EventArgs e)
        {
            if (KontrolEt() == false) return;

            // 1. Varsayılan değeri belirle (Seçim yapılmazsa diye)
            int sehirKodu = 1;

            // 2. ComboBox'tan seçilen değeri 'sehirKodu' değişkenine ata
            if (comboBox_sehir.SelectedValue != null)
            {
                // BURASI KRİTİK: 'secilenPlaka' diye yeni değişken oluşturma, 
                // doğrudan yukarıdaki 'sehirKodu' değişkenini güncelle!
                sehirKodu = (int)comboBox_sehir.SelectedValue;
            }

            decimal carpan = DatabaseManager.Instance.GetLengthMultiplier(true);

            // 2. Kullanıcının kutucuklara girdiği değerleri al ve veritabanı formatına (CM) çevir
            // (Değeri çarpana BÖLÜYORUZ. Örn: 1.80 / 0.01 = 180)
            decimal dbBoy = numericUpDown_boy.Value / carpan;
            decimal dbBacak = numericUpDownbacak.Value / carpan;
            decimal dbKalca = numericUpDown_kalca.Value / carpan;
            decimal dbDiz = numericUpDown_diz.Value / carpan;
            decimal dbAyak = numericUpDown_ayak.Value / carpan;

            // SQL YOK! Sadece parametreleri gönderiyoruz.
            // Artık sehirKodu değişkeni ComboBox'tan gelen güncel değeri taşıyor.
            bool sonuc = DatabaseManager.Instance.AddPatient(
                tc_no.Text,
                textBox_name.Text,
                textBox_surname.Text,
                dateTimePicker1.Value.Date,
                radioButton_man.Checked ? 1 : 2,
               dbBoy,
                numericUpDown_kilo.Value,
                 dbBacak,
                dbKalca,
                dbDiz,
                dbAyak,
                textBox_teshis.Text,
                textBox_teshisacikalma.Text,
                dateTimePicker_tedaviBas.Value.Date,
                maskedTextBox_phone.Text,
                textBox_email.Text,
                textBox_adress.Text,
                sehirKodu // Güncellenmiş kod buraya gidiyor
            );

            if (sonuc)
            {
                MessageBox.Show(GetMessage("SuccessAdd"), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ListeyiYenile();
                clearAll();
            }
        }

        // 2. GÜNCELLEME
        private void button_update_Click(object sender, EventArgs e)
        {
            PerformUpdate();
        }

        // (Eski buton referansı varsa diye bunu da ekledim)
        private void button_update_Click_1(object sender, EventArgs e)
        {
            PerformUpdate();
        }

        private void PerformUpdate()
        {
            if (string.IsNullOrWhiteSpace(tc_no.Text))
            {
                MessageBox.Show("Lütfen güncellenecek hastayı seçiniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (KontrolEt() == false) return;

            // Şehir kodunu ComboBox'tan alıyoruz
            int sehirKodu = 1; // Varsayılan değer
            if (comboBox_sehir.SelectedValue != null)
            {
                // ValueMember olarak "Plaka" atadığımız için SelectedValue bize direkt int döndürür
                sehirKodu = (int)comboBox_sehir.SelectedValue;
            }

            // DatabaseManager içindeki UpdatePatient metoduna güncel sehirKodu gönderiliyor
            bool sonuc = DatabaseManager.Instance.UpdatePatient(
                tc_no.Text,
                textBox_name.Text,
                textBox_surname.Text,
                dateTimePicker1.Value.Date,
                radioButton_man.Checked ? 1 : 2,
                numericUpDown_boy.Value,
                numericUpDown_kilo.Value,
                numericUpDownbacak.Value,
                numericUpDown_kalca.Value,
                numericUpDown_diz.Value,
                numericUpDown_ayak.Value,
                textBox_teshis.Text,
                textBox_teshisacikalma.Text,
                dateTimePicker_tedaviBas.Value.Date,
                maskedTextBox_phone.Text,
                textBox_email.Text,
                textBox_adress.Text,
                sehirKodu // Doğru plaka kodu buraya gidiyor
            );

            if (sonuc)
            {
                MessageBox.Show(GetMessage("SuccessUpdate"), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ListeyiYenile();
                clearAll();
            }
            else
            {
                MessageBox.Show("Güncelleme başarısız veya kayıt bulunamadı.");
            }
        }

        // 3. SİLME
        private void button_remove_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tc_no.Text))
            {

                MessageBox.Show("Tc Kimlik Numarasaını Giriniz !", "Hasta kayıt", MessageBoxButtons.OK);
                return;

            }

            if (MessageBox.Show("Silmek istiyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // SQL YOK!
                bool sonuc = DatabaseManager.Instance.DeletePatient(tc_no.Text);

                if (sonuc)
                {
                    MessageBox.Show(GetMessage("SuccessDelete"), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ListeyiYenile();
                    clearAll();
                }
                else
                {
                    MessageBox.Show("Silinecek kayıt bulunamadı.");
                }
            }
        }

        // 4. ARAMA
        private void button_search_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tc_no.Text))
            {
                MessageBox.Show("Lütfen TC giriniz.");
                return;
            }

            // SQL YOK!
            DataTable dt = DatabaseManager.Instance.SearchPatient(tc_no.Text);

            if (dt != null && dt.Rows.Count > 0)
            {
                dataGridView1.DataSource = dt;
            }
            else
            {
                MessageBox.Show("Kayıt Bulunamadı.");
                // dataGridView1.DataSource = null; // İstersen tabloyu boşaltabilirsin
            }
        }

        // 5. HEPSİNİ LİSTELE
        private void button_lookAll_Click(object sender, EventArgs e)
        {
            ListeyiYenile();
        }

        // 6. TEMİZLE
        private void button_clear_Click(object sender, EventArgs e)
        {
            clearAll();
        }

        // --- YARDIMCI METOTLAR ---

        private void ListeyiYenile()
        {
            // Veriyi DatabaseManager'dan çek
            DataTable dt = DatabaseManager.Instance.GetAllPatients();

            if (dt != null)
            {
                dataGridView1.DataSource = dt;
                dataGridView1.ColumnHeadersVisible = true;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.AllowUserToAddRows = false;
            }
        }

        private void clearAll()
        {
            textBox_adress.Clear();
            textBox_teshis.Clear();
            textBox_teshisacikalma.Clear();
            textBox_email.Clear();
            textBox_name.Clear();
            textBox_surname.Clear();
            tc_no.Clear();
            maskedTextBox_phone.Clear();
            numericUpDown_ayak.Value = 40;
            numericUpDown_diz.Value = 1;
            numericUpDown_kalca.Value = 5;
            numericUpDown_boy.Value = 100;
            numericUpDown_kilo.Value = 50;
            radioButton_woman.Checked = false;
            radioButton_man.Checked = false;
            dateTimePicker1.Value = DateTime.Now;
            dateTimePicker_tedaviBas.Value = DateTime.Now;
        }

        // --- GRID'DEN SEÇİNCE DOLDURMA ---
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            try
            {
                // Kolon sıralarına göre veriyi al (Sıra değişirse burayı güncellemen gerekir!)
                // Kolon 0: TC, 1: Ad, 2: Soyad ...
                tc_no.Text = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                textBox_name.Text = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                textBox_surname.Text = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();

                if (dataGridView1.Rows[e.RowIndex].Cells[3].Value != DBNull.Value)
                    dateTimePicker1.Value = Convert.ToDateTime(dataGridView1.Rows[e.RowIndex].Cells[3].Value);

                string cinsiyetId = dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString();
                if (cinsiyetId == "1") radioButton_man.Checked = true;
                else radioButton_woman.Checked = true;

                numericUpDown_boy.Value = Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells[5].Value);
                numericUpDown_kilo.Value = Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells[6].Value);
                numericUpDownbacak.Value = Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells[7].Value);
                numericUpDown_kalca.Value = Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells[8].Value);
                numericUpDown_diz.Value = Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells[9].Value);
                numericUpDown_ayak.Value = Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells[10].Value);

                textBox_teshis.Text = dataGridView1.Rows[e.RowIndex].Cells[11].Value.ToString();
                textBox_teshisacikalma.Text = dataGridView1.Rows[e.RowIndex].Cells[12].Value.ToString();

                if (dataGridView1.Rows[e.RowIndex].Cells[13].Value != DBNull.Value)
                    dateTimePicker_tedaviBas.Value = Convert.ToDateTime(dataGridView1.Rows[e.RowIndex].Cells[13].Value);

                maskedTextBox_phone.Text = dataGridView1.Rows[e.RowIndex].Cells[14].Value.ToString();
                textBox_email.Text = dataGridView1.Rows[e.RowIndex].Cells[15].Value.ToString();
                textBox_adress.Text = dataGridView1.Rows[e.RowIndex].Cells[16].Value.ToString();
            }
            catch (Exception ex)
            {
                // Sessizce geçebilir veya loglayabilirsin
                MessageBox.Show("Seçim Hatası: " + ex.Message);
            }
        }

        // --- VALIDASYON ---
        private bool KontrolEt()
        {
            bool durum = true;
            errorProvider1.Clear();

            if (string.IsNullOrWhiteSpace(tc_no.Text) || tc_no.Text.Length != 11)
            {
                errorProvider1.SetError(tc_no, GetErrorMsg("TC"));
                durum = false;
            }
            if (string.IsNullOrWhiteSpace(textBox_name.Text))
            {
                errorProvider1.SetError(textBox_name, GetErrorMsg("Required"));
                durum = false;
            }
            if (string.IsNullOrWhiteSpace(textBox_surname.Text))
            {
                errorProvider1.SetError(textBox_surname, GetErrorMsg("Required"));
                durum = false;
            }
            if (!maskedTextBox_phone.MaskCompleted)
            {
                errorProvider1.SetError(maskedTextBox_phone, GetErrorMsg("Phone"));
                durum = false;
            }
            if (!radioButton_man.Checked && !radioButton_woman.Checked)
            {
                errorProvider1.SetError(radioButton_woman, GetErrorMsg("Gender"));
                durum = false;
            }
            return durum;
        }

        // --- DİL YÖNETİMİ ---
        private void ChangeLanguage()
        {
            string secilenDil = comboBox1.SelectedItem?.ToString() ?? "";
            CultureInfo hedefKultur;

            if (secilenDil == "English" || secilenDil == "EN")
            {
                hedefKultur = new CultureInfo("en-US");

                label5.Text = "Select City:";
                label4.Text = "Name:";
                label3.Text = "Surname:";
                label30.Text = "ID / Passport No:";
                label2.Text = "Birth Date:";
                label9.Text = "Gender:";
                label6.Text = "Email:";
                label8.Text = "Address:";
                label7.Text = "Phone Number:";
                label11.Text = "Height (cm):";
                label12.Text = "Weight (kg):";
                label13.Text = "Shoe Size:";
                label20.Text = "Hip-Knee Dist. (cm):";
                label21.Text = "Knee-Heel Dist. (cm):";
                label22.Text = "Diagnosis Description:";
                label23.Text = "Treatment Start:";
                label24.Text = "Diagnosis:";
                label27.Text = "Leg Length:";

                radioButton_man.Text = "Male";
                radioButton_woman.Text = "Female";

                btn_Create.Text = "Save";
                button_remove.Text = "Delete";
                button_update.Text = "Update";
                button_search.Text = "Search";
                button_clear.Text = "Clear";
                button_lookAll.Text = "List All";

                dateTimePicker1.Format = DateTimePickerFormat.Short;
                dateTimePicker_tedaviBas.Format = DateTimePickerFormat.Short;
            }
            else
            {
                hedefKultur = new CultureInfo("tr-TR");

                label4.Text = "Adı:";
                label5.Text = "Şehir Seç:";
                label3.Text = "Soyadı:";
                label30.Text = "Tc Kimlik Numarası:";
                label2.Text = "Doğum Tarihi:";
                label9.Text = "Cinsiyet:";
                label6.Text = "Mail:";
                label8.Text = "Adresi:";
                label7.Text = "Telefon Numarası:";
                label11.Text = "Boy(cm):";
                label12.Text = "Kilo(kg):";
                label13.Text = "Ayak Numarası(cm):";
                label20.Text = "Kalça-Diz Mesafesi(cm):";
                label21.Text = "Diz-Topuk Mesafesi(cm):";
                label22.Text = "Teşhis Açıklaması:";
                label23.Text = "Tedavi Başlangıcı:";
                label24.Text = "Teşhis:";
                label27.Text = "Bacak Boyu:";

                radioButton_man.Text = "Erkek";
                radioButton_woman.Text = "Kadın";

                btn_Create.Text = "Kaydet";
                button_remove.Text = "Sil";
                button_update.Text = "Güncelle";
                button_search.Text = "Ara";
                button_clear.Text = "Temizle";
                button_lookAll.Text = "Hepsine Bak";

                dateTimePicker1.Format = DateTimePickerFormat.Short;
                dateTimePicker_tedaviBas.Format = DateTimePickerFormat.Short;
            }

            Thread.CurrentThread.CurrentCulture = hedefKultur;
            Thread.CurrentThread.CurrentUICulture = hedefKultur;
        }

        private string GetErrorMsg(string hataKodu)
        {
            string dil = comboBox1.SelectedItem?.ToString() ?? "TR";
            bool isEng = (dil == "English" || dil == "EN");

            switch (hataKodu)
            {
                case "Required": return isEng ? "This field cannot be empty!" : "Bu alan boş bırakılamaz!";
                case "TC": return isEng ? "ID must be 11 digits!" : "TC Kimlik 11 haneli olmalı!";
                case "Gender": return isEng ? "Please select a gender!" : "Lütfen cinsiyet seçiniz!";
                case "Phone": return isEng ? "Invalid phone number!" : "Geçersiz telefon numarası!";
                default: return "Error";
            }
        }

        // Basit bir mesaj sözlüğü
        private string GetMessage(string key)
        {
            string dil = comboBox1.SelectedItem?.ToString() ?? "TR";
            bool isEng = (dil == "English" || dil == "EN");

            switch (key)
            {
                case "SuccessAdd": return isEng ? "Patient saved successfully!" : "Kayıt başarıyla eklendi!";
                case "SuccessUpdate": return isEng ? "Patient updated successfully!" : "Kayıt başarıyla güncellendi!";
                case "SuccessDelete": return isEng ? "Patient deleted successfully!" : "Kayıt başarıyla silindi!";
                default: return "Success";
            }
        }

        // Event yönlendirmeleri
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { ChangeLanguage(); }
        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e) { ChangeLanguage(); }
        private void comboBox1_SelectedIndexChanged_2(object sender, EventArgs e) { ChangeLanguage(); }

        // Boş eventler (Designer hatası olmaması için)
        private void textBox_adress_TextChanged(object sender, EventArgs e) { }
        private void label8_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
    }
}
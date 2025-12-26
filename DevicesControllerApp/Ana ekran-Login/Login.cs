
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Newtonsoft.Json;
using DevicesControllerApp.Database;

namespace DevicesControllerApp.Ana_ekran_Login
{
    public partial class form1 : Form
    {
        // Varsayılan değerler (Veritabanı okunamazsa bunlar geçerli olur)
        int dbMaxHak = 3;
        int dbBeklemeSuresi = 10;
        public string GeciciKullaniciAdi { get; private set; }

        private Size originalFormSize;
        private Dictionary<Control, Rectangle> originalControlBounds;
        private string secilenDil = "türkçe";

        public string SecilenDil
        {
            get { return secilenDil; }
        }

        private string jsonData = @"
        {
          ""türkçe"": {
            ""kullanici_adi"": ""Kullanıcı Adı"",
            ""sifre"": ""Şifre"",
            ""giris"": ""Giriş"",
            ""cikis"": ""Çıkış"",
            ""beni_hatirla"": ""Beni Hatırla"",
            ""sifre_sifirlama2"": ""Şifremi Unuttum"",
            ""sifre_goster"": ""Şifreyi Göster"",
            ""bos_kulanici_adi"": ""Kullanıcı adınızı girmediniz."",
            ""bos_sifre"": ""Şifrenizi girmediniz."",
            ""bos_kullanici_adi_ve_sifre"": ""Kullanıcı adınızı ve şifrenizi girmediniz."",
            ""yanlis_kullanici_adi"": ""Kullanıcı adınızı yanlış girdiniz."",
            ""yanlis_sifre"": ""Şifrenizi yanlış girdiniz."",
            ""yanlis_kullanici_adi_ve_sifre"": ""Kullanıcı adınızı ve şifrenizi yanlış girdiniz."",
            ""uc_kez_hatali_giris"":""3 kez yanlış girdiniz. Lütfen 10 saniye bekleyin."",
            ""cikis_sor"": ""Uygulamadan çıkmak istiyor musunuz?"",
            ""cikis"": ""Çıkış"",
            ""sifre_sifirlama"":  ""Şifre sıfırlama işlemi burada yapılacaktır."",
            ""bilgi"": ""Bilgi"",
            ""bekleme_mesaji"": ""Lütfen {0} saniye bekleyin..."",
            ""giris_basarili"": ""Giriş başarılı"",
            ""placeholder_kullanici_adi"": ""kullanıcı adı"",
            ""placeholder_sifre"": ""şifre"",
            ""veritabani_hata"": ""Veritabanına bağlanılamadı. Lütfen daha sonra tekrar deneyin.""

          },
          ""ingilizce"": {
            ""kullanici_adi"": ""Username"",
            ""sifre"": ""Password"",
            ""giris"": ""Login"",
            ""cikis"": ""Exit"",
            ""beni_hatirla"": ""Remember Me"",
            ""sifre_sifirlama2"": ""Forgot Password"",
            ""sifre_goster"": ""Show Password"",
            ""bos_kullanici_adi"": ""You didn't enter your username."",
            ""bos_sifre"": ""You didn't enter your password."",
            ""bos_kullanici_adi_ve_sifre"": ""You didn't enter your username and password."",
            ""yanlis_kullanici_adi"": ""Wrong username."",
            ""yanlis_sifre"": ""Wrong password."",
            ""yanlis_kullanici_adi_ve_sifre"": ""Wrong username and password."",
            ""uc_kez_hatali_giris"": ""3 wrong attempts. Please wait 10 seconds."",
            ""cikis_sor"": ""Do you want to exit the application?"",
            ""cikis"": ""Exit"",
            ""sifre_sifirlama"": ""Password reset will be done here."",
            ""bilgi"": ""Information"",
            ""bekleme_mesaji"": ""Please wait {0} seconds..."",
            ""giris_basarili"": ""Login succesfull"",
            ""placeholder_kullanici_adi"": ""username"",
            ""placeholder_sifre"": ""password"",
            ""veritabani_hata"": ""Cannot connect to the database. Please try again later.""
            },
            ""arapça"": {
            ""kullanici_adi"": ""اسم المستخدم"",
            ""sifre"": ""كلمة المرور"",
            ""giris"": ""تسجيل الدخول"",
            ""cikis"": ""خروج"",
            ""beni_hatirla"": ""تذكرني"",
            ""sifre_sifirlama2"": ""نسيت كلمة المرور"",
            ""sifre_goster"": ""إظهار كلمة المرور"",
            ""bos_kullanici_adi"": ""لم تقم بإدخال اسم المستخدم."",
            ""bos_sifre"": ""لم تقم بإدخال كلمة المرور."",
            ""bos_kullanici_adi_ve_sifre"": ""لم تقم بإدخال اسم المستخدم وكلمة المرور."",
            ""yanlis_kullanici_adi"": ""اسم المستخدم غير صحيح."",
            ""yanlis_sifre"": ""كلمة المرور غير صحيحة."",
            ""yanlis_kullanici_adi_ve_sifre"": ""اسم المستخدم وكلمة المرور غير صحيحين."",
            ""uc_kez_hatali_giris"": ""ثلاث محاولات خاطئة. يرجى الانتظار 10 ثوانٍ."",
            ""cikis_sor"": ""هل تريد الخروج من التطبيق؟"",
            ""cikis"": ""خروج"",
            ""sifre_sifirlama"": ""سيتم تنفيذ إعادة تعيين كلمة المرور هنا."",
            ""bilgi"": ""معلومات"",
            ""bekleme_mesaji"": ""يرجى الانتظار {0} ثانية..."",
            ""giris_basarili"": ""تم تسجيل الدخول بنجاح"",
            ""placeholder_kullanici_adi"": ""اسم المستخدم"",
            ""placeholder_sifre"": ""كلمة المرور"",
            ""veritabani_hata"": ""تعذر الاتصال بقاعدة البيانات. يرجى المحاولة مرة أخرى لاحقًا.""
            }

         
        }";

        private Dictionary<string, Dictionary<string, string>> diller;

        string kullanici_adi; string kullanici_adi2; string sifre; string sifre2; int hatali_giris_sayisi; int kalan_sure;


        public form1()
        {
            InitializeComponent();
            originalFormSize = this.Size;
            originalControlBounds = new Dictionary<Control, Rectangle>();
            foreach (Control c in this.Controls)
            {
                originalControlBounds[c] = c.Bounds;
            }

            // Form resize eventi
            this.Resize += Form1_Resize;

            diller = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonData);

            // ComboBox dil seçenekleri ekle
            comboBox1.Items.Add("Türkçe");
            comboBox1.Items.Add("English");
            comboBox1.Items.Add("العربية");
            comboBox1.SelectedIndex = 0; // default Türkçe

            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;

            // Başlangıçta dil yükle
            YaziGuncelle("türkçe");
        }


        private void Login_Load(object sender, EventArgs e)
        {
            // --- GÜVENLİK AYARLARINI ÇEK ---
            try
            {
                DataRow row = DatabaseManager.Instance.GetSecuritySettings();
                if (row != null)
                {
                    dbMaxHak = Convert.ToInt32(row["max_login_attempts"]);
                    dbBeklemeSuresi = Convert.ToInt32(row["session_timeout_minutes"]);
                }
            }
            catch
            {
                // Hata olursa varsayılanlar (3 ve 10) kalır.
            }

            checkBox2.Checked = Properties.Settings.Default.beni_hatirla;

            // Kullanıcı adı SADECE beni hatırla açıksa yüklensin
            if (checkBox2.Checked &&
                !string.IsNullOrEmpty(Properties.Settings.Default.kullanici_adi))
            {
                textBox1.Text = Properties.Settings.Default.kullanici_adi;
                textBox1.ForeColor = Color.Black;
            }
            else
            {
                SetPlaceholder(textBox1, diller[secilenDil]["placeholder_kullanici_adi"]);
            }

            SetPlaceholder(textBox2, diller[secilenDil]["placeholder_sifre"]);
            textBox2.UseSystemPasswordChar = true;

            Image original = Properties.Resources.view;
            Image resized = new Bitmap(original, button4.Width - 4, button4.Height - 4);
            button4.Image = resized;
            button4.FlatStyle = FlatStyle.Flat;
            button4.Text = "";
        }

        private void SetPlaceholder(System.Windows.Forms.TextBox textBox, string placeholder)
        {
            textBox.Text = placeholder;
            textBox.ForeColor = Color.Gray;

            textBox.GotFocus += (s, e) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.ForeColor = Color.Black;
                }
            };

            textBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.ForeColor = Color.Gray;
                }
            };
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // string secilenDil = "türkçe";

            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    secilenDil = "türkçe";
                    break;
                case 1:
                    secilenDil = "ingilizce";
                    break;
                case 2:
                    secilenDil = "arapça";
                    break;

            }

            YaziGuncelle(secilenDil);
        }

        private void YaziGuncelle(string secilenDil)
        {
            // Arayüz öğelerini güncelle
            button1.Text = diller[secilenDil]["giris"];
            button2.Text = diller[secilenDil]["cikis"];
            checkBox2.Text = diller[secilenDil]["beni_hatirla"];
            button3.Text = diller[secilenDil]["sifre_sifirlama2"];
            SetPlaceholder(textBox1, diller[secilenDil]["placeholder_kullanici_adi"]);
            SetPlaceholder(textBox2, diller[secilenDil]["placeholder_sifre"]);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            float xRatio = (float)this.Width / originalFormSize.Width;
            float yRatio = (float)this.Height / originalFormSize.Height;

            foreach (Control c in this.Controls)
            {
                Rectangle r = originalControlBounds[c];
                c.SetBounds(
                    (int)(r.X * xRatio),
                    (int)(r.Y * yRatio),
                    (int)(r.Width * xRatio),
                    (int)(r.Height * yRatio)
                );
            }
        }

        /*private void Form1_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.kullanici_adi))
            {
                textBox1.Text = Properties.Settings.Default.kullanici_adi;
                checkBox2.Checked = true;
            }
        }*/

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void kullanici_adi_girme_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            /*if (checkBox2.Checked)
            {
                Properties.Settings.Default.kullanici_adi = textBox1.Text;
            }
            else
            {
                Properties.Settings.Default.kullanici_adi = "";
            }

            Properties.Settings.Default.Save();
            */

            DialogResult cevap = MessageBox.Show(diller[secilenDil]["cikis_sor"], diller[secilenDil]["cikis"], MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (cevap == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show(diller[secilenDil]["sifre_sifirlama"], diller[secilenDil]["bilgi"], MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            Console.WriteLine(hatali_giris_sayisi);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string kullaniciAdi = textBox1.Text;
            string password = textBox2.Text;
            
            try
            {
                DatabaseManager db = DatabaseManager.Instance;

                if (!db.OpenConnection())
                {
                    MessageBox.Show(diller[secilenDil]["veritabani_hata"], diller[secilenDil]["bilgi"], MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(kullaniciAdi) && string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show(diller[secilenDil]["bos_kullanici_adi_ve_sifre"], diller[secilenDil]["bilgi"], MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (string.IsNullOrWhiteSpace(kullaniciAdi))
                {
                    MessageBox.Show(diller[secilenDil]["bos_kullanici_adi"], diller[secilenDil]["bilgi"], MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show(diller[secilenDil]["bos_sifre"], diller[secilenDil]["bilgi"], MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (db.ValidateUserLogin(kullaniciAdi, password))
                {
                    GeciciKullaniciAdi = kullaniciAdi;

                    // ✅ Beni hatırla
                    Properties.Settings.Default.beni_hatirla = checkBox2.Checked;
                    if (checkBox2.Checked)
                        Properties.Settings.Default.kullanici_adi = kullaniciAdi;
                    else
                        Properties.Settings.Default.kullanici_adi = "";
                    Properties.Settings.Default.Save();

                    

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    hatali_giris_sayisi++;
                    MessageBox.Show(diller[secilenDil]["yanlis_kullanici_adi_ve_sifre"], diller[secilenDil]["bilgi"], MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    if (hatali_giris_sayisi >= dbMaxHak)
                    {
                        button1.Enabled = false;
                        int kalan_sure = dbBeklemeSuresi;
                        label3.Visible = true;

                        Timer timer = new Timer();
                        timer.Interval = 1000;
                        timer.Tick += (s, ev) =>
                        {
                            kalan_sure--;
                            label3.Text = string.Format(diller[secilenDil]["bekleme_mesaji"], kalan_sure);
                            if (kalan_sure <= 0)
                            {
                                timer.Stop();
                                button1.Enabled = true;
                                hatali_giris_sayisi = 0;
                                label3.Visible = false;
                            }
                        };
                        timer.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(diller[secilenDil]["veritabani_hata"], diller[secilenDil]["bilgi"], MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine(ex.ToString());
            }
        }



        private void button3_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show(diller[secilenDil]["sifre_sifirlama"], diller[secilenDil]["bilgi"], MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button2_Click_1(object sender, EventArgs e)
        {

            DialogResult cevap = MessageBox.Show(diller[secilenDil]["cikis_sor"], diller[secilenDil]["cikis"], MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (cevap == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void checkBox2_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            textBox2.UseSystemPasswordChar = !textBox2.UseSystemPasswordChar;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }
    }
}


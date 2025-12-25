using DevicesControllerApp.Ana_ekran_Login;
using DevicesControllerApp.Ayarlar;
using DevicesControllerApp.Hasta_kayit;
using DevicesControllerApp.Kullanici;
using DevicesControllerApp.Raporlama;
using DevicesControllerApp.Database;
//using DevicesControllerApp.Raporlama;
using DevicesControllerApp.Servis;
using DevicesControllerApp.Terapi;
using DevicesControllerApp.Veri_izleme;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DevicesControllerApp
{
    public partial class MainForm : Form
    {
        private string secilenDil;

        private Dictionary<string, Dictionary<string, string>> diller;

        private string jsonData = @"
{
  ""türkçe"": {
    ""terapi"": ""TERAPİ"",
    ""hasta_kayit"": ""HASTA KAYIT"",
    ""kullanici_kayit"": ""KULLANICI KAYIT"",
    ""rehabilitasyon_izleme"": ""REHABİLİTASYON İZLEME"",
    ""raporlama"": ""RAPORLAMA"",
    ""servis"": ""SERVİS"",
    ""hesabim"": ""HESABIM"",
    ""ayarlar"": ""AYARLAR"",
    ""cikis"": ""ÇIKIŞ""
  },
  ""english"": {
    ""terapi"": ""THERAPY"",
    ""hasta_kayit"": ""PATIENT REGISTRATION"",
    ""kullanici_kayit"": ""USER REGISTRATION"",
    ""rehabilitasyon_izleme"": ""REHABILITATION MONITORING"",
    ""raporlama"": ""REPORTING"",
    ""servis"": ""SERVICE"",
    ""hesabim"": ""MY ACCOUNT"",
    ""ayarlar"": ""SETTINGS"",
    ""cikis"": ""EXIT""
  },
  ""arapça"": {
    ""terapi"": ""العلاج"",
    ""hasta_kayit"": ""تسجيل المرضى"",
    ""kullanici_kayit"": ""تسجيل المستخدم"",
    ""rehabilitasyon_izleme"": ""متابعة إعادة التأهيل"",
    ""raporlama"": ""التقارير"",
    ""servis"": ""الخدمة"",
    ""hesabim"": ""حسابي"",
    ""ayarlar"": ""الإعدادات"",
    ""cikis"": ""خروج""
  }
}";

        public MainForm(string dil)
        {
            InitializeComponent();

            secilenDil = dil;

            // Label'a yaz
            labelDil.Text = secilenDil; // labelDil, formdaki dil gösterecek label

            // JSON'dan metinleri yükle
            diller = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonData);

            // Metinleri seçilen dile göre güncelle
            MetinleriGuncelle(secilenDil);
        }
        //BU satırdan itibaren veritabanından dil ayarını çekip uygulama kısmı
      
        
        public void MainFormDiliGuncelle(string dilKodu)
        {
            // Veritabanında "tr" veya "en" diye kayıtlı, ona göre işlem yapıyoruz.
            // Ayrıca Settings'ten "english" veya "türkçe" kelimesi gelirse onu da kapsayalım.

            if (dilKodu == "en" || dilKodu.ToLower() == "english")
            {
                // --- İNGİLİZCE ---
                // Yan panellerdeki butonların isimlerini buraya yazmalısın.
                // Örnek:
                // btnAnaSayfa.Text = "Home";
                // btnHastalar.Text = "Patients";
                // btnAyarlar.Text = "Settings";
                // btnCikis.Text = "Logout";

                // Eğer buton isimlerini bilmiyorsam senin için genel bir örnek:
                this.Text = "Device Controller"; // Form Başlığı
            }
            else
            {
                // --- TÜRKÇE ---
                // btnAnaSayfa.Text = "Ana Sayfa";
                // btnHastalar.Text = "Hastalar";
                // btnAyarlar.Text = "Ayarlar";
                // btnCikis.Text = "Çıkış";

                this.Text = "Cihaz Kontrolü";
            }
        }

        private void MetinleriGuncelle(string secilenDil)
        {
            btnterapi.Text = diller[secilenDil]["terapi"];
            btnhasta.Text = diller[secilenDil]["hasta_kayit"];
            btnkullanici.Text = diller[secilenDil]["kullanici_kayit"];
            btnrehabilitasyon.Text = diller[secilenDil]["rehabilitasyon_izleme"];
            btnraporlama.Text = diller[secilenDil]["raporlama"];
            btnservis.Text = diller[secilenDil]["servis"];
            btnhesabim.Text = diller[secilenDil]["hesabim"];
            btnayarlar.Text = diller[secilenDil]["ayarlar"];
            btncikis.Text = diller[secilenDil]["cikis"];
        }


        private Color btnOrijinalRenk;
        private Color btnHoverRenk;

        Size orijinalFormBoyutu;

        Point btnterapiOrijinalKonum;
        Size btnterapiOrijinalBoyut;

        Point btnhastaOrijinalKonum;
        Size btnhastaOrijinalBoyut;

        Point btnkullaniciOrijinalKonum;
        Size btnkullaniciOrijinalBoyut;

        Point btnrehabilitasyonOrijinalKonum;
        Size btnrehabilitasyonOrijinalBoyut;

        Point btnraporlamaOrijinalKonum;
        Size btnraporlamaOrijinalBoyut;

        Point btnTerapiOrijinalKonum;
        Size btnTerapiOrijinalBoyut;

        Point btnservisOrijinalKonum;
        Size btnservisOrijinalBoyut;

        Point btnhesabimOrijinalKonum;
        Size btnhesabimOrijinalBoyut;

        Point btnayarlarOrijinalKonum;
        Size btnayarlarOrijinalBoyut;

        Point btnCikisOrijinalKonum;
        Size btnCikisOrijinalBoyut;

        Point pictureBox2OrijinalKonum;
        Size pictureBox2OrijinalBoyut;

        Point pictureBox3OrijinalKonum;
        Size pictureBox3OrijinalBoyut;

        Point pictureBox4OrijinalKonum;
        Size pictureBox4OrijinalBoyut;

        Point pictureBox5OrijinalKonum;
        Size pictureBox5OrijinalBoyut;

        Point pictureBox6OrijinalKonum;
        Size pictureBox6OrijinalBoyut;

        Point pictureBox7OrijinalKonum;
        Size pictureBox7OrijinalBoyut;

        Point pictureBox8OrijinalKonum;
        Size pictureBox8OrijinalBoyut;

        Point pictureBox9OrijinalKonum;
        Size pictureBox9OrijinalBoyut;

        Point pictureBox10OrijinalKonum;
        Size pictureBox10OrijinalBoyut;





        int menuAcikGenislik = 220;
        int menuKapaliGenislik = 15;
        bool menuAcik = false;

        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
            this.Resize += AnaEkran_Resize;

        }

        private void MenuAc()
        {
            splitContainer1.SplitterDistance = menuAcikGenislik;
            menuAcik = true;
        }

        private void MenuKapat()
        {
            splitContainer1.SplitterDistance = menuKapaliGenislik;
            menuAcik = false;
        }


        private void splitContainer1_Panel1_MouseEnter(object sender, EventArgs e)
        {
            MenuAc();
        }

        private void splitContainer1_Panel1_MouseLeave(object sender, EventArgs e)
        {
            Point mousePos = splitContainer1.Panel1.PointToClient(Cursor.Position);

            if (!splitContainer1.Panel1.ClientRectangle.Contains(mousePos))
            {
                MenuKapat();
            }
        }

        private void Menu_MouseEnter(object sender, EventArgs e)
        {
            MenuAc();
        }

        private void Menu_MouseLeave(object sender, EventArgs e)
        {
            // Fare hâlâ panelin içindeyse kapatma
            Point mousePos = splitContainer1.Panel1.PointToClient(Cursor.Position);
            if (!splitContainer1.Panel1.ClientRectangle.Contains(mousePos))
            {
                MenuKapat();
            }
        }

        // ✅ Recursive metod: Panel + altındaki tüm kontroller
        private void AddMouseEvents(Control parent)
        {
            parent.MouseEnter += Menu_MouseEnter;
            parent.MouseLeave += Menu_MouseLeave;

            foreach (Control c in parent.Controls)
                AddMouseEvents(c);
        }


        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        public void SetKullaniciAdi(string kullaniciAdi)
        {
            labelKullaniciAdi.Text = kullaniciAdi;
        }


        private void MainForm_Load(object sender, EventArgs e)
        {


            System.Data.DataRow row = DatabaseManager.Instance.GetGeneralSettings();

            if (row != null)
            {
                // 2. Dil ayarını al ("en" veya "tr" gelecektir)
                string dbDil = row["application_language"].ToString();

                // 3. Dili uygula
                MainFormDiliGuncelle(dbDil);
            }







            foreach (Control c in splitContainer1.Panel1.Controls)
            {
                if (c is Button)
                {
                    c.MouseEnter += MenuButon_MouseEnter;
                    c.MouseLeave += MenuButon_MouseLeave;
                }
            }

            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.IsSplitterFixed = true;

            splitContainer1.SplitterDistance = menuKapaliGenislik;

            AddMouseEvents(splitContainer1.Panel1);

            orijinalFormBoyutu = this.Size;


            btnterapiOrijinalKonum = btnhasta.Location;
            btnterapiOrijinalBoyut = btnhasta.Size;

            btnhastaOrijinalKonum = btnhasta.Location;
            btnhastaOrijinalBoyut = btnhasta.Size;

            btnkullaniciOrijinalKonum = btnkullanici.Location;
            btnkullaniciOrijinalBoyut = btnkullanici.Size;

            btnrehabilitasyonOrijinalKonum = btnrehabilitasyon.Location;
            btnrehabilitasyonOrijinalBoyut = btnrehabilitasyon.Size;

            btnraporlamaOrijinalKonum = btnraporlama.Location;
            btnraporlamaOrijinalBoyut = btnraporlama.Size;

            btnTerapiOrijinalKonum = btnterapi.Location;
            btnTerapiOrijinalBoyut = btnterapi.Size;


            btnservisOrijinalKonum = btnservis.Location;
            btnservisOrijinalBoyut = btnservis.Size;

            btnhesabimOrijinalKonum = btnhesabim.Location;
            btnhesabimOrijinalBoyut = btnhesabim.Size;

            btnayarlarOrijinalKonum = btnayarlar.Location;
            btnayarlarOrijinalBoyut = btnayarlar.Size;

            btnCikisOrijinalKonum = btncikis.Location;
            btnCikisOrijinalBoyut = btncikis.Size;

            pictureBox2OrijinalKonum = pictureBox2.Location;
            pictureBox2OrijinalBoyut = pictureBox2.Size;

            pictureBox3OrijinalKonum = pictureBox3.Location;
            pictureBox3OrijinalBoyut = pictureBox3.Size;

            pictureBox4OrijinalKonum = pictureBox4.Location;
            pictureBox4OrijinalBoyut = pictureBox4.Size;

            pictureBox5OrijinalKonum = pictureBox5.Location;
            pictureBox5OrijinalBoyut = pictureBox5.Size;

            pictureBox6OrijinalKonum = pictureBox6.Location;
            pictureBox6OrijinalBoyut = pictureBox6.Size;

            pictureBox7OrijinalKonum = pictureBox7.Location;
            pictureBox7OrijinalBoyut = pictureBox7.Size;

            pictureBox8OrijinalKonum = pictureBox8.Location;
            pictureBox8OrijinalBoyut = pictureBox8.Size;

            pictureBox9OrijinalKonum = pictureBox9.Location;
            pictureBox9OrijinalBoyut = pictureBox9.Size;

            pictureBox10OrijinalKonum = pictureBox10.Location;
            pictureBox10OrijinalBoyut = pictureBox10.Size;


            // Menü başlangıçta kapalı
            splitContainer1.SplitterDistance = menuKapaliGenislik;

            // Panel ve tüm iç kontroller için Mouse event ekle
            AddMouseEvents(splitContainer1.Panel1);
        }

        private void AnaEkran_Resize(object sender, EventArgs e)
        {
            // FORM BÜYÜTÜLDÜYSE
            if (this.WindowState == FormWindowState.Maximized)

            {

                int a = 185;
                int b = 75;
                int c = 20;
                int d = 50;
                int f = 140;
                int g = 60;

                btnterapi.Size = new Size(a, b);
                btnterapi.Location = new Point(c, 28);

                btnhasta.Size = new Size(a, b);
                btnhasta.Location = new Point(c, 108);

                btnkullanici.Size = new Size(a, b);
                btnkullanici.Location = new Point(c, 188);

                btnrehabilitasyon.Size = new Size(a, b);
                btnrehabilitasyon.Location = new Point(c, 268);

                btnraporlama.Size = new Size(a, b);
                btnraporlama.Location = new Point(c, 348);

                btnservis.Size = new Size(a, b);
                btnservis.Location = new Point(c, 428);

                btnhesabim.Size = new Size(a, b);
                btnhesabim.Location = new Point(c, 508);

                btnayarlar.Size = new Size(a, b);
                btnayarlar.Location = new Point(c, 588);

                btncikis.Size = new Size(a, b);
                btncikis.Location = new Point(c, 668);

                pictureBox2.Size = new Size(d, g);
                pictureBox2.Location = new Point(f, 36);

                pictureBox3.Size = new Size(d, g);
                pictureBox3.Location = new Point(f, 115);

                pictureBox4.Size = new Size(50, 50);
                pictureBox4.Location = new Point(f, 198);

                pictureBox5.Size = new Size(d, g);
                pictureBox5.Location = new Point(152, 273);

                pictureBox6.Size = new Size(d, g);
                pictureBox6.Location = new Point(f, 352);

                pictureBox7.Size = new Size(d, g);
                pictureBox7.Location = new Point(f, 431);

                pictureBox8.Size = new Size(d, g);
                pictureBox8.Location = new Point(f, 514);

                pictureBox9.Size = new Size(d, g);
                pictureBox9.Location = new Point(f, 592);

                pictureBox10.Size = new Size(d, g);
                pictureBox10.Location = new Point(f, 675);




            }
            // FORM KÜÇÜLTÜLDÜYSE (ORİJİNALE DÖN)
            else
            {
                btnterapi.Size = btnterapiOrijinalBoyut;
                btnterapi.Location = btnterapiOrijinalKonum;

                btnhasta.Size = btnhastaOrijinalBoyut;
                btnhasta.Location = btnhastaOrijinalKonum;

                btnkullanici.Size = btnkullaniciOrijinalBoyut;
                btnkullanici.Location = btnkullaniciOrijinalKonum;

                btnrehabilitasyon.Size = btnrehabilitasyonOrijinalBoyut;
                btnrehabilitasyon.Location = btnrehabilitasyonOrijinalKonum;

                btnraporlama.Size = btnraporlamaOrijinalBoyut;
                btnraporlama.Location = btnraporlamaOrijinalKonum;

                btnservis.Size = btnservisOrijinalBoyut;
                btnservis.Location = btnservisOrijinalKonum;

                btnterapi.Size = btnTerapiOrijinalBoyut;
                btnterapi.Location = btnTerapiOrijinalKonum;

                btnhesabim.Size = btnhesabimOrijinalBoyut;
                btnhesabim.Location = btnhesabimOrijinalKonum;

                btnayarlar.Size = btnayarlarOrijinalBoyut;
                btnayarlar.Location = btnayarlarOrijinalKonum;

                btncikis.Size = btnCikisOrijinalBoyut;
                btncikis.Location = btnCikisOrijinalKonum;

                pictureBox2.Size = pictureBox2OrijinalBoyut;
                pictureBox2.Location = pictureBox2OrijinalKonum;

                pictureBox3.Size = pictureBox3OrijinalBoyut;
                pictureBox3.Location = pictureBox3OrijinalKonum;

                pictureBox4.Size = pictureBox4OrijinalBoyut;
                pictureBox4.Location = pictureBox4OrijinalKonum;

                pictureBox5.Size = pictureBox5OrijinalBoyut;
                pictureBox5.Location = pictureBox5OrijinalKonum;

                pictureBox6.Size = pictureBox6OrijinalBoyut;
                pictureBox6.Location = pictureBox6OrijinalKonum;

                pictureBox7.Size = pictureBox7OrijinalBoyut;
                pictureBox7.Location = pictureBox7OrijinalKonum;

                pictureBox8.Size = pictureBox8OrijinalBoyut;
                pictureBox8.Location = pictureBox8OrijinalKonum;

                pictureBox9.Size = pictureBox9OrijinalBoyut;
                pictureBox9.Location = pictureBox9OrijinalKonum;

                pictureBox10.Size = pictureBox10OrijinalBoyut;
                pictureBox10.Location = pictureBox10OrijinalKonum;
            }


        }

        private void MenuButon_MouseEnter(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            btn.BackColor = Color.FromArgb(245, 245, 245);
        }

        private void MenuButon_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            btn.BackColor = Color.Azure; // veya kendi rengin
        }



        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {

        }

        private void btnhasta_Click(object sender, EventArgs e)
        {
            PatientRegistration pr = new PatientRegistration();
            splitContainer2.Panel2.Controls.Clear();
            splitContainer2.Panel2.Controls.Add(pr);
        }

        private void btnkullanici_Click(object sender, EventArgs e)
        {
            UserRegistration us = new UserRegistration();
            splitContainer2.Panel2.Controls.Clear();
            splitContainer2.Panel2.Controls.Add(us);
        }

        private void btnterapi_Click(object sender, EventArgs e)
        {
            Therapy tr = new Therapy();
            splitContainer2.Panel2.Controls.Clear();
            tr.Dock = DockStyle.Fill;
            splitContainer2.Panel2.Controls.Add(tr);
        }

        private void btnservis_Click(object sender, EventArgs e)
        {
            Service srv = new Service();
            srv.Dock = DockStyle.Fill;
            splitContainer2.Panel2.Controls.Clear();
            splitContainer2.Panel2.Controls.Add(srv);
        }

        private void btnrehabilitasyon_Click(object sender, EventArgs e)
        {
            DataMonitoringControl dm = new DataMonitoringControl();
            dm.Dock = DockStyle.Fill;
            splitContainer2.Panel2.Controls.Clear();
            splitContainer2.Panel2.Controls.Add(dm);
        }

        private void btnraporlama_Click(object sender, EventArgs e)
        {
            Reports rp = new Reports();
            rp.Dock = DockStyle.Fill;
            splitContainer2.Panel2.Controls.Clear();
            splitContainer2.Panel2.Controls.Add(rp);
        }

        private void btnayarlar_Click(object sender, EventArgs e)
        {
            Settings st = new Settings();
            st.Dock = DockStyle.Fill;
            st.DilDegisti += St_DilDegisti;
            splitContainer2.Panel2.Controls.Clear();
            splitContainer2.Panel2.Controls.Add(st);
        }


        private void St_DilDegisti(object sender, string yeniDilIsmi)
        {
            // 1. MainForm'un kendi dil değişkenini güncelle (Hafızada kalsın)
            this.secilenDil = yeniDilIsmi;

            // 2. MainForm üzerindeki butonları (Terapi, Hasta Kayıt vb.) yeni dile çevir
            MetinleriGuncelle(yeniDilIsmi);
        }



    }
}

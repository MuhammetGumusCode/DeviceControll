
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using DevicesControllerApp.Database;


namespace DevicesControllerApp.Kullanici
{
    // Bu, yapının bu namespace içindeki tüm sınıflar tarafından erişilebilir olmasını sağlar.
    public struct CultureItem
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public CultureItem(string name, string code)
        {
            Name = name;
            Code = code;
        }
    }

        public partial class UserRegistration : UserControl
    {
        string connectionString = "Server=localhost;Port=5432;Database=lokomat;User Id=postgres;Password=1234;";
       
        // Güncelleme ve Silme için seçili kişinin ID'sini tutmamız lazım
        int seciliPersonelId = 0;
        public UserRegistration()
        {
            InitializeComponent();
            txtArama.Text = "Ara...";
            txtArama.ForeColor = Color.Gray;

            txtArama.Enter += (s, e) =>
            {
                if (txtArama.Text == "Ara...")
                {
                    txtArama.Text = "";
                    txtArama.ForeColor = Color.Black;
                }
            };

            txtArama.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtArama.Text))
                {
                    txtArama.Text = "Ara...";
                    txtArama.ForeColor = Color.Gray;
                }
            };

            IkonlariYukle();
            ListeyiYenile();
        }
        private void IkonlariYukle()
        {
            // 1. Yeni liste oluştur
            ImageList resimListesi = new ImageList();
            resimListesi.ImageSize = new Size(24, 24); // Boyut 24x24 (veya 32x32)
            resimListesi.ColorDepth = ColorDepth.Depth32Bit;

            try
            {
                // Resimlerim dosyasından çekiyoruz
                resimListesi.Images.Add("save_key", Resimlerim.kaydet);
                resimListesi.Images.Add("delete_key", Resimlerim.sil);
                resimListesi.Images.Add("update_key", Resimlerim.guncelle);
                resimListesi.Images.Add("clean_key", Resimlerim.temizle);
                resimListesi.Images.Add("add_photo_key", Resimlerim.fotograf);
            }
            catch { }

            // 2. Butonlara Ata
            if (btnKaydet != null) { btnKaydet.ImageList = resimListesi; btnKaydet.ImageKey = "save_key"; }
            if (btnSil != null) { btnSil.ImageList = resimListesi; btnSil.ImageKey = "delete_key"; }
            if (btnGuncelle != null) { btnGuncelle.ImageList = resimListesi; btnGuncelle.ImageKey = "update_key"; }
            if (btnTemizle != null) { btnTemizle.ImageList = resimListesi; btnTemizle.ImageKey = "clean_key"; }
            if (btnResimEkle != null) { btnResimEkle.ImageList = resimListesi; btnResimEkle.ImageKey = "add_photo_key"; }


            // --- ARAMA İKONU (YENİ) ---
            try
            {
                // Panel içindeki PictureBox'ın adını 'pbAramaIkonu' yaptığını varsayıyorum
                // Tasarım ekranından PictureBox'ın ismini değiştirmeyi unutma!
                pbAramaIkonu.Image = Resimlerim.arama;
                pbAramaIkonu.SizeMode = PictureBoxSizeMode.Zoom;
            }
            catch { }
            // 3. Profil Resmini de Garantiye Al
            try
            {
                // Eğer kutu boşsa veya varsayılan resim olması gerekiyorsa
                if (pictureBox1.Image == null)
                {
                    pictureBox1.Image = Resimlerim.varsayilan_profil; // (veya pp1)
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                }
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)//
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void UserRegistration_Load(object sender, EventArgs e)
        {
            LoadRoles();
            // ComboBox'a dil çiftlerini (Görünen Ad, Kültür Kodu) ekleyelim
            cmbDilSecimi.Items.Add(new CultureItem("Türkçe", "tr-TR"));
            cmbDilSecimi.Items.Add(new CultureItem("English", "en-US"));
            cmbDilSecimi.Items.Add(new CultureItem("العربية", "ar"));


            // ComboBox'ın hangi özelliği göstereceğini belirtin
            cmbDilSecimi.DisplayMember = "Name";

            // Mevcut (varsayılan) dili seçili olarak ayarla
            string currentCulture = Thread.CurrentThread.CurrentUICulture.Name;

            foreach (CultureItem item in cmbDilSecimi.Items)
            {
                if (item.Code == currentCulture)
                {
                    cmbDilSecimi.SelectedItem = item;
                    break;
                }
            }
        }
        private void LoadRoles()
        {
            try
            {
                // DatabaseManager'dan veriyi iste
                DataTable dtRoller = DatabaseManager.Instance.RolleriGetir();

                // 1. ComboBox formda var mı? (Null kontrolü)
                // 2. Veri tablosu boş gelmedi değil mi? (Veri kontrolü)
                if (cmbRol != null && dtRoller != null && dtRoller.Rows.Count > 0)
                {
                    // Veri kaynağını bağla
                    cmbRol.DataSource = dtRoller;

                    // Sütun isimlerini ayarla (Attığın resimdeki isimler: rol_adi, rol_id)
                    cmbRol.DisplayMember = "rol_adi"; // Ekranda görünen: Admin, Operatör
                    cmbRol.ValueMember = "rol_id";    // Arka plandaki ID: 1, 2
                }

                ListeyiYenile();
            }
            catch (Exception ex)
            {
                // Hata olsa bile program ÇÖKMEZ, sadece uyarı verir.
                MessageBox.Show("Roller listesi yüklenemedi. Lütfen veritabanı bağlantısını kontrol edin.\nDetay: " + ex.Message, "Yükleme Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void cmbRol_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        // YARDIMCI METOD: Listeyi Veritabanından Çek
        private void ListeyiYenile(string arama = "")
        {
            if (dgvUsers == null) return; // Tasarımda tablo yoksa hata vermesin

            DataTable dt = DatabaseManager.Instance.KullanicilariListele(arama);
            dgvUsers.DataSource = dt;

            // ID sütunlarını gizle (Kullanıcı görmesin)
            if (dgvUsers.Columns.Contains("personel_id")) dgvUsers.Columns["personel_id"].Visible = false;
            if (dgvUsers.Columns.Contains("rol_id")) dgvUsers.Columns["rol_id"].Visible = false;
        }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            errorProvider1.BlinkStyle = ErrorBlinkStyle.AlwaysBlink;
            errorProvider2.BlinkStyle = ErrorBlinkStyle.AlwaysBlink;
            errorProvider3.BlinkStyle = ErrorBlinkStyle.AlwaysBlink;

            // 1. Önceki hata ikonlarını temizle
            errorProvider1.Clear();
            errorProvider2.Clear();
            errorProvider3.Clear();
            errorProvider4.Clear();
            errorProvider5.Clear();
            errorProvider6.Clear();
            errorProvider7.Clear();
            errorProvider8.Clear();



            List<string> hatalar = new List<string>();
            Control odaklanacakYer = null;

            // --- ZORUNLU ALAN KONTROLLERİ ---

            if (string.IsNullOrWhiteSpace(txtKullaniciAdi.Text))
            {
                errorProvider4.SetError(txtKullaniciAdi, "Kullanıcı adı zorunlu!");
                hatalar.Add("- Kullanıcı adı boş bırakılamaz.");
                if (odaklanacakYer == null) odaklanacakYer = txtKullaniciAdi;
            }

            if (string.IsNullOrWhiteSpace(txtSifre.Text))
            {
                errorProvider5.SetError(txtSifre, "Şifre zorunlu!");
                hatalar.Add("- Şifre boş bırakılamaz.");
                if (odaklanacakYer == null) odaklanacakYer = txtSifre;
            }

            if (string.IsNullOrWhiteSpace(txtAd.Text))
            {
                errorProvider6.SetError(txtAd, "Ad alanı zorunlu!");
                hatalar.Add("- Ad boş bırakılamaz.");
                if (odaklanacakYer == null) odaklanacakYer = txtAd;
            }

            if (string.IsNullOrWhiteSpace(txtSoyad.Text))
            {
                errorProvider7.SetError(txtSoyad, "Soyad alanı zorunlu!");
                hatalar.Add("- Soyad boş bırakılamaz.");
                if (odaklanacakYer == null) odaklanacakYer = txtSoyad;
            }

            if (cmbRol.SelectedIndex == -1)
            {
                errorProvider8.SetError(cmbRol, "Lütfen bir rol seçin!");
                hatalar.Add("- Rol seçilmedi.");
                if (odaklanacakYer == null) odaklanacakYer = cmbRol;
            }

            // --- ÖZEL FORMAT KONTROLLERİ ---

            // TC Kimlik Kontrolü
            if (mskTC.Text.Length < 11 || !mskTC.MaskFull)
            {
                errorProvider1.SetIconAlignment(mskTC, ErrorIconAlignment.MiddleRight); // Düzeltme: txtEmail yerine mskTC olmalı
                errorProvider1.SetError(mskTC, "TC 11 hane olmalıdır!");
                hatalar.Add("- TC Numarası eksiksiz olmalıdır.");
                if (odaklanacakYer == null) odaklanacakYer = mskTC;
            }

            // E-Posta Kontrolü
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !txtEmail.Text.Contains("@") || !txtEmail.Text.Contains("."))
            {
                errorProvider2.SetIconAlignment(txtEmail, ErrorIconAlignment.MiddleRight);
                errorProvider2.SetError(txtEmail, "Geçersiz e-posta formatı!");
                hatalar.Add("- Geçerli bir e-posta giriniz.");
                if (odaklanacakYer == null) odaklanacakYer = txtEmail;
            }

            // Telefon Kontrolü
            if (!mskTelefon.MaskFull)
            {
                errorProvider3.SetIconAlignment(mskTelefon, ErrorIconAlignment.MiddleRight); // Düzeltme: txtEmail yerine mskTelefon
                errorProvider3.SetError(mskTelefon, "Telefon numarası eksik!");
                hatalar.Add("- Telefon numarasını eksiksiz giriniz.");
                if (odaklanacakYer == null) odaklanacakYer = mskTelefon;
            }

            // --- HATA VARSA İŞLEMİ DURDUR ---
            if (hatalar.Count > 0)
            {
                MessageBox.Show(string.Join("\n", hatalar), "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (odaklanacakYer != null) odaklanacakYer.Focus();
                return;
            }

            // --- FOTOĞRAF İŞLEMLERİ (YENİ EKLENEN KISIM) ---
            byte[] resimBytes = null; // Varsayılan olarak boş

            // Eğer PictureBox'ta (pictureBox1) bir resim varsa byte dizisine çevir
            // NOT: Formundaki resim kutusunun adı pictureBox1 değilse burayı değiştir!
            if (pictureBox1.Image != null)
            {
                resimBytes = ResimGonder(pictureBox1.Image);
            }

            // --- VERİTABANI KAYIT İŞLEMİ ---

            // DatabaseManager'daki fonksiyonu çağırıyoruz
            // En sona 'resimBytes' parametresini ekledik.
            string sonuc = DatabaseManager.Instance.KullaniciVePersonelEkle(
                txtAd.Text,
                txtSoyad.Text,
                mskTelefon.Text,
                txtEmail.Text,
                txtKullaniciAdi.Text,
                txtSifre.Text,
                Convert.ToInt32(cmbRol.SelectedValue),
                mskTC.Text,
                resimBytes // <--- YENİ PARAMETRE BURAYA GELDİ
            );

            // Sonucu kontrol et
            if (sonuc == "Basarili")
            {
                MessageBox.Show("Kayıt Başarıyla Tamamlandı!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Temizle();
                ListeyiYenile();
            }
            else if (sonuc.Contains("zaten kullanılıyor"))
            {
                // Önce mesajı göster
                MessageBox.Show("Bu Kullanıcı Adı veya TC Kimlik No sistemde zaten kayıtlı!", "Mükerrer Kayıt Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Sonra ilgili kutucukların yanına ünlem koy
                // TC Kutusu için (Provider 1 kullanıyordun)
                errorProvider1.SetIconAlignment(mskTC, ErrorIconAlignment.MiddleRight);
                errorProvider1.SetError(mskTC, "Bu TC Kimlik No zaten kayıtlı!");

                // Kullanıcı Adı Kutusu için (Provider 4 kullanıyordun)
                errorProvider4.SetIconAlignment(txtKullaniciAdi, ErrorIconAlignment.MiddleRight);
                errorProvider4.SetError(txtKullaniciAdi, "Bu kullanıcı adı alınmış!");
            }
            else
            {
                
                MessageBox.Show(sonuc, "Sistem Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private byte[] ResimGonder(Image resim)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                resim.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        private void btnTemizle_Click(object sender, EventArgs e) => Temizle();

        private void Temizle()
        {
            txtKullaniciAdi.Clear(); txtSifre.Clear(); txtAd.Clear();
            txtSoyad.Clear(); txtEmail.Clear(); mskTC.Clear(); mskTelefon.Clear();
            if (cmbRol.Items.Count > 0) cmbRol.SelectedIndex = 0;
            txtKullaniciAdi.Focus();
            pictureBox1.Image = Resimlerim.varsayilan_profil;
        }
    

// SHA256 ŞİFRELEME
private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void cmbDilSecimi_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            CultureItem selectedCulture = (CultureItem)cmbDilSecimi.SelectedItem;

            // Eğer zaten o dildeysek, bir şey yapmaya gerek yok
            if (selectedCulture.Code == Thread.CurrentThread.CurrentUICulture.Name)
                return;

            // Dil değiştirme metodunu çağır
            ChangeLanguage(selectedCulture.Code);
        }
        private void ChangeLanguage(string cultureCode)
        {
            // 1. Uygulama genelinde kültürü (UI kültürünü) ayarla
            CultureInfo culture = new CultureInfo(cultureCode);

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            System.ComponentModel.ComponentResourceManager resources =
                new System.ComponentModel.ComponentResourceManager(typeof(UserRegistration));

            // 2. Formun yazılarını diller arası değiştir (Bu işlem resimleri siler)
            ApplyResources(resources, this);

            
            IkonlariYukle();
        }

        // Rekürsif (özyinelemeli) yardımcı metot.
        // Kontrol üzerindeki tüm alt kontrolleri dolaşarak kaynakları uygular.
        private void ApplyResources(System.ComponentModel.ComponentResourceManager resources, Control control)
        {
            // 1. Kontrolün kendi özelliklerini (örneğin Form/UserControl'ün başlığını) uygula
            resources.ApplyResources(control, control.Name);

            // 2. Kontrolün içindeki her alt kontrolü dolaş
            foreach (Control childControl in control.Controls)
            {
                // 3. Alt kontrolün kendi özelliklerini uygula
                resources.ApplyResources(childControl, childControl.Name);

                // 4. Eğer alt kontrolün de alt kontrolleri varsa, rekürsif olarak devam et
                if (childControl.HasChildren)
                {
                    ApplyResources(resources, childControl);
                }
            }
        }

        private void mskTC_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void mskTelefon_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void btnResimEkle_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Resim Seç";
            ofd.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = Image.FromFile(ofd.FileName);
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void dgvUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Başlığa tıklandıysa işlem yapma
            if (e.RowIndex < 0) return;

            // Tıklanan satırı al
            DataGridViewRow row = dgvUsers.Rows[e.RowIndex];

            // ID'yi değişkene al (Güncelleme ve Silme için lazım olacak)
            seciliPersonelId = Convert.ToInt32(row.Cells["personel_id"].Value);

            // Kutuları doldur
            txtAd.Text = row.Cells["ad"].Value.ToString();
            txtSoyad.Text = row.Cells["soyad"].Value.ToString();
            txtKullaniciAdi.Text = row.Cells["kullanici_adi"].Value.ToString();
            txtEmail.Text = row.Cells["e_posta"].Value.ToString();
            mskTelefon.Text = row.Cells["telefon"].Value.ToString();

            // Rolü seçili hale getir
            cmbRol.SelectedValue = row.Cells["rol_id"].Value;

            // Şifre kutusunu boşalt (Güvenlik gereği eski şifre gösterilmez)
            txtSifre.Clear();
            if (row.Cells["tc_kimlik_no"].Value != DBNull.Value)
            {
                mskTC.Text = row.Cells["tc_kimlik_no"].Value.ToString();
            }
            else
            {
                mskTC.Clear(); 
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Her harf basıldığında listeyi filtrele
            ListeyiYenile(txtArama.Text.Trim());
        }

        private void btnGuncelle_Click(object sender, EventArgs e)
        {
            if (seciliPersonelId == 0)
            {
                MessageBox.Show("Lütfen güncellenecek kişiyi seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 1. Resmi Hazırla
            byte[] resimBytes = null;
            if (pictureBox1.Image != null)
            {
                // ResimGonder fonksiyonunu class içine eklemiştik, onu kullanıyoruz
                resimBytes = ResimGonder(pictureBox1.Image);
            }

            // 2. Veritabanına Gönder (SIRALAMA ARTIK DOĞRU)
            
            string sonuc = DatabaseManager.Instance.KullaniciGuncelle(
                seciliPersonelId,
                mskTC.Text,         
                txtAd.Text,
                txtSoyad.Text,
                mskTelefon.Text,
                txtEmail.Text,
                txtKullaniciAdi.Text,
                txtSifre.Text,      // Şifre boşsa değişmez
                Convert.ToInt32(cmbRol.SelectedValue),
                resimBytes          // <-- FOTOĞRAFI EKLEDİK
            );

            if (sonuc == "Basarili")
            {
                MessageBox.Show("Kayıt ve Fotoğraf Güncellendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Temizle();
                ListeyiYenile();
            }
            else
            {
                MessageBox.Show(sonuc, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSil_Click(object sender, EventArgs e)
        {
            // 1. Seçili kişi kontrolü
            if (seciliPersonelId == 0)
            {
                MessageBox.Show("Lütfen listeden silinecek kişiyi seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Onay sorusu
            DialogResult cevap = MessageBox.Show("Bu kullanıcıyı silmek (pasife almak) istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (cevap == DialogResult.Yes)
            {
                // 3. DatabaseManager çağırılıyor (Artık string dönüyor)
                string sonuc = DatabaseManager.Instance.KullaniciSil(seciliPersonelId);

                // 4. Sonuç kontrolü ("Basarili" kelimesine bakıyoruz)
                if (sonuc == "Basarili")
                {
                    MessageBox.Show("Kullanıcı silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    Temizle();       // Alanları temizle
                    ListeyiYenile(); // Listeyi güncelle

                    pictureBox1.Image = null; // Ekrandaki resmi kaldır (ÖNEMLİ)
                    seciliPersonelId = 0;     // Seçimi sıfırla
                }
                else
                {
                    // Hata varsa mesaj kutusunda hatanın detayını göster
                    MessageBox.Show(sonuc, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvUsers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }

}

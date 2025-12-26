using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Npgsql;
using System.Windows.Forms;
using System.Security.Cryptography;
using NpgsqlTypes;

namespace DevicesControllerApp.Database
{

    internal class DatabaseManager
    {
        private static DatabaseManager instance ;
        private static readonly object _lock = new object();
        private NpgsqlConnection _connection;
        private Npgsql.NpgsqlConnection conn;
        internal static DatabaseManager Instance
        {
            get
            {
                if ((instance == null))
                {
                    instance = new DatabaseManager();
                }
                return instance;
            }
        }

        public bool OpenConnection()
        {
            try
            {
                if (conn == null)
                    conn = new NpgsqlConnection("Server=localhost;Port=5432;Database=lokomat;User Id=postgres;Password=1234;");

                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                    _connection = conn;
                }


                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı bağlantı hatası: " + ex.Message);
                return false;
            }
        }

        public void CloseConnection()
        {
            if (conn != null && conn.State == ConnectionState.Open)
                conn.Close();
        }

        public bool ValidateUserLogin(string kullaniciAdi, string sifre)
        {
            try
            {
                if (conn == null || conn.State != System.Data.ConnectionState.Open)
                    conn.Open();

                // SQL sorgusu: kullanici_adi eşleşmeli
                string query = "SELECT sifre_hash FROM kullanicihesaplari WHERE kullanici_adi=@kullanici_adi";

                using (var cmd = new Npgsql.NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@kullanici_adi", kullaniciAdi);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedHash = reader.GetString(0); // sifre_hash değerini al

                            // Şifreyi doğrula
                            return VerifyPassword(sifre, storedHash);
                        }
                        else
                        {
                            // Kullanıcı bulunamadı
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
                return false;
            }
        }



        private bool VerifyPassword(string sifre, string storedHash)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(sifre);
                byte[] hashBytes = sha.ComputeHash(bytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                return hash == storedHash.ToLower();
            }
        }



        private string connectionString;
        private DatabaseManager()
        {
            connectionString = "Server=localhost;Port=5432;Database=lokomat1;User Id=postgres;Password=1234;";
        }

       
      

        public DataTable GetAllCitys()
        {
            if (conn.State == ConnectionState.Open)
            {
                string query = "SELECT * FROM sehirler";
                Npgsql.NpgsqlCommand cmd = new Npgsql.NpgsqlCommand(query, conn);
                Npgsql.NpgsqlDataAdapter da = new Npgsql.NpgsqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
            return null;
        }

        public bool HastaSil(long tc)
        {
            string query = "DELETE FROM hastalar WHERE tc_kimlik_no=11122233344";
            Npgsql.NpgsqlCommand cmd = new Npgsql.NpgsqlCommand(query, conn);
            try
            {
                int result = cmd.ExecuteNonQuery();
                if (result > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {

                return false;
            }

        }

        //Kullanıcı kayıt ve yetkilendirma başlangıç
        // -------------------------------------------------------------
        // 2. VERİTABANI İŞLEMLERİ
        // -------------------------------------------------------------


        // Rolleri Getirme
        public DataTable RolleriGetir()
        {
            DataTable dt = new DataTable(); // Veriyi taşımak için bu tabloyu oluşturmak zorundayız, bu hata değil.
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    string sql = "SELECT \"rol_id\", \"rol_adi\" FROM \"public\".\"roller\"";
                    using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, con))
                    {
                        da.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Roller çekilirken hata: " + ex.Message);
            }
            return dt;
        }

        // Personel ve Kullanıcı Ekleme
        public string KullaniciVePersonelEkle(string ad, string soyad, string telefon, string email, string kullaniciAdi, string sifre, int rolId, string tcKimlikNo, byte[] fotograf)
        {
            string hashedPassword = ComputeSha256Hash(sifre);

            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                con.Open();
                using (NpgsqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        // A. Personel Ekle
                        // DEĞİŞİKLİK 1: SQL sorgusunun VALUES kısmına @fotograf eklendi
                        string sqlPersonel = @"INSERT INTO public.personeller 
                                       (ad, soyad, telefon, e_posta, kayit_tarihi, adres, sehir_plaka_kodu, personel_tip_id, tc_kimlik_no, fotograf) 
                                       VALUES 
                                       (@ad, @soyad, @tel, @mail, CURRENT_TIMESTAMP, 'Belirtilmedi', 43, 1, @tc, @fotograf) 
                                       RETURNING personel_id;";

                        int yeniPersonelId = 0;

                        using (NpgsqlCommand cmdPersonel = new NpgsqlCommand(sqlPersonel, con, transaction))
                        {
                            cmdPersonel.Parameters.AddWithValue("@ad", ad);
                            cmdPersonel.Parameters.AddWithValue("@soyad", soyad);
                            cmdPersonel.Parameters.AddWithValue("@tel", telefon);
                            cmdPersonel.Parameters.AddWithValue("@mail", email);
                            cmdPersonel.Parameters.AddWithValue("@tc", tcKimlikNo);

                            // DEĞİŞİKLİK 2: Fotoğraf parametresi eklendi (Null kontrolü ile)
                            if (fotograf != null)
                            {
                                cmdPersonel.Parameters.AddWithValue("@fotograf", fotograf);
                            }
                            else
                            {
                                // Resim seçilmediyse veritabanına NULL olarak kaydet
                                cmdPersonel.Parameters.AddWithValue("@fotograf", DBNull.Value);
                            }

                            yeniPersonelId = (int)cmdPersonel.ExecuteScalar();
                        }

                        // B. Kullanıcı Hesabı Ekle
                        string sqlHesap = @"INSERT INTO public.kullanicihesaplari 
                                   (personel_id, rol_id, kullanici_adi, sifre_hash, aktif_mi, olusturulma_tarihi) 
                                   VALUES 
                                   (@pid, @rid, @kadi, @pass, true, CURRENT_TIMESTAMP);";

                        using (NpgsqlCommand cmdHesap = new NpgsqlCommand(sqlHesap, con, transaction))
                        {
                            cmdHesap.Parameters.AddWithValue("@pid", yeniPersonelId);
                            cmdHesap.Parameters.AddWithValue("@rid", rolId);
                            cmdHesap.Parameters.AddWithValue("@kadi", kullaniciAdi);
                            cmdHesap.Parameters.AddWithValue("@pass", hashedPassword);
                            cmdHesap.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return "Basarili";
                    }
                    catch (PostgresException ex)
                    {
                        transaction.Rollback();
                        if (ex.SqlState == "23505") return "Bu kullanıcı adı veya TC Kimlik No zaten kullanılıyor.";
                        if (ex.SqlState == "23503") return "Şehir kodu veya Rol bulunamadı.";
                        return "Veritabanı Hatası: " + ex.Message;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return "Genel Hata: " + ex.Message;
                    }
                }
            }
        }

        // Kullanıcıları Listeleme
        public DataTable KullanicilariListele(string aramaKelimesi = "")
        {
            DataTable dt = new DataTable();
            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    string sql = @"SELECT 
                                    p.personel_id,
                                    p.ad,
                                    p.soyad,
                                    k.kullanici_adi,
                                    r.rol_adi,
                                    p.tc_kimlik_no,
                                    p.telefon,
                                    p.e_posta,
                                    k.rol_id 
                                FROM public.kullanicihesaplari k
                                JOIN public.personeller p ON k.personel_id = p.personel_id
                                JOIN public.roller r ON k.rol_id = r.rol_id
                                WHERE k.aktif_mi = true";

                    if (!string.IsNullOrEmpty(aramaKelimesi))
                    {
                        sql += " AND (p.ad ILIKE @ara OR p.soyad ILIKE @ara OR k.kullanici_adi ILIKE @ara)";
                    }

                    sql += " ORDER BY k.olusturulma_tarihi DESC";

                    using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, con))
                    {
                        if (!string.IsNullOrEmpty(aramaKelimesi))
                        {
                            da.SelectCommand.Parameters.AddWithValue("@ara", "%" + aramaKelimesi + "%");
                        }
                        da.Fill(dt);
                    }
                }
                catch (Exception) { }
            }
            return dt;
        }

        // Kullanıcı Silme
        public string KullaniciSil(int personelId)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                try
                {
                    con.Open();


                    string sql = "UPDATE public.kullanicihesaplari SET aktif_mi = false WHERE personel_id = @id";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@id", personelId);

                        int etkilenenSayi = cmd.ExecuteNonQuery();

                        if (etkilenenSayi > 0)
                        {
                            return "Basarili";
                        }
                        else
                        {
                            return "Silinecek (pasif yapılacak) kullanıcı hesabı bulunamadı.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    return "İşlem Hatası: " + ex.Message;
                }
            }
        }

        // Kullanıcı Güncelleme
        public string KullaniciGuncelle(int personelId, string tcKimlikNo, string ad, string soyad, string telefon, string email, string kullaniciAdi, string yeniSifre, int rolId, byte[] fotograf)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                con.Open();
                using (NpgsqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        // 1. Personel Bilgilerini ve Fotoğrafı Güncelle
                        string sqlPersonel = @"UPDATE public.personeller 
                                       SET ad=@ad, 
                                           soyad=@soyad, 
                                           telefon=@tel, 
                                           e_posta=@mail, 
                                           tc_kimlik_no=@tc,
                                           fotograf=@fotograf 
                                       WHERE personel_id=@id";

                        using (NpgsqlCommand cmdP = new NpgsqlCommand(sqlPersonel, con, transaction))
                        {
                            cmdP.Parameters.AddWithValue("@id", personelId);
                            cmdP.Parameters.AddWithValue("@ad", ad);
                            cmdP.Parameters.AddWithValue("@soyad", soyad);
                            cmdP.Parameters.AddWithValue("@tel", telefon);
                            cmdP.Parameters.AddWithValue("@mail", email);
                            cmdP.Parameters.AddWithValue("@tc", tcKimlikNo);

                            // FOTOĞRAF KONTROLÜ
                            if (fotograf != null)
                            {
                                cmdP.Parameters.AddWithValue("@fotograf", fotograf);
                            }
                            else
                            {
                                // Eğer resim yoksa NULL bas (veya eski resmi korumak istersen bu kısmı mantığına göre değiştirmen gerekir ama şimdilik null yapalım)
                                cmdP.Parameters.AddWithValue("@fotograf", DBNull.Value);
                            }

                            cmdP.ExecuteNonQuery();
                        }

                        // 2. Kullanıcı Hesap Bilgilerini Güncelle
                        string sqlHesap = "UPDATE public.kullanicihesaplari SET rol_id=@rid, kullanici_adi=@kadi";

                        // Şifre alanı doluysa sorguya şifre kısmını da ekle
                        if (!string.IsNullOrEmpty(yeniSifre))
                        {
                            sqlHesap += ", sifre_hash=@pass";
                        }
                        sqlHesap += " WHERE personel_id=@id";

                        using (NpgsqlCommand cmdH = new NpgsqlCommand(sqlHesap, con, transaction))
                        {
                            cmdH.Parameters.AddWithValue("@id", personelId);
                            cmdH.Parameters.AddWithValue("@rid", rolId);
                            cmdH.Parameters.AddWithValue("@kadi", kullaniciAdi);

                            if (!string.IsNullOrEmpty(yeniSifre))
                            {
                                cmdH.Parameters.AddWithValue("@pass", ComputeSha256Hash(yeniSifre));
                            }
                            cmdH.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return "Basarili";
                    }
                    catch (PostgresException ex)
                    {
                        transaction.Rollback();
                        if (ex.SqlState == "23505") return "Bu kullanıcı adı veya TC zaten kullanılıyor.";
                        return "Hata: " + ex.Message;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return "Genel Hata: " + ex.Message;
                    }
                }
            }
        }
        // Şifreleme Metodu
        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }
        //Kullanıcı kayıt ve yetkilendirme son
        //************Hasta kayıt işlemleri başlangıç**************
         // --- 1. HASTA LİSTELEME ---
        public DataTable GetAllPatients()
        {
            DataTable dt = new DataTable();
            try
            {
                if (OpenConnection())
                {
                    string query = "SELECT * FROM hastalar ORDER BY tc_kimlik_no ASC";
                    using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, _connection))
                    {
                        da.Fill(dt);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Listeleme Hatası: " + ex.Message); }
            finally { CloseConnection(); }
            return dt;
        }

        // --- 2. HASTA EKLEME ---
        public bool AddPatient(string tcNo, string firstName, string lastName, DateTime birthDate,
            int genderId, decimal height, decimal weight, decimal legLength, decimal hipKnee, decimal kneeHeel, decimal footSize,
            string diagnosis, string diagnosisDesc, DateTime treatmentDate, string phone, string email, string address, int cityCode)
        {
            try
            {
                if (OpenConnection())
                {
                    string query = @"INSERT INTO hastalar (
                                    tc_kimlik_no, ad, soyad, dogum_tarihi, cinsiyet_id,
                                    boy_cm, kilo_kg, bacak_boyu_cm, kalca_diz_boyu_cm, diz_bilek_boyu_cm,
                                    ayak_numarasi, teshis, teshis_aciklamasi, rahatsizlandigi_tarih,
                                    telefon, e_posta, adres, sehir_plaka_kodu, olusturulma_tarihi, son_giris_tarihi
                                ) VALUES (
                                    @tc, @ad, @soyad, @dogum, @cinsiyet,
                                    @boy, @kilo, @bacak, @kalca, @diz,
                                    @ayak, @teshis, @aciklama, @tedaviBas,
                                    @tel, @mail, @adres, @sehir, @now, @now)";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, _connection))
                    {
                        cmd.Parameters.AddWithValue("@tc", tcNo);
                        cmd.Parameters.AddWithValue("@ad", firstName);
                        cmd.Parameters.AddWithValue("@soyad", lastName);
                        cmd.Parameters.AddWithValue("@dogum", birthDate);
                        cmd.Parameters.AddWithValue("@cinsiyet", genderId);
                        cmd.Parameters.AddWithValue("@boy", height);
                        cmd.Parameters.AddWithValue("@kilo", weight);
                        cmd.Parameters.AddWithValue("@bacak", legLength);
                        cmd.Parameters.AddWithValue("@kalca", hipKnee);
                        cmd.Parameters.AddWithValue("@diz", kneeHeel);
                        cmd.Parameters.AddWithValue("@ayak", footSize);
                        cmd.Parameters.AddWithValue("@teshis", diagnosis);
                        cmd.Parameters.AddWithValue("@aciklama", diagnosisDesc);
                        cmd.Parameters.AddWithValue("@tedaviBas", treatmentDate);
                        cmd.Parameters.AddWithValue("@tel", phone);
                        cmd.Parameters.AddWithValue("@mail", email);
                        cmd.Parameters.AddWithValue("@adres", address);
                        cmd.Parameters.AddWithValue("@sehir", cityCode);
                        cmd.Parameters.AddWithValue("@now", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                    return true;
                }
                return false;
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                MessageBox.Show("Bu TC veya E-posta zaten kayıtlı!");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ekleme Hatası: " + ex.Message);
                return false;
            }
            finally { CloseConnection(); }
        }

        // --- 3. HASTA GÜNCELLEME ---
        public bool UpdatePatient(string tcNo, string firstName, string lastName, DateTime birthDate,
            int genderId, decimal height, decimal weight, decimal legLength, decimal hipKnee, decimal kneeHeel, decimal footSize,
            string diagnosis, string diagnosisDesc, DateTime treatmentDate, string phone, string email, string address, int cityCode)
        {
            try
            {
                if (OpenConnection())
                {
                    string query = @"UPDATE hastalar SET
                                    ad=@ad, soyad=@soyad, dogum_tarihi=@dogum, cinsiyet_id=@cinsiyet,
                                    boy_cm=@boy, kilo_kg=@kilo, bacak_boyu_cm=@bacak, kalca_diz_boyu_cm=@kalca,
                                    diz_bilek_boyu_cm=@diz, ayak_numarasi=@ayak, teshis=@teshis, 
                                    teshis_aciklamasi=@aciklama, rahatsizlandigi_tarih=@tedaviBas,
                                    telefon=@tel, e_posta=@mail, adres=@adres, sehir_plaka_kodu=@sehir, 
                                    son_giris_tarihi=@now
                                WHERE tc_kimlik_no=@tc";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, _connection))
                    {
                        // Parametreler Ekleme ile aynı, sadece güncelleme için tekrar tanımlıyoruz
                        cmd.Parameters.AddWithValue("@tc", tcNo);
                        cmd.Parameters.AddWithValue("@ad", firstName);
                        cmd.Parameters.AddWithValue("@soyad", lastName);
                        cmd.Parameters.AddWithValue("@dogum", birthDate);
                        cmd.Parameters.AddWithValue("@cinsiyet", genderId);
                        cmd.Parameters.AddWithValue("@boy", height);
                        cmd.Parameters.AddWithValue("@kilo", weight);
                        cmd.Parameters.AddWithValue("@bacak", legLength);
                        cmd.Parameters.AddWithValue("@kalca", hipKnee);
                        cmd.Parameters.AddWithValue("@diz", kneeHeel);
                        cmd.Parameters.AddWithValue("@ayak", footSize);
                        cmd.Parameters.AddWithValue("@teshis", diagnosis);
                        cmd.Parameters.AddWithValue("@aciklama", diagnosisDesc);
                        cmd.Parameters.AddWithValue("@tedaviBas", treatmentDate);
                        cmd.Parameters.AddWithValue("@tel", phone);
                        cmd.Parameters.AddWithValue("@mail", email);
                        cmd.Parameters.AddWithValue("@adres", address);
                        cmd.Parameters.AddWithValue("@sehir", cityCode);
                        cmd.Parameters.AddWithValue("@now", DateTime.Now);

                        int affected = cmd.ExecuteNonQuery();
                        return affected > 0;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Güncelleme Hatası: " + ex.Message);
                return false;
            }
            finally { CloseConnection(); }
        }

        // --- 4. HASTA SİLME ---
        public bool DeletePatient(string tcNo)
        {
            try
            {
                if (OpenConnection())
                {
                    string query = "DELETE FROM hastalar WHERE tc_kimlik_no = @tc";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, _connection))
                    {
                        cmd.Parameters.AddWithValue("@tc", tcNo);
                        int affected = cmd.ExecuteNonQuery();
                        return affected > 0;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Silme Hatası: " + ex.Message);
                return false;
            }
            finally { CloseConnection(); }
        }

        // --- 5. HASTA ARAMA ---
        public DataTable SearchPatient(string tcNo)
        {
            DataTable dt = new DataTable();
            try
            {
                if (OpenConnection())
                {
                    string query = "SELECT * FROM hastalar WHERE tc_kimlik_no = @tc";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, _connection))
                    {
                        cmd.Parameters.AddWithValue("@tc", tcNo);
                        NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
                        da.Fill(dt);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Arama Hatası: " + ex.Message); }
            finally { CloseConnection(); }
            return dt;
        }

        // --- DİĞER MODÜLLER İÇİN BOŞ METOTLAR (Hata vermesin diye eklendi) ---
        // Bunların içi BOŞ kalacak, çünkü sen şu an sadece Hasta Kayıt yapıyorsun.
        // Diğer gruplar kendi kodlarını buraya ekleyecekler.

        //public DataTable GetAllCitys() { return null; }
        //public bool ValidateUserLogin(string u, string p) { return false; }
        public bool AddUser(string u, string p, string tc, string f, string l, string r, string e, string ph) { return false; }
        //-------Hasata kayıt işlemş son -------------------------


        //---------------Rapor başlangıç-----------------------------
        public DataTable GetPatientSessionsByTc(string tc)
        {
            DataTable dt = new DataTable();

            string query = @"
        SELECT
            h.tc_kimlik_no AS ""TC Kimlik No"",
            h.ad || ' ' || h.soyad AS ""Hasta Adı"",
            s.seans_id AS ""Seans ID"",
            s.seans_tarihi_baslangic AS ""Seans Başlangıç"",
            s.seans_suresi_dk AS ""Süre (dk)"",
            s.yurume_hizi_km_s AS ""Yürüme Hızı"",
            s.vucut_agirlik_destegi_yuzde AS ""Ağırlık Desteği"",
            s.yurunen_mesafe_m AS ""Mesafe (m)"",
            s.zorluk_seviyesi AS ""Zorluk"",
            s.ortalama_kalp_atisi_bpm AS ""Ortalama Nabız"",
            s.notlar AS ""Notlar""
        FROM hastalar h
        INNER JOIN seanslar s 
            ON h.tc_kimlik_no = s.hasta_tc
        WHERE h.tc_kimlik_no = @tc
        ORDER BY s.seans_tarihi_baslangic DESC;
    ";

            try
            {
                using (var conn = new Npgsql.NpgsqlConnection(connectionString))
                {
                    conn.Open();

                    using (var cmd = new Npgsql.NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tc", tc);

                        using (var da = new Npgsql.NpgsqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return dt;
        }


        //----------------Rapor son------------------------------

        // =============================================================
        // 2.LOGLARI SORGULAMA VE LİSTELEME
        // =============================================================
        public DataTable GetSystemLogs(DateTime baslangic, DateTime bitis, string aramaMetni, string seviye)
        {
            DataTable dt = new DataTable();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // SQL SORGUSU
                    string sql = @"
                        SELECT 
                            l.log_id, 
                            l.olay_zamani AS tarih, 
                            l.log_seviyesi AS seviye, 
                            k.kullanici_adi AS kullanici, 
                            l.ip_adresi,
                            l.mesaj AS islem_detayi
                        FROM loglar l
                        LEFT JOIN kullanicihesaplari k ON l.hesap_id = k.hesap_id
                        WHERE l.olay_zamani BETWEEN @baslangic AND @bitis
                          AND (@seviye IS NULL OR l.log_seviyesi = @seviye)
                          AND (@aramaMetni IS NULL OR 
                               l.mesaj ILIKE @aramaMetni OR 
                               k.kullanici_adi ILIKE @aramaMetni)
                        ORDER BY l.olay_zamani DESC";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        // Parametreler
                        cmd.Parameters.AddWithValue("@baslangic", baslangic);
                        cmd.Parameters.AddWithValue("@bitis", bitis.Date.AddDays(1).AddSeconds(-1));

                        var pSeviye = new NpgsqlParameter("@seviye", NpgsqlDbType.Varchar);
                        pSeviye.Value = (string.IsNullOrEmpty(seviye) || seviye == "Tümü") ? (object)DBNull.Value : seviye;
                        cmd.Parameters.Add(pSeviye);

                        var pArama = new NpgsqlParameter("@aramaMetni", NpgsqlDbType.Text);
                        pArama.Value = string.IsNullOrWhiteSpace(aramaMetni) ? (object)DBNull.Value : "%" + aramaMetni + "%";
                        cmd.Parameters.Add(pArama);

                        // Veriyi Çek
                        using (var adapter = new NpgsqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Veritabanı bağlantı hatası:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return dt;
        }

        public DataTable GetDeviceStatusLogs(DateTime baslangic, DateTime bitis)
        {
            // Cihaz sensör verileri için boş tablo (İleride doldurulabilir)
            return new DataTable();
        }

        // =============================================================
        // 3. CİHAZ KOMUTLARINI LOGLAMA (LogDeviceCommand)
        // =============================================================
        public void LogDeviceCommand(string commandName, string status)
        {
            // Bu metot, arkadaşının Service.cs kodunda çağrılıyor.
            // Motor hareket ettiğinde veritabanına kayıt atar.

            using (var conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO loglar (olay_zamani, log_seviyesi, mesaj, hesap_id) VALUES (@zaman, 'INFO', @mesaj, 1)";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@zaman", DateTime.Now);
                        cmd.Parameters.AddWithValue("@mesaj", $"Cihaz Komutu: {commandName} - Durum: {status}");
                        // Not: hesap_id=1 varsayılan sistem kullanıcısı kabul edildi.

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    // Komut gönderilirken log hatası olursa program çökmesin, sadece konsola yazsın.
                    System.Diagnostics.Debug.WriteLine("Loglama Hatası: " + ex.Message);
                }
            }
        }

        // -----------------------------------------------------------
        // ------------------    LOADCELL FUNCTIONS   ----------------
        // -----------------------------------------------------------

        public bool SaveLoadCellData(int therapyId, DateTime timestamp, double rightHeel,
            double leftHeel, double rightToe, double leftToe, double weightBalance, int index)
        {
            try
            {
                if (conn == null || conn.State != ConnectionState.Open)
                    if (!OpenConnection()) return false;

                string query = @"
                    INSERT INTO public.loadcell_verileri_tablosu
                    (terapi_id, zaman_damgasi, sag_topuk_basinc_degeri, sol_topuk_basinc_degeri, 
                     sag_on_ayak_basinc_degeri, sol_on_ayak_basinc_degeri, agirlik_dengeleme_degeri, 
                     olcum_index_numarasi)
                    VALUES (@therapyId, @timestamp, @rightHeel, @leftHeel, @rightToe, @leftToe, @weightBalance, @index);
                ";

                using (var cmd = new Npgsql.NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@therapyId", therapyId);
                    cmd.Parameters.AddWithValue("@timestamp", timestamp);
                    cmd.Parameters.AddWithValue("@rightHeel", rightHeel);
                    cmd.Parameters.AddWithValue("@leftHeel", leftHeel);
                    cmd.Parameters.AddWithValue("@rightToe", rightToe);
                    cmd.Parameters.AddWithValue("@leftToe", leftToe);
                    cmd.Parameters.AddWithValue("@weightBalance", weightBalance);
                    cmd.Parameters.AddWithValue("@index", index);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch
            {
                return false;
            }
        }


        public bool SaveLoadCellDataBulk(int therapyId, List<LoadCellData> dataList)
        {
            try
            {
                if (conn == null || conn.State != ConnectionState.Open)
                    if (!OpenConnection()) return false;

                using (var transaction = conn.BeginTransaction())
                {
                    foreach (var d in dataList)
                    {
                        string query = @"
                           INSERT INTO public.loadcell_verileri_tablosu
                           (terapi_id, zaman_damgasi, sag_topuk_basinc_degeri, sol_topuk_basinc_degeri,
                            sag_on_ayak_basinc_degeri, sol_on_ayak_basinc_degeri,
                            agirlik_dengeleme_degeri, olcum_index_numarasi)
                           VALUES
                           (@therapyId, @timestamp, @rh, @lh, @rt, @lt, @wb, @idx);
                        ";

                        using (var cmd = new Npgsql.NpgsqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@therapyId", therapyId);
                            cmd.Parameters.AddWithValue("@timestamp", d.Timestamp);
                            cmd.Parameters.AddWithValue("@rh", d.RightHeel);
                            cmd.Parameters.AddWithValue("@lh", d.LeftHeel);
                            cmd.Parameters.AddWithValue("@rt", d.RightToe);
                            cmd.Parameters.AddWithValue("@lt", d.LeftToe);
                            cmd.Parameters.AddWithValue("@wb", d.WeightBalance);
                            cmd.Parameters.AddWithValue("@idx", d.Index);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public DataTable GetLoadCellData(int therapyId)
        {
            try
            {
                if (conn == null || conn.State != ConnectionState.Open)
                    if (!OpenConnection()) return null;

                string query = @"
                    SELECT
                        zaman_damgasi,
                        sag_topuk_basinc_degeri,
                        sol_topuk_basinc_degeri,
                        sag_on_ayak_basinc_degeri,
                        sol_on_ayak_basinc_degeri,
                        agirlik_dengeleme_degeri,
                        olcum_index_numarasi
                    FROM public.loadcell_verileri_tablosu
                    WHERE terapi_id = @therapyId
                    ORDER BY olcum_index_numarasi ASC;
                ";

                var cmd = new Npgsql.NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@therapyId", therapyId);

                var da = new Npgsql.NpgsqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
            catch
            {
                return null;
            }
        }


        public DataTable GetLoadCellDataByTimeRange(int therapyId, DateTime startTime,
            DateTime endTime)
        {
            try
            {
                if (conn == null || conn.State != ConnectionState.Open)
                    if (!OpenConnection()) return null;

                string query = @"
                    SELECT *
                    FROM public.loadcell_verileri_tablosu
                    WHERE terapi_id = @therapyId
                    AND zaman_damgasi BETWEEN @start AND @end
                    ORDER BY zaman_damgasi ASC;
                ";

                var cmd = new Npgsql.NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@therapyId", therapyId);
                cmd.Parameters.AddWithValue("@start", startTime);
                cmd.Parameters.AddWithValue("@end", endTime);

                var da = new Npgsql.NpgsqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
            catch
            {
                return null;
            }
        }





        public bool UpdateGeneralSettings(string lang, string dateFmt, string lenUnit, string weightUnit, string theme)
        {
            try
            {
                if (OpenConnection())
                {
                    string sql = @"UPDATE general_settings SET 
                           application_language=@p1, date_time_format=@p2, length_unit=@p3, weight_unit=@p4, theme=@p5 
                           WHERE id = (SELECT id FROM general_settings LIMIT 1)";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection))
                    {
                        cmd.Parameters.AddWithValue("@p1", lang);
                        cmd.Parameters.AddWithValue("@p2", dateFmt);
                        cmd.Parameters.AddWithValue("@p3", lenUnit);
                        cmd.Parameters.AddWithValue("@p4", weightUnit);
                        cmd.Parameters.AddWithValue("@p5", theme);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
                return false;
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); return false; }
        }


        // Mevcut DatabaseManager class'ının içine ekle veya güncelle:
        public string GetCurrentDateFormat()
        {
            try
            {
                // Bağlantıyı aç (Senin OpenConnection metodunu kullanıyoruz)
                if (OpenConnection())
                {
                    string query = "SELECT date_time_format FROM general_settings LIMIT 1";

                    // GetData yerine senin kullandığın standart yöntemi (DataAdapter) kullanıyoruz
                    using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            return dt.Rows[0]["date_time_format"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda konsola yaz ama programı kırma
                Console.WriteLine("Tarih formatı alınamadı: " + ex.Message);
            }
            finally
            {
                // İsteğe bağlı: Her sorgudan sonra kapatmak istersen:
                // CloseConnection(); 
            }

            // Eğer bir hata olursa veya veri yoksa varsayılan olarak bunu döndür
            return "dd.MM.yyyy";
        }





        // --- MEVCUT GetGeneralSettings METODUNU BUL VE BUNUNLA DEĞİŞTİR ---
        public DataRow GetGeneralSettings()
        {
            DataTable dt = new DataTable();
            try
            {
                if (OpenConnection())
                {
                    // Tablo boşsa varsayılan kayıt oluştur (SANTİMETRE DEFAULT)
                    using (var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM general_settings", conn))
                    {
                        long count = (long)checkCmd.ExecuteScalar();
                        if (count == 0)
                        {
                            // İLK AÇILIŞTA VARSAYILAN DEĞERLER
                            string sql = "INSERT INTO general_settings (application_language, date_time_format, length_unit, weight_unit, theme) VALUES ('tr', 'dd.MM.yyyy', 'Santimetre (cm)', 'Kilogram (kg)', 'Light')";
                            using (var insertCmd = new NpgsqlCommand(sql, conn))
                                insertCmd.ExecuteNonQuery();
                        }
                    }

                    using (NpgsqlDataAdapter da = new NpgsqlDataAdapter("SELECT * FROM general_settings ORDER BY id ASC LIMIT 1", conn))
                    {
                        da.Fill(dt);
                    }
                }
            }
            catch { }
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        // --- UZUNLUK HESAPLAMA METODUNU BUL VE BUNUNLA DEĞİŞTİR ---
        // isBodyMeasurement: TRUE ise Vücut (DB'de CM), FALSE ise Yol (DB'de Metre)
        // --- UZUNLUK ÇARPANINI GETİR ---
        public decimal GetLengthMultiplier(bool isBodyMeasurement)
        {
            try
            {
                DataRow row = GetGeneralSettings();
                if (row == null) return 1.0m;

                string unit = row["length_unit"].ToString().ToLower();

                // 1. MİLİMETRE (En başta kontrol et!)
                if (unit.Contains("mili") || unit.Contains("mm"))
                {
                    if (isBodyMeasurement) return 10.0m; // 180 cm -> 1800 mm
                    else return 1000.0m;                 // 1 m -> 1000 mm
                }
                // 2. SANTİMETRE (Varsayılan olarak algılasın)
                else if (unit.Contains("santi") || unit.Contains("cm"))
                {
                    if (isBodyMeasurement) return 1.0m;  // 180 cm -> 180 cm
                    else return 100.0m;                  // 1 m -> 100 cm
                }
                // 3. METRE (En sona koyduk, çünkü içinde 'metre' kelimesi geçiyor)
                else if (unit.Contains("metre") || unit.Contains("m"))
                {
                    if (isBodyMeasurement) return 0.01m; // 180 cm -> 1.80 m
                    else return 1.0m;                    // 1 m -> 1 m
                }

                return 1.0m;
            }
            catch { return 1.0m; }
        }

        // --- BİRİM ETİKETİNİ GETİR ---
        public string GetLengthUnitLabel()
        {
            try
            {
                DataRow row = GetGeneralSettings();
                if (row == null) return "cm";

                string unit = row["length_unit"].ToString().ToLower();

                if (unit.Contains("mili") || unit.Contains("mm")) return "mm";
                if (unit.Contains("santi") || unit.Contains("cm")) return "cm";
                if (unit.Contains("metre") || unit.Contains("m")) return "m"; // En son kontrol

                return "cm";
            }
            catch { return "cm"; }
        }







        // --- AĞIRLIK BİRİMİ YÖNETİCİSİ ---
        public decimal GetWeightMultiplier()
        {
            try
            {
                DataRow row = GetGeneralSettings();
                if (row == null) return 1.0m;

                string unit = row["weight_unit"].ToString().ToLower();

                // Eğer veritabanında "gram" yazıyorsa 1000 katı
                if (unit.Contains("gram") && !unit.Contains("kilogram"))
                {
                    return 1000.0m; // 1 kg = 1000 g
                }

                // Varsayılan: KILOGRAM (Çarpan 1)
                return 1.0m;
            }
            catch { return 1.0m; }
        }

        public string GetWeightUnitLabel()
        {
            try
            {
                DataRow row = GetGeneralSettings();
                if (row == null) return "kg";

                string unit = row["weight_unit"].ToString().ToLower();

                if (unit.Contains("gram") && !unit.Contains("kilogram")) return "g";

                return "kg";
            }
            catch { return "kg"; }
        }




































































































































































































































































































































































































































































    }
    // LoadCell verisi için model class
    public class LoadCellData
    {
        public DateTime Timestamp { get; set; }
        public double RightHeel { get; set; }
        public double LeftHeel { get; set; }
        public double RightToe { get; set; }
        public double LeftToe { get; set; }
        public double WeightBalance { get; set; }
        public int Index { get; set; }
    }
}

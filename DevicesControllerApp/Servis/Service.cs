using DevicesControllerApp.Database;
using RehabilitationSystem.Communication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DevicesControllerApp.Servis
{
    public partial class Service : UserControl
    {
        // Yetkilendirme kontrolü için rol tanımı
        string currentUserRole = "Service";

        // Global Nesneler: Haberleşme (Singleton) ve Veritabanı Yöneticisi
        private DeviceCommunication _comm = DeviceCommunication.Instance;
        private DatabaseManager _dbManager = DatabaseManager.Instance;

        private System.Windows.Forms.Timer _readTimer; // Sensör verilerini okuyan zamanlayıcı
        private int _currentLanguageId = 0;

        public Service()
        {
            InitializeComponent();

            // Dil seçimi ComboBox olayını bağla
            if (cmbDilSecimi != null)
            {
                cmbDilSecimi.SelectedIndex = 0;
                cmbDilSecimi.SelectedIndexChanged += CmbDilSecimi_SelectedIndexChanged;
            }

            // Sensör okuma timer'ını ve varsayılan dili ayarla
            SetupReadingTimer();
            UpdateLanguage(0);
        }

        // =============================================================
        // BÖLÜM A: DİL YÖNETİMİ 
        // =============================================================

        private void CmbDilSecimi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDilSecimi != null)
            {
                UpdateLanguage(cmbDilSecimi.SelectedIndex);
            }
        }

        // Arayüzdeki metinleri seçilen dile göre (0:TR, 1:EN, 2:AR) günceller
        public void UpdateLanguage(int langId)
        {
            _currentLanguageId = langId;

            if (langId == 1) // English
            {
                if (btnServoMove != null) btnServoMove.Text = "Move Motor";
                if (btnHomingAll != null) btnHomingAll.Text = "Homing All";
                if (btnCalibration != null) btnCalibration.Text = "Calibrate";
                if (lblLoadCellTitle != null) lblLoadCellTitle.Text = "Weight:";
            }
            else if (langId == 2) // Arabic
            {
                if (btnServoMove != null) btnServoMove.Text = "تحريك المحرك";
                if (btnHomingAll != null) btnHomingAll.Text = "إعادة التعيين";
                if (btnCalibration != null) btnCalibration.Text = "معايرة";
                if (lblLoadCellTitle != null) lblLoadCellTitle.Text = "وزن:";
            }
            else // Default / Turkish
            {
                if (btnServoMove != null) btnServoMove.Text = "Motoru Hareket Ettir";
                if (btnHomingAll != null) btnHomingAll.Text = "Homing Başlat";
                if (btnCalibration != null) btnCalibration.Text = "Kalibrasyon";
                if (lblLoadCellTitle != null) lblLoadCellTitle.Text = "Ağırlık:";
            }
        }

        // =============================================================
        // BÖLÜM B: LOG İŞLEMLERİ (Veritabanı Sorgulama)
        // =============================================================

        private void btnLogGetir_Click(object sender, EventArgs e)
        {
            // 1. Arayüzden filtreleri al
            DateTime baslangic = dtpLogBaslangic.Value;
            DateTime bitis = dtpLogBitis.Value;
            string aramaMetni = txtLogArama.Text.Trim();
            string logTipi = cmbLogTipi.SelectedItem?.ToString();
            string seviye = cmbLogSeviye.SelectedItem?.ToString();

            // Tip seçilmediyse işlem yapma
            if (string.IsNullOrEmpty(logTipi))
            {
                MessageBox.Show("Lütfen bir log tipi seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DataTable logListesi = null;

                // 2. Veritabanından veriyi çek
                if (logTipi == "Sistem Logları")
                {
                    logListesi = DatabaseManager.Instance.GetSystemLogs(baslangic, bitis, aramaMetni, seviye);
                }
                else
                {
                    logListesi = DatabaseManager.Instance.GetDeviceStatusLogs(baslangic, bitis);
                }

                // 3. Veriyi tabloya bağla ve sütunları düzenle
                dgvLoglar.DataSource = logListesi;

                if (dgvLoglar.Columns.Count > 0)
                {
                    if (dgvLoglar.Columns.Contains("log_id")) dgvLoglar.Columns["log_id"].Visible = false; // ID'yi gizle

                    // Başlıkları kullanıcı dostu hale getir
                    if (dgvLoglar.Columns.Contains("tarih"))
                    {
                        dgvLoglar.Columns["tarih"].HeaderText = "İşlem Tarihi";
                        dgvLoglar.Columns["tarih"].Width = 140;
                        dgvLoglar.Columns["tarih"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm:ss";
                    }
                    if (dgvLoglar.Columns.Contains("seviye")) dgvLoglar.Columns["seviye"].HeaderText = "Seviye";
                    if (dgvLoglar.Columns.Contains("kullanici")) dgvLoglar.Columns["kullanici"].HeaderText = "Personel";
                    if (dgvLoglar.Columns.Contains("ip_adresi")) dgvLoglar.Columns["ip_adresi"].HeaderText = "IP Adresi";
                    if (dgvLoglar.Columns.Contains("islem_detayi"))
                    {
                        dgvLoglar.Columns["islem_detayi"].HeaderText = "Açıklama / Mesaj";
                        dgvLoglar.Columns["islem_detayi"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Tablodan satır seçildiğinde yan tarafta detay göster
        private void dgvLoglar_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvLoglar.SelectedRows.Count > 0)
            {
                DataGridViewRow seciliSatir = dgvLoglar.SelectedRows[0];
                StringBuilder detay = new StringBuilder();

                foreach (DataGridViewCell cell in seciliSatir.Cells)
                {
                    string kolonBasligi = dgvLoglar.Columns[cell.ColumnIndex].HeaderText;
                    string hucreDegeri = cell.Value?.ToString() ?? "NULL";

                    detay.AppendLine($"[{kolonBasligi}]:");
                    detay.AppendLine(hucreDegeri);
                    detay.AppendLine("--------------------");
                }
                txtLogDetay.Text = detay.ToString();
            }
            else
            {
                txtLogDetay.Text = "Detayları görmek için listeden bir log satırı seçin.";
            }
        }

        // Tablodaki verileri CSV formatında dışa aktarır
        private void btnLogExport_Click(object sender, EventArgs e)
        {
            if (dgvLoglar.Rows.Count == 0)
            {
                MessageBox.Show("Dışa aktarılacak log bulunamadı. Lütfen önce logları getirin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "CSV Dosyası (*.csv)|*.csv|Tüm Dosyalar (*.*)|*.*";
            saveDialog.Title = "Logları CSV Olarak Kaydet";
            saveDialog.FileName = $"LogKayitlari_{DateTime.Now:yyyyMMdd_HHmm}.csv";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StringBuilder csvContent = new StringBuilder();

                    // Başlıkları al
                    IEnumerable<string> kolonBasliklari = dgvLoglar.Columns.Cast<DataGridViewColumn>()
                                                             .Where(col => col.Visible)
                                                             .Select(col => col.HeaderText.Replace(";", ","));
                    csvContent.AppendLine(string.Join(";", kolonBasliklari));

                    // Satırları al
                    foreach (DataGridViewRow row in dgvLoglar.Rows)
                    {
                        if (row.IsNewRow) continue;
                        IEnumerable<string> hucreDegerleri = row.Cells.Cast<DataGridViewCell>()
                                                        .Where(cell => dgvLoglar.Columns[cell.ColumnIndex].Visible)
                                                        .Select(cell => (cell.Value?.ToString() ?? "").Replace(";", ","));
                        csvContent.AppendLine(string.Join(";", hucreDegerleri));
                    }

                    // Dosyayı UTF8 (Türkçe karakter destekli) kaydet
                    System.IO.File.WriteAllText(saveDialog.FileName, csvContent.ToString(), Encoding.UTF8);
                    MessageBox.Show($"Loglar başarıyla '{saveDialog.FileName}' dosyasına aktarıldı.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Dışa aktarma sırasında bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // =============================================================
        // BÖLÜM C: CİHAZ KONTROL VE HABERLEŞME
        // =============================================================

        // Motor Hareket Komutu
        private void btnServoMove_Click(object sender, EventArgs e)
        {
            // 1. Bağlantı Kontrolü
            if (!_comm.IsConnected)
            {
                ShowStatusMessage("Cihaz bağlı değil!", true);
                return;
            }

            try
            {
                // 2. Girdileri Kontrol Et ve Pars Et
                if (int.TryParse(txtServoSpeed.Text, out int speed) && int.TryParse(txtServoDistance.Text, out int distance))
                {
                    // 3. Protokol üzerinden komutu gönder
                    bool success = _comm.SetServoMotorPosition(0, distance);

                    if (success)
                    {
                        // 4. Başarılıysa arayüzü güncelle ve Veritabanına Logla
                        lblMotorStatus.Text = _currentLanguageId == 0 ? "Komut Gönderildi" : "Command Sent";
                        lblMotorStatus.ForeColor = Color.Green;
                        _dbManager.LogDeviceCommand("SetServoMotor", "SUCCESS");
                    }
                    else
                    {
                        ShowStatusMessage("Komut başarısız!", true);
                        _dbManager.LogDeviceCommand("SetServoMotor", "FAIL");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Haberleşme Hatası: " + ex.Message);
            }
        }

        // Periyodik olarak sensör verilerini okuyan metot (Timer Tick)
        private void ReadData_Tick(object sender, EventArgs e)
        {
            if (_comm.IsConnected)
            {
                // LoadCell (Ağırlık) verisini al
                var packet = _comm.GetLatestLoadCellData();
                if (packet != null && lblLoadCellValue != null)
                {
                    lblLoadCellValue.Text = $"{packet.WeightBalance:F2} kg";
                }

                // Limit Switch durumlarını al
                bool[] switches = _comm.ReadLimitSwitches();
                if (switches != null && switches.Length > 0 && lblLSXMin != null)
                {
                    lblLSXMin.BackColor = switches[0] ? Color.Green : Color.Red;
                    lblLSXMin.Text = $"X Min {(switches[0] ? "AKTİF" : "PASİF")}";
                }
            }
            else
            {
                if (lblLoadCellValue != null) lblLoadCellValue.Text = "BAĞLANTI YOK";
            }
        }

        private void SetupReadingTimer()
        {
            _readTimer = new System.Windows.Forms.Timer { Interval = 500 }; // 500ms'de bir oku
            _readTimer.Tick += ReadData_Tick;
            _readTimer.Start();
        }

        // Kullanıcıya hata/durum mesajı göstermek için yardımcı metot
        private void ShowStatusMessage(string message, bool isError)
        {
            if (lblMotorStatus != null)
            {
                lblMotorStatus.Text = message;
                lblMotorStatus.ForeColor = isError ? Color.Red : Color.Black;
            }
        }

        // =============================================================
        // BÖLÜM D: TEŞHİS VE DİĞERLERİ (Şimdilik Demo Verileri)
        // =============================================================

        private void Service_Load(object sender, EventArgs e)
        {
            // Güvenlik kontrolü
            if (currentUserRole != "Service")
            {
                this.Enabled = false;
                MessageBox.Show("Bu alana sadece servis personeli erişebilir.");
            }
        }

        private void diagnosticsTimer_Tick(object sender, EventArgs e)
        {
            UpdateDiagnostics();
        }

        // Teşhis ekranını sahte verilerle günceller (Simülasyon)
        private void UpdateDiagnostics()
        {
            try
            {
                if (lblDeviceStatus != null)
                {
                    lblDeviceStatus.Text = "DURUM: SAĞLIKLI";
                    lblDeviceStatus.ForeColor = Color.Green;
                }

                if (lblTotalTime != null) lblTotalTime.Text = "Toplam Çalışma Süresi: 125 saat";
                if (lblTherapyCount != null) lblTherapyCount.Text = "Toplam Terapi: 48";
                if (lblErrorCount != null) lblErrorCount.Text = "Toplam Hata: 2";

                LoadLastErrors();
            }
            catch (Exception ex)
            {
                if (lblDeviceStatus != null)
                {
                    lblDeviceStatus.Text = "DURUM: HATA";
                    lblDeviceStatus.ForeColor = Color.Red;
                }
            }
        }

        private void LoadLastErrors()
        {
            if (dgvErrors != null)
            {
                dgvErrors.Rows.Clear();
                // Demo verileri
                dgvErrors.Rows.Add(DateTime.Now, "ERROR", "Motor bağlantı hatası");
                dgvErrors.Rows.Add(DateTime.Now.AddMinutes(-10), "WARN", "LoadCell değer sınırda");
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null) components.Dispose();
                _readTimer?.Stop();
                _readTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        // Tasarımcı hatası almamak için boş bırakılan eventler
        private void tabPage1_Click(object sender, EventArgs e) { }
        private void pnlStats_Paint(object sender, PaintEventArgs e) { }
        private void lblTotalTime_Click(object sender, EventArgs e) { }
        private void lblDeviceStatus_Click(object sender, EventArgs e) { }
        private void txtServoDistance_TextChanged(object sender, EventArgs e) { }
    }
}










/////////////////////////////////


/* private void btnServoMove_Click(object sender, EventArgs e)
 {
     if (!_comm.IsConnected)
     {
         ShowStatusMessage("Cihaz bağlı değil!", true);
         return;
     }

     try
     {
         if (int.TryParse(txtServoSpeed.Text, out int speed) && int.TryParse(txtServoDistance.Text, out int distance))
         {
             bool success = _comm.SetServoMotorPosition(0, distance);

             if (success)
             {
                 lblMotorStatus.Text = _currentLanguageId == 0 ? "Komut Gönderildi" : "Command Sent";
                 lblMotorStatus.ForeColor = Color.Green;
                 _dbManager.LogDeviceCommand("SetServoMotor", "SUCCESS");
             }
             else
             {
                 ShowStatusMessage("Komut başarısız!", true);
                 _dbManager.LogDeviceCommand("SetServoMotor", "FAIL");
             }
         }
     }
     catch (Exception ex)
     {
         MessageBox.Show("Haberleşme Hatası: " + ex.Message);
     }
 }
}
}*/


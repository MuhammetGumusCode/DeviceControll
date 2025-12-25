using DevicesControllerApp.Database; 
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace DevicesControllerApp.Veri_izleme
{
    public partial class DataMonitoringControl : UserControl
    {
        private const int StepPoints = 100;
        private const int LiveFps = 20;
        private const int BalanceWindowPoints = 200;

        private Timer timer;
        private Random rnd = new Random();

        private double[] rightHeel = new double[StepPoints];
        private double[] leftHeel = new double[StepPoints];
        private double[] rightToe = new double[StepPoints];
        private double[] leftToe = new double[StepPoints];

        private int timeIndex = 0;
        private int pointIndex = 0;
        private int stepCount = 0;

        private string currentLanguage = "tr";
        private bool isFrozen = false;

        private List<string> rehabSessions = new List<string>();
        private string selectedSessionId = null;

        // أضفنا Mode.Database
        private enum Mode { Live, History, Database }
        private Mode currentMode = Mode.Live;

        private List<RehabSample> historySamples = new List<RehabSample>();
        private int historyCursor = 0;
        private DateTime? historyStartTime = null;

        // بيانات قادمة من قاعدة البيانات
        private List<LoadCellData> dbSamples = new List<LoadCellData>();
        private int dbCursor = 0;

        public DataMonitoringControl()
        {
            InitializeComponent();

            timer = new Timer();
            timer.Interval = 50;
            timer.Tick += Timer_Tick;

            this.Disposed += DataMonitoringControl_Disposed;
        }

        private void DataMonitoringControl_Load(object sender, EventArgs e)
        {
            cmbLanguage.Items.Clear();
            cmbLanguage.Items.Add("Türkçe");
            cmbLanguage.Items.Add("English");
            cmbLanguage.SelectedIndex = 0;

            PrepareDemoSessions();
            RefreshSessionList(rehabSessions);

            ApplyLanguage();
            InitCharts();
        }

        private void PrepareDemoSessions()
        {
            rehabSessions = new List<string>
            {
                "REHAB-1 / Hasta A",
                "REHAB-2 / Hasta B",
                "REHAB-3 / Hasta C",
                "REHAB-4 / Hasta D"
            };
        }

        private void RefreshSessionList(List<string> items)
        {
            cmbRehabSessions.BeginUpdate();
            cmbRehabSessions.Items.Clear();
            foreach (var it in items) cmbRehabSessions.Items.Add(it);
            if (cmbRehabSessions.Items.Count > 0) cmbRehabSessions.SelectedIndex = 0;
            cmbRehabSessions.EndUpdate();
        }

        private void StyleChart(Chart ch)
        {
            ch.BackColor = Color.White;
            ch.ChartAreas[0].BackColor = Color.WhiteSmoke;

            ch.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            ch.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;

            ch.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.Black;
            ch.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.Black;

            ch.AntiAliasing = AntiAliasingStyles.All;
        }

        private void InitCharts()
        {
            SetupStepChart(chart1, Color.Blue);
            SetupStepChart(chart2, Color.Orange);
            SetupStepChart(chart3, Color.Green);
            SetupStepChart(chart4, Color.Purple);

            SetupTimeChart(chart5, Color.DarkCyan);

            StyleChart(chart1);
            StyleChart(chart2);
            StyleChart(chart3);
            StyleChart(chart4);
            StyleChart(chart5);

            AttachZoom(chart1);
            AttachZoom(chart2);
            AttachZoom(chart3);
            AttachZoom(chart4);
            AttachZoom(chart5);

            RedrawStepCharts();
            ClearBalanceSeries();
        }

        private void SetupStepChart(Chart chart, Color lineColor)
        {
            chart.Series.Clear();

            Series s = new Series("Live");
            s.ChartType = SeriesChartType.Line;
            s.BorderWidth = 3;
            s.Color = lineColor;

            chart.Series.Add(s);

            var area = chart.ChartAreas[0];
            area.AxisX.Minimum = 0;
            area.AxisX.Maximum = StepPoints - 1;
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = 100;

            area.AxisX.Title = "Index";
            area.AxisY.Title = "LoadCell";
        }

        private void SetupTimeChart(Chart chart, Color lineColor)
        {
            chart.Series.Clear();

            Series s = new Series("Live");
            s.ChartType = SeriesChartType.Line;
            s.BorderWidth = 3;
            s.Color = lineColor;

            chart.Series.Add(s);

            var area = chart.ChartAreas[0];
            area.AxisX.Minimum = 0;
            area.AxisX.Maximum = BalanceWindowPoints;
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = 100;

            area.AxisX.Title = "Time";
            area.AxisY.Title = "Balance";
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (currentMode == Mode.Live)
            {
                double v1 = rnd.Next(10, 100);
                double v2 = rnd.Next(10, 100);
                double v3 = rnd.Next(10, 100);
                double v4 = rnd.Next(10, 100);

                double vb = WaveValue(timeIndex);

                rightHeel[pointIndex] = v1;
                leftHeel[pointIndex] = v2;
                rightToe[pointIndex] = v3;
                leftToe[pointIndex] = v4;

                if (!isFrozen)
                {
                    RedrawStepCharts();
                    AppendBalancePoint(vb);
                }

                pointIndex++;
                if (pointIndex >= StepPoints)
                {
                    pointIndex = 0;
                    stepCount++;

                    lblStepCount.Text = currentLanguage == "tr"
                        ? $"Adım Sayısı: {stepCount}"
                        : $"Step Count: {stepCount}";
                }
            }
            else
            {
                HistoryOrDatabaseTick();
            }
        }

        private int historyTickSubCounter = 0;

        private void HistoryOrDatabaseTick()
        {
            historyTickSubCounter++;
            if (historyTickSubCounter < LiveFps) return;
            historyTickSubCounter = 0;

           
            if (currentMode == Mode.Database && dbSamples != null && dbSamples.Count > 0)
            {
                if (dbCursor >= dbSamples.Count) return;

                var sample = dbSamples[dbCursor];

                rightHeel[pointIndex] = sample.RightHeel;
                leftHeel[pointIndex] = sample.LeftHeel;
                rightToe[pointIndex] = sample.RightToe;
                leftToe[pointIndex] = sample.LeftToe;

                if (!isFrozen)
                {
                    RedrawStepCharts();
                    AppendBalancePoint(sample.WeightBalance);
                }

                pointIndex++;
                if (pointIndex >= StepPoints)
                {
                    pointIndex = 0;
                    stepCount++;
                    lblStepCount.Text = currentLanguage == "tr"
                        ? $"Adım Sayısı: {stepCount}"
                        : $"Step Count: {stepCount}";
                }

                if (historyStartTime == null) historyStartTime = sample.Timestamp;
                timeIndex = (int)(sample.Timestamp - historyStartTime.Value).TotalSeconds;

                dbCursor++;
                return;
            }

           
            if (historySamples == null || historySamples.Count == 0) return;
            if (historyCursor >= historySamples.Count) return;

            var hsample = historySamples[historyCursor];

            rightHeel[pointIndex] = hsample.RightHeel;
            leftHeel[pointIndex] = hsample.LeftHeel;
            rightToe[pointIndex] = hsample.RightToe;
            leftToe[pointIndex] = hsample.LeftToe;

            if (!isFrozen)
            {
                RedrawStepCharts();
                AppendBalancePoint(hsample.Balance);
            }

            pointIndex++;
            if (pointIndex >= StepPoints)
            {
                pointIndex = 0;
                stepCount++;
                lblStepCount.Text = currentLanguage == "tr"
                    ? $"Adım Sayısı: {stepCount}"
                    : $"Step Count: {stepCount}";
            }

            if (historyStartTime == null) historyStartTime = hsample.Timestamp;
            timeIndex = (int)(hsample.Timestamp - historyStartTime.Value).TotalSeconds;

            historyCursor++;
        }

        private void LoadFromDatabase(int therapyId)
        {
            if (!DatabaseManager.Instance.OpenConnection())
            {
                MessageBox.Show("Veritabanına bağlanılamadı / Cannot connect DB");
                return;
            }

            DataTable dt = DatabaseManager.Instance.GetLoadCellData(therapyId);


            if (dbSamples == null || dbSamples.Count == 0)
            {
                MessageBox.Show("Bu terapi için veri yok / No data for this therapy");
                return;
            }

            historyStartTime = dbSamples[0].Timestamp;
            dbCursor = 0;
            pointIndex = 0;
            stepCount = 0;
            timeIndex = 0;

            currentMode = Mode.Database;
            timer.Start();

            MessageBox.Show("Database LoadCell data loaded!");
        }

        private double WaveValue(int t)
        {
            double baseLine = 50.0;
            double amp = 25.0;
            double periodPoints = 40.0;
            double noise = rnd.NextDouble() * 4.0 - 2.0;

            double w = baseLine + amp * Math.Sin(2.0 * Math.PI * t / periodPoints) + noise;
            if (w < 0) w = 0;
            if (w > 100) w = 100;
            return w;
        }

        private void RedrawStepCharts()
        {
            Draw(chart1, rightHeel);
            Draw(chart2, leftHeel);
            Draw(chart3, rightToe);
            Draw(chart4, leftToe);
        }

        private void Draw(Chart chart, double[] buffer)
        {
            var s = chart.Series[0];
            s.Points.Clear();

            for (int i = 0; i < StepPoints; i++)
                s.Points.AddXY(i, buffer[i]);
        }

        private void AppendBalancePoint(double value)
        {
            var s = chart5.Series[0];
            s.Points.AddXY(timeIndex, value);

            if (s.Points.Count > BalanceWindowPoints)
                s.Points.RemoveAt(0);

            chart5.ChartAreas[0].AxisX.Minimum = Math.Max(0, timeIndex - BalanceWindowPoints);
            chart5.ChartAreas[0].AxisX.Maximum = timeIndex;

            timeIndex++;
        }

        private void ClearBalanceSeries()
        {
            if (chart5.Series.Count == 0) return;
            chart5.Series[0].Points.Clear();
            chart5.ChartAreas[0].AxisX.Minimum = 0;
            chart5.ChartAreas[0].AxisX.Maximum = BalanceWindowPoints;
        }

        private void DataMonitoringControl_Disposed(object sender, EventArgs e)
        {
            timer.Stop();
            timer.Dispose();
            DatabaseManager.Instance.CloseConnection();
        }

        private void btnStart_Click(object sender, EventArgs e) => timer.Start();
        private void btnStop_Click(object sender, EventArgs e) => timer.Stop();

        private void btnFreeze_Click(object sender, EventArgs e)
        {
            if (!isFrozen)
            {
                CreateFrozen(chart1);
                CreateFrozen(chart2);
                CreateFrozen(chart3);
                CreateFrozen(chart4);
                CreateFrozen(chart5);

                isFrozen = true;
                btnFreeze.Text = currentLanguage == "tr" ? "Çöz" : "Unfreeze";
            }
            else
            {
                RemoveFrozen(chart1);
                RemoveFrozen(chart2);
                RemoveFrozen(chart3);
                RemoveFrozen(chart4);
                RemoveFrozen(chart5);

                isFrozen = false;
                btnFreeze.Text = currentLanguage == "tr" ? "Dondur" : "Freeze";

                RedrawStepCharts();
            }
        }

        private void CreateFrozen(Chart chart)
        {
            if (chart.Series.IndexOf("Frozen") != -1)
                return;

            Series frozen = new Series("Frozen");
            frozen.ChartType = SeriesChartType.Line;
            frozen.BorderWidth = 2;
            frozen.Color = Color.Red;

            foreach (var p in chart.Series[0].Points)
                frozen.Points.AddXY(p.XValue, p.YValues[0]);

            chart.Series.Add(frozen);
        }

        private void RemoveFrozen(Chart chart)
        {
            if (chart.Series.IndexOf("Frozen") != -1)
                chart.Series.Remove(chart.Series["Frozen"]);
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "PNG|*.png";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Bitmap bmp = new Bitmap(this.Width, this.Height);
                    this.DrawToBitmap(bmp, new Rectangle(0, 0, this.Width, this.Height));
                    bmp.Save(dlg.FileName);

                    MessageBox.Show(currentLanguage == "tr"
                        ? "Kayıt başarılı!"
                        : "Saved successfully!");
                }
            }
        }

        private void btnCompare_Click(object sender, EventArgs e)
        {
            double a = rightHeel[pointIndex];
            double b = leftHeel[pointIndex];

            if (currentLanguage == "tr")
                MessageBox.Show(a > b ? "Sağ Topuk daha yüksek" : "Sol Topuk daha yüksek");
            else
                MessageBox.Show(a > b ? "Right heel is higher" : "Left heel is higher");
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            timer.Stop();

            Array.Clear(rightHeel, 0, rightHeel.Length);
            Array.Clear(leftHeel, 0, leftHeel.Length);
            Array.Clear(rightToe, 0, rightToe.Length);
            Array.Clear(leftToe, 0, leftToe.Length);

            timeIndex = 0;
            pointIndex = 0;
            stepCount = 0;
            historyCursor = 0;
            historyStartTime = null;
            historyTickSubCounter = 0;
            dbCursor = 0;
            dbSamples.Clear();

            RemoveFrozen(chart1);
            RemoveFrozen(chart2);
            RemoveFrozen(chart3);
            RemoveFrozen(chart4);
            RemoveFrozen(chart5);
            isFrozen = false;

            RedrawStepCharts();
            ClearBalanceSeries();

            lblStepCount.Text = currentLanguage == "tr"
                ? "Adım Sayısı: 0"
                : "Step Count: 0";

            btnFreeze.Text = currentLanguage == "tr" ? "Dondur" : "Freeze";
        }

        private void cmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentLanguage = cmbLanguage.SelectedIndex == 0 ? "tr" : "en";
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            if (currentLanguage == "tr")
            {
                label1.Text = "REHABİLİTASYON VERİSİ İZLEME";
                btnStart.Text = "BAŞLA";
                btnStop.Text = "Durdur";
                btnFreeze.Text = isFrozen ? "Çöz" : "Dondur";
                btnExport.Text = "Dışa Aktar";
                btnCompare.Text = "Karşılaştırma";
                btnClear.Text = "Temizle";

                lblStepCount.Text = $"Adım Sayısı: {stepCount}";

                lblSearch.Text = "Ara:";
                lblSession.Text = "Rehab:";
                btnLoadSession.Text = "Yükle";
                btnHistoryPlay.Text = "Geçmiş Oynat";
                btnLiveMode.Text = "Canlı Mod";
                lblGoto.Text = "Zaman (dk):";
                btnGoTime.Text = "Git";

                chart1.Titles.Clear(); chart1.Titles.Add("Sağ Topuk Sensörü");
                chart2.Titles.Clear(); chart2.Titles.Add("Sol Topuk Sensörü");
                chart3.Titles.Clear(); chart3.Titles.Add("Sağ Parmak Sensörü");
                chart4.Titles.Clear(); chart4.Titles.Add("Sol Parmak Sensörü");
                chart5.Titles.Clear(); chart5.Titles.Add("Denge Sensörü (Dalga)");
            }
            else
            {
                label1.Text = "REHABILITATION DATA MONITORING";
                btnStart.Text = "Start";
                btnStop.Text = "Stop";
                btnFreeze.Text = isFrozen ? "Unfreeze" : "Freeze";
                btnExport.Text = "Export";
                btnCompare.Text = "Compare";
                btnClear.Text = "Clear";

                lblStepCount.Text = $"Step Count: {stepCount}";

                lblSearch.Text = "Search:";
                lblSession.Text = "Rehab:";
                btnLoadSession.Text = "Load";
                btnHistoryPlay.Text = "Play History";
                btnLiveMode.Text = "Live Mode";
                lblGoto.Text = "Time (min):";
                btnGoTime.Text = "Go";

                chart1.Titles.Clear(); chart1.Titles.Add("Right Heel Sensor");
                chart2.Titles.Clear(); chart2.Titles.Add("Left Heel Sensor");
                chart3.Titles.Clear(); chart3.Titles.Add("Right Toe Sensor");
                chart4.Titles.Clear(); chart4.Titles.Add("Left Toe Sensor");
                chart5.Titles.Clear(); chart5.Titles.Add("Balance Sensor (Wave)");
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string q = (txtSearch.Text ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(q))
            {
                RefreshSessionList(rehabSessions);
                return;
            }

            var filtered = rehabSessions
                .Where(x => (x ?? "").ToLowerInvariant().Contains(q))
                .ToList();

            RefreshSessionList(filtered);
        }

        private void cmbRehabSessions_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedSessionId = cmbRehabSessions.SelectedItem?.ToString();
        }

    
        private void btnLoadSession_Click(object sender, EventArgs e)
        {
            int therapyId = 1; 
            LoadFromDatabase(therapyId);
        }

        private void btnHistoryPlay_Click(object sender, EventArgs e)
        {
            if (historySamples == null || historySamples.Count == 0)
            {
                MessageBox.Show(currentLanguage == "tr"
                    ? "Önce rehab verisini yükleyin."
                    : "Load rehab data first.");
                return;
            }

            currentMode = Mode.History;
            timer.Start();
        }

        private void btnLiveMode_Click(object sender, EventArgs e)
        {
            currentMode = Mode.Live;
            timer.Start();
        }

        private List<RehabSample> GenerateDemoHistory(int minutes)
        {
            var list = new List<RehabSample>();
            var start = DateTime.Now.Date.AddHours(10);

            int totalSeconds = Math.Max(10, minutes * 60);
            for (int s = 0; s < totalSeconds; s++)
            {
                double balance = 50 + 25 * Math.Sin(2.0 * Math.PI * s / 10.0);

                list.Add(new RehabSample
                {
                    Timestamp = start.AddSeconds(s),
                    RightHeel = 20 + rnd.NextDouble() * 60,
                    LeftHeel = 20 + rnd.NextDouble() * 60,
                    RightToe = 20 + rnd.NextDouble() * 60,
                    LeftToe = 20 + rnd.NextDouble() * 60,
                    Balance = Clamp(balance, 0, 100)
                });
            }
            return list;
        }

        private static double Clamp(double v, double lo, double hi)
        {
            if (v < lo) return lo;
            if (v > hi) return hi;
            return v;
        }

        private void btnGoTime_Click(object sender, EventArgs e)
        {
            int min = (int)nudMinute.Value;
            int sec = (int)nudSecond.Value;
            var t = new TimeSpan(0, 0, min, sec);

            GoToTime(t);
        }

        private void GoToTime(TimeSpan t)
        {
            int targetIndex;

            if (currentMode == Mode.Live)
            {
                targetIndex = (int)(t.TotalSeconds * LiveFps);
            }
            else
            {
                targetIndex = (int)(t.TotalSeconds);

                if (historySamples != null && historySamples.Count > 0 && historyStartTime != null)
                {
                    DateTime wanted = historyStartTime.Value.AddSeconds(targetIndex);
                    int idx = historySamples.FindIndex(x => x.Timestamp >= wanted);
                    if (idx >= 0) historyCursor = idx;
                }
            }

            var area = chart5.ChartAreas[0];

            double minX = Math.Max(0, targetIndex - BalanceWindowPoints);
            double maxX = Math.Max(BalanceWindowPoints, targetIndex);

            area.AxisX.Minimum = minX;
            area.AxisX.Maximum = maxX;

            try
            {
                area.AxisX.ScaleView.ZoomReset();
                area.AxisX.ScaleView.Zoom(minX, maxX);
            }
            catch { }
        }

        private void AttachZoom(Chart chart)
        {
            chart.MouseWheel += Chart_MouseWheel;
            chart.MouseMove += Chart_MouseMove;
        }

        private void Chart_MouseWheel(object sender, MouseEventArgs e)
        {
            var chart = sender as Chart;
            var area = chart.ChartAreas[0];

            try
            {
                double xMin = area.AxisX.ScaleView.ViewMinimum;
                double xMax = area.AxisX.ScaleView.ViewMaximum;
                double pos = area.AxisX.PixelPositionToValue(e.Location.X);

                if (e.Delta < 0)
                    area.AxisX.ScaleView.ZoomReset();
                else
                {
                    double newSize = (xMax - xMin) * 0.7;
                    if (double.IsNaN(newSize) || newSize <= 0) return;
                    area.AxisX.ScaleView.Zoom(pos - newSize / 2, pos + newSize / 2);
                }
            }
            catch { }
        }

        private void Chart_MouseMove(object sender, MouseEventArgs e)
        {
            var chart = sender as Chart;
            var hit = chart.HitTest(e.X, e.Y);

            if (hit.ChartElementType == ChartElementType.DataPoint)
            {
                var p = chart.Series[hit.Series.Name].Points[hit.PointIndex];
                chart.Series[0].ToolTip = $"X={p.XValue}, Y={p.YValues[0]}";
            }
            else
                chart.Series[0].ToolTip = "";
        }
        private void chart1_Click(object sender, EventArgs e) { }
        private void chart2_Click(object sender, EventArgs e) { }
        private void chart3_Click(object sender, EventArgs e) { }
        private void chart4_Click(object sender, EventArgs e) { }
        private void chart5_Click(object sender, EventArgs e) { }

        private class RehabSample
        {
            public DateTime Timestamp { get; set; }
            public double RightHeel { get; set; }
            public double LeftHeel { get; set; }
            public double RightToe { get; set; }
            public double LeftToe { get; set; }
            public double Balance { get; set; }
        }

      
    }
}

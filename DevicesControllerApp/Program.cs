using System;
using System.Windows.Forms;
using DevicesControllerApp.Ana_ekran_Login;

namespace DevicesControllerApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var login = new form1()) // Login formu
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    MainForm main = new MainForm(login.SecilenDil); // artık erişilebilir
                    main.SetKullaniciAdi(login.GeciciKullaniciAdi);
                    Application.Run(main);
                }

                else
                {
                    Application.Exit();
                }
            }
        }
    }
}

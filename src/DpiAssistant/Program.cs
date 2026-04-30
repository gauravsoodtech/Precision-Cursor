using System;
using System.Windows.Forms;

namespace DpiAssistant
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                using (TrayAppContext context = new TrayAppContext())
                {
                    Application.Run(context);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    AppInfo.ProductName + " could not start.\r\n\r\n" + ex.Message,
                    AppInfo.ProductName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}

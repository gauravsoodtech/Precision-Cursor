using System;
using System.Windows.Forms;

namespace MouseLineLock
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
                    "Mouse Line Lock could not start.\r\n\r\n" + ex.Message,
                    "Mouse Line Lock",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}

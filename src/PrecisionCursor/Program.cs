using System;
using System.Windows.Forms;

namespace PrecisionCursor
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
                    "Precision Cursor could not start.\r\n\r\n" + ex.Message,
                    "Precision Cursor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}

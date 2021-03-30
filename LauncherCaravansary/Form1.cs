using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace LauncherCaravansary
{
    public partial class Form1 : Form
    {


        static string appdataAPPDATA_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static string appdataCFOLDER_PATH = Path.Combine(appdataAPPDATA_PATH, "Caravansary" + Path.DirectorySeparatorChar);


        static string dumpFileLocation = Path.Combine(appdataCFOLDER_PATH, "dump.txt");



        string launcherApplicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        static string launcherApplicationDirectoryPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString();


        static string AppdataCLauncherPathZip = appdataCFOLDER_PATH + Path.DirectorySeparatorChar + "LauncherCaravansary.zip";
        static string AppdataCLauncherPathExe = appdataCFOLDER_PATH + Path.DirectorySeparatorChar + "LauncherCaravansary.exe";

        static string appdataAppPath = appdataCFOLDER_PATH + Path.DirectorySeparatorChar + "Caravansary.exe";

        static string appdataZipPath = appdataCFOLDER_PATH + "x64Caravansary.zip";

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        private const UInt32 WM_CLOSE = 0x0010;

        void CloseWindow(IntPtr hwnd)
        {
            SendMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        List<string> args;

        List<string> returnedArgs;

        private bool ArgsContain(string str, out string result)
        {


            for (int i = args.Count - 1; i >= 0; i--)
            {
                args[i] = args[i].ToLower().Trim();


                if (args[i].Contains(str.ToLower().Trim()))
                {
                    result = args[i];
                    return true;
                }
            }


            result = null;
            return false;
        }

        private bool ArgsContain(string str, string withoutString, out string result)
        {


            for (int i = args.Count - 1; i >= 0; i--)
            {
                args[i] = args[i].ToLower().Trim();

                if (args[i].Contains(withoutString.ToLower().Trim()))
                {
                    continue;
                }
                if (args[i].Contains(str.ToLower().Trim()))
                {
                    result = args[i];
                    return true;
                }
            }


            result = null;
            return false;
        }

        string _mainAppLocationDir;
        string GetMainAppLocationDir()
        {
            if (_mainAppLocationDir == null)
            {

                bool hasLocationArg = ArgsContain("Caravansary", "LauncherCaravansary", out string locationArg);

                if (hasLocationArg)
                {
                    _mainAppLocationDir = locationArg;
                }

                if (File.Exists(appdataAppPath))
                {
                    _mainAppLocationDir = Directory.GetParent(appdataAppPath).FullName;
                }

                _mainAppLocationDir = @".\";
            }


            return _mainAppLocationDir;
        }

        string _mainAppLocationExe;
        string GetMainAppLocationExe()
        {
            if (_mainAppLocationExe == null) _mainAppLocationExe = GetMainAppLocationDir() + Path.DirectorySeparatorChar + "Caravansary.exe";
            return _mainAppLocationExe;
        }

        private void StartMainApp()
        {
            string stringArg = "";

            foreach (var item in returnedArgs)
            {
                stringArg = item + " ";
            }


            Process.Start(GetMainAppLocationExe(), stringArg);
            this.Close();

        }

        public Form1()
        {
            InitializeComponent();

        }


        private async void Form1_Shown(object sender, EventArgs e)
        {
            args = Environment.GetCommandLineArgs().ToList();
            returnedArgs = new List<string>();
            returnedArgs.Add("StartedThroughLauncher");

            this.topLabel.Text = "Launching caravansary...";

            //debug label
            args.Add("RequstedUpdate");

            if (args.Contains("RequstedUpdate"))
            {

                bool res = await UpdateMainApp();
                if (!res) return;

                returnedArgs.Add("Updated");
                StartMainApp();

            }
            else
            {
                var appPath = GetMainAppLocationExe();
                if (!File.Exists(appPath))
                {
                    await UpdateMainApp();
                    returnedArgs.Add("DownloadedEXEFromLauncherToAppdata");
                    StartMainApp();
                }
                else
                {
                    StartMainApp();


                }


            }

        }
        private void Form1_Load(object sender, EventArgs e)
        {


        }


        private bool DeleteMainApplication()
        {


            int tries = 0;

            while (tries < 20)
            {
                if (tries > 0)
                {
                    System.Threading.Thread.Sleep(1000);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                }

                try
                {
                    if (File.Exists(GetMainAppLocationExe()))
                    {

                        File.Delete(GetMainAppLocationExe());
                        return true;
                    }

                    return true;
                }
                catch
                {
                    // File.WriteAllText(dumpFileLocation, "Can't delete Caravansary.exe");
                    tries++;
                    UpdateLabelText(progressText, "Deleting main app failed..." + tries + " times");
                }

            }

            return false;

        }
        private async Task<bool> DownloadNewVersion()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    UpdateLabelText(this.progressText, "Downloading...");

                    await Task.Run(() => client.DownloadFile("https://github.com/RobertJaskowski/Caravansary/releases/latest/download/x64Caravansary.zip", appdataZipPath));

                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> ExtractZip()
        {

            UpdateLabelText(this.progressText, "Extracting...");

            try
            {


                //string tempExtractingPathToExe = Path.Combine(appdataCFOLDER_PATH, @"Caravansary.exe");
                //string tempExtractingPathToUpdaterExe = Path.Combine(appdataCFOLDER_PATH, @"LauncherCaravansary.exe");

                //if (File.Exists(tempExtractingPathToExe))
                //{
                //    File.Delete(tempExtractingPathToExe);
                //    File.Delete(tempExtractingPathToUpdaterExe);
                //}



                using (var archive = ZipFile.OpenRead(appdataZipPath))
                {

                    foreach (var entry in archive.Entries)
                    {

                        entry.ExtractToFile(Path.Combine(GetMainAppLocationDir(), entry.FullName), true);
                    }
                }


                //ZipFile.ExtractToDirectory(zipPath, appdataCFOLDER_PATH);

                //FileInfo fi = new FileInfo(tempExtractingPathToExe);
                //fi.CopyTo(GetMainAppLocationDir(), true);



                File.Delete(appdataZipPath);


                return true;
            }
            catch
            {
                return false;
            }

        }


        private async Task<bool> UpdateMainApp()
        {
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            UpdateLabelText(progressText, "Updating Caravansary...");


            bool deleteSuccess = await Task.Run(() => DeleteMainApplication());
            if (!deleteSuccess)
            {
                this.progressText.Text = "Couldn't delate app";
                retryButton.Visible = true;
                closeButton.Visible = true;
                return false;
            }

            bool downloadSuccess = await Task.Run(() => DownloadNewVersion());
            if (!downloadSuccess)
            {
                this.progressText.Text = "Couldn't download app";
                retryButton.Visible = true;
                closeButton.Visible = true;
                return false;
            }

            bool extractSuccess = await Task.Run(() => ExtractZip());
            if (!extractSuccess)
            {
                this.progressText.Text = "Couldn't extract app";
                retryButton.Visible = true;
                closeButton.Visible = true;
                return false;
            }

            UpdateLabelText(progressText, "");
            UpdateLabelText(topLabel, "Finished updating...");

            return true;
        }

        private async void retryButton_Click(object sender, EventArgs e)
        {

            retryButton.Visible = false;
            closeButton.Visible = false;
            await UpdateMainApp();
            returnedArgs.Add("DownloadedEXEFromLauncherToAppdata");
            StartMainApp();


        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void UpdateLabelText(Label label, string textToUpdate)
        {
            label.Invoke((MethodInvoker)delegate { label.Text = textToUpdate; });

        }

    }
}

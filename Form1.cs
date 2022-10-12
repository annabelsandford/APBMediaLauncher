//            !!!"..?!!.' ......        !!!!!!! 
//           !!! e2$ .<!!!!!!!!!`~!~!!!!!!~! ""!`.` 
//           !!!!^:!!!!!!!!!!!!!!.:!!!!!!!!! *@ !4:'
//          . >! !!!!!!!!!!!!!!!!!:^:!!!!!!!!:  J!: 
//          .!! ,<!!!!!!!!!!!!!...`*."!!!!!!!!!!.~~
//          !!~!!!!!!!!!f !!!! #$$$$$$b`!!!!!L!!!(  
//         !!! ! !!!!! !>b"!!!!. ^$$$*"!!~!4!!!!!!`x 
//        .!!!! !`!! d "= "$N !!f u `!!!~' !!!!!!!!! 
//        !!!!!  !XH.=m" C..^*$!.  .~L:u@ !! !!!!~:` 
//       !!!!!   '`"*:$$P k  $$$$e$R""" mee"<!!!!!  
//      :!!!!"    $N $$$  * x$$$$$$$   <-m.` !!!!!'<! 
//     .!!!!f     "$ $$$.  u$$$$$$$e $ : ee `  !`:!!!`
//     !!!!!.        $$$$$$$$$$$ $$   u$$" r'    !!!!!             ~4
//    !!!!!          "$$$$$$&""%$$$ee$$$ @"      !!!!!h            $b`
//   !!!!!             $$$$     $$$$$$$           !!!!!           @$ 
//  !!!!! X             "&$c   $$$$$"              !!!!!       `e$$
// !!!!! !              $$."***""                   !!!!h     z$$$$$$$$$$$$$$eJ
//!!!!! !!     .....     ^"'$$$            $         !!!!    J$$$$$$$$$$$"
//!!!! !!  .d$$$$$$$$$$e( <  d            4$          ~!!! z$$F$$$$$$$$$$b
//!!! !!  J$$$$$$*****$$$$. "J<=    t'b  `)$b' ,C)`    `!~@$$$$$J'$$$$$$$
//!!~:!   $$$$"e$$$$$$$$c"$N". - ". :F$ ?P"$$$ #$$      .$$$$$$$FL$$$$$$$
//!`:!    $$"$$$$$$$$$$$$$$e $$$.   '>@ z$&$$$eC$"    .d$$$$$$$P      "*$$.
// !!     #$$$$$$$*"zddaaa""e^*F""*. "$ $$P.#$$$$E:: d$$$$$$$$           ^$ 
//!!~      ;$$$$"d$$$$$$$$$$$$$u       $c#d$$@$\$>`x$$$$$$$$"             "c
//!!        ;e?(."$$$$$$$$$$$$$$$$u     "$NJ$$$d"x$$$$$$$$$ 

// Written by Annabel Jocelyn Sandford (@annie_sandford)
// Do not remove (:

using System.Diagnostics;
using System.Drawing;
using System.Net;

namespace APBML2
{
    public partial class Form1 : Form
    {
        public bool openPanel = false;
        public bool firstRun = false;
        public string APBMLVersion = "1.0";
        public string nwln = Environment.NewLine;
        public string apbml_path = "";
        
        public Form1()
        {
            InitializeComponent();
            checkInternetConnection();
            checkForAPB();
            checkInstallation();
            if (firstRun == true)
            {
                panelButton.Visible = false;
            }
        }

        private void panelButton_Click(object sender, EventArgs e)
        {
            openPanel = true;
            // hide panelButton
            panelButton.Visible = false;
            panelFeedback.Visible = true;

            // open MediaLauncherPanel Form and hide this Form
            MediaLauncherPanel panel = new MediaLauncherPanel();
            panel.Show();
            this.Hide();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void firstRunInit() {
            FirstRunWindow panel = new FirstRunWindow();
            panel.Show();
            this.Hide();
        }

        private void checkForAPB() {
            // check if APB.exe is running, if yes give error
            Process[] pname = Process.GetProcessesByName("APB");
            if (pname.Length > 0)
            {
                MessageBox.Show("APB.exe is running, please close it before using APBML.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        private void checkInternetConnection() {
            try {
                using (var client = new WebClient())
                using (client.OpenRead("http://clients3.google.com/generate_204")) {
                    // do absolutely nothing
                }
            }
            catch {
                // show error message and quit application
                MessageBox.Show("You are not connected to the internet. Please connect to the internet and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void checkInstallation() {
            // check if "firstRun.anna" exists, if not, create it
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string firstRunFile = appdata + "\\APBML\\config.ini";
            if (!File.Exists(firstRunFile)) {
                firstRun = true;
                // stop using file
                //File.Create(firstRunFile).Dispose();
                firstRunInit();
            } else {
                this.Load += (s, e) => { Task.Delay(3000).ContinueWith(t => initializeMusic()); };
            }
        }

        private void checkMusicSyncVer()
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string apbml_folder = appdata + "\\APBML";
            string apbml_config = apbml_folder + "\\config.ini";
            string[] config = File.ReadAllLines(apbml_config);
            string apb_directory = config[3];
            string apb_media_folder = apb_directory + "\\APBGame\\Content\\Audio\\DefaultMusicLibrary";
            string apbml_ver = apb_media_folder + "\\mlver.ini";
            string ver_num = null;

            if (File.Exists(apbml_ver)) {
                // read first line
                string[] ver = File.ReadAllLines(apbml_ver);
                ver_num = ver[0];
            } 

            // get source-code of https://raw.githubusercontent.com/annabelsandford/APBMediaLauncher/main/Media/mlver.ini
            string mlver = "https://raw.githubusercontent.com/annabelsandford/APBMediaLauncher/main/Media/mlver.ini";
            string mlver_num = new WebClient().DownloadString(mlver);
            
            if (mlver_num == ver_num) {
                // library is up-to-date
                MethodInvoker action_visible = () => panelFeedback.Visible = true;
                MethodInvoker action_uptodate = () => panelFeedback.Text = "Library up-to-date! Starting APB...";
                panelFeedback.BeginInvoke(action_visible);
                panelFeedback.BeginInvoke(action_uptodate);
                startAPB();
            } else {
                // library needs update
                MethodInvoker action_visible = () => panelFeedback.Visible = true;
                MethodInvoker action_uptodate = () => panelFeedback.Text = "Syncing library... (Do not quit!)";
                panelFeedback.BeginInvoke(action_visible);
                panelFeedback.BeginInvoke(action_uptodate);
                updateSyncFromServer();
                startAPB();
            }
        }

        private void startAPB() {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string apbml_folder = appdata + "\\APBML";
            string apbml_config = apbml_folder + "\\config.ini";
            string[] config = File.ReadAllLines(apbml_config);
            string apb_binary = config[4];

            try {
                // start APB as admin
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = apb_binary;
                startInfo.Verb = "runas";
                Process.Start(startInfo);

                // wait 5 seconds then close
                Thread.Sleep(10000);
                Application.Exit();
            } catch {
                // show error message and quit application
                MessageBox.Show("Could not start APB. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void updateSyncFromServer()
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string apbml_folder = appdata + "\\APBML";
            string apbml_config = apbml_folder + "\\config.ini";
            string[] config = File.ReadAllLines(apbml_config);
            string apb_directory = config[3];
            string apb_binary = config[4];
            string apb_media_folder = apb_directory + "\\APBGame\\Content\\Audio\\DefaultMusicLibrary";

            string server_library_tree = "https://raw.githubusercontent.com/annabelsandford/APBMediaLauncher/main/Media/tree.ini";
            // get each line of tree.ini
            string[] tree = new WebClient().DownloadString(server_library_tree).Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            // check if each file is in the library
            foreach (string file in tree) {
                // replace .anna extension of file to .mp3
                string file_mp3 = file.Replace(".anna", ".mp3");

                string server_file = "https://raw.githubusercontent.com/annabelsandford/APBMediaLauncher/main/Media/" + file;
                string local_file = apb_media_folder + "\\" + file_mp3;
                if (!File.Exists(local_file)) {
                    // download file
                    new WebClient().DownloadFile(server_file, local_file);
                }
            }

            // download mlver.ini
            string server_mlver = "https://raw.githubusercontent.com/annabelsandford/APBMediaLauncher/main/Media/mlver.ini";
            string server_xml = "https://raw.githubusercontent.com/annabelsandford/APBMediaLauncher/main/Media/defaultmusiclibrary.xml";
            string local_mlver = apb_media_folder + "\\mlver.ini";
            string local_xml = apb_media_folder + "\\defaultmusiclibrary.xml";
            new WebClient().DownloadFile(server_mlver, local_mlver);
            new WebClient().DownloadFile(server_xml, local_xml);
            
            MethodInvoker action_uptodate = () => panelFeedback.Text = "Library synced! Starting APB...";
            panelFeedback.BeginInvoke(action_uptodate);
            GC.Collect();
        }

        private void initializeMusic() {
            MethodInvoker action = () => panelButton.Visible = false;
            panelButton.BeginInvoke(action);

            if (openPanel == false) {
                checkMusicSyncVer();
            }
        }

    }
}
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
// 12.10.2022
// Matt where content updates? :c

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace APBML2
{
    public partial class MediaLauncherPanel : Form
    {
        string changed_directory; string changed_binary;

        public MediaLauncherPanel()
        {
            InitializeComponent();
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MediaLauncherPanel_FormClosing);
            progress_label.Text = "";
            checkConfiguration();
        }

        // if closing window, restart application
        private void MediaLauncherPanel_FormClosing(object sender, FormClosingEventArgs e)
        {
            // tell user to restart application
            MessageBox.Show("Please restart the application to apply changes.", "Restart Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Application.Exit();
        }

        private void improperConfiguration() {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string apbml_folder = appdata + "\\APBML";
            string apbml_config = apbml_folder + "\\config.ini";

            // if apbml_config exists, delete
            if (File.Exists(apbml_config)) {
                File.Delete(apbml_config);
            }

            MessageBox.Show("APB:ML is not properly configured. Please restart APB:ML.", "APB:ML - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
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
                label5.Text = "Library up to date.";
                MessageBox.Show("You are up to date!", "APB:ML - Music Sync", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } else {
                // messagebox option to update or cancel
                label5.Text = "Library sync available.";
                DialogResult dialogResult = MessageBox.Show("There is a new version of the Music Sync available. Would you like to update?", "APB:ML - Music Sync", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dialogResult == DialogResult.Yes) {
                    updateSyncFromServer();
                }
            }
        }

        private void updateSyncFromServer()
        {
            label5.Text = "Library Sync in progress...";
            progress_label.Text = "Downloading files...";

            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string apbml_folder = appdata + "\\APBML";
            string apbml_config = apbml_folder + "\\config.ini";
            string[] config = File.ReadAllLines(apbml_config);
            string apb_directory = config[3];
            string apb_media_folder = apb_directory + "\\APBGame\\Content\\Audio\\DefaultMusicLibrary";

            string server_library_tree = "https://raw.githubusercontent.com/annabelsandford/APBMediaLauncher/main/Media/tree.ini";
            // get each line of tree.ini
            string[] tree = new WebClient().DownloadString(server_library_tree).Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            // count lines and set as progressbar
            int line_count = tree.Length;
            progressBar1.Maximum = line_count+1; // +1 for the mlver.ini
            progress_label.Text = "Syncing files (" + progressBar1.Value + "/" + progressBar1.Maximum + ")";

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
                progressBar1.Value++;
                progress_label.Text = "Syncing files (" + progressBar1.Value + "/" + progressBar1.Maximum + ")";
            }

            // download mlver.ini
            string server_mlver = "https://raw.githubusercontent.com/annabelsandford/APBMediaLauncher/main/Media/mlver.ini";
            string server_xml = "https://raw.githubusercontent.com/annabelsandford/APBMediaLauncher/main/Media/defaultmusiclibrary.xml";
            string local_mlver = apb_media_folder + "\\mlver.ini";
            string local_xml = apb_media_folder + "\\defaultmusiclibrary.xml";
            new WebClient().DownloadFile(server_mlver, local_mlver);
            new WebClient().DownloadFile(server_xml, local_xml);
            progressBar1.Value++;
            progress_label.Text = "Sync complete!";

            // messagebox to say sync complete
            label5.Text = "Library up to date.";
            MessageBox.Show("Sync complete!", "APB:ML - Music Sync", MessageBoxButtons.OK, MessageBoxIcon.Information);
            progressBar1.Value = 0;
            progress_label.Text = "";
            GC.Collect();
        }

        private void checkConfiguration() {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string apbml_folder = appdata + "\\APBML";
            string apbml_config = apbml_folder + "\\config.ini";

            // check if apbml_config exists, if not, error message
            if (!File.Exists(apbml_config)) {
                improperConfiguration();
            } else {
                // get first line of config.ini as string "music_sync"
                string[] config = File.ReadAllLines(apbml_config);
                string music_sync = config[0];
                string clear_logs = config[1];
                string clear_temp = config[2];
                string apb_directory = config[3];
                string apb_binary = config[4];

                changed_directory = apb_directory;
                changed_binary = apb_binary;

                // check if music_sync, clear_logs, clear_temp are true or false, if neither error
                if (music_sync == "True") {
                    check_musicsync.Checked = true;
                } else if (music_sync == "False") {
                    check_musicsync.Checked = false;
                } else {
                    improperConfiguration();
                }

                if (clear_logs == "True") {
                    check_logs.Checked = true;
                } else if (clear_logs == "False") {
                    check_logs.Checked = false;
                } else {
                    improperConfiguration();
                }

                if (clear_temp == "True") {
                    check_temp.Checked = true;
                } else if (clear_temp == "False") {
                    check_temp.Checked = false;
                } else {
                    improperConfiguration();
                }

                // check if directory apb_directory exists, if yes, set textbox1 text to apb_directory
                if (Directory.Exists(apb_directory)) {
                    textBox1.Text = apb_directory;
                } else {
                    improperConfiguration();
                }

                // check if file apb_binary exists, if yes, set textbox2 text to apb_binary
                if (File.Exists(apb_binary)) {
                    textBox2.Text = apb_binary;
                } else {
                    improperConfiguration();
                }
            }
        }

        private void configurationToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        // change directory path button
        private void button3_Click(object sender, EventArgs e)
        {
            // open folder browser with title "Select APB Directory"
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.Description = "Select APB Directory";
            folderBrowserDialog1.ShowNewFolderButton = false;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                // set apb_folder to selected folder
                changed_directory = folderBrowserDialog1.SelectedPath;
                // set textbox1 to selected folder
                textBox1.Text = changed_directory;
            }
        }

        // change binary path button
        private void button4_Click(object sender, EventArgs e)
        {
            // open file browser with title "Select APB Launcher/Binary"
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select APB Launcher/Binary";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // set apb_launcher to selected file
                changed_binary = openFileDialog1.FileName;
                // set textbox2 to selected file
                textBox2.Text = changed_binary;
            }
        }

        // Save new settings
        private void button5_Click(object sender, EventArgs e)
        {
            // fill progressbar1 100%
            progressBar1.Maximum = 100;
            progressBar1.Value = 100;

            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string apbml_folder = appdata + "\\APBML";
            string apbml_config = apbml_folder + "\\config.ini";
            string[] config = File.ReadAllLines(apbml_config);
            string music_sync = config[0];

            string[] new_settings = { music_sync, check_logs.Checked.ToString(), check_temp.Checked.ToString(), changed_directory, changed_binary };
            File.WriteAllLines(apbml_config, new_settings);
            MessageBox.Show("Settings saved.", "APB:ML - Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // reset progressbar1
            progressBar1.Value = 0;
            GC.Collect();
        }

        // Save Music Sync Settings
        private void button6_Click(object sender, EventArgs e)
        {
            progressBar1.Maximum = 100;
            progressBar1.Value = 100;
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string apbml_folder = appdata + "\\APBML";
            string apbml_config = apbml_folder + "\\config.ini";
            string[] config = File.ReadAllLines(apbml_config);
            string music_sync = check_musicsync.Checked.ToString();
            string clear_logs = config[1];
            string clear_temp = config[2];
            string apb_directory = config[3];
            string apb_binary = config[4];

            string[] new_settings = { music_sync, clear_logs, clear_temp, apb_directory, apb_binary };
            File.WriteAllLines(apbml_config, new_settings);
            MessageBox.Show("Settings saved.", "APB:ML - Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            progressBar1.Value = 0;
            GC.Collect();
        }

        // Synchronize Music with Server (Check)
        private void button7_Click(object sender, EventArgs e)
        {
            checkMusicSyncVer();
        }

        // Restore Backup (defaultmusiclibrary.xml)
        private void restoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try {
                string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string apbml_folder = appdata + "\\APBML";
                string apbml_config = apbml_folder + "\\config.ini";
                string[] config = File.ReadAllLines(apbml_config);
                string apb_directory = config[3];

                string apb_media_folder = apb_directory + "\\APBGame\\Content\\Audio\\DefaultMusicLibrary";
                string apb_music_backup = apbml_folder + "\\Backup\\defaultmusiclibrary.xml";
                string apbml_ver_ini = apb_directory + "\\APBGame\\Content\\Audio\\DefaultMusicLibrary\\mlver.ini";

                if (File.Exists(apb_music_backup)) {
                    // copy file as admin
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = "/C copy /Y \"" + apb_music_backup + "\" \"" + apb_media_folder + "\"";
                    startInfo.Verb = "runas";
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    Process.Start(startInfo);

                    // delete mlver.ini if exists
                    if (File.Exists(apbml_ver_ini)) {
                        File.Delete(apbml_ver_ini);
                    }

                    //File.Copy(apb_media_folder, apb_music_backup, true);
                    MessageBox.Show("Backup restored.", "APB:ML - Restore Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } else {
                    MessageBox.Show("No backup found.", "APB:ML - Restore Backup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "APB:ML - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

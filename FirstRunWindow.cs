using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Principal;

namespace APBML2
{
    public partial class FirstRunWindow : Form
    {
        bool apb_steam = false;
        bool apb_g1 = false;
        string apb_version = "";

        string apb_folder = "";
        string apb_launcher = "";

        public FirstRunWindow()
        {
            InitializeComponent();

            // Ridiculously important attributes
            this.TopMost = true;
            this.ControlBox = false;

            checkGameVersion();
        }

        private void FirstRunWindow_Load(object sender, EventArgs e)
        {
            // empty. like rockstars plans for gta 5 singleplayer content.
        }

        private void checkGameVersion() {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (programFiles == null) {
                programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            }
            // check if APBML folder exists, if not, create it
            string apb_steam_path = programFiles + "\\Steam\\steamapps\\common\\APB Reloaded";
            string apb_g1_path = programFiles + "\\Gamersfirst\\APB Reloaded";
            if (Directory.Exists(apb_steam_path)) {
                apb_steam = true;
            }
            if (Directory.Exists(apb_g1_path)) {
                apb_g1 = true;
            }
            if (apb_steam == true && apb_g1 == true) {
                apb_version = "both";
            } else if (apb_steam == true && apb_g1 == false) {
                apb_version = "steam";
            } else if (apb_steam == false && apb_g1 == true) {
                apb_version = "g1";
            } else {
                apb_version = "none";
            }

            if (apb_version == "both") {
                // create messagebox with 3 options
                DialogResult result = MessageBox.Show("It seems like you have both Steam and Gamersfirst versions of APB installed. Which one do you want to use? (YES = STEAM // NO = GAMERSFIRST)", "APB:ML - Version Challenge", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes) {
                    apb_version = "steam";
                } else if (result == DialogResult.No) {
                    apb_version = "g1";
                } else {
                    apb_version = "none";
                }
            }

            if (apb_version == "none") {
                //no apb version found
            }

            if (apb_version == "steam") {
                apb_folder = apb_steam_path;
                apb_launcher = apb_folder + "\\Binaries\\APB.exe";
                // change textbox1 and textbox2
                textBox1.Text = apb_folder;
                textBox2.Text = apb_launcher;

                label_gameversion.Text = "Steam Version Determined";
            } else if (apb_version == "g1") {
                apb_folder = apb_g1_path;
                apb_launcher = apb_folder + "\\Binaries\\APB.exe";
                // change textbox1 and textbox2
                textBox1.Text = apb_folder;
                textBox2.Text = apb_launcher;

                label_gameversion.Text = "G1 Version Determined";
            }

            // determined versions here
        }

        // File Chooser for APB Directory
        private void button1_Click(object sender, EventArgs e)
        {
            // open folder browser with title "Select APB Directory"
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.Description = "Select APB Directory";
            folderBrowserDialog1.ShowNewFolderButton = false;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                // set apb_folder to selected folder
                apb_folder = folderBrowserDialog1.SelectedPath;
                // set textbox1 to selected folder
                textBox1.Text = apb_folder;
                // set apb_launcher to selected folder + \Binaries\APB.exe
                apb_launcher = apb_folder + "\\Binaries\\APB.exe";
                // set textbox2 to apb_launcher
                textBox2.Text = apb_launcher;

                label_gameversion.Text = "User APB Directory Selected"; 
            }
        }

        // File Chooser for Launcher/Binary
        private void button2_Click(object sender, EventArgs e)
        {
            // open file browser with title "Select APB Launcher/Binary"
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select APB Launcher/Binary";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // set apb_launcher to selected file
                apb_launcher = openFileDialog1.FileName;
                // set textbox2 to selected file
                textBox2.Text = apb_launcher;
            }
        }

        // Save and Continue Button here
        private void button3_Click(object sender, EventArgs e)
        {
           // check if checkbox3 is checked, if no then show messagebox
           if (checkBox3.Checked == false) {
            MessageBox.Show("You must agree to the terms of service to use APB:ML.", "APB:ML - Terms of Service", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } else {
                try {
                    // if checkbox1 is checked, set clear_logs bool to true else to false (same for checkbox2)
                    bool clear_logs = false;
                    bool clear_temp = false;
                    if (checkBox1.Checked == true) {
                        clear_logs = true;
                    }
                    if (checkBox2.Checked == true) {
                        clear_temp = true;
                    }

                    string[] config = { "True", clear_logs.ToString(), clear_temp.ToString(), apb_folder, apb_launcher };

                    // save apb_folder and apb_launcher to config file in %appdata%\APBML
                    string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string apbml_folder = appdata + "\\APBML";
                    string apbml_config = apbml_folder + "\\config.ini";
                    string apbml_config_2 = apbml_folder + "\\paths.ini";
                    if (!Directory.Exists(apbml_folder)) {
                        Directory.CreateDirectory(apbml_folder);
                    }
                    if (!File.Exists(apbml_config)) {
                        File.WriteAllLines(apbml_config, config);
                    }

                    try {
                        string apb_media_folder = apb_folder + "\\APBGame\\Content\\Audio\\DefaultMusicLibrary";
                        string apbml_backup_folder = appdata + "\\APBML\\Backup";
                        // check if folder exists
                        if (Directory.Exists(apb_media_folder)) {
                            // check if backup folder exists, if not create
                            if (!Directory.Exists(apbml_backup_folder)) {
                                Directory.CreateDirectory(apbml_backup_folder);
                            }
                            // check if "defaultmusiclibrary.xml" exists in media folder, if yes then copy to backup folder
                            if (File.Exists(apb_media_folder + "\\defaultmusiclibrary.xml")) {
                                File.Copy(apb_media_folder + "\\defaultmusiclibrary.xml", apbml_backup_folder + "\\defaultmusiclibrary.xml", true);
                            } else {
                                MessageBox.Show("Your APB Directory seems to be wrong or your defaultmusiclibrary.xml is corrupt. Please restart APB:ML.", "APB:ML - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                if (File.Exists(apbml_config)) {
                                    File.Delete(apbml_config);
                                }
                                Application.Exit();
                            }
                        }
                    } catch (Exception ex2) {
                        MessageBox.Show("An error occured while trying to backup your defaultmusiclibrary.xml. Please restart APB:ML.", "APB:ML - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        if (File.Exists(apbml_config)) {
                            File.Delete(apbml_config);
                        }
                        Application.Exit();
                    }
                    
                    // change permission of folder apb_media_folder to allow everyone to read and write
                    try {
                        string apb_media_folder = apb_folder + "\\APBGame\\Content\\Audio\\DefaultMusicLibrary";
                        //var ds = new FileInfo(apb_media_folder).GetAccessControl();

                        // var owner this user
                        var owner = new NTAccount(Environment.UserName);

                        var ds = new DirectorySecurity();
                        ds.AddAccessRule(new FileSystemAccessRule(owner, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                        ds.SetAccessRuleProtection(true, false); // disable inheritance and clear any inherited permissions
                        System.IO.FileSystemAclExtensions.SetAccessControl(new DirectoryInfo(apb_media_folder), ds);
                    } catch (Exception ex3) {
                        MessageBox.Show("An error occured while trying to change the permissions of your defaultmusiclibrary.xml. Please restart APB:ML. Error: ("+ex3+")", "APB:ML - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        if (File.Exists(apbml_config)) {
                            File.Delete(apbml_config);
                        }
                        Application.Exit();
                    }

                    // show form1
                    Form1 form1 = new Form1();
                    form1.Show();

                    GC.Collect();
                    this.Close();
                } catch (Exception ex) {
                    MessageBox.Show("An error occured while saving your settings. Please try again. If this error persists, please contact us. (Error: "+ex+"", "APB:ML - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Cancel Button
        private void button4_Click(object sender, EventArgs e)
        {
            // show messagebox, if yes quit entire application
            DialogResult result = MessageBox.Show("Are you sure you want to quit APB:ML?", "APB:ML - Quit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes) {
                Application.Exit();
            }
        }
    }
}

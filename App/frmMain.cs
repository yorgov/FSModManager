using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FSModManager
{
    public partial class frmMain : Form
    {
        private string? modsFolderPath;
        private string? gamesModsFolderPath;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            
        }



        private void btn_BrowseModsFolder_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            dlg.ShowNewFolderButton = false;            
            dlg.RootFolder = Environment.SpecialFolder.MyDocuments;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                modsFolderPath = dlg.SelectedPath;
                lblMods.Text = modsFolderPath;
                toolTip1.SetToolTip(lblMods,modsFolderPath);
                lblMods.Refresh();
                
            }
        }
    }
}

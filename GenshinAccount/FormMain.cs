using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GenshinAccount
{
    public partial class FormMain : Form
    {

        private readonly string userDataPath =
            Path.Combine(Application.StartupPath, "UserData");

        private readonly string endfieldAccountPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                @"AppData\LocalLow\HyperGryph\Endfield\sdk_data_f654bce49f1470d3852027a0da9f09a4"
            );

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {

            this.Text += " Endfield Account Switcher";

            if (!Directory.Exists(userDataPath))
                Directory.CreateDirectory(userDataPath);

            lvwAcct.Columns[0].Width = lvwAcct.Width;

            ImageList imageList = new ImageList();
            imageList.ImageSize = new Size(10, 20);

            lvwAcct.SmallImageList = imageList;

            RefreshList();
        }

        private void RefreshList()
        {

            lvwAcct.Items.Clear();

            DirectoryInfo root = new DirectoryInfo(userDataPath);

            foreach (var dir in root.GetDirectories())
            {

                lvwAcct.Items.Add(new ListViewItem()
                {
                    Text = dir.Name
                });

            }

            btnDelete.Enabled = lvwAcct.Items.Count > 0;
            btnSwitch.Enabled = lvwAcct.Items.Count > 0;

        }

        private string ShowInputBox(string title, string prompt)
        {

            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;

            label.Text = prompt;
            label.SetBounds(9, 20, 372, 13);

            textBox.SetBounds(12, 36, 372, 20);

            buttonOk.Text = "确定";
            buttonCancel.Text = "取消";

            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            form.ClientSize = new Size(396, 107);

            form.Controls.AddRange(new Control[]
            {
                label,textBox,buttonOk,buttonCancel
            });

            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            if (form.ShowDialog() == DialogResult.OK)
                return textBox.Text;
            else
                return null;

        }

        private void btnSaveCurr_Click(object sender, EventArgs e)
        {

            if (!Directory.Exists(endfieldAccountPath))
            {
                MessageBox.Show("未找到终末地账号数据");
                return;
            }

            string name = ShowInputBox("保存账号", "输入账号名称");

            if (string.IsNullOrEmpty(name))
                return;

            string savePath = Path.Combine(userDataPath, name);

            if (Directory.Exists(savePath))
                Directory.Delete(savePath, true);

            CopyDirectory(endfieldAccountPath, savePath);

            RefreshList();

            MessageBox.Show("账号保存成功");

        }

        private void btnSwitch_Click(object sender, EventArgs e)
        {

            if (lvwAcct.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择账号");
                return;
            }

            string name = lvwAcct.SelectedItems[0].Text;

            Switch(name, chkAutoStartYS.Checked);

        }

        private void Switch(string name, bool autoRestart)
        {

            string sourcePath = Path.Combine(userDataPath, name);

            if (!Directory.Exists(sourcePath))
            {
                MessageBox.Show("账号不存在");
                return;
            }

            if (autoRestart)
            {

                var pros = Process.GetProcessesByName("Endfield");

                foreach (var p in pros)
                    p.Kill();

            }

            if (Directory.Exists(endfieldAccountPath))
                Directory.Delete(endfieldAccountPath, true);

            CopyDirectory(sourcePath, endfieldAccountPath);

            if (autoRestart)
                StartGame();

            MessageBox.Show("账号切换成功");

        }

        private void StartGame()
        {

            if (!File.Exists(Path.Combine(txtPath.Text, "Endfield.exe")))
            {
                MessageBox.Show("游戏路径错误");
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.FileName = Path.Combine(txtPath.Text, "Endfield.exe");
            startInfo.WorkingDirectory = txtPath.Text;

            if (!string.IsNullOrEmpty(txtStartParam.Text))
                startInfo.Arguments = txtStartParam.Text;

            startInfo.Verb = "runas";

            Process.Start(startInfo);

            StartLauncher();

        }

        private void StartLauncher()
        {

            if (string.IsNullOrEmpty(txtDllPath.Text))
                return;

            if (!File.Exists(txtDllPath.Text))
            {
                MessageBox.Show("Launcher路径错误");
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.FileName = txtDllPath.Text;
            startInfo.WorkingDirectory = Path.GetDirectoryName(txtDllPath.Text);

            Process.Start(startInfo);

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {

            if (lvwAcct.SelectedItems.Count == 0)
                return;

            string name = lvwAcct.SelectedItems[0].Text;

            string path = Path.Combine(userDataPath, name);

            if (Directory.Exists(path))
                Directory.Delete(path, true);

            RefreshList();

        }

        private void lvwAcct_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            if (lvwAcct.SelectedItems.Count == 0)
                return;

            string name = lvwAcct.SelectedItems[0].Text;

            Switch(name, chkAutoStartYS.Checked);

        }

        private void btnChoosePath_Click(object sender, EventArgs e)
        {

            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "Endfield.exe|Endfield.exe";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = Path.GetDirectoryName(dialog.FileName);
            }

        }

        private void btnDllPath_Click(object sender, EventArgs e)
        {

            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "EXE (*.exe)|*.exe";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtDllPath.Text = dialog.FileName;
            }

        }

        private void CopyDirectory(string source, string target)
        {

            Directory.CreateDirectory(target);

            foreach (string file in Directory.GetFiles(source))
            {
                string dest = Path.Combine(target, Path.GetFileName(file));
                File.Copy(file, dest, true);
            }

            foreach (string dir in Directory.GetDirectories(source))
            {
                string dest = Path.Combine(target, Path.GetFileName(dir));
                CopyDirectory(dir, dest);
            }

        }

        private void btnStartGame_Click(object sender, EventArgs e)
        {
            StartGame();
        }

        private void txtPath_TextChanged(object sender, EventArgs e) { }

        private void FormMain_SizeChanged(object sender, EventArgs e) { }

        private void notifyIcon_DoubleClick(object sender, EventArgs e) { }

        private void 显示主界面ToolStripMenuItem_Click(object sender, EventArgs e) { }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void chkMinimizeToNotifyArea_CheckedChanged(object sender, EventArgs e) { }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e) { }

    }
}
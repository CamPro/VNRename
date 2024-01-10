using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TagLib;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace VNRename
{
    public partial class FormMain : Form
    {
        public static DataTable myTable;
        public static string myPath = string.Empty;
        public static string myHistory = Path.GetTempPath() + "histories.log";
        public static string myError = Path.GetTempPath() + "errors.log";

        public FormMain()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            dataGridRename.AllowDrop = true;
            dataGridRename.DragEnter += new DragEventHandler(dataGridRename_DragEnter);
            dataGridRename.DragDrop += new DragEventHandler(dataGridRename_DragDrop);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            myTable = new DataTable();
            myTable.Columns.Add("_cOldName", typeof(string));
            myTable.Columns.Add("_cNewName", typeof(string));
            myTable.Columns.Add("_cFilepath", typeof(string));
            dataGridRename.DataSource = myTable;
        }

        private void buttonBrowser_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                myPath = dialog.SelectedPath;
                SetPath();
                GetFileFolder();
            }
        }

        private void buttonRename_Click(object sender, EventArgs e)
        {
            string histories = string.Empty;
            histories += $"[{myPath}]|[{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}]{Environment.NewLine}";
            progressStatus.Maximum = myTable.Rows.Count;
            for (int i = 0; i < myTable.Rows.Count; i++)
            {
                progressStatus.Value = (i + 1);
                string newname = myTable.Rows[i]["_cNewName"].ToString();
                string filepath = myTable.Rows[i]["_cFilepath"].ToString();
                string[] array = filepath.Split('\\');

                array[array.Length - 1] = "~%" + new Random().Next(0, 9).ToString() + array[array.Length - 1];
                string tmppath = string.Join("\\", array);

                array[array.Length - 1] = newname;
                string newpath = string.Join("\\", array);
                string name = string.Empty;

                if (string.IsNullOrEmpty(newname) || newpath.Equals(filepath))
                {
                    continue;
                }
                try
                {
                    if (System.IO.File.Exists(filepath))
                    {
                        System.IO.File.Move(filepath, tmppath);
                        for (int j = 2; j < 999999999; j++)
                        {
                            newpath = string.Join("\\", array);
                            if (System.IO.File.Exists(newpath))
                            {
                                FileInfo info = new FileInfo(newpath);
                                if (string.IsNullOrEmpty(name))
                                {
                                    name = info.Name.Substring(0, info.Name.Length - info.Extension.Length);
                                }
                                array[array.Length - 1] = $"{name}_{j}{info.Extension}";
                            }
                            else
                            {
                                break;
                            }
                            Thread.Sleep(5);
                        }
                        histories += $"{filepath}|{newpath}{Environment.NewLine}";
                        System.IO.File.Move(tmppath, newpath);
                    }
                    if (Directory.Exists(filepath))
                    {
                        Directory.Move(filepath, tmppath);
                        for (int j = 2; j < 999999999; j++)
                        {
                            newpath = string.Join("\\", array);
                            if (Directory.Exists(newpath))
                            {
                                DirectoryInfo info = new DirectoryInfo(newpath);
                                if (string.IsNullOrEmpty(name))
                                {
                                    name = info.Name;
                                }
                                array[array.Length - 1] = $"{name}_{j}{info.Extension}";
                            }
                            else
                            {
                                break;
                            }
                            Thread.Sleep(5);
                        }
                        histories += $"{filepath}|{newpath}{Environment.NewLine}";
                        Directory.Move(tmppath, newpath);
                    }
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText(myError, ex.Message + Environment.NewLine);
                }
            }
            System.IO.File.WriteAllText(myHistory, histories);
            GetFileFolder();
            buttonRestore.Enabled = true;
            if (System.IO.File.Exists(myError))
            {
                Process.Start(myError);
                Thread.Sleep(1000);
                System.IO.File.Delete(myError);
            }
        }

        private void buttonRestore_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(myHistory))
            {
                string[] lines = System.IO.File.ReadAllLines(myHistory);
                if (lines.Length == 0)
                {

                }
                else
                {
                    for (int i = 1; i < lines.Length; i++)
                    {
                        try
                        {
                            string oldpath = lines[i].Split('|')[0];
                            string newpath = lines[i].Split('|')[1];
                            string tmppath = newpath + "~%" + new Random().Next(0, 9);
                            if (System.IO.File.Exists(newpath))
                            {
                                System.IO.File.Move(newpath, tmppath);
                                System.IO.File.Move(tmppath, oldpath);
                            }
                            if (Directory.Exists(newpath))
                            {
                                Directory.Move(newpath, tmppath);
                                Directory.Move(tmppath, oldpath);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.IO.File.AppendAllText(myError, ex.Message + Environment.NewLine);
                        }
                    }
                }
                System.IO.File.Delete(myHistory);
                GetFileFolder();
                buttonRestore.Enabled = false;
                if (System.IO.File.Exists(myError))
                {
                    Process.Start(myError);
                    Thread.Sleep(1000);
                    System.IO.File.Delete(myError);
                }
            }
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            // tab co ban
            radioVietHoaChuCaiDau.Checked = false;
            radioVietHoaChuCaiDauTien.Checked = false;
            radioVietHoaToanBo.Checked = false;
            radioVietThuongToanBo.Checked = false;
            radioLoaiBoKhoangTrang.Checked = false;
            radioKhoiPhucKhoangTrang.Checked = false;
            radioChuyenTenThanhSo.Checked = false;
            radioDaoNguocTen.Checked = false;
            checkLoaiBoDauTiengViet.Checked = false;
            checkXoaKyTuDacBiet.Checked = false;
            checkGopNhieuKhoangTrang.Checked = false;
            checkBoKhoangTrangDauCuoi.Checked = true;
            checkLoaiBoSo.Checked = false;
            radioLoaiBoSoDau.Checked = false;
            radioLoaiBoSoCuoi.Checked = false;
            textDoiDuoiTapTin.Text = string.Empty;
            // tab nang cao
            textThay.Text = string.Empty;
            textThayBang.Text = string.Empty;

            textDang.Text = string.Empty;
            textDangBang.Text = string.Empty;

            radioGiuSoKyTu.Checked = true;
            radioBoSoKyTu.Checked = false;
            radioDauSoKyTu.Checked = true;
            radioCuoiSoKyTu.Checked = false;
            numSoKyTu.Value = 0;

            radioGiuDenKyTu.Checked = true;
            radioBoDenKyTu.Checked = false;
            radioTruocDenKyTu.Checked = true;
            radioSauDenKyTu.Checked = false;
            textDenKyTu.Text = string.Empty;

            textThemKyTu.Text = string.Empty;
            radioThemKyTuDau.Checked = true;
            radioThemKyTuCuoi.Checked = false;

            radioCheckMD5.Checked = false;
            radioCheckSHA1.Checked = false;
            radioCheckMD5toName.Checked = false;
            radioCheckSHA1toName.Checked = false;
            // tab so thu tu
            numSttChuSo.Value = 2;
            numSttBatDau.Value = 1;
            textStt.Text = " - ";
            radioSttDau.Checked = false;
            radioSttCuoi.Checked = false;
            // tab khac
            radioAudioRemoveInfo.Checked = false;
            radioAudioCopyNameToTitle.Checked = false;
            radioAudioCopyTitleToName.Checked = false;
            textAudio.Text = string.Empty;
        }

        private void buttonExportFilename_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            for (int i = 0; i < myTable.Rows.Count; i++)
            {
                string oldname = myTable.Rows[i]["_cOldName"].ToString();
                text += oldname + Environment.NewLine;
            }
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "TXT files|*.txt";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = dialog.FileName;
                System.IO.File.WriteAllText(fileName, text);
                Process.Start(fileName);
            }
        }

        private void buttonInportFilename_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Open Text File";
            dialog.Filter = "TXT files|*.txt";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string[] texts = System.IO.File.ReadAllLines(dialog.FileName);
                for (int i = 0; i < texts.Length && i < myTable.Rows.Count; i++)
                {
                    myTable.Rows[i]["_cNewName"] = texts[i];
                }
            }
        }

        private void buttonExportResult_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            for (int i = 0; i < myTable.Rows.Count; i++)
            {
                string newname = myTable.Rows[i]["_cNewName"].ToString();
                text += newname + Environment.NewLine;
            }
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "TXT files|*.txt";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = dialog.FileName;
                System.IO.File.WriteAllText(fileName, text);
                Process.Start(fileName);
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            progressStatus.Value = 0;
            GetFileFolder();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            progressStatus.Value = 0;
            myPath = string.Empty;
            myTable.Rows.Clear();
            SetPath();
        }

        private void textPath_TextChanged(object sender, EventArgs e)
        {
            myPath = textPath.Text;
        }

        private void checkRenameFolder_CheckedChanged(object sender, EventArgs e)
        {
            progressStatus.Value = 0;
            GetFileFolder();
        }

        private void checkRenameFile_CheckedChanged(object sender, EventArgs e)
        {
            progressStatus.Value = 0;
            GetFileFolder();
        }

        private void checkRenameSubfolder_CheckedChanged(object sender, EventArgs e)
        {
            progressStatus.Value = 0;
            GetFileFolder();
        }

        public void dataGridRename_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        public void dataGridRename_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 0)
            {
                return;
            }
            buttonClear_Click(null, null);
            if (files.Length == 1 && Directory.Exists(files[0]))
            {
                myPath = files[0];
                SetPath();
                GetFileFolder();
            }
            else
            {
                foreach (string item in files)
                {
                    string name = item.Split('\\').Last();
                    myTable.Rows.Add(name, "", item);
                }
            }
        }

        private void radioVietHoaChuCaiDau_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioVietHoaChuCaiDauTien_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioVietHoaToanBo_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioVietThuongToanBo_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioLoaiBoKhoangTrang_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioKhoiPhucKhoangTrang_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioChuyenTenThanhSo_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioDaoNguocTen_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void checkLoaiBoDauTiengViet_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void checkXoaKyTuDacBiet_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void checkGopNhieuKhoangTrang_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void checkBoKhoangTrangDauCuoi_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void checkLoaiBoSo_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioLoaiBoSoDau_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioLoaiBoSoCuoi_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void textDoiDuoiTapTin_TextChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void textThay_TextChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void textThayBang_TextChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void textDang_TextChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void textDangBang_TextChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioGiuSoKyTu_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioBoSoKyTu_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioDauSoKyTu_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioCuoiSoKyTu_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void numSoKyTu_ValueChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioGiuDenKyTu_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioBoDenKyTu_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioTruocDenKyTu_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioSauDenKyTu_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void textDenKyTu_TextChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void textThemKyTu_TextChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioThemKyTuDau_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioThemKyTuCuoi_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioCheckMD5_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioCheckSHA1_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioCheckMD5toName_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioCheckSHA1toName_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void numSttLuong_ValueChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void numSttStart_ValueChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void textStt_TextChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioSttDau_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioSttCuoi_CheckedChanged(object sender, EventArgs e)
        {
            NewNameResult();
        }

        private void radioAudioRemoveInfo_CheckedChanged(object sender, EventArgs e)
        {
            if (radioAudioRemoveInfo.Checked == false)
            {
                return;
            }
            DialogResult dialogResult = MessageBox.Show("Bạn có muốn thực hiện hành động?", "", MessageBoxButtons.YesNo);
            bool isOk = dialogResult == DialogResult.Yes;

            progressStatus.Maximum = myTable.Rows.Count;
            for (int i = 0; i < myTable.Rows.Count && isOk; i++)
            {
                try
                {
                    progressStatus.Value = (i + 1);
                    string filepath = myTable.Rows[i]["_cFilepath"].ToString();
                    if (System.IO.File.Exists(filepath))
                    {
                        TagLib.File file = TagLib.File.Create(filepath);
                        file.RemoveTags(TagTypes.Id3v1);
                        file.RemoveTags(TagTypes.Id3v2);
                        file.Save();
                    }
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText(myError, ex.Message + Environment.NewLine);
                }
            }
            buttonReset_Click(null, null);
            if (System.IO.File.Exists(myError))
            {
                Process.Start(myError);
                Thread.Sleep(1000);
                System.IO.File.Delete(myError);
            }
        }

        private void radioAudioCopyNameToTitle_CheckedChanged(object sender, EventArgs e)
        {
            if (radioAudioCopyNameToTitle.Checked == false)
            {
                return;
            }
            DialogResult dialogResult = MessageBox.Show("Bạn có muốn thực hiện hành động?", "", MessageBoxButtons.YesNo);
            bool isOk = dialogResult == DialogResult.Yes;

            progressStatus.Maximum = myTable.Rows.Count;
            for (int i = 0; i < myTable.Rows.Count && isOk; i++)
            {
                try
                {
                    progressStatus.Value = (i + 1);
                    string filepath = myTable.Rows[i]["_cFilepath"].ToString();
                    if (System.IO.File.Exists(filepath))
                    {
                        TagLib.File file = TagLib.File.Create(filepath);
                        FileInfo info = new FileInfo(filepath);
                        string name = info.Name.Substring(0, info.Name.Length - info.Extension.Length);
                        file.Tag.Title = name;
                        file.Save();
                    }
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText(myError, ex.Message + Environment.NewLine);
                }
            }
            buttonReset_Click(null, null);
            if (System.IO.File.Exists(myError))
            {
                Process.Start(myError);
                Thread.Sleep(1000);
                System.IO.File.Delete(myError);
            }
        }

        private void radioAudioCopyTitleToName_CheckedChanged(object sender, EventArgs e)
        {
            if (radioAudioCopyTitleToName.Checked == false)
            {
                return;
            }
            for (int i = 0; i < myTable.Rows.Count; i++)
            {
                try
                {
                    string filepath = myTable.Rows[i]["_cFilepath"].ToString();
                    if (System.IO.File.Exists(filepath))
                    {
                        TagLib.File file = TagLib.File.Create(filepath);
                        FileInfo info = new FileInfo(filepath);

                        myTable.Rows[i]["_cNewName"] = file.Tag.Title + info.Extension;
                    }
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText(myError, ex.Message + Environment.NewLine);
                }
            }
            if (System.IO.File.Exists(myError))
            {
                Process.Start(myError);
                Thread.Sleep(1000);
                System.IO.File.Delete(myError);
            }
        }

        private void buttonAudioAlbum_Click(object sender, EventArgs e)
        {
            string text = textAudio.Text;
            progressStatus.Maximum = myTable.Rows.Count;
            for (int i = 0; i < myTable.Rows.Count; i++)
            {
                try
                {
                    progressStatus.Value = (i + 1);
                    string filepath = myTable.Rows[i]["_cFilepath"].ToString();
                    if (System.IO.File.Exists(filepath))
                    {
                        TagLib.File file = TagLib.File.Create(filepath);
                        file.Tag.Album = text;
                        file.Save();
                    }
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText(myError, ex.Message + Environment.NewLine);
                }
            }
            buttonReset_Click(null, null);
            if (System.IO.File.Exists(myError))
            {
                Process.Start(myError);
                Thread.Sleep(1000);
                System.IO.File.Delete(myError);
            }
        }

        private void buttonAudioArtist_Click(object sender, EventArgs e)
        {
            string text = textAudio.Text;
            progressStatus.Maximum = myTable.Rows.Count;
            for (int i = 0; i < myTable.Rows.Count; i++)
            {
                try
                {
                    progressStatus.Value = (i + 1);
                    string filepath = myTable.Rows[i]["_cFilepath"].ToString();
                    if (System.IO.File.Exists(filepath))
                    {
                        TagLib.File file = TagLib.File.Create(filepath);
                        file.Tag.AlbumArtists = new string[] { text };
                        file.Save();
                    }
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText(myError, ex.Message + Environment.NewLine);
                }
            }
            buttonReset_Click(null, null);
            if (System.IO.File.Exists(myError))
            {
                Process.Start(myError);
                Thread.Sleep(1000);
                System.IO.File.Delete(myError);
            }
        }

        public void SetPath()
        {
            textPath.Text = myPath;
        }

        public void GetFileFolder()
        {
            buttonReset_Click(null, null);
            myTable.Rows.Clear();
            SearchOption option = SearchOption.TopDirectoryOnly;
            if (checkRenameSubfolder.Checked)
            {
                option = SearchOption.AllDirectories;
            }
            if (string.IsNullOrEmpty(myPath) || !Directory.Exists(myPath))
            {
                return;
            }
            if (checkRenameFolder.Checked)
            {
                string[] folders = Directory.GetDirectories(myPath, "*", option);
                foreach (var item in folders)
                {
                    DirectoryInfo info = new DirectoryInfo(item);
                    myTable.Rows.Add(info.Name, "", item);
                }
            }
            if (checkRenameFile.Checked)
            {
                string[] files = Directory.GetFiles(myPath, "*", option);
                foreach (var item in files)
                {
                    FileInfo info = new FileInfo(item);
                    myTable.Rows.Add(info.Name, "", item);
                }
            }
            labelTotal.Text = myTable.Rows.Count.ToString();
        }

        public void NewNameResult()
        {
            for (int i = 0; i < myTable.Rows.Count; i++)
            {
                try
                {
                    string namenew = string.Empty;
                    string extold = string.Empty;
                    string extnew = string.Empty;
                    //string oldname = myTable.Rows[i]["_cOldName"].ToString();
                    string filepath = myTable.Rows[i]["_cFilepath"].ToString();
                    bool isFile = true;
                    if (System.IO.File.Exists(filepath))
                    {
                        FileInfo info = new FileInfo(filepath);
                        namenew = info.Name.Substring(0, info.Name.Length - info.Extension.Length);
                        extold = info.Extension;
                        extnew = info.Extension;
                    }
                    else if (Directory.Exists(filepath))
                    {
                        isFile = false;
                        DirectoryInfo info = new DirectoryInfo(filepath);
                        namenew = info.Name;
                    }
                    else
                    {
                        continue;
                    }

                    if (radioVietHoaChuCaiDau.Checked)
                    {
                        string[] array = namenew.Split(' ');
                        for (int j = 0; j < array.Length; j++)
                        {
                            if (array[j].Length > 0)
                            {
                                string first = array[j].Substring(0, 1).ToUpper();
                                string last = array[j].Substring(1).ToLower();
                                array[j] = first + last;
                            }
                        }
                        namenew = string.Join(" ", array);
                        extnew = extold;
                    }
                    if (radioVietHoaChuCaiDauTien.Checked && namenew.Length > 0)
                    {
                        string first = namenew.Substring(0, 1).ToUpper();
                        string last = namenew.Substring(1).ToLower();
                        namenew = first + last;
                        extnew = extold;
                    }
                    if (radioVietHoaToanBo.Checked)
                    {
                        namenew = namenew.ToUpper();
                        extnew = extold;
                    }
                    if (radioVietThuongToanBo.Checked)
                    {
                        namenew = namenew.ToLower();
                        extnew = extold;
                    }
                    if (radioLoaiBoKhoangTrang.Checked)
                    {
                        namenew = namenew.Replace(" ", "");
                        extnew = extold;
                    }
                    if (radioKhoiPhucKhoangTrang.Checked)
                    {
                        string[] uppers = "Q W E R T Y U I O P A S D F G H J K L Z X C V B N M".Split(' ');
                        foreach (var item in uppers)
                        {
                            namenew = namenew.Replace(item, " " + item);
                        }
                        extnew = extold;
                    }
                    if (radioChuyenTenThanhSo.Checked)
                    {
                        byte[] bt = Encoding.Default.GetBytes(namenew);
                        var hexString = BitConverter.ToString(bt);
                        namenew = hexString.Replace("-", "");
                    }
                    if (radioDaoNguocTen.Checked)
                    {
                        char[] charArray = namenew.ToCharArray();
                        Array.Reverse(charArray);
                        namenew = new string(charArray);
                        extnew = extold;
                    }
                    if (checkLoaiBoDauTiengViet.Checked)
                    {
                        const string FindText = "áàảãạâấầẩẫậăắằẳẵặđéèẻẽẹêếềểễệíìỉĩịóòỏõọôốồổỗộơớờởỡợúùủũụưứừửữựýỳỷỹỵÁÀẢÃẠÂẤẦẨẪẬĂẮẰẲẴẶĐÉÈẺẼẸÊẾỀỂỄỆÍÌỈĨỊÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢÚÙỦŨỤƯỨỪỬỮỰÝỲỶỸỴ";
                        const string ReplText = "aaaaaaaaaaaaaaaaadeeeeeeeeeeeiiiiiooooooooooooooooouuuuuuuuuuuyyyyyAAAAAAAAAAAAAAAAADEEEEEEEEEEEIIIIIOOOOOOOOOOOOOOOOOUUUUUUUUUUUYYYYY";
                        int index = -1;
                        char[] arrChar = FindText.ToCharArray();
                        while ((index = namenew.IndexOfAny(arrChar)) != -1)
                        {
                            int index2 = FindText.IndexOf(namenew[index]);
                            namenew = namenew.Replace(namenew[index], ReplText[index2]);
                        }
                        extnew = extold;
                    }
                    if (checkXoaKyTuDacBiet.Checked)
                    {
                        namenew = Regex.Replace(namenew, @"[^A-Za-z0-9]", "");
                        extnew = extold;
                    }
                    if (checkGopNhieuKhoangTrang.Checked)
                    {
                        namenew = Regex.Replace(namenew, @"\s+", " ");
                        extnew = extold;
                    }
                    if (checkBoKhoangTrangDauCuoi.Checked)
                    {
                        namenew = namenew.Trim();
                        extnew = extold;
                    }
                    if (checkLoaiBoSo.Checked)
                    {
                        if (radioLoaiBoSoDau.Checked)
                        {
                            namenew = Regex.Replace(namenew, @"^\d+", "");
                        }
                        else if (radioLoaiBoSoCuoi.Checked)
                        {
                            namenew = Regex.Replace(namenew, @"\d+$", "");
                        }
                        else
                        {
                            namenew = Regex.Replace(namenew, @"\d+", "");
                        }
                        extnew = extold;
                    }
                    if (!string.IsNullOrEmpty(textDoiDuoiTapTin.Text))
                    {
                        extnew = textDoiDuoiTapTin.Text;
                    }

                    // tab nang cao
                    if (textThay.Text.Length > 0)
                    {
                        string search = textThay.Text;
                        string replace = textThayBang.Text;
                        namenew = namenew.Replace(search, replace);
                        extnew = extold;
                    }
                    if (!string.IsNullOrEmpty(textDang.Text))
                    {
                        string search = textDang.Text;
                        string replace = textDangBang.Text;
                        try { namenew = Regex.Replace(namenew, search, replace); } catch { }
                        extnew = extold;
                    }
                    if (numSoKyTu.Value > 0)
                    {
                        int length = Convert.ToInt32(numSoKyTu.Value);
                        if (radioGiuSoKyTu.Checked)
                        {
                            if (radioDauSoKyTu.Checked)
                            {
                                namenew = namenew.Substring(0, length);
                            }
                            if (radioCuoiSoKyTu.Checked)
                            {
                                namenew = namenew.Substring(namenew.Length - length);
                            }
                        }
                        if (radioBoSoKyTu.Checked)
                        {
                            if (radioDauSoKyTu.Checked)
                            {
                                namenew = namenew.Substring(length);
                            }
                            if (radioCuoiSoKyTu.Checked)
                            {
                                namenew = namenew.Substring(0, namenew.Length - length);
                            }
                        }
                        extnew = extold;
                    }
                    if (textDenKyTu.Text.Length > 0)
                    {
                        string timkiem = textDenKyTu.Text;
                        int index = namenew.IndexOf(timkiem);
                        if (radioGiuDenKyTu.Checked && index > 0)
                        {
                            if (radioTruocDenKyTu.Checked)
                            {
                                namenew = namenew.Substring(0, index);
                            }
                            if (radioSauDenKyTu.Checked)
                            {
                                namenew = namenew.Substring(index + timkiem.Length);
                            }
                        }
                        if (radioBoDenKyTu.Checked && index > 0)
                        {
                            if (radioTruocDenKyTu.Checked)
                            {
                                namenew = namenew.Substring(index);
                            }
                            if (radioSauDenKyTu.Checked)
                            {
                                namenew = namenew.Substring(0, index + timkiem.Length);
                            }
                        }
                        extnew = extold;
                    }
                    if (textThemKyTu.Text.Length > 0)
                    {
                        string them = textThemKyTu.Text;
                        if (radioThemKyTuDau.Checked)
                        {
                            namenew = them + namenew;
                        }
                        if (radioThemKyTuCuoi.Checked)
                        {
                            namenew += them;
                        }
                        extnew = extold;
                    }
                    if (radioCheckMD5.Checked)
                    {
                        namenew = isFile ? CalculateMD5(filepath) : "";
                        extnew = string.Empty;
                    }
                    if (radioCheckSHA1.Checked)
                    {
                        namenew = isFile ? CalculateSHA1(filepath) : "";
                        extnew = string.Empty;
                    }
                    if (radioCheckMD5toName.Checked)
                    {
                        namenew = isFile ? CalculateMD5(filepath) : "";
                        extnew = isFile ? extold : string.Empty;
                    }
                    if (radioCheckSHA1toName.Checked)
                    {
                        namenew = isFile ? CalculateSHA1(filepath) : "";
                        extnew = isFile ? extold : string.Empty;
                    }

                    // tab so thu tu
                    if (radioSttDau.Checked || radioSttCuoi.Checked)
                    {
                        int numStart = Convert.ToInt32(numSttBatDau.Value);
                        int numSoChu = Convert.ToInt32(numSttChuSo.Value);
                        string kytunoi = textStt.Text;
                        int stt = numStart + i;
                        string zero = string.Empty;
                        for (int z = 0; z < numSoChu - stt.ToString().Length; z++)
                        {
                            zero += "0";
                        }
                        if (radioSttDau.Checked)
                        {
                            namenew = $"{zero}{stt}{kytunoi}{namenew}";
                        }
                        if (radioSttCuoi.Checked)
                        {
                            namenew = $"{namenew}{kytunoi}{zero}{stt}";
                        }
                        extnew = extold;
                    }

                    // export result
                    myTable.Rows[i]["_cNewName"] = namenew + extnew;
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText(myError, ex.Message + Environment.NewLine);
                }
            }
            if (System.IO.File.Exists(myError))
            {
                Process.Start(myError);
                Thread.Sleep(1000);
                System.IO.File.Delete(myError);
            }
        }

        public string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public string CalculateSHA1(string filename)
        {
            using (var sha1 = SHA1.Create())
            {
                using (var stream = System.IO.File.OpenRead(filename))
                {
                    var hash = sha1.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

    }
}

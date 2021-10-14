using Microsoft.WindowsAPICodePack.Dialogs;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PPv3_Application
{
    public partial class Form1 : Form
    { 
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;

        private Color normalHoverBG = Color.FromArgb(40, 40, 40);
        private Color activeHoverBG = Color.FromArgb(10, 10, 10);
        private string current_folder, current_map;
        private PPv3_Application.Properties.Settings settings = new Properties.Settings();
        private List<string> OsuSongFolder;
        private OsuBeatmapReader.OsuBeatmapReader obr = new OsuBeatmapReader.OsuBeatmapReader();
        PPv3_System.PPv3PatternAnalyzer PatternAnalyzer = new PPv3_System.PPv3PatternAnalyzer();
        PPv3_System.PPv3System PPv3 = new PPv3_System.PPv3System();
        public Form1()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            OsuSongFolder = new List<string>();
        }

        #region Resize

        private ReSize resize = new ReSize();     // ReSize Class "/\" To Help Resize Form <None Style>

        private const int cGrip = 16;      // Grip size
        private const int cCaption = 32;   // Caption bar height;

        //set MinimumSize to Form
        public override Size MinimumSize
        {
            get
            {
                return base.MinimumSize;
            }
            set
            {
                base.MinimumSize = new Size(179, 51);
            }
        }

        //
        //override  WndProc
        //
        protected override void WndProc(ref Message m)
        {
            //****************************************************************************

            int x = (int)(m.LParam.ToInt64() & 0xFFFF);               //get x mouse position
            int y = (int)((m.LParam.ToInt64() & 0xFFFF0000) >> 16);   //get y mouse position  you can gave (x,y) it from "MouseEventArgs" too
            Point pt = PointToClient(new Point(x, y));

            if (m.Msg == 0x84)
            {
                switch (resize.getMosuePosition(pt, this))
                {
                    case "l": m.Result = (IntPtr)10; return;  // the Mouse on Left Form
                    case "r": m.Result = (IntPtr)11; return;  // the Mouse on Right Form
                    case "a": m.Result = (IntPtr)12; return;
                    case "la": m.Result = (IntPtr)13; return;
                    case "ra": m.Result = (IntPtr)14; return;
                    case "u": m.Result = (IntPtr)15; return;
                    case "lu": m.Result = (IntPtr)16; return;
                    case "ru": m.Result = (IntPtr)17; return; // the Mouse on Right_Under Form
                    case "": m.Result = pt.Y < 32 /*mouse on title Bar*/ ? (IntPtr)2 : (IntPtr)1; return;
                }
            }

            base.WndProc(ref m);
        }

        #endregion Resize

        #region UI

        private void label1_MouseEnter(object sender, EventArgs e)
        {
            label1.BackColor = activeHoverBG;
        }

        private void label1_MouseLeave(object sender, EventArgs e)
        {
            label1.BackColor = normalHoverBG;
        }

        private void label2_MouseEnter(object sender, EventArgs e)
        {
            label2.BackColor = activeHoverBG;
        }

        private void label2_MouseLeave(object sender, EventArgs e)
        {
            label2.BackColor = normalHoverBG;
        }
        private void label8_MouseEnter(object sender, EventArgs e)
        {
            label8.BackColor = activeHoverBG;
        }

        private void label8_MouseLeave(object sender, EventArgs e)
        {
            label8.BackColor = normalHoverBG;
        }

        #endregion UI

        #region Form
        private void label8_Click(object sender, EventArgs e)
        {

            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            } else if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion


        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = settings.osuSongPath;
            timer1.Start();
            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlaybackStopped;
            }
            // chart1.Series[0].Points.AddXY(1, 1);
            
        }

      
        private void metroButton1_Click(object sender, EventArgs e)
        {
            Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog di1 = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog();
            di1.IsFolderPicker = true;
            di1.Title = "Pick your osu! song folder";
            di1.ShowHiddenItems = true;

            if (di1.ShowDialog() == CommonFileDialogResult.Ok)
            {
                textBox1.Text = di1.FileName;
                settings.osuSongPath = textBox1.Text;
                settings.Save();
            }
        }

        private void Search(string c)
        {
            try
            {
                textBox2.Enabled = false;
                int maps = 0, folder = 0;
                listBox1.DataSource = null;
                listBox1.Items.Clear();

                string[] TempPath;
                listBox2.DataSource = null;
                listBox2.Items.Clear();

                foreach (string s in OsuSongFolder)
                {
                    if (s.ToLower().Contains(c.ToLower()))
                    {
                        listBox1.Items.Add(s);
                        folder++;

                        TempPath = Directory.GetFiles(settings.osuSongPath + "\\" + s);
                        foreach (String a in TempPath)
                        {
                            if (a.Contains(".osu"))
                            {
                                maps++;
                            }
                        }
                    }
                }
                label7.Text = "Folders " + Environment.NewLine + "(" + folder + ")" + Environment.NewLine + "Beatmaps" + Environment.NewLine + "(" + maps + ")";
                textBox2.Enabled = true;
                textBox2.Focus();
            }
            catch
            { //label2.Text = "Error: Invailid Path" + Environment.NewLine + "(" + settings.osuSongPath + ")";
            }

            textBox2.Enabled = true;
            textBox2.Focus();
        }

        private void IndexSongFolder()
        {
            try
            {
                int maps = 0, folder = 0;
                listBox1.DataSource = null;
                listBox1.Items.Clear();

                string[] TempPath;
                listBox2.DataSource = null;
                listBox2.Items.Clear();

                string[] filelist1;
                filelist1 = Directory.GetDirectories(settings.osuSongPath);
                foreach (string s in filelist1)
                {
                    OsuSongFolder.Add(s.Replace(settings.osuSongPath + "\\", ""));
                    folder++;

                    TempPath = Directory.GetFiles(s);
                    foreach (String a in TempPath)
                    {
                        if (a.Contains(".osu"))
                        {
                            maps++;
                        }
                    }
                }
                label7.Text = "Folders " + Environment.NewLine + "(" + folder + ")" + Environment.NewLine + "Beatmaps" + Environment.NewLine + "(" + maps + ")";
                listBox1.DataSource = OsuSongFolder;
            }
            catch
            { //label2.Text = "Error: Invailid Path" + Environment.NewLine + "(" + settings.osuSongPath + ")";
            }

            textBox2.Enabled = true;
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            if (settings.osuSongPath == "")
            {
                MessageBox.Show("You didn't pick a path yet (got to settings)", settings.osuSongPath);
            }
            else
            {
                CheckForIllegalCrossThreadCalls = false;
                //Thread TH = new Thread(IndexSongFolder);

                // TH.Start();
                IndexSongFolder();
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Search(textBox2.Text);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox2.DataSource = null;
            listBox2.Items.Clear();

            string s = settings.osuSongPath + "\\" + listBox1.GetItemText(listBox1.SelectedItem);
            current_folder = s;
            string[] TempPath;
            TempPath = Directory.GetFiles(s);
            foreach (String a in TempPath)
            {
                if (a.Contains(".osu"))
                {
                    listBox2.Items.Add(a.Replace(s + "\\", ""));
                }
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string s = current_folder + "\\" + listBox2.GetItemText(listBox2.SelectedItem);
            current_map = s;
            ReadBeatmap(s);
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            audioFile.Dispose();
            audioFile = null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                outputDevice.Pause();
                button2.BackgroundImage = PPv3_Application.Properties.Resources.play;
            }
            else if (outputDevice.PlaybackState == PlaybackState.Paused)
            {
                outputDevice.Play();
                button2.BackgroundImage = PPv3_Application.Properties.Resources.pause;
            }
            else if (outputDevice.PlaybackState == PlaybackState.Stopped)
            {
                if (File.Exists(obr.AudioFilePath))
                {
                    audioFile = new AudioFileReader(obr.AudioFilePath);
                    outputDevice.Init(audioFile);
                    outputDevice.Play();
                    button2.BackgroundImage = PPv3_Application.Properties.Resources.pause;
                }
                else
                {
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            outputDevice.Stop();
            button2.BackgroundImage = PPv3_Application.Properties.Resources.play;
        }

        private void metroTrackBar1_Scroll(object sender, ScrollEventArgs e)
        {
            try
            {
                outputDevice.Volume = metroTrackBar1.Value / 100f;
                label9.Text = "Volume: " + metroTrackBar1.Value + "%";
            }
            catch (NullReferenceException nre)
            {
                Console.WriteLine(nre);
            }
        }

        private void metroTrackBar1_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                outputDevice.Volume = metroTrackBar1.Value / 100f;
                label9.Text = "Volume: " + metroTrackBar1.Value + "%";
            }
            catch (NullReferenceException nre)
            {
                Console.WriteLine(nre);
            }
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            if (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                metroProgressBar1.Maximum = Convert.ToInt32(audioFile.TotalTime.TotalMilliseconds);
                metroProgressBar1.Value = Convert.ToInt32(audioFile.CurrentTime.TotalMilliseconds);

                TimeSpan currenttime = TimeSpan.FromSeconds(audioFile.CurrentTime.TotalSeconds);
                TimeSpan total_time = TimeSpan.FromSeconds(audioFile.TotalTime.TotalSeconds);
                string str = currenttime.ToString(@"hh\:mm\:ss");
                string str2 = total_time.ToString(@"hh\:mm\:ss");

                label10.Text = str + Environment.NewLine + str2;
            }

            if (outputDevice.PlaybackState == PlaybackState.Stopped)
            {
                metroProgressBar1.Value = 0;
            }
        }

      

        private void ReadBeatmap(string path)
        {
            try
            {
                chart1.Series[0].Points.Clear();

                obr.GetBeatmapData(path);
                if (File.Exists(obr.BackgroundFilePath))
                {
                    pictureBox1.Image = Image.FromFile(obr.BackgroundFilePath);

                }
                else
                {
                    pictureBox1.Image = PPv3_Application.Properties.Resources.nobg;
                }
                label11.Text = obr.Artist + " - " + obr.Title;

                PatternAnalyzer.GetPatternOverview(obr.Circle_PosY, obr.Circle_PosX, obr.Circle_Time);
                int index = 0;
                foreach (double d in PatternAnalyzer.TimeGap)
                {
                    chart1.Series[0].Points.AddXY(obr.Circle_Time[index], PatternAnalyzer.DistanceGap[index]);
                    index++;
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
    #region Classes
    
    internal class ReSize
    {
        private bool Above, Right, Under, Left, Right_above, Right_under, Left_under, Left_above;

        private int Thickness = 6;  //Thickness of border  u can cheang it
        private int Area = 8;     //Thickness of Angle border

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="thickness">set thickness of form border</param>
        public ReSize(int thickness)
        {
            Thickness = thickness;
        }

        /// <summary>
        /// Constructor set thickness of form border=1
        /// </summary>
        public ReSize()
        {
            Thickness = 10;
        }

        //Get Mouse Position
        public string getMosuePosition(Point mouse, Form form)
        {
            bool above_underArea = mouse.X > Area && mouse.X < form.ClientRectangle.Width - Area; /* |\AngleArea(Left_Above)\(=======above_underArea========)/AngleArea(Right_Above)/| */ //Area===>(==)
            bool right_left_Area = mouse.Y > Area && mouse.Y < form.ClientRectangle.Height - Area;

            bool _Above = mouse.Y <= Thickness;  //Mouse in Above All Area
            bool _Right = mouse.X >= form.ClientRectangle.Width - Thickness;
            bool _Under = mouse.Y >= form.ClientRectangle.Height - Thickness;
            bool _Left = mouse.X <= Thickness;

            Above = _Above && (above_underArea); if (Above) return "a";   /*Mouse in Above All Area WithOut Angle Area */
            Right = _Right && (right_left_Area); if (Right) return "r";
            Under = _Under && (above_underArea); if (Under) return "u";
            Left = _Left && (right_left_Area); if (Left) return "l";

            Right_above =/*Right*/ (_Right && (!right_left_Area)) && /*Above*/ (_Above && (!above_underArea)); if (Right_above) return "ra";     /*if Mouse  Right_above */
            Right_under =/* Right*/((_Right) && (!right_left_Area)) && /*Under*/(_Under && (!above_underArea)); if (Right_under) return "ru";     //if Mouse  Right_under
            Left_under = /*Left*/((_Left) && (!right_left_Area)) && /*Under*/ (_Under && (!above_underArea)); if (Left_under) return "lu";      //if Mouse  Left_under
            Left_above = /*Left*/((_Left) && (!right_left_Area)) && /*Above*/(_Above && (!above_underArea)); if (Left_above) return "la";      //if Mouse  Left_above

            return "";
        }
    }
    #endregion

}
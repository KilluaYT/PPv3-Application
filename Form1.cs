using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using PPv3_Application;
using System.IO;


namespace PPv3_Application
{
    public partial class Form1 : Form
    {

        Color normalHoverBG = Color.FromArgb(40, 40, 40);
        Color activeHoverBG = Color.FromArgb(10, 10, 10);
        PPv3_Application.Properties.Settings settings = new Properties.Settings();
        ListBox OsuSongFolder;

        public Form1()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.ResizeRedraw, true);

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
        #endregion
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
        #endregion




        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = settings.osuSongPath;

        }


        private void label2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Application.Exit();
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

        private void Search()
        {


            try
            {
                textBox2.Enabled = false;
                int maps = 0, folder = 0;
                listBox1.Items.Clear();
                string[] TempPath;
                listBox2.Items.Clear();


                string[] filelist1;
                filelist1 = Directory.GetDirectories(settings.osuSongPath);
                foreach (string s in filelist1)
                    if (s.Contains(textBox2.Text))
                    {
                        listBox1.Items.Add(s.Replace(settings.osuSongPath + "\\", ""));
                        folder++;
                        label7.Text = "Folders: " + Environment.NewLine + "(" + folder + ")";

                        TempPath = Directory.GetFiles(s);
                        foreach (String a in TempPath)
                        {


                            if (a.Contains(".osu"))
                            {
                                maps++;
                                label6.Text = "Beatmaps" + Environment.NewLine + "(" + maps + ")";

                            }


                        }
                    }
            }

            catch
            { //label2.Text = "Error: Invailid Path" + Environment.NewLine + "(" + settings.osuSongPath + ")";
            }

            textBox2.Enabled = true;
            textBox2.Focus();
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            if (settings.osuSongPath == "")
            {

                MessageBox.Show("This Folder doesn't exist, or you didn't pick a path yet (got to settings)", settings.osuSongPath);

            }
            else
            {
                Search();

            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Search();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }

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
}


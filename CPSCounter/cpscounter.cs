using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CPSCounter
{

	public partial class cpscounter : Form
	{
		#region Do not touch
		// Rounded corners
		[DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
		private static extern IntPtr CreateRoundRectRgn
		(
			int nLeftRect,
			int nTopRect, 
			int nRightRect,
			int nBottomRect,
			int nWidthEllipse, 
			int nHeightEllipse 
		);

		// Titlebar height
		int TtBHeight;

		// Things important for CPS measurement
		//  - Timer
		Stopwatch millis = new Stopwatch();
		//  - List of CPS times
		List<long> lcList = new List<long>();
		List<long> rcList = new List<long>();

		// Record
		int MaxLCPS = 0;
		int MaxRCPS = 0;

		bool cursor_anchored = false;
		Size minimumSize = new Size();

		public cpscounter()
		{
			InitializeComponent();
			millis.Start();
			minimumSize = MinimumSize;
		}

        private bool listfind(List<long> list, int ms, int t, int ms2, int t2, string whatfor = "") {
			int c = 0;
			//List<long> l = new List<long>();
			for (int i = 0; i < list.Count - 1; i++)
			{
				long v1 = list.Count > 1 ? list[i + 1] : 1000;
				long v = v1 - list[i];
				if (v <= ms && v >= ms2)
				{
					c++;
				}
			}
			bool b = c >= t && c <= t2 && list.Count <= t2 && list.Count >= t;

			return b;
		}

		string ScMethod = "";
		private void clickMethodDetection()
		{
            List<long> clc = new List<long>();

            clc.AddRange(lcList.Count >= rcList.Count ? lcList : rcList);
			if (lcList.Count == rcList.Count) clc.AddRange(rcList);

            ScMethod = click_method_detection.FindClickingMethod(clc, millis);
		}
			
		private void hideTitlebarToolStripMenuItem_Click(object sender, EventArgs e)
		{
            this.MinimumSize = new Size(0,0);
            this.Height -= 24;
            this.Left += 8;
            FormBorderStyle = FormBorderStyle.None;
            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 32, 32));
            this.menuStrip1.Visible = false;
        }

		private void Form1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (FormBorderStyle == FormBorderStyle.None)
			{
				FormBorderStyle = FormBorderStyle.SizableToolWindow;
                this.MinimumSize = minimumSize;
                this.Height += 24;
				this.Top -= TtBHeight + 24;
				this.Left -= 8;
				this.menuStrip1.Visible = true;
			}
			else 
			{
                this.MinimumSize = new Size(0, 0);
                this.Height -= 24;
				this.Top += TtBHeight + 24;
				this.Left += 8;
                FormBorderStyle = FormBorderStyle.None;
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 32, 32));
				this.menuStrip1.Visible = false;
            }
		}

		enum ClickType
		{
			left, right, both
		}

		void UpdateCPS(ClickType clicktype = ClickType.both)
		{
			if (clicktype == ClickType.left || clicktype == ClickType.both)
			{
				LeftCPS.Text = string.Format("{0,2}", lcList.Count.ToString());
				if (lcList.Count > MaxLCPS)
				{
					MaxLCPS = lcList.Count;
					lcpsmax.Text = MaxLCPS.ToString();
					
					if (overwriteAllowed)
					{
						if (Properties.Settings.Default.leftRecord < MaxLCPS)
							Properties.Settings.Default.leftRecord = MaxLCPS;
					}
				}
			}
			if (clicktype == ClickType.right || clicktype == ClickType.both)
			{
				RightCPS.Text = string.Format("{0,2}", rcList.Count.ToString());
				if (rcList.Count > MaxRCPS)
				{
					MaxRCPS = rcList.Count;
					rcpsmax.Text = MaxRCPS.ToString();
					
					if (overwriteAllowed)
					{
						if (Properties.Settings.Default.rightRecord < MaxRCPS)
							Properties.Settings.Default.rightRecord = MaxRCPS;
					}
				}
			}
		}


		private void timer1_Tick(object sender, EventArgs e)
        {
            float lastc = Math.Min(millis.ElapsedMilliseconds - lcList.LastOrDefault(), millis.ElapsedMilliseconds - rcList.LastOrDefault());

            if (lastc >= 1000 && cursor_anchored)
            {
                Console.WriteLine(lastc);
                Release();
            }

            lcList = lcList.Where(val => millis.ElapsedMilliseconds - val <= 1000).ToList();
			rcList = rcList.Where(val => millis.ElapsedMilliseconds - val <= 1000).ToList();

			if (lcList.Count < 1 && rcList.Count < 1)
				ScMethod = "";

			UpdateCPS();
		}

		private void Anyclick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				//Debug.WriteLine(" -------- left click " + lcList.Count + " ------- ");
				lcList.Add(millis.ElapsedMilliseconds);
				UpdateCPS(ClickType.left);
			}
			if (e.Button == MouseButtons.Right)
			{
				//Debug.WriteLine(" ------- right click " + rcList.Count + " ------- ");
				rcList.Add(millis.ElapsedMilliseconds);
				UpdateCPS(ClickType.right);
			}
			clickMethodDetection();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			Hook.GlobalEvents().MouseDown += Anyclick;
			Rectangle screenRectangle = this.RectangleToScreen(this.ClientRectangle);
			TtBHeight = screenRectangle.Top - this.Top;
			MaxLCPS = Properties.Settings.Default.leftRecord;
			MaxRCPS = Properties.Settings.Default.rightRecord;
			lcpsmax.Text = MaxLCPS.ToString();
			rcpsmax.Text = MaxRCPS.ToString();
		}

		private void transparentModeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if(TransparencyKey == Color.Empty)
			{
				TransparencyKey = Color.FromArgb(0, 16, 16);
				transparentModeToolStripMenuItem.Checked = true;
			} else
			{
				TransparencyKey = Color.Empty;
				transparentModeToolStripMenuItem.Checked = false;
			}
		}

		private void timer2_Tick(object sender, EventArgs e)
		{
			cMethod.Text = ScMethod;
		}

		bool overwriteAllowed = true;

		private void resetToolStripMenuItem_Click(object sender, EventArgs e)
		{
			overwriteAllowed = false;
			MaxLCPS = 0;
			MaxRCPS = 0;
			lcpsmax.Text = MaxLCPS.ToString();
			rcpsmax.Text = MaxRCPS.ToString();
		}

		private void resetToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			Properties.Settings.Default.leftRecord = 0;
			Properties.Settings.Default.rightRecord = 0;
		}

		// anchor the cursor
		private void panel5_Click(object sender, EventArgs e)
		{
			Panel panel = sender as Panel;
			Point location = panel.PointToScreen(new Point(0, 0));
			Cursor.Clip = new Rectangle(location, panel.Bounds.Size);
			cursor_anchored = true;
		}

		// when clicked Esc or didn't click a mouse button for 1 second
		private void Release()
		{
			Cursor.Clip = Rectangle.Empty;
			cursor_anchored = false;
		}

        private void cpscounter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Release();
            }
        }

        private void antiACToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.com/invite/BFJR7DMnVg");
        }

        private void minecraftPjatyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://namemc.com/profile/pjaty_.1");
        }

        private void minecraftPjeToolStripMenuItem_Click(object sender, EventArgs e)
        {
			Process.Start("https://namemc.com/profile/pje_.1");
        }
    }
	#endregion
}

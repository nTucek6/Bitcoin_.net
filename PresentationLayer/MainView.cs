using System;
using System.Drawing;
using System.Windows.Forms;
using PresentationLayer.Tabs;

namespace PresentationLayer
{
    public partial class MainView : Form
    {
        private bool dragging = false;
        private Point startPoint = new Point(0, 0);
        private UserControlHome home = new UserControlHome(); //Kreira se jedanput bolje performanse
        private UserControlHistory History = new UserControlHistory();

        public MainView()
        {
            InitializeComponent();
            
            addUserControl(home);
        }
      
        private void MainView_Load(object sender, EventArgs e)
        {
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
           

        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - this.startPoint.X, p.Y - this.startPoint.Y);
            }

        }

        private void btnMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnSize_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
            }
        }
        private void panelClose_MouseHover(object sender, EventArgs e)
        {
            panelClose.BackColor = Color.FromArgb(139, 0, 0);
        }

        private void panelMax_MouseHover(object sender, EventArgs e)
        {
            panelMax.BackColor = Color.FromArgb(0, 0, 0);
        } 

        private void panelMin_MouseHover(object sender, EventArgs e)
        {
            panelMin.BackColor = Color.FromArgb(0, 0, 0);
        }

        private void panelClose_MouseLeave(object sender, EventArgs e)
        {
            panelClose.BackColor = Color.Transparent;
        }

        private void panelMax_MouseLeave(object sender, EventArgs e)
        {
            panelMax.BackColor = Color.Transparent;
        } 

        private void panelMin_MouseLeave(object sender, EventArgs e)
        {
            panelMin.BackColor = Color.Transparent;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
           
        }

        private void addUserControl(UserControl userControl)
        {
            //Metoda stvara userControl kojega dohvati i postavi unutar panelDispaly
            userControl.Dock = DockStyle.Fill;
            panelDisplay.Controls.Clear();
            panelDisplay.Controls.Add(userControl);
            userControl.BringToFront();

        }
        private void btnHome_Click(object sender, EventArgs e)
        {
            //Metoda poziva metodu koja stavlja po kliku odabranog gumba tab na prednju stranu
            addUserControl(home);
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            addUserControl(History);
        }
    }
}

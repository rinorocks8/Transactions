using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Convert
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            label7.BackColor = Properties.Settings.Default.back;
            label8.BackColor = Properties.Settings.Default.front;
            label9.BackColor = Properties.Settings.Default.front2;
            label10.BackColor = Properties.Settings.Default.text;
            label11.BackColor = Properties.Settings.Default.glow;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Back
            ColorDialog colorDlg = new ColorDialog();
            colorDlg.SolidColorOnly = true;
            colorDlg.FullOpen = true;
            colorDlg.ShowDialog();
            Properties.Settings.Default.back = colorDlg.Color;
            Properties.Settings.Default.Save();
            label7.BackColor = colorDlg.Color;
            var Form1 = Application.OpenForms.OfType<Form1>().Single();
            Form1.ColorSet("Custom", false);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Front
            ColorDialog colorDlg = new ColorDialog();
            colorDlg.SolidColorOnly = true;
            colorDlg.FullOpen = true;
            colorDlg.ShowDialog();
            Properties.Settings.Default.front = colorDlg.Color;
            Properties.Settings.Default.Save();
            label8.BackColor = colorDlg.Color;
            var Form1 = Application.OpenForms.OfType<Form1>().Single();
            Form1.ColorSet("Custom", false);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Front 2
            ColorDialog colorDlg = new ColorDialog();
            colorDlg.SolidColorOnly = true;
            colorDlg.FullOpen = true;
            colorDlg.ShowDialog();
            Properties.Settings.Default.front2 = colorDlg.Color;
            Properties.Settings.Default.Save();
            label9.BackColor = colorDlg.Color;
            var Form1 = Application.OpenForms.OfType<Form1>().Single();
            Form1.ColorSet("Custom", false);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Text
            ColorDialog colorDlg = new ColorDialog();
            colorDlg.SolidColorOnly = true;
            colorDlg.FullOpen = true;
            colorDlg.ShowDialog();
            Properties.Settings.Default.text = colorDlg.Color;
            Properties.Settings.Default.Save();
            label10.BackColor = colorDlg.Color;
            var Form1 = Application.OpenForms.OfType<Form1>().Single();
            Form1.ColorSet("Custom", false);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //Glow
            ColorDialog colorDlg = new ColorDialog();
            colorDlg.SolidColorOnly = true;
            colorDlg.FullOpen = true;
            colorDlg.ShowDialog();
            Properties.Settings.Default.glow = colorDlg.Color;
            Properties.Settings.Default.Save();
            label11.BackColor = colorDlg.Color;
            var Form1 = Application.OpenForms.OfType<Form1>().Single();
            Form1.ColorSet("Custom", false);
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAZ3D_JPEGifier
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label1.Text = trackBar1.Value.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Program.textureSize = Convert.ToInt32(comboBox1.Text.ToString());
            Program.resize = checkBox1.Checked;
            Program.quality = trackBar1.Value;
            Program.ready = true;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Program.ready = false;
            Close();
        }
    }
}

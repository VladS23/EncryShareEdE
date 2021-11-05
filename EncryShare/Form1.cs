using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;

namespace EncryShare
{
    public partial class Form1 : Form
    {
        static Process thisProcess;
        public Form1()
        {
            InitializeComponent();
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ServerForm sf = new ServerForm();
            sf.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ClientForm cf = new ClientForm();
            cf.Show();
            this.Hide();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            thisProcess = Process.GetCurrentProcess();
            
        }

        public static void CloseForm()
        {
            
            thisProcess.Kill();
        }
    }
}

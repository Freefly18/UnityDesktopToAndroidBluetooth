using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ServerClient;

// Icon : http://icones.pro/en/bluetooth-5-png-image.html

namespace BluetoothToTCP
{
    public partial class Form1 : Form
    {

        private MainClass mainClass;
        private Timer timer;

        public Form1()
        {
            InitializeComponent();
        }

        public void setLastTCPMessage(string text)
        {
            TCPLastMessageTB.Text = text;
        }

        public void setLastBTMessage(string text)
        {
            BTLastMessageTB.Text = text;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Hide();
            mainClass = new MainClass(this);

            timer = new Timer();
            timer.Tick += new EventHandler(Update);
            timer.Interval = 1000 / 30; // 30 FPS, in miliseconds
            timer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (disposing)
                mainClass.Dispose();

            base.Dispose(disposing);
        }

        private void Update(object sender, EventArgs e)
        {
            TCPConnectionStateTB.Text = TransmitterSingleton.Instance.connectionState;
            BTConnectionStateTB.Text = BTTransmitterSingleton.Instance.connectionState;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ServerClient;

namespace BluetoothToTCP
{
    class MainClass
    {

        private Form1 originalForm;
        private IContainer components;
        private DataClient client;
        private Timer timer;
        private LockFreeLinkPool<string> BTDataIn;

        public MainClass(Form form)
        {
            originalForm = (Form1)form;
            Console.WriteLine("Starting BluetoothToTCP");
            createTrayIcon();

            client = new DataClient();
            client.getDataDelegate += getTCPData;
            client.Start();

            BTDataIn = new LockFreeLinkPool<string>();
            timer = new Timer();
            timer.Tick += new EventHandler(Update);
            timer.Interval = 1000 / 60; // 60 FPS, in miliseconds
            timer.Start();

            BTTransmitterSingleton.Instance.getDataDelegate += getBTDataFromThread;
            BTTransmitterSingleton.Instance.StartBluetooth();
        }

        void Update(object sender, EventArgs e)
        {
            // Check Concurrent Pool for data
            List<string> dataList = new List<string>();
            SingleLinkNode<string> node = null;
            while (BTDataIn.Pop(out node))
            {
                dataList.Add(node.Item);
            }
            if (dataList.Count > 0)
            {
                // Turn the LIFO list into a FIFO list
                dataList.Reverse();
                foreach (string data in dataList)
                {
                    getBTData(data);
                }
            }
        }

        public void Dispose()
        {
            if (components != null)
                components.Dispose();

            client.OnDestroy();
        }



        private void getTCPData(string data)
        {
            originalForm.setLastTCPMessage(data);

            // Send it by Bluetooth
            BTTransmitterSingleton.Instance.Send(data);
        }

        // Can be called from another thread
        private void getBTDataFromThread(string data)
        {
            SingleLinkNode<string> node = new SingleLinkNode<string>();
            node.Item = data;
            BTDataIn.Push(node);
        }

        private void getBTData(string data)
        {
            originalForm.setLastBTMessage(data);

            // Send it by TCP
            client.sendData(data);
        }
        
        
        
        private void createTrayIcon()
        {
            components = new Container();
            NotifyIcon trayIcon = new NotifyIcon(components);
            trayIcon.Text = "Bluetooth To TCP";
            trayIcon.Icon = BluetoothToTCP.Properties.Resources.Icon;
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem();
            contextMenu.MenuItems.AddRange(new MenuItem[] { menuItem });
            menuItem.Index = 0;
            menuItem.Text = "E&xit";
            menuItem.Click += new System.EventHandler(this.menuItem_Click);
            trayIcon.ContextMenu = contextMenu;
            trayIcon.DoubleClick += new System.EventHandler(this.trayIcon_DoubleClick);
            trayIcon.Visible = true;
        }

        private void trayIcon_DoubleClick(object Sender, EventArgs e)
        {
            originalForm.Show();
            originalForm.WindowState = FormWindowState.Normal;
        }

        private void menuItem_Click(object Sender, EventArgs e)
        {
            originalForm.Close();
        }
    }
}

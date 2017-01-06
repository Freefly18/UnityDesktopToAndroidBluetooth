namespace BluetoothToTCP
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.TCPLastMessageTB = new System.Windows.Forms.TextBox();
            this.TCPConnectionStateTB = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.BTLastMessageTB = new System.Windows.Forms.TextBox();
            this.BTConnectionStateTB = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.TCPLastMessageTB);
            this.groupBox1.Controls.Add(this.TCPConnectionStateTB);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(492, 173);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "TCP";
            // 
            // TCPLastMessageTB
            // 
            this.TCPLastMessageTB.Location = new System.Drawing.Point(9, 60);
            this.TCPLastMessageTB.Multiline = true;
            this.TCPLastMessageTB.Name = "TCPLastMessageTB";
            this.TCPLastMessageTB.ReadOnly = true;
            this.TCPLastMessageTB.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TCPLastMessageTB.Size = new System.Drawing.Size(477, 107);
            this.TCPLastMessageTB.TabIndex = 2;
            // 
            // TCPConnectionStateTB
            // 
            this.TCPConnectionStateTB.Location = new System.Drawing.Point(133, 30);
            this.TCPConnectionStateTB.Name = "TCPConnectionStateTB";
            this.TCPConnectionStateTB.ReadOnly = true;
            this.TCPConnectionStateTB.Size = new System.Drawing.Size(353, 22);
            this.TCPConnectionStateTB.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Connection State:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.BTLastMessageTB);
            this.groupBox2.Controls.Add(this.BTConnectionStateTB);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(12, 195);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(492, 173);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Bluetooth";
            // 
            // BTLastMessageTB
            // 
            this.BTLastMessageTB.Location = new System.Drawing.Point(9, 60);
            this.BTLastMessageTB.Multiline = true;
            this.BTLastMessageTB.Name = "BTLastMessageTB";
            this.BTLastMessageTB.ReadOnly = true;
            this.BTLastMessageTB.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.BTLastMessageTB.Size = new System.Drawing.Size(477, 107);
            this.BTLastMessageTB.TabIndex = 5;
            // 
            // BTConnectionStateTB
            // 
            this.BTConnectionStateTB.Location = new System.Drawing.Point(133, 30);
            this.BTConnectionStateTB.Name = "BTConnectionStateTB";
            this.BTConnectionStateTB.ReadOnly = true;
            this.BTConnectionStateTB.Size = new System.Drawing.Size(353, 22);
            this.BTConnectionStateTB.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Connection State:";
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(264, 389);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(117, 38);
            this.button1.TabIndex = 2;
            this.button1.Text = "Hide";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(387, 389);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(117, 38);
            this.button2.TabIndex = 3;
            this.button2.Text = "Stop";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(516, 444);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "BluetoothToTCP";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TCPConnectionStateTB;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox TCPLastMessageTB;
        private System.Windows.Forms.TextBox BTLastMessageTB;
        private System.Windows.Forms.TextBox BTConnectionStateTB;
        private System.Windows.Forms.Label label2;
    }
}


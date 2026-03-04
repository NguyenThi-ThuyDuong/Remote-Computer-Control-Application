namespace Client
{
    partial class frmClient
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grpPower = new System.Windows.Forms.GroupBox();
            this.btnSleep = new System.Windows.Forms.Button();
            this.btnRestart = new System.Windows.Forms.Button();
            this.btnShutdown = new System.Windows.Forms.Button();
            this.btnXoa = new System.Windows.Forms.Button();
            this.btnGui = new System.Windows.Forms.Button();
            this.txtChat = new System.Windows.Forms.TextBox();
            this.lstChat = new System.Windows.Forms.ListBox();
            this.cmbClients = new System.Windows.Forms.ComboBox();
            this.grpScreen = new System.Windows.Forms.GroupBox();
            this.btnScreen = new System.Windows.Forms.Button();
            this.grpCMD = new System.Windows.Forms.GroupBox();
            this.txtLenhCMD = new System.Windows.Forms.TextBox();
            this.btnRunCMD = new System.Windows.Forms.Button();
            this.picScreen = new System.Windows.Forms.PictureBox();
            this.grpPower.SuspendLayout();
            this.grpScreen.SuspendLayout();
            this.grpCMD.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picScreen)).BeginInit();
            this.SuspendLayout();
            // 
            // grpPower
            // 
            this.grpPower.Controls.Add(this.btnSleep);
            this.grpPower.Controls.Add(this.btnRestart);
            this.grpPower.Controls.Add(this.btnShutdown);
            this.grpPower.Location = new System.Drawing.Point(92, 474);
            this.grpPower.Margin = new System.Windows.Forms.Padding(4);
            this.grpPower.Name = "grpPower";
            this.grpPower.Padding = new System.Windows.Forms.Padding(4);
            this.grpPower.Size = new System.Drawing.Size(333, 123);
            this.grpPower.TabIndex = 11;
            this.grpPower.TabStop = false;
            this.grpPower.Text = "Điều khiển nguồn";
            // 
            // btnSleep
            // 
            this.btnSleep.BackColor = System.Drawing.Color.MediumAquamarine;
            this.btnSleep.Location = new System.Drawing.Point(100, 80);
            this.btnSleep.Margin = new System.Windows.Forms.Padding(4);
            this.btnSleep.Name = "btnSleep";
            this.btnSleep.Size = new System.Drawing.Size(133, 37);
            this.btnSleep.TabIndex = 0;
            this.btnSleep.Text = "Ngủ";
            this.btnSleep.UseVisualStyleBackColor = false;
            // 
            // btnRestart
            // 
            this.btnRestart.BackColor = System.Drawing.Color.LightPink;
            this.btnRestart.Location = new System.Drawing.Point(173, 37);
            this.btnRestart.Margin = new System.Windows.Forms.Padding(4);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(133, 37);
            this.btnRestart.TabIndex = 1;
            this.btnRestart.Text = "Khởi động lại";
            this.btnRestart.UseVisualStyleBackColor = false;
            // 
            // btnShutdown
            // 
            this.btnShutdown.BackColor = System.Drawing.Color.Crimson;
            this.btnShutdown.Location = new System.Drawing.Point(27, 37);
            this.btnShutdown.Margin = new System.Windows.Forms.Padding(4);
            this.btnShutdown.Name = "btnShutdown";
            this.btnShutdown.Size = new System.Drawing.Size(133, 37);
            this.btnShutdown.TabIndex = 2;
            this.btnShutdown.Text = "Tắt máy";
            this.btnShutdown.UseVisualStyleBackColor = false;
            // 
            // btnXoa
            // 
            this.btnXoa.BackColor = System.Drawing.Color.Crimson;
            this.btnXoa.Location = new System.Drawing.Point(569, 425);
            this.btnXoa.Margin = new System.Windows.Forms.Padding(4);
            this.btnXoa.Name = "btnXoa";
            this.btnXoa.Size = new System.Drawing.Size(100, 28);
            this.btnXoa.TabIndex = 12;
            this.btnXoa.Text = "Xóa";
            this.btnXoa.UseVisualStyleBackColor = false;
            // 
            // btnGui
            // 
            this.btnGui.BackColor = System.Drawing.Color.DarkCyan;
            this.btnGui.Location = new System.Drawing.Point(448, 425);
            this.btnGui.Margin = new System.Windows.Forms.Padding(4);
            this.btnGui.Name = "btnGui";
            this.btnGui.Size = new System.Drawing.Size(100, 28);
            this.btnGui.TabIndex = 13;
            this.btnGui.Text = "Gửi";
            this.btnGui.UseVisualStyleBackColor = false;
            // 
            // txtChat
            // 
            this.txtChat.Location = new System.Drawing.Point(91, 425);
            this.txtChat.Margin = new System.Windows.Forms.Padding(4);
            this.txtChat.Name = "txtChat";
            this.txtChat.Size = new System.Drawing.Size(335, 22);
            this.txtChat.TabIndex = 14;
            // 
            // lstChat
            // 
            this.lstChat.BackColor = System.Drawing.Color.MistyRose;
            this.lstChat.ItemHeight = 16;
            this.lstChat.Location = new System.Drawing.Point(92, 79);
            this.lstChat.Margin = new System.Windows.Forms.Padding(4);
            this.lstChat.Name = "lstChat";
            this.lstChat.Size = new System.Drawing.Size(577, 324);
            this.lstChat.TabIndex = 15;
            // 
            // cmbClients
            // 
            this.cmbClients.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbClients.Location = new System.Drawing.Point(693, 429);
            this.cmbClients.Margin = new System.Windows.Forms.Padding(4);
            this.cmbClients.Name = "cmbClients";
            this.cmbClients.Size = new System.Drawing.Size(377, 24);
            this.cmbClients.TabIndex = 16;
            // 
            // grpScreen
            // 
            this.grpScreen.Controls.Add(this.btnScreen);
            this.grpScreen.Location = new System.Drawing.Point(433, 474);
            this.grpScreen.Margin = new System.Windows.Forms.Padding(4);
            this.grpScreen.Name = "grpScreen";
            this.grpScreen.Padding = new System.Windows.Forms.Padding(4);
            this.grpScreen.Size = new System.Drawing.Size(204, 123);
            this.grpScreen.TabIndex = 10;
            this.grpScreen.TabStop = false;
            this.grpScreen.Text = "Màn hình & Tiện ích";
            // 
            // btnScreen
            // 
            this.btnScreen.BackColor = System.Drawing.Color.Pink;
            this.btnScreen.Location = new System.Drawing.Point(27, 37);
            this.btnScreen.Margin = new System.Windows.Forms.Padding(4);
            this.btnScreen.Name = "btnScreen";
            this.btnScreen.Size = new System.Drawing.Size(133, 37);
            this.btnScreen.TabIndex = 1;
            this.btnScreen.Text = "Xem màn hình";
            this.btnScreen.UseVisualStyleBackColor = false;
            // 
            // grpCMD
            // 
            this.grpCMD.Controls.Add(this.txtLenhCMD);
            this.grpCMD.Controls.Add(this.btnRunCMD);
            this.grpCMD.Location = new System.Drawing.Point(710, 474);
            this.grpCMD.Margin = new System.Windows.Forms.Padding(4);
            this.grpCMD.Name = "grpCMD";
            this.grpCMD.Padding = new System.Windows.Forms.Padding(4);
            this.grpCMD.Size = new System.Drawing.Size(360, 123);
            this.grpCMD.TabIndex = 8;
            this.grpCMD.TabStop = false;
            this.grpCMD.Text = "Lệnh CMD";
            // 
            // txtLenhCMD
            // 
            this.txtLenhCMD.Location = new System.Drawing.Point(27, 37);
            this.txtLenhCMD.Margin = new System.Windows.Forms.Padding(4);
            this.txtLenhCMD.Name = "txtLenhCMD";
            this.txtLenhCMD.Size = new System.Drawing.Size(305, 22);
            this.txtLenhCMD.TabIndex = 0;
            // 
            // btnRunCMD
            // 
            this.btnRunCMD.BackColor = System.Drawing.Color.PaleVioletRed;
            this.btnRunCMD.Location = new System.Drawing.Point(113, 74);
            this.btnRunCMD.Margin = new System.Windows.Forms.Padding(4);
            this.btnRunCMD.Name = "btnRunCMD";
            this.btnRunCMD.Size = new System.Drawing.Size(133, 37);
            this.btnRunCMD.TabIndex = 1;
            this.btnRunCMD.Text = "Chạy Lệnh";
            this.btnRunCMD.UseVisualStyleBackColor = false;
            // 
            // picScreen
            // 
            this.picScreen.BackColor = System.Drawing.Color.Azure;
            this.picScreen.Location = new System.Drawing.Point(710, 78);
            this.picScreen.Name = "picScreen";
            this.picScreen.Size = new System.Drawing.Size(669, 325);
            this.picScreen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picScreen.TabIndex = 9;
            this.picScreen.TabStop = false;
            // 
            // frmClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.SteelBlue;
            this.ClientSize = new System.Drawing.Size(1471, 674);
            this.Controls.Add(this.grpPower);
            this.Controls.Add(this.btnXoa);
            this.Controls.Add(this.btnGui);
            this.Controls.Add(this.txtChat);
            this.Controls.Add(this.lstChat);
            this.Controls.Add(this.cmbClients);
            this.Controls.Add(this.grpScreen);
            this.Controls.Add(this.grpCMD);
            this.Controls.Add(this.picScreen);
            this.Name = "frmClient";
            this.Text = "Client Control";
            this.grpPower.ResumeLayout(false);
            this.grpScreen.ResumeLayout(false);
            this.grpCMD.ResumeLayout(false);
            this.grpCMD.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picScreen)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpPower;
        private System.Windows.Forms.Button btnSleep;
        private System.Windows.Forms.Button btnRestart;
        private System.Windows.Forms.Button btnShutdown;
        private System.Windows.Forms.Button btnXoa;
        private System.Windows.Forms.Button btnGui;
        private System.Windows.Forms.TextBox txtChat;
        private System.Windows.Forms.ListBox lstChat;
        private System.Windows.Forms.ComboBox cmbClients;
        private System.Windows.Forms.GroupBox grpScreen;
        private System.Windows.Forms.Button btnScreen;
        private System.Windows.Forms.GroupBox grpCMD;
        private System.Windows.Forms.TextBox txtLenhCMD;
        private System.Windows.Forms.Button btnRunCMD;
        private System.Windows.Forms.PictureBox picScreen;
    }
}
﻿namespace chat_system_winforms
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            chatRoomsBox = new RichTextBox();
            btnJoin = new Button();
            sendMessageBtn = new Button();
            tbUsername = new TextBox();
            tbMessage = new TextBox();
            SuspendLayout();
            // 
            // chatRoomsBox
            // 
            chatRoomsBox.Location = new Point(294, 12);
            chatRoomsBox.Name = "chatRoomsBox";
            chatRoomsBox.ReadOnly = true;
            chatRoomsBox.Size = new Size(511, 414);
            chatRoomsBox.TabIndex = 0;
            chatRoomsBox.Text = "";
            // 
            // btnJoin
            // 
            btnJoin.Location = new Point(15, 46);
            btnJoin.Name = "btnJoin";
            btnJoin.Size = new Size(114, 30);
            btnJoin.TabIndex = 1;
            btnJoin.Text = "Join room";
            btnJoin.UseVisualStyleBackColor = true;
            btnJoin.Click += btnJoin_Click;
            // 
            // sendMessageBtn
            // 
            sendMessageBtn.Location = new Point(158, 46);
            sendMessageBtn.Name = "sendMessageBtn";
            sendMessageBtn.Size = new Size(114, 30);
            sendMessageBtn.TabIndex = 2;
            sendMessageBtn.Text = "Send message";
            sendMessageBtn.UseVisualStyleBackColor = true;
            sendMessageBtn.Click += sendMessageBtn_Click;
            // 
            // tbUsername
            // 
            tbUsername.Location = new Point(15, 17);
            tbUsername.Name = "tbUsername";
            tbUsername.Size = new Size(114, 23);
            tbUsername.TabIndex = 3;
            // 
            // tbMessage
            // 
            tbMessage.Location = new Point(158, 17);
            tbMessage.Name = "tbMessage";
            tbMessage.Size = new Size(114, 23);
            tbMessage.TabIndex = 4;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tbMessage);
            Controls.Add(tbUsername);
            Controls.Add(sendMessageBtn);
            Controls.Add(btnJoin);
            Controls.Add(chatRoomsBox);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox chatRoomsBox;
        private Button btnJoin;
        private Button sendMessageBtn;
        private TextBox tbUsername;
        private TextBox tbMessage;
    }
}
namespace chat_system_winforms
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
            SuspendLayout();
            // 
            // chatRoomsBox
            // 
            chatRoomsBox.Location = new Point(12, 12);
            chatRoomsBox.Name = "chatRoomsBox";
            chatRoomsBox.ReadOnly = true;
            chatRoomsBox.Size = new Size(764, 366);
            chatRoomsBox.TabIndex = 0;
            chatRoomsBox.Text = "";
            // 
            // btnJoin
            // 
            btnJoin.Location = new Point(329, 396);
            btnJoin.Name = "btnJoin";
            btnJoin.Size = new Size(114, 30);
            btnJoin.TabIndex = 1;
            btnJoin.Text = "Join room";
            btnJoin.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnJoin);
            Controls.Add(chatRoomsBox);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox chatRoomsBox;
        private Button btnJoin;
    }
}
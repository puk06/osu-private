using System.ComponentModel;
using System.Windows.Forms;

namespace osu_private.Forms
{
    partial class RegistrationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(RegistrationForm));
            button1 = new Button();
            label1 = new Label();
            usernameForm = new ComboBox();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Font = new System.Drawing.Font("メイリオ", 12F);
            button1.Location = new System.Drawing.Point(117, 55);
            button1.Margin = new Padding(4, 4, 4, 4);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(144, 43);
            button1.TabIndex = 0;
            button1.Text = "Play osu!";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("メイリオ", 10F);
            label1.Location = new System.Drawing.Point(8, 17);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(91, 21);
            label1.TabIndex = 2;
            label1.Text = "ユーザー名 :";
            // 
            // usernameForm
            // 
            usernameForm.Font = new System.Drawing.Font("メイリオ", 10F);
            usernameForm.FormattingEnabled = true;
            usernameForm.Location = new System.Drawing.Point(107, 13);
            usernameForm.Margin = new Padding(4, 4, 4, 4);
            usernameForm.Name = "usernameForm";
            usernameForm.Size = new System.Drawing.Size(259, 28);
            usernameForm.TabIndex = 3;
            // 
            // RegistrationForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(379, 111);
            Controls.Add(usernameForm);
            Controls.Add(label1);
            Controls.Add(button1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 4, 4, 4);
            MaximizeBox = false;
            Name = "RegistrationForm";
            Text = "ユーザー名の選択";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Label label1;
        private ComboBox usernameForm;
    }
}
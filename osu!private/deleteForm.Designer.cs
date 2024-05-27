using System.ComponentModel;
using System.Windows.Forms;

namespace osu_private
{
    partial class DeleteForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeleteForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.modeValue = new System.Windows.Forms.ComboBox();
            this.scoreList = new System.Windows.Forms.ComboBox();
            this.deleteButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("メイリオ", 16F);
            this.label1.Location = new System.Drawing.Point(15, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 33);
            this.label1.TabIndex = 0;
            this.label1.Text = "Mode :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("メイリオ", 16F);
            this.label2.Location = new System.Drawing.Point(12, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 33);
            this.label2.TabIndex = 1;
            this.label2.Text = "Score :";
            // 
            // modeValue
            // 
            this.modeValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.modeValue.Font = new System.Drawing.Font("メイリオ", 10F);
            this.modeValue.FormattingEnabled = true;
            this.modeValue.Items.AddRange(new object[] {
            "osu!standard",
            "osu!taiko",
            "osu!catch",
            "osu!mania"});
            this.modeValue.Location = new System.Drawing.Point(109, 14);
            this.modeValue.Name = "modeValue";
            this.modeValue.Size = new System.Drawing.Size(174, 28);
            this.modeValue.TabIndex = 2;
            this.modeValue.SelectedIndexChanged += new System.EventHandler(this.modeValue_SelectedIndexChanged);
            // 
            // scoreList
            // 
            this.scoreList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.scoreList.Font = new System.Drawing.Font("メイリオ", 10F);
            this.scoreList.FormattingEnabled = true;
            this.scoreList.Location = new System.Drawing.Point(109, 56);
            this.scoreList.Name = "scoreList";
            this.scoreList.Size = new System.Drawing.Size(364, 28);
            this.scoreList.TabIndex = 3;
            // 
            // deleteButton
            // 
            this.deleteButton.Font = new System.Drawing.Font("メイリオ", 14F);
            this.deleteButton.Location = new System.Drawing.Point(168, 90);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(151, 50);
            this.deleteButton.TabIndex = 4;
            this.deleteButton.Text = "Delete";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // deleteForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 152);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.scoreList);
            this.Controls.Add(this.modeValue);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DeleteForm";
            this.Text = "削除するスコアの選択";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private ComboBox modeValue;
        private ComboBox scoreList;
        private Button deleteButton;
    }
}
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace osu_private.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(MainForm));
            this.accText = new Label();
            this.ppText = new Label();
            this.bonusPPText = new Label();
            this.accValue = new Label();
            this.globalPPValue = new Label();
            this.BonusPPValue = new Label();
            this.BestPerformance = new ListBox();
            this.label1 = new Label();
            this.modeValue = new ComboBox();
            this.changePPValue = new Label();
            this.changeACCValue = new Label();
            this.changeBonusPPValue = new Label();
            this.deleteScore = new Button();
            this.label2 = new Label();
            this.label3 = new Label();
            this.playtimeValue = new Label();
            this.playcountValue = new Label();
            this.errorText = new Label();
            this.SuspendLayout();
            // 
            // accText
            // 
            this.accText.AutoSize = true;
            this.accText.Font = new Font("メイリオ", 16.25F);
            this.accText.Location = new Point(320, 54);
            this.accText.Name = "accText";
            this.accText.Size = new Size(76, 33);
            this.accText.TabIndex = 0;
            this.accText.Text = "ACC :";
            // 
            // ppText
            // 
            this.ppText.AutoSize = true;
            this.ppText.Font = new Font("メイリオ", 16.25F);
            this.ppText.Location = new Point(14, 54);
            this.ppText.Name = "ppText";
            this.ppText.Size = new Size(124, 33);
            this.ppText.TabIndex = 1;
            this.ppText.Text = "GlobalPP :";
            // 
            // bonusPPText
            // 
            this.bonusPPText.AutoSize = true;
            this.bonusPPText.Font = new Font("メイリオ", 16.25F);
            this.bonusPPText.Location = new Point(548, 54);
            this.bonusPPText.Name = "bonusPPText";
            this.bonusPPText.Size = new Size(124, 33);
            this.bonusPPText.TabIndex = 2;
            this.bonusPPText.Text = "BonusPP :";
            // 
            // accValue
            // 
            this.accValue.AutoSize = true;
            this.accValue.Font = new Font("メイリオ", 20.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.accValue.Location = new Point(390, 48);
            this.accValue.Name = "accValue";
            this.accValue.Size = new Size(63, 41);
            this.accValue.TabIndex = 3;
            this.accValue.Text = "0%";
            // 
            // globalPPValue
            // 
            this.globalPPValue.AutoSize = true;
            this.globalPPValue.Font = new Font("メイリオ", 20.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.globalPPValue.Location = new Point(131, 48);
            this.globalPPValue.Name = "globalPPValue";
            this.globalPPValue.Size = new Size(67, 41);
            this.globalPPValue.TabIndex = 4;
            this.globalPPValue.Text = "0pp";
            // 
            // BonusPPValue
            // 
            this.BonusPPValue.AutoSize = true;
            this.BonusPPValue.Font = new Font("メイリオ", 20.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.BonusPPValue.Location = new Point(667, 48);
            this.BonusPPValue.Name = "BonusPPValue";
            this.BonusPPValue.Size = new Size(67, 41);
            this.BonusPPValue.TabIndex = 5;
            this.BonusPPValue.Text = "0pp";
            // 
            // listBox1
            // 
            this.BestPerformance.Font = new Font("メイリオ", 9.7F);
            this.BestPerformance.FormattingEnabled = true;
            this.BestPerformance.ItemHeight = 20;
            this.BestPerformance.Location = new Point(16, 121);
            this.BestPerformance.Name = "BestPerformance";
            this.BestPerformance.Size = new Size(848, 524);
            this.BestPerformance.TabIndex = 6;
            this.BestPerformance.KeyDown += new KeyEventHandler(this.listBox1_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new Font("メイリオ", 16.25F);
            this.label1.Location = new Point(20, 9);
            this.label1.Name = "label1";
            this.label1.Size = new Size(96, 33);
            this.label1.TabIndex = 7;
            this.label1.Text = "MODE :";
            // 
            // modeValue
            // 
            this.modeValue.DropDownStyle = ComboBoxStyle.DropDownList;
            this.modeValue.Font = new Font("メイリオ", 12F);
            this.modeValue.FormattingEnabled = true;
            this.modeValue.Items.AddRange(new object[] {
            "osu!standard",
            "osu!taiko",
            "osu!catch",
            "osu!mania"});
            this.modeValue.Location = new Point(122, 10);
            this.modeValue.Name = "modeValue";
            this.modeValue.Size = new Size(197, 32);
            this.modeValue.TabIndex = 8;
            this.modeValue.SelectedIndexChanged += new EventHandler(this.modeValue_SelectedIndexChanged);
            // 
            // changePPValue
            // 
            this.changePPValue.AutoSize = true;
            this.changePPValue.Font = new Font("メイリオ", 12F);
            this.changePPValue.Location = new Point(134, 87);
            this.changePPValue.Name = "changePPValue";
            this.changePPValue.Size = new Size(0, 24);
            this.changePPValue.TabIndex = 9;
            // 
            // changeACCValue
            // 
            this.changeACCValue.AutoSize = true;
            this.changeACCValue.Font = new Font("メイリオ", 12F);
            this.changeACCValue.Location = new Point(393, 87);
            this.changeACCValue.Name = "changeACCValue";
            this.changeACCValue.Size = new Size(0, 24);
            this.changeACCValue.TabIndex = 10;
            // 
            // changeBonusPPValue
            // 
            this.changeBonusPPValue.AutoSize = true;
            this.changeBonusPPValue.Font = new Font("メイリオ", 12F);
            this.changeBonusPPValue.Location = new Point(670, 87);
            this.changeBonusPPValue.Name = "changeBonusPPValue";
            this.changeBonusPPValue.Size = new Size(0, 24);
            this.changeBonusPPValue.TabIndex = 11;
            // 
            // deleteScore
            // 
            this.deleteScore.Location = new Point(341, 10);
            this.deleteScore.Name = "deleteScore";
            this.deleteScore.Size = new Size(112, 32);
            this.deleteScore.TabIndex = 12;
            this.deleteScore.Text = "Delete score";
            this.deleteScore.UseVisualStyleBackColor = true;
            this.deleteScore.Click += new EventHandler(this.deleteScore_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new Font("メイリオ", 10F);
            this.label2.Location = new Point(493, 9);
            this.label2.Name = "label2";
            this.label2.Size = new Size(84, 21);
            this.label2.TabIndex = 13;
            this.label2.Text = "Playtime : ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new Font("メイリオ", 10F);
            this.label3.Location = new Point(485, 30);
            this.label3.Name = "label3";
            this.label3.Size = new Size(92, 21);
            this.label3.TabIndex = 14;
            this.label3.Text = "Playcount : ";
            // 
            // playtimeValue
            // 
            this.playtimeValue.AutoSize = true;
            this.playtimeValue.Font = new Font("メイリオ", 11F);
            this.playtimeValue.Location = new Point(572, 9);
            this.playtimeValue.Name = "playtimeValue";
            this.playtimeValue.Size = new Size(56, 23);
            this.playtimeValue.TabIndex = 15;
            this.playtimeValue.Text = "0h 0m";
            // 
            // playcountValue
            // 
            this.playcountValue.AutoSize = true;
            this.playcountValue.Font = new Font("メイリオ", 11F);
            this.playcountValue.Location = new Point(572, 30);
            this.playcountValue.Name = "playcountValue";
            this.playcountValue.Size = new Size(19, 23);
            this.playcountValue.TabIndex = 16;
            this.playcountValue.Text = "0";
            // 
            // errorText
            // 
            this.errorText.AutoSize = true;
            this.errorText.Font = new Font("メイリオ", 12F);
            this.errorText.ForeColor = Color.Red;
            this.errorText.Location = new Point(12, 656);
            this.errorText.Name = "errorText";
            this.errorText.Size = new Size(68, 24);
            this.errorText.TabIndex = 17;
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 18F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(876, 694);
            this.Controls.Add(this.errorText);
            this.Controls.Add(this.playcountValue);
            this.Controls.Add(this.playtimeValue);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.deleteScore);
            this.Controls.Add(this.changeBonusPPValue);
            this.Controls.Add(this.changeACCValue);
            this.Controls.Add(this.changePPValue);
            this.Controls.Add(this.modeValue);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BestPerformance);
            this.Controls.Add(this.BonusPPValue);
            this.Controls.Add(this.globalPPValue);
            this.Controls.Add(this.accValue);
            this.Controls.Add(this.bonusPPText);
            this.Controls.Add(this.ppText);
            this.Controls.Add(this.accText);
            this.Font = new Font("メイリオ", 9F);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Icon = ((Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new Padding(4);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "osu!private";
            this.Closed += new EventHandler(this.MainForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public Label accText;
        public Label ppText;
        public Label bonusPPText;
        public Label accValue;
        public Label globalPPValue;
        public Label BonusPPValue;
        public ListBox BestPerformance;
        public Label label1;
        public ComboBox modeValue;
        private Label changePPValue;
        private Label changeACCValue;
        private Label changeBonusPPValue;
        private Button deleteScore;
        private Label label2;
        private Label label3;
        private Label playtimeValue;
        private Label playcountValue;
        private Label errorText;
    }
}


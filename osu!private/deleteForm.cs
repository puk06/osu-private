using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace osu_private
{
    public partial class DeleteForm : Form
    {
        public DeleteForm()
        {
            InitializeComponent();
            modeValue.SelectedIndex = 0;
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(scoreList.Text))
            {
                MessageBox.Show("スコアを選択してください！", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            DialogResult result = MessageBox.Show($"本当にこのスコアを削除してもよろしいですか？\n\n スコア: {scoreList.Items[scoreList.SelectedIndex]}", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.Yes)
            {
                try
                {
                    StreamReader sr = new StreamReader(@"./src/user/" + MainForm.Username + ".json", Encoding.GetEncoding("UTF-8"));
                    string json = sr.ReadToEnd();
                    sr.Close();
                    JObject jo = JObject.Parse(json);
                    string mode = MainForm.ConvertMode(modeValue.Text);
                    int index = scoreList.SelectedIndex;
                    jo["pp"][mode][index].Remove();
                    StreamWriter sw = new StreamWriter(@"./src/user/" + MainForm.Username + ".json", false, new UTF8Encoding(false));
                    sw.Write(jo.ToString(Formatting.Indented));
                    sw.Close();
                    MessageBox.Show("削除が完了しました！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Hide();
                }
                catch
                {
                    MessageBox.Show("削除に失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("キャンセルしました。", "キャンセル", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void modeValue_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                scoreList.Items.Clear();
                StreamReader userdataRecent = new StreamReader($"./src/user/{MainForm.Username}.json");
                string userdataStringRecent = userdataRecent.ReadToEnd();
                userdataRecent.Close();
                JObject userdataJsonRecent = JObject.Parse(userdataStringRecent);
                string mode = MainForm.ConvertMode(modeValue.Text);
                foreach (var score in userdataJsonRecent["pp"][mode])
                {
                    scoreList.Items.Add($"{score["title"]} [{score["version"]}] | {score["pp"]}pp");
                }

                if (scoreList.Items.Count != 0)
                {
                    deleteButton.Enabled = true;
                    scoreList.SelectedIndex = 0;
                    int maxWidth = (from object item in scoreList.Items select TextRenderer.MeasureText(item.ToString(), scoreList.Font).Width).Prepend(0).Max();
                    scoreList.DropDownWidth = maxWidth + 10;
                }
                else
                {
                    deleteButton.Enabled = false;
                }
            }
            catch
            {
                MessageBox.Show("スコアの読み込み中にエラーが発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Hide();
            }
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiteDB;

namespace osu_private.Forms
{
    public partial class RegistrationForm : Form
    {
        public LiteDatabase Db = new("scores.db");

        public RegistrationForm()
        {
            InitializeComponent();
            var collection = Db.GetCollection<PlayerStats>("playerStats");
            var userNames = collection.FindAll().Select(playerStats => playerStats.Username).ToList();
            foreach (var user in userNames)
            {
                usernameForm.Items.Add(user);
            }

            if (usernameForm.Items.Count != 0)
            {
                usernameForm.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("新規ユーザーのようです！このゲームについて軽く説明します。\n\n1. ユーザー名は、選べばそのユーザーのデータが読み込めて、新しい名前を入力したらユーザーが作成されます\n\n2. 新しいユーザーで、記録をつけなかった場合はそのユーザー名は保存されません！記録をつけるとデータが作成されます！\n\n3. pp計算式は2024/11/28時点のStableのものを使っています。\n\n4. 完全にオフラインで実行可能です。PCにネット環境が無くても動作します！osuでのログインも必要ありません！\n\n5. プライベートサーバーのように、GlobalPPやBonusPPなどの計算を行います！タイムアタックなどに使ってください！\n ※プレイが終わった後、すぐ選曲画面に戻るのでは無く、リザルト画面が表示され、PPが反映されたら戻るようにしてください！", "ゲームの説明", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (usernameForm.Text == "")
            {
                MessageBox.Show("登録したいユーザー名を入力してください。\nPlease enter a username you wanna set!", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Db.Dispose();
            MainForm mainForm = new MainForm(usernameForm.Text);
            mainForm.Show();
            Hide();
        }
    }
}

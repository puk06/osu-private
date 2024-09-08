using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiteDB;
using Octokit;

namespace osu_private.Forms
{
    public partial class RegistrationForm : Form
    {
        private const string CURRENT_VERSION = "1.0.0-Release";
        public LiteDatabase Db = new("scores.db");

        public RegistrationForm()
        {
            InitializeComponent();
            var collection = Db.GetCollection<PlayerStats>("playerStats");
            var userNames = collection.FindAll().Select(playerStats => playerStats.Username).ToList();
            GithubUpdateChecker();
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
                MessageBox.Show("新規ユーザーのようです！このゲームについて軽く説明します。\n\n1. ユーザー名は、選べばそのユーザーのデータが読み込めて、新しい名前を入力したらユーザーが作成されます\n\n2. 新しいユーザーで、記録をつけなかった場合はそのユーザー名は保存されません！記録をつけるとデータが作成されます！\n\n3. pp計算式は2024/09/08時点のStableのものを使っています。\n\n4. 完全にオフラインで実行可能です。PCにネット環境が無くても動作します！osuでのログインも必要ありません！\n\n5. プライベートサーバーのように、GlobalPPやBonusPPなどの計算を行います！タイムアタックなどに使ってください！\n ※プレイが終わった後、すぐ選曲画面に戻るのでは無く、リザルト画面が表示され、PPが反映されたら戻るようにしてください！", "ゲームの説明", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private static async void GithubUpdateChecker()
        {
            try
            {
                var latestRelease = await GetVersion(CURRENT_VERSION);
                if (latestRelease == CURRENT_VERSION) return;
                DialogResult result = MessageBox.Show($"最新バージョンがあります！\n\n現在: {CURRENT_VERSION} \n更新後: {latestRelease}\n\nダウンロードしますか？", "アップデートのお知らせ", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result != DialogResult.Yes) return;

                if (!File.Exists("./Updater/Software Updater.exe"))
                {
                    MessageBox.Show("アップデーターが見つかりませんでした。手動でダウンロードしてください。", "エラー", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                string updaterPath = Path.GetFullPath("./Updater/Software Updater.exe");
                const string author = "puk06";
                const string repository = "osu-private";
                const string executableName = "osu!private";
                ProcessStartInfo args = new()
                {
                    FileName = $"\"{updaterPath}\"",
                    Arguments = $"\"{latestRelease}\" \"{author}\" \"{repository}\" \"{executableName}\"",
                    UseShellExecute = true
                };

                Process.Start(args);
            }
            catch (Exception exception)
            {
                MessageBox.Show("アップデートチェック中にエラーが発生しました" + exception.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static async Task<string> GetVersion(string currentVersion)
        {
            try
            {
                var releaseType = currentVersion.Split('-')[1];
                var githubClient = new GitHubClient(new ProductHeaderValue("osu-private"));
                var tags = await githubClient.Repository.GetAllTags("puk06", "osu-private");
                string latestVersion = currentVersion;
                foreach (var tag in tags)
                {
                    if (releaseType == "Release")
                    {
                        if (tag.Name.Split("-")[1] != "Release") continue;
                        latestVersion = tag.Name;
                        break;
                    }

                    latestVersion = tag.Name;
                    break;
                }

                return latestVersion;
            }
            catch
            {
                throw new Exception("アップデートの取得に失敗しました");
            }
        }
    }
}

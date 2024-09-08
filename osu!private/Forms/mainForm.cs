using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiteDB;
using osu_private.Classes;
using OsuMemoryDataProvider;
using OsuMemoryDataProvider.OsuMemoryModels;
using OsuParsers.Decoders;
using ObjectId = LiteDB.ObjectId;
using Beatmap = OsuParsers.Beatmaps.Beatmap;

namespace osu_private.Forms
{
    public partial class MainForm : Form
    {
        private readonly StructuredOsuMemoryReader sreader = new();
        private readonly OsuBaseAddresses baseAddresses = new();

        public static string Username;

        private bool isDirectoryLoaded;
        private string osuDirectory;
        private string songsPath;
        private int currentMode;
        private OsuMemoryStatus currentStatus;
        private DateTime? startTime;
        private DateTime? endTime;
        private bool hasEnded;
        private bool nowPlaying;
        private bool hasChanged;


        private static readonly Dictionary<string, double> globalPp = new()
        {
            {"osu", 0},
            {"taiko", 0},
            {"catch", 0},
            {"mania", 0}
        };

        private static readonly Dictionary<string, double> globalAcc = new()
        {
            {"osu", 0},
            {"taiko", 0},
            {"catch", 0},
            {"mania", 0}
        };

        private static readonly Dictionary<string, double> bonusPp = new()
        {
            {"osu", 0},
            {"taiko", 0},
            {"catch", 0},
            {"mania", 0}
        };

        public static readonly Dictionary<int, string> OSU_MODS = new()
        {
            { 0, "NM" },
            { 1, "NF" },
            { 2, "EZ" },
            { 4, "TD" },
            { 8, "HD" },
            { 16, "HR" },
            { 32, "SD" },
            { 64, "DT" },
            { 128, "RX" },
            { 256, "HT" },
            { 512, "NC" },
            { 1024, "FL" },
            { 2048, "AT" },
            { 4096, "SO" },
            { 8192, "RX2" },
            { 16384, "PF" },
            { 32768, "4K" },
            { 65536, "5K" },
            { 131072, "6K" },
            { 262144, "7K" },
            { 524288, "8K" },
            { 1048576, "FI" },
            { 2097152, "RD" },
            { 4194304, "CM" },
            { 8388608, "TP" },
            { 16777216, "9K" },
            { 33554432, "CP" },
            { 67108864, "1K" },
            { 134217728, "3K" },
            { 268435456, "2K" },
            { 536870912, "SV2" },
            { 1073741824, "MR" }
        };

        private static readonly string[] gameModes = { "osu", "taiko", "catch", "mania" };
        private readonly LiteDatabase db = new("scores.db");

        public MainForm(string username)
        {
            Username = username;
            Thread thread = new(Loop)
            {
                IsBackground = true
            };
            thread.Start();
            InitializeComponent();
            modeValue.SelectedIndex = 0;
            UpdateFormData();
            FormLoop();
        }

        private void Loop()
        {
            var collection = db.GetCollection<PlayerStats>("playerStats");
            while (true)
            {
                Thread.Sleep(10);
                try
                {
                    if (Process.GetProcessesByName("osu!").Length == 0) continue;
                    if (!isDirectoryLoaded)
                    {
                        Process osuProcess = Process.GetProcessesByName("osu!")[0];
                        osuDirectory = Path.GetDirectoryName(osuProcess.MainModule.FileName);

                        if (string.IsNullOrEmpty(osuDirectory) || !Directory.Exists(osuDirectory)) continue;

                        songsPath = GetSongsFolderLocation(osuDirectory);
                        isDirectoryLoaded = true;
                    }

                    if (!isDirectoryLoaded) continue;

                    if (!sreader.CanRead) continue;

                    sreader.TryRead(baseAddresses.Player);
                    sreader.TryRead(baseAddresses.GeneralData);
                    sreader.TryRead(baseAddresses.Beatmap);

                    if (baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.Playing)
                        currentMode = baseAddresses.Player.Mode;

                    if (currentStatus == OsuMemoryStatus.Playing &&
                        baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.ResultsScreen && !baseAddresses.Player.IsReplay)
                    {
                        //Ranked, Approvedのみ記録をつけるときに消してください
                        //if (_baseAddresses.Beatmap.Status is not (4 or 5)) continue;

                        hasEnded = false;
                        endTime = DateTime.Now;
                        currentStatus = baseAddresses.GeneralData.OsuStatus;
                        string bannedModstext = "AT,CN,AP,RX,SV2,SO,TG";
                        if (!File.Exists("BannedMods.txt"))
                        {
                            var stream = File.Create("BannedMods.txt");
                            byte[] bannedModsBytes = System.Text.Encoding.UTF8.GetBytes(bannedModstext);
                            stream.Write(bannedModsBytes, 0, bannedModsBytes.Length);
                            stream.Close();
                        }
                        else
                        {
                            bannedModstext = File.ReadAllText("BannedMods.txt");
                        }
                        string[] bannedMods = bannedModstext.Split(',')
                            .Where(mod => !string.IsNullOrWhiteSpace(mod) && mod.Length == 2).ToArray();
                        string[] modCalculate = ParseModsCalculate(baseAddresses.Player.Mods.Value);
                        bool bannedmodflag = modCalculate.Any(resultMod => bannedMods.Contains(resultMod));
                        if (bannedmodflag)
                        {
                            startTime = null;
                            endTime = null;
                            currentStatus = baseAddresses.GeneralData.OsuStatus;
                            continue;
                        }

                        string mappath = Path.Combine(songsPath ?? "", baseAddresses.Beatmap.FolderName ?? "",
                            baseAddresses.Beatmap.OsuFileName ?? "");
                        HitsResult hits = new()
                        {
                            Hit300 = baseAddresses.Player.Hit300,
                            Hit100 = baseAddresses.Player.Hit100,
                            Hit50 = baseAddresses.Player.Hit50,
                            HitMiss = baseAddresses.Player.HitMiss,
                            HitGeki = baseAddresses.Player.HitGeki,
                            HitKatu = baseAddresses.Player.HitKatu,
                            Combo = baseAddresses.Player.Combo,
                            Score = baseAddresses.Player.Score
                        };

                        CalculateArgs args = new()
                        {
                            Accuracy = baseAddresses.Player.Accuracy,
                            Combo = baseAddresses.Player.Combo,
                            Score = baseAddresses.Player.Score,
                            Mods = modCalculate
                        };

                        double pp = new PpCalculator(mappath, currentMode).Calculate(args, hits);
                        string confirmHash = baseAddresses.Beatmap.Md5;
                        Beatmap beatmap = BeatmapDecoder.Decode(mappath);

                        string[] mod = ParseMods(baseAddresses.Player.Mods.Value);
                        ScoreData result = new()
                        {
                            Title = beatmap.MetadataSection.Title + " by " + beatmap.MetadataSection.Artist,
                            Mapper = beatmap.MetadataSection.Creator,
                            Version = beatmap.MetadataSection.Version,
                            Pp = pp,
                            Score = baseAddresses.Player.Score,
                            Mods = string.Join(",", mod),
                            Acc = baseAddresses.Player.Accuracy,
                            Combo = baseAddresses.Player.Combo,
                            Hit300 = baseAddresses.Player.Hit300,
                            Hit100 = baseAddresses.Player.Hit100,
                            Hit50 = baseAddresses.Player.Hit50,
                            HitMiss = baseAddresses.Player.HitMiss,
                            HitKatu = baseAddresses.Player.HitKatu,
                            HitGeki = baseAddresses.Player.HitGeki,
                            Date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                            Hash = confirmHash
                        };

                        if (collection.FindOne(Query.EQ("Username", Username)) == null)
                        {
                            var playerStats = new PlayerStats
                            {
                                Username = Username,
                                LastGamemode = 0,
                                GlobalPp = new Dictionary<string, double>
                                {
                                    { "osu", 0 },
                                    { "taiko", 0 },
                                    { "catch", 0 },
                                    { "mania", 0 }
                                },
                                BonusPp = new Dictionary<string, double>
                                {
                                    { "osu", 0 },
                                    { "taiko", 0 },
                                    { "catch", 0 },
                                    { "mania", 0 }
                                },
                                GlobalAcc = new Dictionary<string, double>
                                {
                                    { "osu", 0 },
                                    { "taiko", 0 },
                                    { "catch", 0 },
                                    { "mania", 0 }
                                },
                                Playtime = new Dictionary<string, string>
                                {
                                    { "osu", "0h 0m" },
                                    { "taiko", "0h 0m" },
                                    { "catch", "0h 0m" },
                                    { "mania", "0h 0m" }
                                },
                                PlaytimeCalculate = new Dictionary<string, long>
                                {
                                    { "osu", 0 },
                                    { "taiko", 0 },
                                    { "catch", 0 },
                                    { "mania", 0 }
                                },
                                Playcount = new Dictionary<string, int>
                                {
                                    { "osu", 0 },
                                    { "taiko", 0 },
                                    { "catch", 0 },
                                    { "mania", 0 }
                                },
                                Scores = new Dictionary<string, List<ScoreData>>
                                {
                                    { "osu", new List<ScoreData>() },
                                    { "taiko", new List<ScoreData>() },
                                    { "catch", new List<ScoreData>() },
                                    { "mania", new List<ScoreData>() }
                                },
                                ID = ObjectId.NewObjectId()
                            };
                            collection.Insert(playerStats);
                        }

                        var userStats = collection.FindOne(Query.EQ("Username", Username));
                        var resultScore = userStats.Scores[ConvertModeValue(currentMode)]
                            ?.Find(x => x.Hash == confirmHash);

                        if (resultScore != null)
                        {
                            if ((resultScore.Score < result.Score &&
                                 resultScore.Mods == string.Join(",", mod)) || result.Pp > resultScore.Pp)
                            {
                                resultScore.Pp = result.Pp;
                                resultScore.Score = result.Score;
                                resultScore.Mods = string.Join(",", mod);
                                resultScore.Acc = baseAddresses.Player.Accuracy;
                                resultScore.Combo = baseAddresses.Player.Combo;
                                resultScore.Hit300 = baseAddresses.Player.Hit300;
                                resultScore.Hit100 = baseAddresses.Player.Hit100;
                                resultScore.Hit50 = baseAddresses.Player.Hit50;
                                resultScore.HitMiss = baseAddresses.Player.HitMiss;
                                resultScore.HitKatu = baseAddresses.Player.HitKatu;
                                resultScore.HitGeki = baseAddresses.Player.HitGeki;
                                resultScore.Date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                            }
                            else
                            {
                                if (startTime != null)
                                {
                                    var currentPlaytime = userStats.PlaytimeCalculate[ConvertModeValue(currentMode)];
                                    var time = (long)(endTime - startTime).Value.TotalMilliseconds;
                                    string playtime = FormatTime(currentPlaytime + time);
                                    userStats.Playtime[ConvertModeValue(currentMode)] = playtime;
                                    userStats.PlaytimeCalculate[ConvertModeValue(currentMode)] += time;
                                }

                                userStats.Playcount[ConvertModeValue(currentMode)] += 1;
                                userStats.LastGamemode = currentMode;
                                collection.Update(userStats);
                                hasChanged = true;
                                startTime = null;
                                endTime = null;
                                currentStatus = baseAddresses.GeneralData.OsuStatus;
                                continue;
                            }
                        }
                        else
                        {
                            userStats.Scores[ConvertModeValue(currentMode)].Add(result);
                        }

                        userStats.Scores[ConvertModeValue(currentMode)] = userStats
                            .Scores[ConvertModeValue(currentMode)].OrderByDescending(x => x.Pp).ToList();

                        double calculateBonusPp = CalculateBonusPp(userStats.Scores[ConvertModeValue(currentMode)].Count);
                        double calculateGlobalPp = CalculateGlobalPp(userStats.Scores[ConvertModeValue(currentMode)]) + calculateBonusPp;
                        double calculateGlobalAcc = CalculateGlobalAcc(userStats.Scores[ConvertModeValue(currentMode)]);

                        if (startTime != null)
                        {
                            var currentPlaytime = userStats.PlaytimeCalculate[ConvertModeValue(currentMode)];
                            var time = (long)(endTime - startTime).Value.TotalMilliseconds;
                            string playtime = FormatTime(currentPlaytime + time);
                            userStats.Playtime[ConvertModeValue(currentMode)] = playtime;
                            userStats.PlaytimeCalculate[ConvertModeValue(currentMode)] += time;
                        }

                        userStats.Playcount[ConvertModeValue(currentMode)] += 1;
                        userStats.BonusPp[ConvertModeValue(currentMode)] = calculateBonusPp;
                        userStats.GlobalPp[ConvertModeValue(currentMode)] = calculateGlobalPp;
                        userStats.GlobalAcc[ConvertModeValue(currentMode)] = calculateGlobalAcc;
                        userStats.LastGamemode = currentMode;
                        collection.Update(userStats);
                        hasChanged = true;
                    }

                    if (currentStatus != OsuMemoryStatus.Playing && baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.Playing) nowPlaying = true;
                    if (currentStatus is OsuMemoryStatus.Playing && baseAddresses.GeneralData.OsuStatus is OsuMemoryStatus.ResultsScreen or OsuMemoryStatus.SongSelect or OsuMemoryStatus.MultiplayerRoom) hasEnded = true;

                    if (nowPlaying)
                    {
                        nowPlaying = false;
                        startTime = DateTime.Now;
                    }

                    if (hasEnded)
                    {
                        hasEnded = false;
                        if (collection.FindOne(Query.EQ("Username", Username)) == null || startTime == null)
                        {
                            startTime = null;
                            endTime = null;
                        }
                        else
                        {
                            endTime = DateTime.Now;

                            if (endTime - startTime < TimeSpan.FromSeconds(10))
                            {
                                startTime = null;
                                endTime = null;
                                currentStatus = baseAddresses.GeneralData.OsuStatus;
                                continue;
                            }

                            var userStats = collection.FindOne(Query.EQ("Username", Username));

                            if (startTime != null)
                            {
                                var currentPlaytime = userStats.PlaytimeCalculate[ConvertModeValue(currentMode)];
                                var time = (long)(endTime - startTime).Value.TotalMilliseconds;
                                string playtime = FormatTime(currentPlaytime + time);
                                userStats.Playtime[ConvertModeValue(currentMode)] = playtime;
                                userStats.PlaytimeCalculate[ConvertModeValue(currentMode)] += time;
                            }

                            userStats.Playcount[ConvertModeValue(currentMode)] += 1;
                            userStats.LastGamemode = currentMode;
                            collection.Update(userStats);
                            hasChanged = true;
                            startTime = null;
                            endTime = null;
                            currentStatus = baseAddresses.GeneralData.OsuStatus;
                            continue;
                        }
                    }

                    currentStatus = baseAddresses.GeneralData.OsuStatus;

                }
                catch (Exception e)
                {
                    MessageBox.Show($"エラーが発生しました。\nエラー内容: {e}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private async void FormLoop()
        {
            while (true)
            {
                await Task.Delay(10);
                if (!hasChanged) continue;
                await Task.Delay(100);
                UpdateFormData();
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private async void UpdateFormData(bool modeChanged = false)
        {
            try
            {
                hasChanged = false;
                var collection = db.GetCollection<PlayerStats>("playerStats");
                var userdata = collection.FindOne(Query.EQ("Username", Username));

                if (userdata == null) return;

                if (!modeChanged) modeValue.SelectedIndex = userdata.LastGamemode;

                globalPPValue.Text = Math.Round(userdata.GlobalPp[ConvertMode(modeValue.Text)], 2) + "pp";
                accValue.Text = Math.Round(userdata.GlobalAcc[ConvertMode(modeValue.Text)], 2) + "%";
                BonusPPValue.Text = Math.Round(userdata.BonusPp[ConvertMode(modeValue.Text)], 2) + "pp";
                playtimeValue.Text = userdata.Playtime[ConvertMode(modeValue.Text)];
                playcountValue.Text = userdata.Playcount[ConvertMode(modeValue.Text)].ToString();
                BestPerformance.Items.Clear();

                if (userdata.Scores[ConvertMode(modeValue.Text)].Count == 0)
                {
                    BestPerformance.Items.Add("No plays.");
                    return;
                }

                BestPerformance.Items.Add("-----------------------------------------------------------------------------------------------------------------");
                for (int i = 0; i < userdata.Scores[ConvertMode(modeValue.Text)].Count; i++)
                {
                    string itemTitle = $"Title: {userdata.Scores[ConvertMode(modeValue.Text)][i].Title}";
                    string versionName = $"Mapper: {userdata.Scores[ConvertMode(modeValue.Text)][i].Mapper}   Difficulty: {userdata.Scores[ConvertMode(modeValue.Text)][i].Version}";
                    string hitsInfo = ConvertMode(modeValue.Text) switch
                    {
                        "osu" =>
                            $"Hits: {{{userdata.Scores[ConvertMode(modeValue.Text)][i].Hit300}/{userdata.Scores[ConvertMode(modeValue.Text)][i].Hit100}/{userdata.Scores[ConvertMode(modeValue.Text)][i].Hit50}/{userdata.Scores[ConvertMode(modeValue.Text)][i].HitMiss}}}",
                        "taiko" =>
                            $"Hits: {{{userdata.Scores[ConvertMode(modeValue.Text)][i].Hit300}/{userdata.Scores[ConvertMode(modeValue.Text)][i].Hit100}/{userdata.Scores[ConvertMode(modeValue.Text)][i].HitMiss}}}",
                        "catch" =>
                            $"Hits: {{{userdata.Scores[ConvertMode(modeValue.Text)][i].Hit300}/{userdata.Scores[ConvertMode(modeValue.Text)][i].Hit100}/{userdata.Scores[ConvertMode(modeValue.Text)][i].Hit50}/{userdata.Scores[ConvertMode(modeValue.Text)][i].HitMiss}}}",
                        "mania" =>
                            $"Hits: {{{userdata.Scores[ConvertMode(modeValue.Text)][i].HitGeki}/{userdata.Scores[ConvertMode(modeValue.Text)][i].Hit300}/{userdata.Scores[ConvertMode(modeValue.Text)][i].HitKatu}/{userdata.Scores[ConvertMode(modeValue.Text)][i].Hit100}/{userdata.Scores[ConvertMode(modeValue.Text)][i].Hit50}/{userdata.Scores[ConvertMode(modeValue.Text)][i].HitMiss}}}",
                        _ => ""
                    };

                    if (itemTitle.Length > 110) itemTitle = itemTitle[..110] + "...";
                    if (versionName.Length > 110) versionName = versionName[..110] + "...";

                    string resultInfo = $"Score: {userdata.Scores[ConvertMode(modeValue.Text)][i].Score:#,0} / {userdata.Scores[ConvertMode(modeValue.Text)][i].Combo}x   {hitsInfo}";
                    BestPerformance.Items.Add($"#{i + 1}");
                    BestPerformance.Items.Add(itemTitle);
                    BestPerformance.Items.Add(versionName);
                    BestPerformance.Items.Add(resultInfo);
                    BestPerformance.Items.Add($"Mod: {userdata.Scores[ConvertMode(modeValue.Text)][i].Mods}   Accuracy: {Math.Round(userdata.Scores[ConvertMode(modeValue.Text)][i].Acc, 2)}%   PP: {Math.Round(userdata.Scores[ConvertMode(modeValue.Text)][i].Pp, 2)}pp");
                    BestPerformance.Items.Add("-----------------------------------------------------------------------------------------------------------------");
                }

                changePPValue.Text =
                    Math.Abs(Math.Round(
                        userdata.GlobalPp[ConvertMode(modeValue.Text)] -
                        globalPp[ConvertMode(modeValue.Text)], 2)) == 0
                        ? ""
                        : $"{(Math.Round(userdata.GlobalPp[ConvertMode(modeValue.Text)] - globalPp[ConvertMode(modeValue.Text)], 2) >= 0 ? "+" : "-")} {Math.Abs(Math.Round(userdata.GlobalPp[ConvertMode(modeValue.Text)] - globalPp[ConvertMode(modeValue.Text)], 2))}pp";
                changePPValue.ForeColor =
                    Math.Round(
                        userdata.GlobalPp[ConvertMode(modeValue.Text)] -
                        globalPp[ConvertMode(modeValue.Text)], 2) >= 0
                        ? Color.ForestGreen
                        : Color.Red;
                changeACCValue.Text =
                    Math.Abs(Math.Round(
                        userdata.GlobalAcc[ConvertMode(modeValue.Text)] -
                        globalAcc[ConvertMode(modeValue.Text)], 2)) == 0
                        ? ""
                        : $"{(Math.Round(userdata.GlobalAcc[ConvertMode(modeValue.Text)] - globalAcc[ConvertMode(modeValue.Text)], 2) >= 0 ? "+" : "-")} {Math.Abs(Math.Round(userdata.GlobalAcc[ConvertMode(modeValue.Text)] - globalAcc[ConvertMode(modeValue.Text)], 2))}%";
                changeACCValue.ForeColor =
                    Math.Round(
                        userdata.GlobalAcc[ConvertMode(modeValue.Text)] -
                        globalAcc[ConvertMode(modeValue.Text)], 2) >= 0
                        ? Color.ForestGreen
                        : Color.Red;
                changeBonusPPValue.Text =
                    Math.Abs(Math.Round(
                        userdata.BonusPp[ConvertMode(modeValue.Text)] -
                        bonusPp[ConvertMode(modeValue.Text)], 2)) == 0
                        ? ""
                        : $"{(Math.Round(userdata.BonusPp[ConvertMode(modeValue.Text)] - bonusPp[ConvertMode(modeValue.Text)], 2) >= 0 ? "+" : "-")} {Math.Abs(Math.Round(userdata.BonusPp[ConvertMode(modeValue.Text)] - bonusPp[ConvertMode(modeValue.Text)], 2))}pp";
                changeBonusPPValue.ForeColor =
                    Math.Round(
                        userdata.BonusPp[ConvertMode(modeValue.Text)] -
                        bonusPp[ConvertMode(modeValue.Text)], 2) >= 0
                        ? Color.ForestGreen
                        : Color.Red;

                foreach (var mode in gameModes)
                {
                    globalPp[mode] = userdata.GlobalPp[mode];
                    globalAcc[mode] = userdata.GlobalAcc[mode];
                    bonusPp[mode] = userdata.BonusPp[mode];
                }

                errorText.Text = "";

                await Task.Delay(3000);
                changePPValue.Text = "";
                changeACCValue.Text = "";
                changeBonusPPValue.Text = "";
            }
            catch
            {
                errorText.Text = "※エラーが発生しました";
            }
        }

        private static double CalculateBonusPp(int scoresLength) => (417 - 1.0 / 3.0) * (1 - Math.Pow(0.995, Math.Min(1000, scoresLength)));

        private static double CalculateGlobalPp(IReadOnlyList<ScoreData> scores)
        {
            double calculateGlobalPp = 0;
            for (int i = 0; i < Math.Min(scores.Count, 100); i++)
            {
                calculateGlobalPp += scores[i].Pp * Math.Pow(0.95, i);
            }
            return calculateGlobalPp;
        }

        private static double CalculateGlobalAcc(IReadOnlyList<ScoreData> scores)
        {
            double calculateGlobalAcc = 0;
            for (int i = 0; i < Math.Min(scores.Count, 100); i++)
            {
                calculateGlobalAcc += scores[i].Acc * Math.Pow(0.95, i);
            }
            calculateGlobalAcc *= 100 / (20 * (1 - Math.Pow(0.95, scores.Count)));
            return Math.Round(calculateGlobalAcc) / 100;
        }

        private static string[] ParseMods(int mods)
        {
            List<string> activeMods = new();
            for (int i = 0; i < 32; i++)
            {
                int bit = 1 << i;
                if ((mods & bit) == bit) activeMods.Add(OSU_MODS[bit]);
            }
            if (activeMods.Contains("NC") && activeMods.Contains("DT")) activeMods.Remove("DT");
            if (activeMods.Count == 0) activeMods.Add("NM");
            return activeMods.ToArray();
        }

        private static string[] ParseModsCalculate(int mods)
        {
            List<string> activeMods = new();
            for (int i = 0; i < 32; i++)
            {
                int bit = 1 << i;
                if ((mods & bit) == bit) activeMods.Add(OSU_MODS[bit]);
            }
            if (activeMods.Contains("NC") && activeMods.Contains("DT")) activeMods.Remove("NC");
            return activeMods.ToArray();
        }

        private static string FormatTime(long time)
        {

            var hours = time / 3600000;
            var minutes = (time % 3600000) / 60000;
            return $"{hours}h {minutes}m";
        }

        private static string GetSongsFolderLocation(string osuDirectory)
        {
            foreach (string file in Directory.GetFiles(osuDirectory))
            {
                if (!Regex.IsMatch(file, @"^osu!\.+\.cfg$")) continue;
                foreach (string readLine in File.ReadLines(file))
                {
                    if (!readLine.StartsWith("BeatmapDirectory")) continue;
                    string path = readLine.Split('=')[1].Trim(' ');
                    return path == "Songs" ? Path.Combine(osuDirectory, path) : path;
                }
            }
            return Path.Combine(osuDirectory, "Songs");
        }

        public static string ConvertMode(string value)
        {
            return value switch
            {
                "osu!standard" => "osu",
                "osu!taiko" => "taiko",
                "osu!catch" => "catch",
                "osu!mania" => "mania",
                _ => "osu"
            };
        }

        public static string ConvertModeValue(int mode)
        {
            return mode switch
            {
                0 => "osu",
                1 => "taiko",
                2 => "catch",
                3 => "mania",
                _ => "osu"
            };
        }

        private void modeValue_SelectedIndexChanged(object sender, EventArgs e) => UpdateFormData(true);

        private void deleteScore_Click(object sender, EventArgs e)
        {
            var collection = db.GetCollection<PlayerStats>("playerStats");
            if (collection.FindOne(Query.EQ("Username", Username)) == null)
            {
                MessageBox.Show("スコアが見つかりませんでした。 \n 削除機能は新しくユーザーを作成してから１つ記録を作ることで有効化されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //選択されているのが線の上(セパレータ)の場合
            if (BestPerformance.SelectedIndex == -1 || BestPerformance.SelectedIndex % 6 == 0)
            {
                MessageBox.Show("スコアが選択されていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var scoreIndex = (int)Math.Floor((double)BestPerformance.SelectedIndex / 6);

            var mode = ConvertModeValue(modeValue.SelectedIndex);
            var userStats = collection.FindOne(Query.EQ("Username", Username));
            var score = userStats.Scores[mode][scoreIndex];

            var result = MessageBox.Show("選択したスコアを削除しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            userStats.Scores[mode].Remove(score);
            userStats.Scores[mode] = userStats.Scores[mode].OrderByDescending(x => x.Pp).ToList();
            collection.Update(userStats);
            hasChanged = true;
            MessageBox.Show("スコアを削除しました。", "削除完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) BestPerformance.ClearSelected();
        }

        private void MainForm_FormClosing(object sender, EventArgs args)
        {
            db.Dispose();
            Application.Exit();
        }
    }


    public class PlayerStats
    {
        public string Username { get; set; }
        public int LastGamemode { get; set; }
        public Dictionary<string, double> GlobalPp { get; set; }
        public Dictionary<string, double> BonusPp { get; set; }
        public Dictionary<string, double> GlobalAcc { get; set; }
        public Dictionary<string, string> Playtime { get; set; }
        public Dictionary<string, long> PlaytimeCalculate { get; set; }
        public Dictionary<string, int> Playcount { get; set; }
        public Dictionary<string, List<ScoreData>> Scores { get; set; }
        // ReSharper disable once InconsistentNaming
        public ObjectId ID { get; set; }
    }

    public class ScoreData
    {
        public string Title { get; set; }
        public string Mapper { get; set; }
        public string Version { get; set; }
        public double Pp { get; set; }
        public int Score { get; set; }
        public string Mods { get; set; }
        public double Acc { get; set; }
        public int Combo { get; set; }
        public int Hit300 { get; set; }
        public int Hit100 { get; set; }
        public int Hit50 { get; set; }
        public int HitMiss { get; set; }
        public int HitKatu { get; set; }
        public int HitGeki { get; set; }
        public string Date { get; set; }
        public string Hash { get; set; }
    }
}

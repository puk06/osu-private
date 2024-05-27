using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OsuMemoryDataProvider.OsuMemoryModels;
using OsuMemoryDataProvider;
using System.Text.RegularExpressions;
using LiteDB;
using System.Threading;
using OsuParsers.Decoders;
using ObjectId = LiteDB.ObjectId;
using Beatmap = OsuParsers.Beatmaps.Beatmap;

namespace osu_private
{
    public partial class MainForm : Form
    {
        private readonly StructuredOsuMemoryReader _sreader = new();
        private readonly OsuBaseAddresses _baseAddresses = new();

        public static string Username;

        private bool _isDbLoaded;
        private string _osuDirectory;
        private string _songsPath;
        private int _currentMode;
        private OsuMemoryStatus _currentStatus;
        private DateTime? _startTime;
        private DateTime? _endTime;
        private bool _hasEnded;
        private bool _nowPlaying;
        private bool _hasChanged;

        private static readonly Dictionary<string, double> GlobalPp = new()
        {
            {"osu", 0},
            {"taiko", 0},
            {"catch", 0},
            {"mania", 0}
        };

        private static readonly Dictionary<string, double> GlobalAcc = new()
        {
            {"osu", 0},
            {"taiko", 0},
            {"catch", 0},
            {"mania", 0}
        };

        private static readonly Dictionary<string, double> BonusPp = new()
        {
            {"osu", 0},
            {"taiko", 0},
            {"catch", 0},
            {"mania", 0}
        };

        public static readonly Dictionary<int, string> OsuMods = new()
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

        private static readonly string[] GameModes = new[] { "osu", "taiko", "catch", "mania" };
        private readonly LiteDatabase _db = new("scores.db");

        public MainForm(string username)
        {
            Username = username;
            Thread thread = new Thread(Loop)
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
            var collection = _db.GetCollection<PlayerStats>("playerStats");
            while (true)
            {
                Thread.Sleep(10);
                try
                {
                    if (Process.GetProcessesByName("osu!").Length == 0) continue;
                    if (!_isDbLoaded)
                    {
                        Process osuProcess = Process.GetProcessesByName("osu!")[0];
                        _osuDirectory = Path.GetDirectoryName(osuProcess.MainModule.FileName);

                        if (string.IsNullOrEmpty(_osuDirectory) || !Directory.Exists(_osuDirectory)) continue;

                        _songsPath = GetSongsFolderLocation(_osuDirectory);
                        _isDbLoaded = true;
                    }

                    if (!_sreader.CanRead) continue;

                    _sreader.TryRead(_baseAddresses.Player);
                    _sreader.TryRead(_baseAddresses.GeneralData);
                    _sreader.TryRead(_baseAddresses.Beatmap);

                    if (_baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.Playing)
                        _currentMode = _baseAddresses.Player.Mode;

                    if (_currentStatus == OsuMemoryStatus.Playing &&
                        _baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.ResultsScreen && !_baseAddresses.Player.IsReplay)
                    {
                        //Ranked, Approvedのみ記録をつけるときに消してください
                        //if (_baseAddresses.Beatmap.Status is not (4 or 5)) continue;

                        _hasEnded = false;
                        _endTime = DateTime.Now;
                        _currentStatus = _baseAddresses.GeneralData.OsuStatus;
                        string bannedModstext = "AT,CN,AP,RX,V2,SO,TG";
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
                        string[] modCalculate = ParseModsCalculate(_baseAddresses.Player.Mods.Value);
                        bool bannedmodflag = modCalculate.Any(resultMod => bannedMods.Contains(resultMod));
                        if (bannedmodflag)
                        {
                            _startTime = null;
                            _endTime = null;
                            _currentStatus = _baseAddresses.GeneralData.OsuStatus;
                            continue;
                        }

                        string mappath = Path.Combine(_songsPath ?? "", _baseAddresses.Beatmap.FolderName ?? "",
                            _baseAddresses.Beatmap.OsuFileName ?? "");
                        HitsResult hits = new()
                        {
                            Hit300 = _baseAddresses.Player.Hit300,
                            Hit100 = _baseAddresses.Player.Hit100,
                            Hit50 = _baseAddresses.Player.Hit50,
                            HitMiss = _baseAddresses.Player.HitMiss,
                            HitGeki = _baseAddresses.Player.HitGeki,
                            HitKatu = _baseAddresses.Player.HitKatu,
                            Combo = _baseAddresses.Player.Combo,
                            Score = _baseAddresses.Player.Score
                        };

                        CalculateArgs args = new()
                        {
                            Accuracy = _baseAddresses.Player.Accuracy,
                            Combo = _baseAddresses.Player.Combo,
                            Score = _baseAddresses.Player.Score,
                            Mods = modCalculate,
                            NoClassicMod = true
                        };

                        double pp = new PpCalculator(mappath, _currentMode).Calculate(args, hits);
                        string confirmHash = _baseAddresses.Beatmap.Md5;
                        Beatmap beatmap = BeatmapDecoder.Decode(mappath);

                        string[] mod = ParseMods(_baseAddresses.Player.Mods.Value);
                        var result = new Score()
                        {
                            Title = beatmap.MetadataSection.Title + " by " + beatmap.MetadataSection.Artist,
                            Mapper = beatmap.MetadataSection.Creator,
                            Version = beatmap.MetadataSection.Version,
                            Pp = pp,
                            score = _baseAddresses.Player.Score,
                            Mods = string.Join(",", mod),
                            Acc = _baseAddresses.Player.Accuracy,
                            Combo = _baseAddresses.Player.Combo,
                            Hit300 = _baseAddresses.Player.Hit300,
                            Hit100 = _baseAddresses.Player.Hit100,
                            Hit50 = _baseAddresses.Player.Hit50,
                            HitMiss = _baseAddresses.Player.HitMiss,
                            HitKatu = _baseAddresses.Player.HitKatu,
                            HitGeki = _baseAddresses.Player.HitGeki,
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
                                Scores = new Dictionary<string, List<Score>>
                                {
                                    { "osu", new List<Score>() },
                                    { "taiko", new List<Score>() },
                                    { "catch", new List<Score>() },
                                    { "mania", new List<Score>() }
                                },
                                ID = ObjectId.NewObjectId()
                            };
                            collection.Insert(playerStats);
                        }

                        var userStats = collection.FindOne(Query.EQ("Username", Username));
                        var resultScore = userStats.Scores[ConvertModeValue(_currentMode)]
                            ?.Find(x => x.Hash == confirmHash);

                        if (resultScore != null)
                        {
                            if ((resultScore.score < result.score &&
                                 resultScore.Mods == string.Join(",", mod)) || result.Pp > resultScore.Pp)
                            {
                                resultScore.Pp = result.Pp;
                                resultScore.score = result.score;
                                resultScore.Mods = string.Join(",", mod);
                                resultScore.Acc = _baseAddresses.Player.Accuracy;
                                resultScore.Combo = _baseAddresses.Player.Combo;
                                resultScore.Hit300 = _baseAddresses.Player.Hit300;
                                resultScore.Hit100 = _baseAddresses.Player.Hit100;
                                resultScore.Hit50 = _baseAddresses.Player.Hit50;
                                resultScore.HitMiss = _baseAddresses.Player.HitMiss;
                                resultScore.HitKatu = _baseAddresses.Player.HitKatu;
                                resultScore.HitGeki = _baseAddresses.Player.HitGeki;
                                resultScore.Date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                            }
                            else
                            {
                                if (_startTime != null)
                                {
                                    var currentPlaytime = userStats.PlaytimeCalculate[ConvertModeValue(_currentMode)];
                                    var time = (long)(_endTime - _startTime).Value.TotalMilliseconds;
                                    string playtime = FormatTime(currentPlaytime + time);
                                    userStats.Playtime[ConvertModeValue(_currentMode)] = playtime;
                                    userStats.PlaytimeCalculate[ConvertModeValue(_currentMode)] += time;
                                }

                                userStats.Playcount[ConvertModeValue(_currentMode)] += 1;
                                userStats.LastGamemode = _currentMode;
                                collection.Update(userStats);
                                _hasChanged = true;
                                _startTime = null;
                                _endTime = null;
                                _currentStatus = _baseAddresses.GeneralData.OsuStatus;
                                continue;
                            }
                        }
                        else
                        {
                            userStats.Scores[ConvertModeValue(_currentMode)].Add(result);
                        }

                        userStats.Scores[ConvertModeValue(_currentMode)] = userStats
                            .Scores[ConvertModeValue(_currentMode)].OrderByDescending(x => x.Pp).ToList();

                        double bonusPP = CalculateBonusPp(userStats.Scores[ConvertModeValue(_currentMode)].Count);
                        double globalPP = CalculateGlobalPp(userStats.Scores[ConvertModeValue(_currentMode)]) + bonusPP;
                        double globalACC = CalculateGlobalAcc(userStats.Scores[ConvertModeValue(_currentMode)]);

                        if (_startTime != null)
                        {
                            var currentPlaytime = userStats.PlaytimeCalculate[ConvertModeValue(_currentMode)];
                            var time = (long)(_endTime - _startTime).Value.TotalMilliseconds;
                            string playtime = FormatTime(currentPlaytime + time);
                            userStats.Playtime[ConvertModeValue(_currentMode)] = playtime;
                            userStats.PlaytimeCalculate[ConvertModeValue(_currentMode)] += time;
                        }

                        userStats.Playcount[ConvertModeValue(_currentMode)] += 1;
                        userStats.BonusPp[ConvertModeValue(_currentMode)] = bonusPP;
                        userStats.GlobalPp[ConvertModeValue(_currentMode)] = globalPP;
                        userStats.GlobalAcc[ConvertModeValue(_currentMode)] = globalACC;
                        userStats.LastGamemode = _currentMode;
                        collection.Update(userStats);
                        _hasChanged = true;
                    }

                    if (_currentStatus != OsuMemoryStatus.Playing && _baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.Playing) _nowPlaying = true;
                    if (_currentStatus is OsuMemoryStatus.Playing && _baseAddresses.GeneralData.OsuStatus is OsuMemoryStatus.ResultsScreen or OsuMemoryStatus.SongSelect or OsuMemoryStatus.MultiplayerRoom) _hasEnded = true;

                    if (_nowPlaying)
                    {
                        _nowPlaying = false;
                        _startTime = DateTime.Now;
                    }

                    if (_hasEnded)
                    {
                        _hasEnded = false;
                        if (collection.FindOne(Query.EQ("Username", Username)) == null || _startTime == null)
                        {
                            _startTime = null;
                            _endTime = null;
                        }
                        else
                        {
                            _endTime = DateTime.Now;

                            if (_endTime - _startTime < TimeSpan.FromSeconds(10))
                            {
                                _startTime = null;
                                _endTime = null;
                                _currentStatus = _baseAddresses.GeneralData.OsuStatus;
                                continue;
                            }

                            var userStats = collection.FindOne(Query.EQ("Username", Username));

                            if (_startTime != null)
                            {
                                var currentPlaytime = userStats.PlaytimeCalculate[ConvertModeValue(_currentMode)];
                                var time = (long)(_endTime - _startTime).Value.TotalMilliseconds;
                                string playtime = FormatTime(currentPlaytime + time);
                                userStats.Playtime[ConvertModeValue(_currentMode)] = playtime;
                                userStats.PlaytimeCalculate[ConvertModeValue(_currentMode)] += time;
                            }

                            userStats.Playcount[ConvertModeValue(_currentMode)] += 1;
                            userStats.LastGamemode = _currentMode;
                            collection.Update(userStats);
                            _hasChanged = true;
                            _startTime = null;
                            _endTime = null;
                            _currentStatus = _baseAddresses.GeneralData.OsuStatus;
                            continue;
                        }
                    }

                    _currentStatus = _baseAddresses.GeneralData.OsuStatus;

                }
                catch (Exception e)
                {
                    MessageBox.Show($"エラーが発生しました。\nエラー内容: {e}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void FormLoop()
        {
            while (true)
            {
                await Task.Delay(10);
                if (!_hasChanged) continue;
                await Task.Delay(100);
                UpdateFormData();
            }
        }

        private async void UpdateFormData(bool modeChanged = false)
        {
            try
            {
                _hasChanged = false;
                var collection = _db.GetCollection<PlayerStats>("playerStats");
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

                    string resultInfo = $"Score: {userdata.Scores[ConvertMode(modeValue.Text)][i].score:#,0} / {userdata.Scores[ConvertMode(modeValue.Text)][i].Combo}x   {hitsInfo}";
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
                        GlobalPp[ConvertMode(modeValue.Text)], 2)) == 0
                        ? ""
                        : $"{(Math.Round(userdata.GlobalPp[ConvertMode(modeValue.Text)] - GlobalPp[ConvertMode(modeValue.Text)], 2) >= 0 ? "+" : "-")} {Math.Abs(Math.Round(userdata.GlobalPp[ConvertMode(modeValue.Text)] - GlobalPp[ConvertMode(modeValue.Text)], 2))}pp";
                changePPValue.ForeColor =
                    Math.Round(
                        userdata.GlobalPp[ConvertMode(modeValue.Text)] -
                        GlobalPp[ConvertMode(modeValue.Text)], 2) >= 0
                        ? Color.ForestGreen
                        : Color.Red;
                changeACCValue.Text =
                    Math.Abs(Math.Round(
                        userdata.GlobalAcc[ConvertMode(modeValue.Text)] -
                        GlobalAcc[ConvertMode(modeValue.Text)], 2)) == 0
                        ? ""
                        : $"{(Math.Round(userdata.GlobalAcc[ConvertMode(modeValue.Text)] - GlobalAcc[ConvertMode(modeValue.Text)], 2) >= 0 ? "+" : "-")} {Math.Abs(Math.Round(userdata.GlobalAcc[ConvertMode(modeValue.Text)] - GlobalAcc[ConvertMode(modeValue.Text)], 2))}%";
                changeACCValue.ForeColor =
                    Math.Round(
                        userdata.GlobalAcc[ConvertMode(modeValue.Text)] -
                        GlobalAcc[ConvertMode(modeValue.Text)], 2) >= 0
                        ? Color.ForestGreen
                        : Color.Red;
                changeBonusPPValue.Text =
                    Math.Abs(Math.Round(
                        userdata.BonusPp[ConvertMode(modeValue.Text)] -
                        BonusPp[ConvertMode(modeValue.Text)], 2)) == 0
                        ? ""
                        : $"{(Math.Round(userdata.BonusPp[ConvertMode(modeValue.Text)] - BonusPp[ConvertMode(modeValue.Text)], 2) >= 0 ? "+" : "-")} {Math.Abs(Math.Round(userdata.BonusPp[ConvertMode(modeValue.Text)] - BonusPp[ConvertMode(modeValue.Text)], 2))}pp";
                changeBonusPPValue.ForeColor =
                    Math.Round(
                        userdata.BonusPp[ConvertMode(modeValue.Text)] -
                        BonusPp[ConvertMode(modeValue.Text)], 2) >= 0
                        ? Color.ForestGreen
                        : Color.Red;

                foreach (var mode in GameModes)
                {
                    GlobalPp[mode] = userdata.GlobalPp[mode];
                    GlobalAcc[mode] = userdata.GlobalAcc[mode];
                    BonusPp[mode] = userdata.BonusPp[mode];
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

        private static double CalculateGlobalPp(IReadOnlyList<Score> scores)
        {
            double globalPp = 0;
            for (int i = 0; i < Math.Min(scores.Count, 100); i++)
            {
                globalPp += scores[i].Pp * Math.Pow(0.95, i);
            }
            return globalPp;
        }

        private static double CalculateGlobalAcc(IReadOnlyList<Score> scores)
        {
            double globalAcc = 0;
            for (int i = 0; i < Math.Min(scores.Count, 100); i++)
            {
                globalAcc += scores[i].Acc * Math.Pow(0.95, i);
            }
            globalAcc *= 100 / (20 * (1 - Math.Pow(0.95, scores.Count)));
            return Math.Round(globalAcc) / 100;
        }

        private static string[] ParseMods(int mods)
        {
            List<string> activeMods = new();
            for (int i = 0; i < 32; i++)
            {
                int bit = 1 << i;
                if ((mods & bit) == bit) activeMods.Add(OsuMods[bit]);
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
                if ((mods & bit) == bit) activeMods.Add(OsuMods[bit]);
            }
            if (activeMods.Contains("NC") && activeMods.Contains("DT")) activeMods.Remove("NC");
            return activeMods.ToArray();
        }

        private static string FormatTime(long time)
        {
            var hours = time / 3600000;
            var minutes = time / 60000;
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
            var collection = _db.GetCollection<PlayerStats>("playerStats");
            if (collection.FindOne(Query.EQ("Username", Username)) == null)
            {
                MessageBox.Show("スコアが見つかりませんでした。 \n 削除機能は新しくユーザーを作成してから１つ記録を作ることで有効化されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            new DeleteForm().ShowDialog();
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) BestPerformance.ClearSelected();
        }

        private void MainForm_FormClosing(object sender, EventArgs args)
        {
            _db.Dispose();
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
        public Dictionary<string, List<Score>> Scores { get; set; }
        public ObjectId ID { get; set; }
    }

    public class Score
    {
        public string Title { get; set; }
        public string Mapper { get; set; }
        public string Version { get; set; }
        public double Pp { get; set; }
        public int score { get; set; }
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

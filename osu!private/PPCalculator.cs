using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace osu_private
{
    public class PpCalculator
    {
        private readonly Ruleset _ruleset;
        private readonly ProcessorWorkingBeatmap _workingBeatmap;
        private readonly int _mode;

        public PpCalculator(string file, int mode)
        {
            _ruleset = SetRuleset(mode);
            _workingBeatmap = ProcessorWorkingBeatmap.FromFile(file);
            _mode = mode;
        }

        private static Ruleset SetRuleset(int mode)
        {
            return mode switch
            {
                0 => new OsuRuleset(),
                1 => new TaikoRuleset(),
                2 => new CatchRuleset(),
                3 => new ManiaRuleset(),
                _ => throw new ArgumentException("Invalid ruleset ID provided.")
            };
        }

        private static Mod[] GetMods(Ruleset ruleset, CalculateArgs args)
        {
            if (args.Mods.Length == 0) return Array.Empty<Mod>();
            var availableMods = ruleset.CreateAllMods().ToList();
            return args.Mods.Select(modString => availableMods.FirstOrDefault(m => string.Equals(m.Acronym, modString.ToLower(), StringComparison.CurrentCultureIgnoreCase))).Where(newMod => newMod != null).ToArray();
        }

        public double Calculate(CalculateArgs args, HitsResult hits)
        {
            var mods = args.NoClassicMod ? GetMods(_ruleset, args) : LegacyHelper.FilterDifficultyAdjustmentMods(_workingBeatmap.BeatmapInfo, _ruleset, GetMods(_ruleset, args));
            var beatmap = _workingBeatmap.GetPlayableBeatmap(_ruleset.RulesetInfo, mods);
            var statisticsCurrent = GenerateHitResultsForCurrent(hits, _mode);
            var resultScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, _ruleset.RulesetInfo)
            {
                Accuracy = args.Accuracy / 100,
                MaxCombo = args.Combo,
                Statistics = statisticsCurrent,
                Mods = mods
            };
            var difficultyCalculator = _ruleset.CreateDifficultyCalculator(_workingBeatmap);
            var difficultyAttributes = difficultyCalculator.Calculate(mods);
            var performanceCalculator = _ruleset.CreatePerformanceCalculator();
            var performanceAttributes = performanceCalculator?.Calculate(resultScoreInfo, difficultyAttributes);
            return performanceAttributes?.Total ?? 0;
        }

        private static Dictionary<HitResult, int> GenerateHitResultsForCurrent(HitsResult hits, int mode)
        {
            return mode switch
            {
                0 => new Dictionary<HitResult, int>
                {
                    { HitResult.Great, hits.Hit300 },
                    { HitResult.Ok, hits.Hit100 },
                    { HitResult.Meh, hits.Hit50 },
                    { HitResult.Miss, hits.HitMiss }
                },
                1 => new Dictionary<HitResult, int>
                {
                    { HitResult.Great, hits.Hit300 },
                    { HitResult.Ok, hits.Hit100 },
                    { HitResult.Miss, hits.HitMiss }
                },
                2 => new Dictionary<HitResult, int>
                {
                    { HitResult.Great, hits.Hit300 },
                    { HitResult.LargeTickHit, hits.Hit100 },
                    { HitResult.SmallTickHit, hits.Hit50 },
                    { HitResult.SmallTickMiss, hits.HitKatu },
                    { HitResult.Miss, hits.HitMiss }
                },
                3 => new Dictionary<HitResult, int>
                {
                    { HitResult.Perfect, hits.HitGeki },
                    { HitResult.Great, hits.Hit300 },
                    { HitResult.Good, hits.HitKatu },
                    { HitResult.Ok, hits.Hit100 },
                    { HitResult.Meh, hits.Hit50 },
                    { HitResult.Miss, hits.HitMiss }
                },
                _ => throw new ArgumentException("Invalid mode provided.")
            };
        }
    }

    public class ProcessorWorkingBeatmap : WorkingBeatmap
    {
        private readonly Beatmap _beatmap;

        public ProcessorWorkingBeatmap(Beatmap beatmap)
            : base(beatmap.BeatmapInfo, null)
        {
            _beatmap = beatmap;
            beatmap.BeatmapInfo.Ruleset = LegacyHelper.GetRulesetFromLegacyId(beatmap.BeatmapInfo.Ruleset.OnlineID).RulesetInfo;
        }

        private static Beatmap ReadFromFile(string filename)
        {
            using var stream = File.OpenRead(filename);
            using var reader = new LineBufferedReader(stream);
            return Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
        }

        public static ProcessorWorkingBeatmap FromFile(string file) => new(ReadFromFile(file));

        protected override IBeatmap GetBeatmap() => _beatmap;
        protected override Texture GetBackground() => null!;
        protected override Track GetBeatmapTrack() => null!;
        protected override ISkin GetSkin() => null!;
        public override Stream? GetStream(string storagePath) => null;
    }

    public static class LegacyHelper
    {
        public static Ruleset GetRulesetFromLegacyId(int id)
        {
            return id switch
            {
                0 => new OsuRuleset(),
                1 => new TaikoRuleset(),
                2 => new CatchRuleset(),
                3 => new ManiaRuleset(),
                _ => throw new ArgumentException("Invalid ruleset ID provided.")
            };
        }

        private const LegacyMods KeyMods = LegacyMods.Key1 | LegacyMods.Key2 | LegacyMods.Key3 | LegacyMods.Key4 |
                                            LegacyMods.Key5 | LegacyMods.Key6 | LegacyMods.Key7 | LegacyMods.Key8
                                            | LegacyMods.Key9 | LegacyMods.KeyCoop;

        private static LegacyMods MaskRelevantMods(LegacyMods mods, bool isConvertedBeatmap, int rulesetId)
        {
            LegacyMods relevantMods =
                LegacyMods.DoubleTime | LegacyMods.HalfTime | LegacyMods.HardRock | LegacyMods.Easy;

            switch (rulesetId)
            {
                case 0:
                    if ((mods & LegacyMods.Flashlight) > 0)
                        relevantMods |= LegacyMods.Flashlight | LegacyMods.Hidden | LegacyMods.TouchDevice;
                    else
                        relevantMods |= LegacyMods.Flashlight | LegacyMods.TouchDevice;
                    break;

                case 3:
                    if (isConvertedBeatmap)
                        relevantMods |= KeyMods;
                    break;
            }

            return mods & relevantMods;
        }

        private static LegacyMods ConvertToLegacyDifficultyAdjustmentMods(BeatmapInfo beatmapInfo, Ruleset ruleset,
            Mod?[] mods)
        {
            var legacyMods = ruleset.ConvertToLegacyMods(mods!);

            // mods that are not represented in `LegacyMods` (but we can approximate them well enough with others)
            if (mods.Any(mod => mod is ModDaycore))
                legacyMods |= LegacyMods.HalfTime;

            return MaskRelevantMods(legacyMods, ruleset.RulesetInfo.OnlineID != beatmapInfo.Ruleset.OnlineID,
                ruleset.RulesetInfo.OnlineID);
        }

        public static Mod?[] FilterDifficultyAdjustmentMods(BeatmapInfo beatmapInfo, Ruleset ruleset, Mod?[] mods)
            => ruleset.ConvertFromLegacyMods(ConvertToLegacyDifficultyAdjustmentMods(beatmapInfo, ruleset, mods))
                .ToArray();
    }

    public class HitsResult
    {
        public int HitGeki { get; set; }
        public int Hit300 { get; set; }
        public int HitKatu { get; set; }
        public int Hit100 { get; set; }
        public int Hit50 { get; set; }
        public int HitMiss { get; set; }
        public int Combo { get; set; }
        public int Score { get; set; }
    }

    public class CalculateArgs
    {
        public double Accuracy { get; set; } = 100;
        public int Combo { get; set; }
        public int Score { get; set; }
        public bool NoClassicMod { get; set; }
        public string[] Mods { get; set; } = Array.Empty<string>();
        public int? Time { get; set; }
        public bool PplossMode { get; set; }
    }
}

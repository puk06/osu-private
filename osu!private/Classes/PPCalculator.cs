using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using System.Collections.Generic;
using System.Linq;
using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu_private.Classes
{
    public class PpCalculator(string file, int mode)
    {
        private readonly Ruleset ruleset = SetRuleset(mode);
        private readonly ProcessorWorkingBeatmap workingBeatmap = ProcessorWorkingBeatmap.FromFile(file);

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
            var mods = GetMods(ruleset, args);
            var beatmap = workingBeatmap.GetPlayableBeatmap(ruleset.RulesetInfo, mods);
            var statisticsCurrent = GenerateHitResultsForCurrent(hits, mode);
            var resultScoreInfo = new ScoreInfo(beatmap.BeatmapInfo, ruleset.RulesetInfo)
            {
                Accuracy = args.Accuracy / 100,
                MaxCombo = args.Combo,
                Statistics = statisticsCurrent,
                Mods = mods
            };
            var difficultyCalculator = ruleset.CreateDifficultyCalculator(workingBeatmap);
            var difficultyAttributes = difficultyCalculator.Calculate(mods);

            // Fix the combo for osu! standard
            var maxCombo = GetMaxCombo(beatmap, mode);
            difficultyAttributes.MaxCombo = maxCombo;

            var performanceCalculator = ruleset.CreatePerformanceCalculator();
            var performanceAttributes = performanceCalculator?.Calculate(resultScoreInfo, difficultyAttributes);
            return performanceAttributes?.Total ?? 0;
        }

        private static int GetMaxCombo(IBeatmap beatmap, int mode)
        {
            return mode switch
            {
                0 => beatmap.HitObjects.Count +
                     beatmap.HitObjects.OfType<Slider>().Sum(s => s.NestedHitObjects.Count - 1),
                1 => beatmap.HitObjects.OfType<Hit>().Count(),
                2 => beatmap.HitObjects.Count(h => h is Fruit) + beatmap.HitObjects.OfType<JuiceStream>()
                    .SelectMany(j => j.NestedHitObjects)
                    .Count(h => h is not TinyDroplet),
                3 => beatmap.HitObjects.Count,
                _ => throw new ArgumentException("Invalid ruleset ID provided.")
            };
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
}

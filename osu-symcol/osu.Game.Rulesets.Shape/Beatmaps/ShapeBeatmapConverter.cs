﻿using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Shape.Objects;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Types;
using System;
using osu.Game.Audio;
using System.Linq;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Shape.Beatmaps
{
    internal class ShapeBeatmapConverter : BeatmapConverter<ShapeHitObject>
    {
        protected override IEnumerable<Type> ValidConversionTypes { get; } = new[] { typeof(HitObject) };

        /// <summary>
        /// osu! is generally slower than taiko, so a factor is added to increase
        /// speed. This must be used everywhere slider length or beat length is used.
        /// </summary>
        private const float legacy_velocity_multiplier = 1.4f;

        /// <summary>
        /// Base osu! slider scoring distance.
        /// </summary>
        private const float osu_base_scoring_distance = 100;

        /// <summary>
        /// Drum roll distance that results in a duration of 1 speed-adjusted beat length.
        /// </summary>
        private const float shape_base_distance = 100;

        public ShapeBeatmapConverter(IBeatmap beatmap)
        : base(beatmap)
        {
        }

        protected override IEnumerable<ShapeHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap)
        {
            var endTimeData = original as IHasEndTime;
            var positionData = original as IHasPosition;
            var comboData = original as IHasCombo;

            List<SampleInfo> samples = original.Samples;

            if (original is IHasCurve curveData)
            {
                // Number of spans of the object - one for the initial length and for each repeat
                int spans = curveData?.SpanCount() ?? 1;

                TimingControlPoint timingPoint = beatmap.ControlPointInfo.TimingPointAt(original.StartTime);
                DifficultyControlPoint difficultyPoint = beatmap.ControlPointInfo.DifficultyPointAt(original.StartTime);

                double speedAdjustment = difficultyPoint.SpeedMultiplier;
                double speedAdjustedBeatLength = timingPoint.BeatLength / speedAdjustment;

                double distance = curveData.Distance * spans * legacy_velocity_multiplier;

                double shapeVelocity = shape_base_distance * beatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier * legacy_velocity_multiplier / speedAdjustedBeatLength;
                double shapeDuration = distance / shapeVelocity;

                // The velocity of the osu! hit object - calculated as the velocity of a slider
                double osuVelocity = osu_base_scoring_distance * beatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier * legacy_velocity_multiplier / speedAdjustedBeatLength;
                // The duration of the osu! hit object
                double osuDuration = distance / osuVelocity;

                // osu-stable always uses the speed-adjusted beatlength to determine the velocities, but
                // only uses it for tick rate if beatmap version < 8
                if (beatmap.BeatmapInfo.BeatmapVersion >= 8)
                    speedAdjustedBeatLength *= speedAdjustment;

                // If the drum roll is to be split into hit circles, assume the ticks are 1/8 spaced within the duration of one beat
                double tickSpacing = Math.Min(speedAdjustedBeatLength / beatmap.BeatmapInfo.BaseDifficulty.SliderTickRate, shapeDuration / spans);

                List<List<SampleInfo>> allSamples = curveData != null ? curveData.RepeatSamples : new List<List<SampleInfo>>(new[] { samples });

                int i = 0;
                for (double j = original.StartTime; j <= original.StartTime + shapeDuration + tickSpacing / 8; j += tickSpacing)
                {
                    List<SampleInfo> currentSamples = allSamples[i];

                    bool isSquare = currentSamples.Any(s => s.Name == SampleInfo.HIT_WHISTLE);
                    bool isTriangle = currentSamples.Any(s => s.Name == SampleInfo.HIT_FINISH);
                    bool isX = currentSamples.Any(s => s.Name == SampleInfo.HIT_CLAP);

                    if (isSquare)
                    {
                        yield return new BaseShape
                        {
                            StartTime = j,
                            StartPosition = positionData?.Position ?? Vector2.Zero,
                            Samples = currentSamples,
                            NewCombo = comboData?.NewCombo ?? false,
                            ShapeID = 2,
                        };
                    }
                    else if (isTriangle)
                    {
                        yield return new BaseShape
                        {
                            StartTime = j,
                            StartPosition = positionData?.Position ?? Vector2.Zero,
                            Samples = currentSamples,
                            NewCombo = comboData?.NewCombo ?? false,
                            ShapeID = 3,
                        };
                    }
                    else if (isX)
                    {
                        yield return new BaseShape
                        {
                            StartTime = j,
                            StartPosition = positionData?.Position ?? Vector2.Zero,
                            Samples = currentSamples,
                            NewCombo = comboData?.NewCombo ?? false,
                            ShapeID = 4,
                        };
                    }
                    else
                    {
                        yield return new BaseShape
                        {
                            StartTime = j,
                            StartPosition = positionData?.Position ?? Vector2.Zero,
                            Samples = currentSamples,
                            NewCombo = comboData?.NewCombo ?? false,
                            ShapeID = 1,
                        };
                    }

                    i = (i + 1) % allSamples.Count;
                }
            }
            else
            {
                bool isSquare = samples.Any(s => s.Name == SampleInfo.HIT_WHISTLE);
                bool isTriangle = samples.Any(s => s.Name == SampleInfo.HIT_FINISH);
                bool isX = samples.Any(s => s.Name == SampleInfo.HIT_CLAP);

                if (isSquare)
                {
                    yield return new BaseShape
                    {
                        StartTime = original.StartTime,
                        StartPosition = positionData?.Position ?? Vector2.Zero,
                        Samples = original.Samples,
                        NewCombo = comboData?.NewCombo ?? false,
                        ShapeID = 2,
                    };
                }
                else if (isTriangle)
                {
                    yield return new BaseShape
                    {
                        StartTime = original.StartTime,
                        StartPosition = positionData?.Position ?? Vector2.Zero,
                        Samples = original.Samples,
                        NewCombo = comboData?.NewCombo ?? false,
                        ShapeID = 3,
                    };
                }
                else if (isX)
                {
                    yield return new BaseShape
                    {
                        StartTime = original.StartTime,
                        StartPosition = positionData?.Position ?? Vector2.Zero,
                        Samples = original.Samples,
                        NewCombo = comboData?.NewCombo ?? false,
                        ShapeID = 4,
                    };
                }
                else
                {
                    yield return new BaseShape
                    {
                        StartTime = original.StartTime,
                        StartPosition = positionData?.Position ?? Vector2.Zero,
                        Samples = original.Samples,
                        NewCombo = comboData?.NewCombo ?? false,
                        ShapeID = 1,
                    };
                }
            }
        }
    }
}

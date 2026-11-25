using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Animations;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Animations
{
	public class CueTests
	{
		[Theory]
		[InlineData("60%", 0.6)]
		[InlineData("0.6", 0.6)]
		[InlineData("0", 0.0)]
		[InlineData("100%", 1.0)]
		public void ParsesFractions(string text, double expected)
		{
			var cue = (Cue)new CueTypeConverter().ConvertFromInvariantString(text);

			Assert.False(cue.HasTimeSpan);
			Assert.Equal(expected, cue.Fraction, 12);
		}

		[Fact]
		public void ParsesTimes()
		{
			var cue = (Cue)new CueTypeConverter().ConvertFromInvariantString("0:0:0.24");

			Assert.True(cue.HasTimeSpan);
			Assert.Equal(TimeSpan.FromMilliseconds(240), cue.TimeSpan);
			Assert.Throws<InvalidOperationException>(() => cue.Fraction);
		}

		[Fact]
		public void EmptyIsUnspecified()
			=> Assert.True(((Cue)new CueTypeConverter().ConvertFromInvariantString("")).IsUnspecified);

		[Theory]
		[InlineData("150%")]
		[InlineData("1.5")]
		[InlineData("-0.1")]
		[InlineData("abc")]
		[InlineData("-0:0:1")]
		public void RejectsOutOfRangeOrMalformed(string text)
			=> Assert.Throws<FormatException>(() => new CueTypeConverter().ConvertFromInvariantString(text));

		[Theory]
		[InlineData("60%")]
		[InlineData("00:00:00.2400000")]
		public void RoundTripsInvariantly(string text)
		{
			var converter = new CueTypeConverter();
			var cue = (Cue)converter.ConvertFromInvariantString(text);

			Assert.Equal(cue, (Cue)converter.ConvertFromInvariantString(cue.ToString()));
		}

		[Theory]
		[InlineData(0.007)]
		[InlineData(0.1)]
		[InlineData(1.0 / 3.0)]
		[InlineData(0.29)]
		public void FractionsRoundTripExactly(double fraction)
		{
			var cue = Cue.FromFraction(fraction);

			Assert.Equal(cue, (Cue)new CueTypeConverter().ConvertFromInvariantString(cue.ToString()));
		}

		[Fact]
		public void FactoriesValidate()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => Cue.FromFraction(1.01));
			Assert.Throws<ArgumentOutOfRangeException>(() => Cue.FromTimeSpan(TimeSpan.FromSeconds(-1)));
			Assert.Equal(Cue.FromFraction(0.5), Cue.FromFraction(0.5));
			Assert.NotEqual(Cue.FromFraction(0.5), Cue.Unspecified);
		}
	}

	public class CuePlacementTests
	{
		static double[] Place(params string[] cues)
		{
			var keyFrames = new List<KeyFrame>();
			foreach (var cue in cues)
				keyFrames.Add(new KeyFrame { Cue = cue is null ? Cue.Unspecified : (Cue)new CueTypeConverter().ConvertFromInvariantString(cue) });

			return CompiledStoryboard.PlaceCues(keyFrames, TimeSpan.FromSeconds(1));
		}

		[Fact]
		public void AllUnspecifiedSpreadEvenly() => Assert.Equal(new[] { 0, 0.5, 1 }, Place(null, null, null));

		[Fact]
		public void LoneUnspecifiedSitsAtTheEnd() => Assert.Equal(new[] { 1.0 }, Place(new string[] { null }));

		[Fact]
		public void LeadingAndTrailingRunsArePinned() => Assert.Equal(new[] { 0, 0.8, 1 }, Place(null, "0.8", null));

		[Fact]
		public void InteriorRunIsSpreadBetweenBounds()
		{
			var cues = Place("0.2", null, null, "0.8");

			Assert.Equal(0.2, cues[0], 12);
			Assert.Equal(0.4, cues[1], 12);
			Assert.Equal(0.6, cues[2], 12);
			Assert.Equal(0.8, cues[3], 12);
		}

		[Fact]
		public void TimeCuesResolveAgainstDuration() => Assert.Equal(new[] { 0.25, 1.0 }, Place("0:0:0.25", "0:0:1"));

		[Fact]
		public void DecreasingCuesAreAnError()
			=> Assert.Contains("MAUIX2306", Assert.Throws<InvalidOperationException>(() => Place("0.6", "0.4")).Message, StringComparison.Ordinal);

		[Fact]
		public void TimeCueBeyondDurationIsAnError()
			=> Assert.Contains("MAUIX2301", Assert.Throws<InvalidOperationException>(() => Place("0:0:2")).Message, StringComparison.Ordinal);
	}

	public class RepeatBehaviorTests
	{
		[Theory]
		[InlineData("3x", 3.0)]
		[InlineData("0.5x", 0.5)]
		[InlineData("1X", 1.0)]
		public void ParsesCounts(string text, double count)
		{
			var repeat = (RepeatBehavior)new RepeatBehaviorTypeConverter().ConvertFromInvariantString(text);

			Assert.True(repeat.HasCount);
			Assert.Equal(count, repeat.Count);
		}

		[Fact]
		public void ParsesDurationsAndForever()
		{
			var converter = new RepeatBehaviorTypeConverter();

			var duration = (RepeatBehavior)converter.ConvertFromInvariantString("0:0:10");
			Assert.True(duration.HasDuration);
			Assert.Equal(TimeSpan.FromSeconds(10), duration.Duration);

			Assert.Equal(RepeatBehavior.Forever, (RepeatBehavior)converter.ConvertFromInvariantString("Forever"));
			Assert.False(RepeatBehavior.Forever.HasCount);
		}

		[Fact]
		public void DefaultIsOnce()
		{
			Assert.True(default(RepeatBehavior).HasCount);
			Assert.Equal(1.0, default(RepeatBehavior).Count);
			Assert.Equal(new RepeatBehavior(1), default(RepeatBehavior));
		}

		[Theory]
		[InlineData("-1x")]
		[InlineData("x")]
		[InlineData("sometimes")]
		public void RejectsMalformed(string text)
			=> Assert.Throws<FormatException>(() => new RepeatBehaviorTypeConverter().ConvertFromInvariantString(text));

		[Theory]
		[InlineData("3x")]
		[InlineData("Forever")]
		[InlineData("00:00:10")]
		public void RoundTrips(string text)
		{
			var converter = new RepeatBehaviorTypeConverter();

			Assert.Equal(text, converter.ConvertToInvariantString(converter.ConvertFromInvariantString(text)));
		}
	}
}

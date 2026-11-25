#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// How one run of a storyboard plays. Read once, when the player is created (or when
	/// <see cref="Storyboard.PlayAsync(VisualElement, PlaybackOptions?, System.Threading.CancellationToken)"/> is
	/// called); changing the object afterwards doesn't affect the run.
	/// </summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public sealed class PlaybackOptions
	{
		double _speedRatio = 1.0;

		/// <summary>What happens to properties another run owns. Defaults to taking them over in place.</summary>
		public StoryboardHandoffBehavior HandoffBehavior { get; set; } = StoryboardHandoffBehavior.SnapshotAndReplace;

		/// <summary>Multiplies the storyboard's own <see cref="Storyboard.SpeedRatio"/>. Must be positive.</summary>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the value is not positive.</exception>
		public double SpeedRatio
		{
			get => _speedRatio;
			set
			{
				if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
					throw new ArgumentOutOfRangeException(nameof(value), value, "SpeedRatio must be positive.");
				_speedRatio = value;
			}
		}

		/// <summary>Replaces the storyboard's <see cref="Storyboard.BeginTime"/> for this run, for example to stagger.</summary>
		public TimeSpan? BeginTimeOverride { get; set; }

		/// <summary>How a forward fill is kept when the run completes. <see cref="FillCompletion.Hold"/> is only
		/// valid for runs someone can stop (<see cref="Storyboard.Begin(VisualElement, PlaybackOptions?)"/> and
		/// <see cref="BeginStoryboard"/>).</summary>
		public FillCompletion FillCompletion { get; set; } = FillCompletion.Commit;

		internal PlaybackOptions Clone() => new()
		{
			HandoffBehavior = HandoffBehavior,
			_speedRatio = _speedRatio,
			BeginTimeOverride = BeginTimeOverride,
			FillCompletion = FillCompletion,
		};
	}
}

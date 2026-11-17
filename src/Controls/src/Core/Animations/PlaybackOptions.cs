#nullable enable
using System;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// Options for one run of a <see cref="Storyboard"/>.
	/// </summary>
	public sealed class PlaybackOptions
	{
		/// <summary>
		/// Gets or sets what the run does with properties another run animates. The default takes them over.
		/// </summary>
		public StoryboardHandoffBehavior HandoffBehavior { get; set; } = StoryboardHandoffBehavior.SnapshotAndReplace;

		/// <summary>
		/// Gets or sets a speed multiplier for this run. Must be greater than zero.
		/// </summary>
		public double SpeedRatio { get; set; } = 1.0;

		/// <summary>
		/// Gets or sets a begin time that replaces the storyboard's <see cref="Storyboard.BeginTime"/> for this run.
		/// </summary>
		public TimeSpan? BeginTimeOverride { get; set; }

		/// <summary>
		/// Gets or sets how the run keeps its end values. <see cref="FillCompletion.Hold"/> is only valid for
		/// <see cref="Storyboard.Begin(VisualElement, PlaybackOptions?)"/>.
		/// </summary>
		public FillCompletion FillCompletion { get; set; } = FillCompletion.Commit;
	}

	/// <summary>
	/// Data for <see cref="StoryboardPlayer.StateChanged"/>.
	/// </summary>
	public sealed class PlaybackStateChangedEventArgs : EventArgs
	{
		/// <summary>Creates the event data.</summary>
		/// <param name="oldState">The previous state.</param>
		/// <param name="newState">The new state.</param>
		public PlaybackStateChangedEventArgs(PlaybackState oldState, PlaybackState newState)
		{
			OldState = oldState;
			NewState = newState;
		}

		/// <summary>Gets the previous state.</summary>
		public PlaybackState OldState { get; }

		/// <summary>Gets the new state.</summary>
		public PlaybackState NewState { get; }
	}

	/// <summary>
	/// Data for <see cref="StoryboardPlayer.Completed"/>.
	/// </summary>
	public sealed class StoryboardCompletedEventArgs : EventArgs
	{
		/// <summary>Creates the event data.</summary>
		/// <param name="result">How the run ended.</param>
		/// <param name="replacedTracks">How many tracks other runs or user input took over.</param>
		public StoryboardCompletedEventArgs(AnimationResult result, int replacedTracks)
		{
			Result = result;
			ReplacedTracks = replacedTracks;
		}

		/// <summary>Gets how the run ended.</summary>
		public AnimationResult Result { get; }

		/// <summary>Gets how many tracks other runs or user input took over before the run ended.</summary>
		public int ReplacedTracks { get; }
	}
}

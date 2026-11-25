#nullable enable
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>How a storyboard run ended.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public enum AnimationResult
	{
		/// <summary>The run reached its end, or was skipped to its fill.</summary>
		Completed,

		/// <summary>The run was stopped: <see cref="StoryboardPlayer.Stop(StopBehavior)"/>, disposal, the target
		/// unloading or being collected, the window being destroyed, or a forever run skipped to its fill.</summary>
		Stopped,

		/// <summary>Every track was taken over: by another run, by user input, or by a TwoWay or OneWayToSource binding.</summary>
		Replaced,

		/// <summary>Every track was owned by another run and the handoff behavior was <see cref="StoryboardHandoffBehavior.Ignore"/>.</summary>
		Ignored,

		/// <summary>The run was settled at its end state because the system asks for reduced motion.</summary>
		ReducedMotionSkipped,
	}

	/// <summary>The playback state of a <see cref="StoryboardPlayer"/>.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public enum PlaybackState
	{
		/// <summary>Created and not yet started or sought.</summary>
		Created,

		/// <summary>Advancing with its clock.</summary>
		Running,

		/// <summary>Paused; its values stay on screen.</summary>
		Paused,

		/// <summary>Ended by reaching its end (or skipping to its fill).</summary>
		Finished,

		/// <summary>Ended early: stopped, replaced, faulted, or its target went away.</summary>
		Stopped,
	}

	/// <summary>What <see cref="StoryboardPlayer.Stop(StopBehavior)"/> leaves on the animated properties.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public enum StopBehavior
	{
		/// <summary>Remove the animated values; each property shows its own value again.</summary>
		Release,

		/// <summary>Apply the storyboard's <see cref="AnimationFillMode"/>, as if the run had ended.</summary>
		ApplyFill,

		/// <summary>Keep the values on screen now, committed beneath their value sources.</summary>
		CommitCurrent,
	}

	/// <summary>The point a seek is measured from.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public enum TimeSeekOrigin
	{
		/// <summary>From the start of the storyboard.</summary>
		BeginTime,

		/// <summary>Back from the end of the storyboard's active duration.</summary>
		Duration,
	}

	/// <summary>What a new run does with a property another run owns.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public enum StoryboardHandoffBehavior
	{
		/// <summary>Take the property over in place, continuing from the value on screen.</summary>
		SnapshotAndReplace,

		/// <summary>Leave the property to the run that owns it.</summary>
		Ignore,
	}

	/// <summary>How a kept end value is kept after a run with a forward fill completes.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public enum FillCompletion
	{
		/// <summary>Write the end values beneath their value sources and release the hold: the element keeps its
		/// final appearance while its bindings, resources, styles and visual states stay live.</summary>
		Commit,

		/// <summary>Keep the end values at animation rank until the player is stopped or disposed.</summary>
		Hold,
	}

	/// <summary>What a storyboard shows outside its active interval.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public enum AnimationFillMode
	{
		/// <summary>Nothing before it starts; released after it ends.</summary>
		None,

		/// <summary>Nothing before it starts; its end state after it ends.</summary>
		Forward,

		/// <summary>Its start state during the begin delay; released after it ends.</summary>
		Backward,

		/// <summary>Its start state during the begin delay and its end state after it ends.</summary>
		Both,
	}

	/// <summary>Which way each iteration runs (CSS names).</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public enum AnimationDirection
	{
		/// <summary>Every iteration runs forward.</summary>
		Normal,

		/// <summary>Every iteration runs backward.</summary>
		Reverse,

		/// <summary>Even iterations forward, odd ones backward. Not WPF's AutoReverse: n× Alternate is n passes.</summary>
		Alternate,

		/// <summary>Even iterations backward, odd ones forward.</summary>
		AlternateReverse,
	}

	/// <summary>What a run does when the system asks for reduced motion.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public enum ReducedMotionBehavior
	{
		/// <summary>Settle at exactly the state a normal completion produces, and complete with
		/// <see cref="AnimationResult.ReducedMotionSkipped"/>.</summary>
		SkipToEnd,

		/// <summary>Play the whole run in 10% of its length, capped at 100 ms. A forever run is skipped.</summary>
		Shorten,

		/// <summary>Ignore the setting, for motion that carries information, such as a progress indicator.</summary>
		Play,
	}
}

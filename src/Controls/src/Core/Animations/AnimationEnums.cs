#nullable enable

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// What an animation does when the operating system asks for reduced motion.
	/// </summary>
	public enum ReducedMotionBehavior
	{
		/// <summary>Jump to the end state and complete immediately.</summary>
		SkipToEnd,

		/// <summary>Play at 10% of the duration, capped at 100 milliseconds.</summary>
		Shorten,

		/// <summary>Ignore the setting, for motion that is the information, such as a progress indicator.</summary>
		Play,
	}

	/// <summary>
	/// The direction in which iterations of a storyboard play.
	/// </summary>
	public enum AnimationDirection
	{
		/// <summary>Every iteration plays forward.</summary>
		Normal,

		/// <summary>Every iteration plays backward.</summary>
		Reverse,

		/// <summary>Iterations alternate, starting forward.</summary>
		Alternate,

		/// <summary>Iterations alternate, starting backward.</summary>
		AlternateReverse,
	}

	/// <summary>
	/// Which state a storyboard shows before it begins and after it ends.
	/// </summary>
	public enum AnimationFillMode
	{
		/// <summary>No effect outside the active interval.</summary>
		None,

		/// <summary>The end state is kept after the last iteration.</summary>
		Forward,

		/// <summary>The start state is shown during the begin delay.</summary>
		Backward,

		/// <summary>Both <see cref="Forward"/> and <see cref="Backward"/>.</summary>
		Both,
	}

	/// <summary>
	/// How a run ended.
	/// </summary>
	public enum AnimationResult
	{
		/// <summary>The run reached its end.</summary>
		Completed,

		/// <summary>The run was stopped before its end.</summary>
		Stopped,

		/// <summary>Every track was taken over, by another run or by user input.</summary>
		Replaced,

		/// <summary>The run owned nothing, because its handoff behavior is <see cref="StoryboardHandoffBehavior.Ignore"/>.</summary>
		Ignored,

		/// <summary>The run was skipped because the operating system asks for reduced motion.</summary>
		ReducedMotionSkipped,
	}

	/// <summary>
	/// The state of a <see cref="StoryboardPlayer"/>.
	/// </summary>
	public enum PlaybackState
	{
		/// <summary>The player was created and hasn't started.</summary>
		Created,

		/// <summary>The player is running.</summary>
		Running,

		/// <summary>The player is paused.</summary>
		Paused,

		/// <summary>The player reached its end.</summary>
		Finished,

		/// <summary>The player was stopped.</summary>
		Stopped,
	}

	/// <summary>
	/// What <see cref="StoryboardPlayer.Stop(StopBehavior)"/> does with the animated values.
	/// </summary>
	public enum StopBehavior
	{
		/// <summary>Remove the animated values; properties show their own values again.</summary>
		Release,

		/// <summary>Apply the storyboard's fill rule, as if it had reached its end.</summary>
		ApplyFill,

		/// <summary>Keep the current animated values, preserving their value sources.</summary>
		CommitCurrent,
	}

	/// <summary>
	/// The origin of a seek.
	/// </summary>
	public enum TimeSeekOrigin
	{
		/// <summary>Seek relative to the beginning of the timeline.</summary>
		BeginTime,

		/// <summary>Seek relative to the end of one iteration.</summary>
		Duration,
	}

	/// <summary>
	/// What a new run does with a property that another run animates.
	/// </summary>
	public enum StoryboardHandoffBehavior
	{
		/// <summary>Take the property over from the current animated value.</summary>
		SnapshotAndReplace,

		/// <summary>Leave the property to the run that animates it.</summary>
		Ignore,
	}

	/// <summary>
	/// How a run keeps its end values.
	/// </summary>
	public enum FillCompletion
	{
		/// <summary>Keep the end values while bindings, resources, styles and visual states stay attached.</summary>
		Commit,

		/// <summary>Hold the end values above every other value until the player is stopped or disposed.</summary>
		Hold,
	}
}

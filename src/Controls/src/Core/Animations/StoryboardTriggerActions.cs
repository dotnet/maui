#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>Starts a storyboard on the trigger's element.</summary>
	/// <remarks>A run is identified by (storyboard, element), so the stop, pause, resume and skip actions find it in
	/// Styles and templates too, without a name.</remarks>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public sealed class BeginStoryboard : TriggerAction<VisualElement>
	{
		/// <summary>The storyboard to start.</summary>
		public Storyboard? Storyboard { get; set; }

		/// <summary>What the run does with properties another run owns.</summary>
		public StoryboardHandoffBehavior HandoffBehavior { get; set; }

		/// <summary>How fast the run plays.</summary>
		public double SpeedRatio { get; set; } = 1.0;

		/// <summary>How a forward fill is kept. <see cref="FillCompletion.Hold"/> keeps it until a <see cref="StopStoryboard"/>.</summary>
		public FillCompletion FillCompletion { get; set; }

		/// <inheritdoc/>
		protected override void Invoke(VisualElement sender)
		{
			if (Storyboard is not Storyboard storyboard)
				return;

			// The new run starts while the previous one still holds its values, so the engine applies the handoff:
			// SnapshotAndReplace continues from the value on screen, Ignore leaves the previous run in charge.
			var player = storyboard.Begin(sender, new PlaybackOptions
			{
				HandoffBehavior = HandoffBehavior,
				SpeedRatio = SpeedRatio,
				FillCompletion = FillCompletion,
			});
			StoryboardRuns.Replace(sender, storyboard, player, HandoffBehavior);
		}
	}

	/// <summary>Stops the run a <see cref="BeginStoryboard"/> started on the trigger's element.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public sealed class StopStoryboard : TriggerAction<VisualElement>
	{
		/// <summary>The storyboard whose run to stop.</summary>
		public Storyboard? Storyboard { get; set; }

		/// <summary>What the stop leaves on the properties. Defaults to removing the animated values.</summary>
		public StopBehavior Behavior { get; set; }

		/// <inheritdoc/>
		protected override void Invoke(VisualElement sender)
		{
			if (Storyboard is Storyboard storyboard)
				StoryboardRuns.Stop(sender, storyboard, Behavior);
		}
	}

	/// <summary>Pauses the run a <see cref="BeginStoryboard"/> started on the trigger's element.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public sealed class PauseStoryboard : TriggerAction<VisualElement>
	{
		/// <summary>The storyboard whose run to control.</summary>
		public Storyboard? Storyboard { get; set; }

		/// <inheritdoc/>
		protected override void Invoke(VisualElement sender) => StoryboardRuns.Find(sender, Storyboard)?.Pause();
	}

	/// <summary>Resumes the run a <see cref="BeginStoryboard"/> started on the trigger's element.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public sealed class ResumeStoryboard : TriggerAction<VisualElement>
	{
		/// <summary>The storyboard whose run to control.</summary>
		public Storyboard? Storyboard { get; set; }

		/// <inheritdoc/>
		protected override void Invoke(VisualElement sender) => StoryboardRuns.Find(sender, Storyboard)?.Resume();
	}

	/// <summary>Skips the run a <see cref="BeginStoryboard"/> started on the trigger's element to its fill.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public sealed class SkipStoryboardToFill : TriggerAction<VisualElement>
	{
		/// <summary>The storyboard whose run to control.</summary>
		public Storyboard? Storyboard { get; set; }

		/// <inheritdoc/>
		protected override void Invoke(VisualElement sender) => StoryboardRuns.Find(sender, Storyboard)?.SkipToFill();
	}

	/// <summary>Stops storyboard runs on an element.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public static class StoryboardElementExtensions
	{
		/// <summary>
		/// Stops every storyboard run that trigger actions or <see cref="Storyboard.PlayOnLoadedProperty"/> started on
		/// <paramref name="element"/>. <c>ViewExtensions.CancelAnimations</c> is unchanged and doesn't stop storyboards.
		/// </summary>
		public static void StopStoryboards(this VisualElement element, StopBehavior behavior = StopBehavior.Release)
		{
			if (element is null)
				throw new ArgumentNullException(nameof(element));

			StoryboardRuns.StopAll(element, behavior);
		}
	}

	/// <summary>
	/// The runs trigger actions and PlayOnLoaded started on an element, kept on the element itself (no global
	/// registry). A run is remembered while it holds values, so a later StopStoryboard can still release a held fill;
	/// runs that hold nothing are forgotten, so repeated triggers don't accumulate players.
	/// </summary>
	internal static class StoryboardRuns
	{
		static readonly BindableProperty RunsProperty =
			BindableProperty.CreateAttached("StoryboardRuns", typeof(List<StoryboardPlayer>), typeof(StoryboardRuns), null);

		public static void Add(VisualElement element, StoryboardPlayer player)
		{
			// A run that has already ended holding nothing (fully ignored, say) is never registered: the run actions
			// would otherwise find it instead of the run that is actually playing.
			if ((player.State == PlaybackState.Finished || player.State == PlaybackState.Stopped) && !player.HoldsAnything())
				return;

			var runs = (List<StoryboardPlayer>?)element.GetValue(RunsProperty);
			if (runs is null)
			{
				runs = new List<StoryboardPlayer>();
				element.SetValue(RunsProperty, runs);
			}

			Prune(runs);
			runs.Add(player);
		}

		/// <summary>
		/// Records a run that has just started for <paramref name="storyboard"/>. Earlier runs of the same storyboard
		/// that it took over are forgotten; one it was ignored in favour of keeps running and stays registered.
		/// </summary>
		public static void Replace(VisualElement element, Storyboard storyboard, StoryboardPlayer player, StoryboardHandoffBehavior handoff)
		{
			List<Exception>? errors = null;
			// An Ignore run leaves earlier runs in charge of whatever they still own; only a takeover replaces them.
			if (handoff == StoryboardHandoffBehavior.SnapshotAndReplace && (player.State != PlaybackState.Stopped || player.HoldsAnything()))
			{
				if (element.GetValue(RunsProperty) is List<StoryboardPlayer> runs)
				{
					foreach (var run in runs.ToArray())
					{
						if (ReferenceEquals(run.Storyboard, storyboard) && !ReferenceEquals(run, player))
						{
							// Whatever the previous run still holds (nothing, after a full takeover) is released. Every earlier
							// run is retired, and the new one registered, even when a release throws; the errors follow.
							try
							{
								run.Stop(StopBehavior.Release);
							}
							catch (Exception ex)
							{
								(errors ??= new List<Exception>()).Add(ex);
							}

							runs.Remove(run);
						}
					}
				}
			}

			Add(element, player);

			// Thrown as Stop throws it (one failure as is), so callers see the same exception shape.
			if (errors is { Count: 1 })
				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(errors[0]).Throw();
			if (errors is not null)
				throw new AggregateException(errors);
		}

		public static StoryboardPlayer? Find(VisualElement element, Storyboard? storyboard)
		{
			if (storyboard is null || element.GetValue(RunsProperty) is not List<StoryboardPlayer> runs)
				return null;

			// The latest run of the storyboard that is still playing or still holds values; a run that ended holding
			// nothing (taken over after it was registered) is skipped.
			for (var i = runs.Count - 1; i >= 0; i--)
			{
				var run = runs[i];
				var inert = (run.State == PlaybackState.Finished || run.State == PlaybackState.Stopped) && !run.HoldsAnything();
				if (ReferenceEquals(run.Storyboard, storyboard) && !inert)
					return run;
			}

			return null;
		}

		public static void Stop(VisualElement element, Storyboard storyboard, StopBehavior behavior)
		{
			if (element.GetValue(RunsProperty) is not List<StoryboardPlayer> runs)
				return;

			List<Exception>? errors = null;
			foreach (var run in runs.ToArray())
			{
				if (ReferenceEquals(run.Storyboard, storyboard))
				{
					try
					{
						run.Stop(behavior);
					}
					catch (Exception ex)
					{
						(errors ??= new List<Exception>()).Add(ex);
					}

					if (!run.HoldsAnything())
						runs.Remove(run);
				}
			}

			if (errors is not null)
				throw new AggregateException(errors);
		}

		public static void StopAll(VisualElement element, StopBehavior behavior)
		{
			if (element.GetValue(RunsProperty) is not List<StoryboardPlayer> runs)
				return;

			// Every run is stopped and the list pruned even when one stop throws; the failures are rethrown together.
			List<Exception>? errors = null;
			foreach (var run in runs.ToArray())
			{
				try
				{
					run.Stop(behavior);
				}
				catch (Exception ex)
				{
					(errors ??= new List<Exception>()).Add(ex);
				}
			}

			Prune(runs);
			if (errors is not null)
				throw new AggregateException(errors);
		}

		static void Prune(List<StoryboardPlayer> runs)
			=> runs.RemoveAll(run => (run.State == PlaybackState.Finished || run.State == PlaybackState.Stopped) && !run.HoldsAnything());
	}
}

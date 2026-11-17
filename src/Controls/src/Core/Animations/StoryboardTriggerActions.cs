#nullable enable
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// A trigger action that starts a <see cref="Storyboard"/> on the element that raised the trigger.
	/// </summary>
	public sealed class BeginStoryboard : TriggerAction<VisualElement>
	{
		/// <summary>Gets or sets the storyboard to start.</summary>
		public Storyboard? Storyboard { get; set; }

		/// <summary>Gets or sets what the run does with properties another run animates.</summary>
		public StoryboardHandoffBehavior HandoffBehavior { get; set; }

		/// <summary>Gets or sets a speed multiplier for the run.</summary>
		public double SpeedRatio { get; set; } = 1.0;

		/// <summary>
		/// Gets or sets how the run keeps its end values. Use <see cref="FillCompletion.Hold"/> with a
		/// <see cref="StopStoryboard"/> for an enter and exit pair that reverts.
		/// </summary>
		public FillCompletion FillCompletion { get; set; }

		/// <inheritdoc/>
		protected override void Invoke(VisualElement sender)
		{
			if (Storyboard is not { } storyboard)
				return;

			var player = storyboard.Begin(sender, new PlaybackOptions
			{
				HandoffBehavior = HandoffBehavior,
				SpeedRatio = SpeedRatio,
				FillCompletion = FillCompletion,
			});

			TriggerStoryboardRuns.Add(sender, player);
		}
	}

	/// <summary>
	/// A trigger action that stops the runs a <see cref="BeginStoryboard"/> started on the element that raised the trigger.
	/// </summary>
	public sealed class StopStoryboard : TriggerAction<VisualElement>
	{
		/// <summary>Gets or sets the storyboard whose runs stop. When <see langword="null"/>, every run started by a trigger stops.</summary>
		public Storyboard? Storyboard { get; set; }

		/// <summary>Gets or sets what happens to the animated values.</summary>
		public StopBehavior StopBehavior { get; set; }

		/// <inheritdoc/>
		protected override void Invoke(VisualElement sender) => TriggerStoryboardRuns.Stop(sender, Storyboard, StopBehavior);
	}

	static class TriggerStoryboardRuns
	{
		static readonly ConditionalWeakTable<VisualElement, List<StoryboardPlayer>> s_runs = new();

		public static void Add(VisualElement element, StoryboardPlayer player)
		{
			var runs = s_runs.GetValue(element, static _ => new List<StoryboardPlayer>());
			runs.RemoveAll(p => p.State == PlaybackState.Stopped);
			runs.Add(player);
		}

		public static void Stop(VisualElement element, Storyboard? storyboard, StopBehavior behavior)
		{
			if (!s_runs.TryGetValue(element, out var runs))
				return;

			foreach (var player in runs.ToArray())
			{
				if (storyboard is null || ReferenceEquals(player.Storyboard, storyboard))
				{
					player.Stop(behavior);
					runs.Remove(player);
				}
			}
		}
	}
}

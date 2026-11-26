#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// A source of animation time. Players advance when <see cref="Tick"/> is raised and read <see cref="Elapsed"/>.
	/// </summary>
	/// <remarks>Players normally run on the window's vsync clock. Pass a <see cref="ManualStoryboardClock"/> to
	/// <see cref="Storyboard.CreatePlayer(VisualElement, IStoryboardClock?, PlaybackOptions?)"/> to drive one
	/// deterministically, for example in a unit test.</remarks>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public interface IStoryboardClock
	{
		/// <summary>The clock's current time. It only moves forward.</summary>
		TimeSpan Elapsed { get; }

		/// <summary>Raised once per frame, after <see cref="Elapsed"/> has advanced.</summary>
		event EventHandler? Tick;
	}

	/// <summary>
	/// A clock that advances only when told to: no ticker, no window and no real time.
	/// </summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public sealed class ManualStoryboardClock : IStoryboardClock
	{
		/// <inheritdoc/>
		public TimeSpan Elapsed { get; private set; }

		/// <inheritdoc/>
		public event EventHandler? Tick;

		/// <summary>Moves the clock forward and raises <see cref="Tick"/> synchronously.</summary>
		/// <param name="delta">How far to move. Must not be negative.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="delta"/> is negative.</exception>
		public void Advance(TimeSpan delta)
		{
			if (delta < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(delta), delta, "A clock only moves forward.");

			Elapsed += delta;
			Tick?.Invoke(this, EventArgs.Empty);
		}
	}
}

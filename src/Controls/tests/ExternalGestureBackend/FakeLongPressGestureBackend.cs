using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Tests.ExternalGestureBackend
{
	/// <summary>
	/// Stands in for a third-party platform backend (for example <c>Maui.Tizen</c>) that detects long
	/// presses natively and forwards them to <see cref="LongPressGestureRecognizer"/>.
	/// </summary>
	/// <remarks>
	/// This type lives in an assembly that is intentionally excluded from
	/// <c>Microsoft.Maui.Controls</c>'s <c>InternalsVisibleTo</c> list, so it can only compile against
	/// the public gesture dispatch surface. It exists to guarantee that external backends can raise
	/// long press gestures without reflection or internal access.
	/// </remarks>
	public sealed class FakeLongPressGestureBackend
	{
		/// <summary>
		/// Offset applied when a caller asks for a position relative to an element other than the
		/// pressed view, so tests can distinguish the two code paths.
		/// </summary>
		public static readonly Point RelativeElementOffset = new Point(100, 200);

		readonly View _view;
		readonly List<LongPressGestureRecognizer> _active = new();

		Point _origin;

		/// <summary>
		/// Initializes a new instance of the <see cref="FakeLongPressGestureBackend"/> class.
		/// </summary>
		/// <param name="view">The view the platform backend is attached to.</param>
		public FakeLongPressGestureBackend(View view)
		{
			_view = view ?? throw new ArgumentNullException(nameof(view));
		}

		/// <summary>
		/// Gets or sets the number of touches the simulated platform gesture is tracking.
		/// Only recognizers whose <see cref="LongPressGestureRecognizer.NumberOfTouchesRequired"/>
		/// matches this value participate in the gesture.
		/// </summary>
		public int TouchCount { get; set; } = 1;

		/// <summary>
		/// Gets the recognizers that are currently tracking the in-flight gesture.
		/// </summary>
		public IReadOnlyList<LongPressGestureRecognizer> ActiveRecognizers => _active;

		/// <summary>
		/// Raises <see cref="GestureStatus.Started"/> for every matching recognizer on the view.
		/// </summary>
		/// <param name="origin">The press location relative to the pressed view.</param>
		public void RaisePressStarted(Point origin)
		{
			_origin = origin;
			_active.Clear();
			_active.AddRange(_view.GestureRecognizers
				.OfType<LongPressGestureRecognizer>()
				.Where(recognizer => recognizer.NumberOfTouchesRequired == TouchCount));

			foreach (var recognizer in _active)
				recognizer.SendLongPressing(_view, GestureStatus.Started, GetPosition(origin));
		}

		/// <summary>
		/// Raises <see cref="GestureStatus.Running"/> while the press is held, cancelling any
		/// recognizer whose <see cref="LongPressGestureRecognizer.AllowableMovement"/> was exceeded.
		/// </summary>
		/// <param name="location">The current press location relative to the pressed view.</param>
		public void RaisePressMoved(Point location)
		{
			var distance = _origin.Distance(location);

			foreach (var recognizer in _active.ToArray())
			{
				if (distance > recognizer.AllowableMovement)
				{
					_active.Remove(recognizer);
					recognizer.SendLongPressing(_view, GestureStatus.Canceled, GetPosition(location));
					continue;
				}

				recognizer.SendLongPressing(_view, GestureStatus.Running, GetPosition(location));
			}
		}

		/// <summary>
		/// Completes the gesture, raising the long pressed event followed by
		/// <see cref="GestureStatus.Completed"/>, mirroring the built-in platform backends.
		/// </summary>
		/// <param name="location">The release location relative to the pressed view.</param>
		public void RaisePressCompleted(Point? location = null)
		{
			var position = GetPosition(location ?? _origin);

			foreach (var recognizer in _active)
			{
				recognizer.SendLongPressed(_view, position);
				recognizer.SendLongPressing(_view, GestureStatus.Completed, position);
			}

			_active.Clear();
		}

		/// <summary>
		/// Cancels the gesture, raising <see cref="GestureStatus.Canceled"/> without a long pressed event.
		/// </summary>
		/// <param name="location">The location the gesture was cancelled at, relative to the pressed view.</param>
		public void RaisePressCanceled(Point? location = null)
		{
			var position = GetPosition(location ?? _origin);

			foreach (var recognizer in _active)
				recognizer.SendLongPressing(_view, GestureStatus.Canceled, position);

			_active.Clear();
		}

		Func<IElement?, Point?> GetPosition(Point origin) => relativeTo =>
			relativeTo is null || ReferenceEquals(relativeTo, _view)
				? origin
				: new Point(origin.X + RelativeElementOffset.X, origin.Y + RelativeElementOffset.Y);
	}
}

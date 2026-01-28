#nullable disable
using System;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	/// <summary>Recognizes swipe gestures on the attached element.</summary>
	public sealed class SwipeGestureRecognizer : GestureRecognizer, ISwipeGestureController
	{
		// Default threshold in pixels before a swipe is detected.
		const uint DefaultSwipeThreshold = 100;

		double _totalX, _totalY;

		/// <summary>Bindable property for <see cref="Command"/>. This is a bindable property.</summary>
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SwipeGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="CommandParameter"/>. This is a bindable property.</summary>
		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(SwipeGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="Direction"/>. This is a bindable property.</summary>
		public static readonly BindableProperty DirectionProperty = BindableProperty.Create(nameof(Direction), typeof(SwipeDirection), typeof(SwipeGestureRecognizer), default(SwipeDirection));

		/// <summary>Bindable property for <see cref="Threshold"/>. This is a bindable property.</summary>
		public static readonly BindableProperty ThresholdProperty = BindableProperty.Create(nameof(Threshold), typeof(uint), typeof(SwipeGestureRecognizer), DefaultSwipeThreshold);

		/// <summary>Gets or sets the command to invoke when the gesture is recognized. This is a bindable property.</summary>
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <summary>Gets or sets the parameter to pass to the <see cref="Command"/>. This is a bindable property.</summary>
		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <summary>Gets or sets the direction(s) of swipes to recognize. This is a bindable property.</summary>
		public SwipeDirection Direction
		{
			get { return (SwipeDirection)GetValue(DirectionProperty); }
			set { SetValue(DirectionProperty, value); }
		}

		/// <summary>Gets or sets the minimum distance in pixels the swipe must travel to be recognized. Default is 100. This is a bindable property.</summary>
		public uint Threshold
		{
			get { return (uint)GetValue(ThresholdProperty); }
			set { SetValue(ThresholdProperty, value); }
		}

		/// <summary>Occurs when a swipe gesture is recognized on the element.</summary>
		public event EventHandler<SwipedEventArgs> Swiped;

		void ISwipeGestureController.SendSwipe(Element sender, double totalX, double totalY)
		{
			_totalX = totalX;
			_totalY = totalY;
		}

		bool ISwipeGestureController.DetectSwipe(View sender, SwipeDirection direction)
		{
			var detected = false;
			var threshold = Threshold;

			var detectedDirection = (SwipeDirection)(0);

			if (direction.IsLeft())
			{
				if (_totalX < -threshold)
				{
					detected = true;
					detectedDirection |= SwipeDirection.Left;
				}
			}

			if (direction.IsRight())
			{
				if (_totalX > threshold)
				{
					detected = true;
					detectedDirection |= SwipeDirection.Right;
				}
			}

			if (direction.IsDown())
			{
				if (_totalY > threshold)
				{
					detected = true;
					detectedDirection |= SwipeDirection.Down;
				}
			}

			if (direction.IsUp())
			{
				if (_totalY < -threshold)
				{
					detected = true;
					detectedDirection |= SwipeDirection.Up;
				}
			}

			if (detected)
			{
				SendSwiped(sender, detectedDirection);
			}

			return detected;
		}

		/// <summary>Invokes the <see cref="Swiped"/> event and executes the <see cref="Command"/>.</summary>
		/// <param name="sender">The view that detected the swipe.</param>
		/// <param name="direction">The direction of the swipe.</param>
		public void SendSwiped(View sender, SwipeDirection direction)
		{
			ICommand cmd = Command;
			if (cmd != null && cmd.CanExecute(CommandParameter))
				cmd.Execute(CommandParameter);

			Swiped?.Invoke(sender, new SwipedEventArgs(CommandParameter, direction));
		}
	}

	static class SwipeDirectionExtensions
	{
		public static bool IsLeft(this SwipeDirection self)
		{
			return (self & SwipeDirection.Left) == SwipeDirection.Left;
		}
		public static bool IsRight(this SwipeDirection self)
		{
			return (self & SwipeDirection.Right) == SwipeDirection.Right;
		}
		public static bool IsUp(this SwipeDirection self)
		{
			return (self & SwipeDirection.Up) == SwipeDirection.Up;
		}
		public static bool IsDown(this SwipeDirection self)
		{
			return (self & SwipeDirection.Down) == SwipeDirection.Down;
		}
	}
}
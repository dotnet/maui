#nullable disable
using System;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/SwipeGestureRecognizer.xml" path="Type[@FullName='Microsoft.Maui.Controls.SwipeGestureRecognizer']/Docs/*" />
	public sealed class SwipeGestureRecognizer : GestureRecognizer, ISwipeGestureController
	{
		// Default threshold in pixels before a swipe is detected.
		const uint DefaultSwipeThreshold = 100;

		double _totalX, _totalY;

		/// <summary>Bindable property for <see cref="Command"/>.</summary>
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SwipeGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create("CommandParameter", typeof(object), typeof(SwipeGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="Direction"/>.</summary>
		public static readonly BindableProperty DirectionProperty = BindableProperty.Create("Direction", typeof(SwipeDirection), typeof(SwipeGestureRecognizer), default(SwipeDirection));

		/// <summary>Bindable property for <see cref="Threshold"/>.</summary>
		public static readonly BindableProperty ThresholdProperty = BindableProperty.Create("Threshold", typeof(uint), typeof(SwipeGestureRecognizer), DefaultSwipeThreshold);

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeGestureRecognizer.xml" path="//Member[@MemberName='Command']/Docs/*" />
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeGestureRecognizer.xml" path="//Member[@MemberName='CommandParameter']/Docs/*" />
		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeGestureRecognizer.xml" path="//Member[@MemberName='Direction']/Docs/*" />
		public SwipeDirection Direction
		{
			get { return (SwipeDirection)GetValue(DirectionProperty); }
			set { SetValue(DirectionProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeGestureRecognizer.xml" path="//Member[@MemberName='Threshold']/Docs/*" />
		public uint Threshold
		{
			get { return (uint)GetValue(ThresholdProperty); }
			set { SetValue(ThresholdProperty, value); }
		}

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

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeGestureRecognizer.xml" path="//Member[@MemberName='SendSwiped']/Docs/*" />
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
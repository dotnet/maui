using System;
using System.Windows.Input;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Recognizer for long press gestures. Fires when the user presses and holds for a specified duration.
	/// </summary>
	public sealed class LongPressGestureRecognizer : GestureRecognizer
	{
		/// <summary>Bindable property for <see cref="Command"/>.</summary>
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(LongPressGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(LongPressGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="MinimumPressDuration"/>.</summary>
		public static readonly BindableProperty MinimumPressDurationProperty = BindableProperty.Create(nameof(MinimumPressDuration), typeof(int), typeof(LongPressGestureRecognizer), 500);

		/// <summary>Bindable property for <see cref="NumberOfTouchesRequired"/>.</summary>
		public static readonly BindableProperty NumberOfTouchesRequiredProperty = BindableProperty.Create(nameof(NumberOfTouchesRequired), typeof(int), typeof(LongPressGestureRecognizer), 1);

		/// <summary>Bindable property for <see cref="AllowableMovement"/>.</summary>
		public static readonly BindableProperty AllowableMovementProperty = BindableProperty.Create(nameof(AllowableMovement), typeof(double), typeof(LongPressGestureRecognizer), 10.0);

		/// <summary>Bindable property for <see cref="State"/>.</summary>
		public static readonly BindableProperty StateProperty = BindableProperty.Create(nameof(State), typeof(GestureStatus), typeof(LongPressGestureRecognizer), GestureStatus.Canceled, BindingMode.OneWayToSource);

		/// <summary>
		/// Initializes a new instance of the <see cref="LongPressGestureRecognizer"/> class.
		/// </summary>
		public LongPressGestureRecognizer()
		{
		}

		/// <summary>
		/// Gets or sets the command to invoke when the long press gesture is recognized.
		/// </summary>
		public ICommand? Command
		{
			get { return (ICommand?)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <summary>
		/// Gets or sets the parameter to pass to the <see cref="Command"/>.
		/// </summary>
		public object? CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <summary>
		/// Gets or sets the minimum duration (in milliseconds) the user must press before the gesture is recognized.
		/// Default is 500 milliseconds.
		/// </summary>
		/// <remarks>
		/// <para><b>Platform Limitations:</b></para>
		/// <para><b>Android:</b> This property is not configurable on Android. The platform uses the system-default 
		/// long press timeout (typically 400ms from ViewConfiguration.getLongPressTimeout()), which cannot be changed 
		/// per gesture recognizer. The MinimumPressDuration value is ignored on Android.</para>
		/// </remarks>
		public int MinimumPressDuration
		{
			get { return (int)GetValue(MinimumPressDurationProperty); }
			set { SetValue(MinimumPressDurationProperty, value); }
		}

		/// <summary>
		/// Gets or sets the number of touches required for the gesture to be recognized.
		/// Default is 1. Only supported on iOS and Mac Catalyst.
		/// </summary>
		/// <remarks>
		/// On Android and Windows, this property is ignored and the gesture always requires 1 touch.
		/// </remarks>
		public int NumberOfTouchesRequired
		{
			get { return (int)GetValue(NumberOfTouchesRequiredProperty); }
			set { SetValue(NumberOfTouchesRequiredProperty, value); }
		}

		/// <summary>
		/// Gets or sets the maximum distance (in pixels) the touch can move before the gesture is cancelled.
		/// Default is 10 pixels.
		/// </summary>
		public double AllowableMovement
		{
			get { return (double)GetValue(AllowableMovementProperty); }
			set { SetValue(AllowableMovementProperty, value); }
		}

		/// <summary>
		/// Gets the current state of the gesture. This property is updated in real-time on iOS and Mac Catalyst.
		/// On Android and Windows, it transitions directly from Canceled to Completed.
		/// </summary>
		public GestureStatus State
		{
			get { return (GestureStatus)GetValue(StateProperty); }
			internal set { SetValue(StateProperty, value); }
		}

		/// <summary>
		/// Occurs when the long press gesture is completed (when the user releases after the minimum duration).
		/// </summary>
		public event EventHandler<LongPressedEventArgs>? LongPressed;

		/// <summary>
		/// Occurs when the long press gesture state changes. Primarily useful on iOS and Mac Catalyst
		/// for tracking Started, Running, Completed, and Canceled states.
		/// </summary>
		/// <remarks>
		/// On Android and Windows, this event fires when the gesture completes or is cancelled,
		/// but does not provide Started/Running state updates.
		/// </remarks>
		public event EventHandler<LongPressingEventArgs>? LongPressing;

		/// <summary>
		/// Sends the long pressed event and executes the command.
		/// </summary>
		internal void SendLongPressed(View sender, Func<IElement?, Point?>? getPosition = null)
		{
			var cmd = Command;
			if (cmd != null && cmd.CanExecute(CommandParameter))
				cmd.Execute(CommandParameter);

			LongPressed?.Invoke(sender, new LongPressedEventArgs(CommandParameter, getPosition));
		}

		/// <summary>
		/// Sends the long pressing event with state information.
		/// </summary>
		internal void SendLongPressing(View sender, GestureStatus status, Func<IElement?, Point?>? getPosition = null)
		{
			State = status;
			LongPressing?.Invoke(sender, new LongPressingEventArgs(status, getPosition));
		}
	}
}

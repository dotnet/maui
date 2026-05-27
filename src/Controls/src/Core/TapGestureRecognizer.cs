using System;
using System.Windows.Input;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>Recognizes tap gestures on the attached element.</summary>
	public sealed class TapGestureRecognizer : GestureRecognizer
	{
		/// <summary>Bindable property for <see cref="Command"/>. This is a bindable property.</summary>
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(TapGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="CommandParameter"/>. This is a bindable property.</summary>
		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(TapGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="NumberOfTapsRequired"/>. This is a bindable property.</summary>
		public static readonly BindableProperty NumberOfTapsRequiredProperty = BindableProperty.Create(nameof(NumberOfTapsRequired), typeof(int), typeof(TapGestureRecognizer), 1);

		/// <summary>Bindable property for <see cref="Buttons"/>. This is a bindable property.</summary>
		public static readonly BindableProperty ButtonsProperty = BindableProperty.Create(nameof(Buttons), typeof(ButtonsMask), typeof(TapGestureRecognizer), ButtonsMask.Primary);

		/// <summary>Initializes a new instance of the <see cref="TapGestureRecognizer"/> class.</summary>
		public TapGestureRecognizer()
		{
		}

		/// <summary>Gets or sets the command to invoke when the gesture is recognized. This is a bindable property.</summary>
		public ICommand? Command
		{
			get { return (ICommand?)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <summary>Gets or sets the parameter to pass to the <see cref="Command"/>. This is a bindable property.</summary>
		public object? CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <summary>Gets or sets the number of taps required to trigger the gesture. Default is 1. This is a bindable property.</summary>
		public int NumberOfTapsRequired
		{
			get { return (int)GetValue(NumberOfTapsRequiredProperty); }
			set { SetValue(NumberOfTapsRequiredProperty, value); }
		}

		/// <summary>Gets or sets the button mask that triggers the gesture. This is a bindable property.</summary>
		public ButtonsMask Buttons
		{
			get { return (ButtonsMask)GetValue(ButtonsProperty); }
			set { SetValue(ButtonsProperty, value); }
		}

		/// <summary>Occurs when a tap gesture is recognized on the element.</summary>
		public event EventHandler<TappedEventArgs>? Tapped;

		internal void SendTapped(View sender, Func<IElement?, Point?>? getPosition = null)
		{
			var cmd = Command;
			if (cmd != null && cmd.CanExecute(CommandParameter))
				cmd.Execute(CommandParameter);

			Tapped?.Invoke(sender, new TappedEventArgs(CommandParameter, getPosition, Buttons));
		}

	}
}
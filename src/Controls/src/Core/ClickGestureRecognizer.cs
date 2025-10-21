#nullable disable
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	/// <summary>Recognizer for click gestures.</summary>
	[Obsolete("The ClickGestureRecognizer is deprecated; please use TapGestureRecognizer or PointerGestureRecognizer instead.")]
	public sealed class ClickGestureRecognizer : GestureRecognizer
	{
		/// <summary>Bindable property for <see cref="Command"/>.</summary>
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ClickGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ClickGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="NumberOfClicksRequired"/>.</summary>
		public static readonly BindableProperty NumberOfClicksRequiredProperty = BindableProperty.Create(nameof(NumberOfClicksRequired), typeof(int), typeof(ClickGestureRecognizer), 1);

		/// <summary>Bindable property for <see cref="Buttons"/>.</summary>
		public static readonly BindableProperty ButtonsProperty = BindableProperty.Create(nameof(Buttons), typeof(ButtonsMask), typeof(ClickGestureRecognizer), ButtonsMask.Primary);

		/// <summary>Creates a new <see cref="Microsoft.Maui.Controls.ClickGestureRecognizer"/> with default values.</summary>
		public ClickGestureRecognizer()
		{
		}

		/// <summary>Gets or sets the command to run.</summary>
		/// <remarks>The command may be null.</remarks>
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <summary>Gets or sets the command parameter.</summary>
		/// <remarks>The command parameter may be null.</remarks>
		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <summary>The number of clicks required to activate the recognizer. This is a bindable property.</summary>
		public int NumberOfClicksRequired
		{
			get { return (int)GetValue(NumberOfClicksRequiredProperty); }
			set { SetValue(NumberOfClicksRequiredProperty, value); }
		}

		/// <summary>Gets or sets flag values that indicate which button or buttons were clicked.</summary>
		/// <remarks>On a mouse used in the right hand, the left button is typically the primary button. It is the button typically under the index finger. For a mouse used in the left hand, the right button is typically the primary button.</remarks>
		public ButtonsMask Buttons
		{
			get { return (ButtonsMask)GetValue(ButtonsProperty); }
			set { SetValue(ButtonsProperty, value); }
		}

		public event EventHandler Clicked;

		/// <summary>Runs the command for the click, if present, and invokes the click event handler when appropriate.</summary>
		/// <param name="sender">The object that is sending the click event.</param>
		/// <param name="buttons">The buttons that were involved in the click event.</param>
		/// <remarks>Both the command and the event are run, if present.</remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendClicked(View sender, ButtonsMask buttons)
		{
			ICommand cmd = Command;
			object parameter = CommandParameter;
			if (cmd != null && cmd.CanExecute(parameter))
				cmd.Execute(parameter);

			Clicked?.Invoke(sender, new ClickedEventArgs(buttons, parameter));

		}
	}
}

using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Xamarin.Forms
{
	[Flags]
	public enum ButtonsMask
	{
		Primary = 1 << 0,
		Secondary = 1 << 1
	}

	public sealed class ClickGestureRecognizer : GestureRecognizer
	{
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ClickGestureRecognizer), null);

		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ClickGestureRecognizer), null);

		public static readonly BindableProperty NumberOfClicksRequiredProperty = BindableProperty.Create(nameof(NumberOfClicksRequired), typeof(int), typeof(ClickGestureRecognizer), 1);

		public static readonly BindableProperty ButtonsProperty = BindableProperty.Create(nameof(Buttons), typeof(ButtonsMask), typeof(ClickGestureRecognizer), ButtonsMask.Primary);

		public ClickGestureRecognizer()
		{
		}

		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		public int NumberOfClicksRequired
		{
			get { return (int)GetValue(NumberOfClicksRequiredProperty); }
			set { SetValue(NumberOfClicksRequiredProperty, value); }
		}

		public ButtonsMask Buttons
		{
			get { return (ButtonsMask)GetValue(ButtonsProperty); }
			set { SetValue(ButtonsProperty, value); }
		}

		public event EventHandler Clicked;

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
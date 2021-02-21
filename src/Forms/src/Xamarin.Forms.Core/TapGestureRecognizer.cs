using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Xamarin.Forms
{
	public sealed class TapGestureRecognizer : GestureRecognizer
	{
		public static readonly BindableProperty CommandProperty = BindableProperty.Create("Command", typeof(ICommand), typeof(TapGestureRecognizer), null);

		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create("CommandParameter", typeof(object), typeof(TapGestureRecognizer), null);

		public static readonly BindableProperty NumberOfTapsRequiredProperty = BindableProperty.Create("NumberOfTapsRequired", typeof(int), typeof(TapGestureRecognizer), 1);

		public TapGestureRecognizer()
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

		public int NumberOfTapsRequired
		{
			get { return (int)GetValue(NumberOfTapsRequiredProperty); }
			set { SetValue(NumberOfTapsRequiredProperty, value); }
		}

		public event EventHandler Tapped;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendTapped(View sender)
		{
			ICommand cmd = Command;
			if (cmd != null && cmd.CanExecute(CommandParameter))
				cmd.Execute(CommandParameter);

			EventHandler handler = Tapped;
			if (handler != null)
				handler(sender, new TappedEventArgs(CommandParameter));
		}
	}
}
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/TapGestureRecognizer.xml" path="Type[@FullName='Microsoft.Maui.Controls.TapGestureRecognizer']/Docs" />
	public sealed class TapGestureRecognizer : GestureRecognizer
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/TapGestureRecognizer.xml" path="//Member[@MemberName='CommandProperty']/Docs" />
		public static readonly BindableProperty CommandProperty = BindableProperty.Create("Command", typeof(ICommand), typeof(TapGestureRecognizer), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/TapGestureRecognizer.xml" path="//Member[@MemberName='CommandParameterProperty']/Docs" />
		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create("CommandParameter", typeof(object), typeof(TapGestureRecognizer), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/TapGestureRecognizer.xml" path="//Member[@MemberName='NumberOfTapsRequiredProperty']/Docs" />
		public static readonly BindableProperty NumberOfTapsRequiredProperty = BindableProperty.Create("NumberOfTapsRequired", typeof(int), typeof(TapGestureRecognizer), 1);

		/// <include file="../../docs/Microsoft.Maui.Controls/TapGestureRecognizer.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public TapGestureRecognizer()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TapGestureRecognizer.xml" path="//Member[@MemberName='Command']/Docs" />
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TapGestureRecognizer.xml" path="//Member[@MemberName='CommandParameter']/Docs" />
		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TapGestureRecognizer.xml" path="//Member[@MemberName='NumberOfTapsRequired']/Docs" />
		public int NumberOfTapsRequired
		{
			get { return (int)GetValue(NumberOfTapsRequiredProperty); }
			set { SetValue(NumberOfTapsRequiredProperty, value); }
		}

		public event EventHandler Tapped;

		internal void SendTapped(View sender)
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
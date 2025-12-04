using System;
using System.Windows.Input;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/TapGestureRecognizer.xml" path="Type[@FullName='Microsoft.Maui.Controls.TapGestureRecognizer']/Docs/*" />
	public sealed class TapGestureRecognizer : GestureRecognizer
	{
		/// <summary>Bindable property for <see cref="Command"/>.</summary>
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(TapGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(TapGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="NumberOfTapsRequired"/>.</summary>
		public static readonly BindableProperty NumberOfTapsRequiredProperty = BindableProperty.Create(nameof(NumberOfTapsRequired), typeof(int), typeof(TapGestureRecognizer), 1);

		/// <summary>Bindable property for <see cref="Buttons"/>.</summary>
		public static readonly BindableProperty ButtonsProperty = BindableProperty.Create(nameof(Buttons), typeof(ButtonsMask), typeof(TapGestureRecognizer), ButtonsMask.Primary);

		/// <include file="../../docs/Microsoft.Maui.Controls/TapGestureRecognizer.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public TapGestureRecognizer()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TapGestureRecognizer.xml" path="//Member[@MemberName='Command']/Docs/*" />
		public ICommand? Command
		{
			get { return (ICommand?)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TapGestureRecognizer.xml" path="//Member[@MemberName='CommandParameter']/Docs/*" />
		public object? CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TapGestureRecognizer.xml" path="//Member[@MemberName='NumberOfTapsRequired']/Docs/*" />
		public int NumberOfTapsRequired
		{
			get { return (int)GetValue(NumberOfTapsRequiredProperty); }
			set { SetValue(NumberOfTapsRequiredProperty, value); }
		}

		public ButtonsMask Buttons
		{
			get { return (ButtonsMask)GetValue(ButtonsProperty); }
			set { SetValue(ButtonsProperty, value); }
		}

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
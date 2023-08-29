#nullable disable
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ClickGestureRecognizer.xml" path="Type[@FullName='Microsoft.Maui.Controls.ClickGestureRecognizer']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/ClickGestureRecognizer.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ClickGestureRecognizer()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ClickGestureRecognizer.xml" path="//Member[@MemberName='Command']/Docs/*" />
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ClickGestureRecognizer.xml" path="//Member[@MemberName='CommandParameter']/Docs/*" />
		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ClickGestureRecognizer.xml" path="//Member[@MemberName='NumberOfClicksRequired']/Docs/*" />
		public int NumberOfClicksRequired
		{
			get { return (int)GetValue(NumberOfClicksRequiredProperty); }
			set { SetValue(NumberOfClicksRequiredProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ClickGestureRecognizer.xml" path="//Member[@MemberName='Buttons']/Docs/*" />
		public ButtonsMask Buttons
		{
			get { return (ButtonsMask)GetValue(ButtonsProperty); }
			set { SetValue(ButtonsProperty, value); }
		}

		public event EventHandler Clicked;

		/// <include file="../../docs/Microsoft.Maui.Controls/ClickGestureRecognizer.xml" path="//Member[@MemberName='SendClicked']/Docs/*" />
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

#nullable disable
using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="Type[@FullName='Microsoft.Maui.Controls.TextCell']/Docs/*" />
	public class TextCell : Cell, ICommandElement
	{
		/// <summary>Bindable property for <see cref="Command"/>.</summary>
		public static readonly BindableProperty CommandProperty =
			BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(TextCell),
			propertyChanging: CommandElement.OnCommandChanging,
			propertyChanged: CommandElement.OnCommandChanged);

		/// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
		public static readonly BindableProperty CommandParameterProperty =
			BindableProperty.Create(nameof(CommandParameter),
				typeof(object),
				typeof(TextCell),
				null,
				propertyChanged: CommandElement.OnCommandParameterChanged);

		/// <summary>Bindable property for <see cref="Text"/>.</summary>
		public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(TextCell), default(string));

		/// <summary>Bindable property for <see cref="Detail"/>.</summary>
		public static readonly BindableProperty DetailProperty = BindableProperty.Create(nameof(Detail), typeof(string), typeof(TextCell), default(string));

		/// <summary>Bindable property for <see cref="TextColor"/>.</summary>
		public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(TextCell), null);

		/// <summary>Bindable property for <see cref="DetailColor"/>.</summary>
		public static readonly BindableProperty DetailColorProperty = BindableProperty.Create(nameof(DetailColor), typeof(Color), typeof(TextCell), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="//Member[@MemberName='Command']/Docs/*" />
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="//Member[@MemberName='CommandParameter']/Docs/*" />
		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="//Member[@MemberName='Detail']/Docs/*" />
		public string Detail
		{
			get { return (string)GetValue(DetailProperty); }
			set { SetValue(DetailProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="//Member[@MemberName='DetailColor']/Docs/*" />
		public Color DetailColor
		{
			get { return (Color)GetValue(DetailColorProperty); }
			set { SetValue(DetailColorProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="//Member[@MemberName='Text']/Docs/*" />
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="//Member[@MemberName='TextColor']/Docs/*" />
		public Color TextColor
		{
			get { return (Color)GetValue(TextColorProperty); }
			set { SetValue(TextColorProperty, value); }
		}

		protected internal override void OnTapped()
		{
			base.OnTapped();

			if (!IsEnabled)
			{
				return;
			}

			Command?.Execute(CommandParameter);
		}

		void ICommandElement.CanExecuteChanged(object sender, EventArgs eventArgs)
		{
			if (Command is null)
				return;

			IsEnabled = Command.CanExecute(CommandParameter);
		}

		WeakCommandSubscription ICommandElement.CleanupTracker { get; set; }
	}
}
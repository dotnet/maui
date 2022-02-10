using System;
using System.Windows.Input;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="Type[@FullName='Microsoft.Maui.Controls.TextCell']/Docs" />
	public class TextCell : Cell
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="//Member[@MemberName='CommandProperty']/Docs" />
		public static readonly BindableProperty CommandProperty = BindableProperty.Create("Command", typeof(ICommand), typeof(TextCell), default(ICommand),
			propertyChanging: (bindable, oldvalue, newvalue) =>
			{
				var textCell = (TextCell)bindable;
				var oldcommand = (ICommand)oldvalue;
				if (oldcommand != null)
					oldcommand.CanExecuteChanged -= textCell.OnCommandCanExecuteChanged;
			}, propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var textCell = (TextCell)bindable;
				var newcommand = (ICommand)newvalue;
				if (newcommand != null)
				{
					textCell.IsEnabled = newcommand.CanExecute(textCell.CommandParameter);
					newcommand.CanExecuteChanged += textCell.OnCommandCanExecuteChanged;
				}
			});

		/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="//Member[@MemberName='CommandParameterProperty']/Docs" />
		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create("CommandParameter", typeof(object), typeof(TextCell), default(object),
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var textCell = (TextCell)bindable;
				if (textCell.Command != null)
				{
					textCell.IsEnabled = textCell.Command.CanExecute(newvalue);
				}
			});

		/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="//Member[@MemberName='TextProperty']/Docs" />
		public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(TextCell), default(string));

		/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="//Member[@MemberName='DetailProperty']/Docs" />
		public static readonly BindableProperty DetailProperty = BindableProperty.Create("Detail", typeof(string), typeof(TextCell), default(string));

		/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="//Member[@MemberName='TextColorProperty']/Docs" />
		public static readonly BindableProperty TextColorProperty = BindableProperty.Create("TextColor", typeof(Color), typeof(TextCell), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="//Member[@MemberName='DetailColorProperty']/Docs" />
		public static readonly BindableProperty DetailColorProperty = BindableProperty.Create("DetailColor", typeof(Color), typeof(TextCell), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="//Member[@MemberName='Command']/Docs" />
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="//Member[@MemberName='CommandParameter']/Docs" />
		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="//Member[@MemberName='Detail']/Docs" />
		public string Detail
		{
			get { return (string)GetValue(DetailProperty); }
			set { SetValue(DetailProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="//Member[@MemberName='DetailColor']/Docs" />
		public Color DetailColor
		{
			get { return (Color)GetValue(DetailColorProperty); }
			set { SetValue(DetailColorProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="//Member[@MemberName='Text']/Docs" />
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/TextCell.xml" path="//Member[@MemberName='TextColor']/Docs" />
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

		void OnCommandCanExecuteChanged(object sender, EventArgs eventArgs)
		{
			IsEnabled = Command.CanExecute(CommandParameter);
		}
	}
}
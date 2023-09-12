#nullable disable
using System;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/BackButtonBehavior.xml" path="Type[@FullName='Microsoft.Maui.Controls.BackButtonBehavior']/Docs/*" />
	public class BackButtonBehavior : BindableObject
	{
		/// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
		public static readonly BindableProperty CommandParameterProperty =
			BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(BackButtonBehavior), null, BindingMode.OneTime,
				propertyChanged: OnCommandParameterChanged);

		/// <summary>Bindable property for <see cref="Command"/>.</summary>
		public static readonly BindableProperty CommandProperty =
			BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(BackButtonBehavior), null, BindingMode.OneTime,
				propertyChanged: OnCommandChanged);

		/// <summary>Bindable property for <see cref="IconOverride"/>.</summary>
		public static readonly BindableProperty IconOverrideProperty =
			BindableProperty.Create(nameof(IconOverride), typeof(ImageSource), typeof(BackButtonBehavior), null, BindingMode.OneTime);

		/// <summary>Bindable property for <see cref="IsEnabled"/>.</summary>
		public static readonly BindableProperty IsEnabledProperty =
			BindableProperty.Create(nameof(IsEnabled), typeof(bool), typeof(BackButtonBehavior), true, BindingMode.OneTime);

		/// <summary>Bindable property for <see cref="IsVisible"/>.</summary>
		public static readonly BindableProperty IsVisibleProperty =
			BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(BackButtonBehavior), true, BindingMode.OneTime);

		/// <summary>Bindable property for <see cref="TextOverride"/>.</summary>
		public static readonly BindableProperty TextOverrideProperty =
			BindableProperty.Create(nameof(TextOverride), typeof(string), typeof(BackButtonBehavior), null, BindingMode.OneTime);

		/// <include file="../../../docs/Microsoft.Maui.Controls/BackButtonBehavior.xml" path="//Member[@MemberName='Command']/Docs/*" />
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/BackButtonBehavior.xml" path="//Member[@MemberName='CommandParameter']/Docs/*" />
		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/BackButtonBehavior.xml" path="//Member[@MemberName='IconOverride']/Docs/*" />
		public ImageSource IconOverride
		{
			get { return (ImageSource)GetValue(IconOverrideProperty); }
			set { SetValue(IconOverrideProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/BackButtonBehavior.xml" path="//Member[@MemberName='IsEnabled']/Docs/*" />
		public bool IsEnabled
		{
			get { return (bool)GetValue(IsEnabledProperty); }
			set { SetValue(IsEnabledProperty, value); }
		}

		public bool IsVisible
		{
			get { return (bool)GetValue(IsVisibleProperty); }
			set { SetValue(IsVisibleProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/BackButtonBehavior.xml" path="//Member[@MemberName='TextOverride']/Docs/*" />
		public string TextOverride
		{
			get { return (string)GetValue(TextOverrideProperty); }
			set { SetValue(TextOverrideProperty, value); }
		}

		bool IsEnabledCore { set => SetValue(IsEnabledProperty, value); }

		static void OnCommandChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var self = (BackButtonBehavior)bindable;
			var oldCommand = (ICommand)oldValue;
			var newCommand = (ICommand)newValue;
			self.OnCommandChanged(oldCommand, newCommand);
		}

		static void OnCommandParameterChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((BackButtonBehavior)bindable).OnCommandParameterChanged();
		}

		void CanExecuteChanged(object sender, EventArgs e)
		{
			IsEnabledCore = Command.CanExecute(CommandParameter);
		}

		void OnCommandChanged(ICommand oldCommand, ICommand newCommand)
		{
			if (oldCommand != null)
			{
				oldCommand.CanExecuteChanged -= CanExecuteChanged;
			}

			if (newCommand != null)
			{
				newCommand.CanExecuteChanged += CanExecuteChanged;
				IsEnabledCore = Command.CanExecute(CommandParameter);
			}
			else
			{
				IsEnabledCore = true;
			}
		}

		void OnCommandParameterChanged()
		{
			if (Command != null)
				IsEnabledCore = Command.CanExecute(CommandParameter);
		}
	}
}

using System;
using System.Windows.Input;

namespace Xamarin.Forms
{
	public class BackButtonBehavior : BindableObject
	{
		public static readonly BindableProperty CommandParameterProperty =
			BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(BackButtonBehavior), null, BindingMode.OneTime,
				propertyChanged: OnCommandParameterChanged);

		public static readonly BindableProperty CommandProperty =
			BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(BackButtonBehavior), null, BindingMode.OneTime,
				propertyChanged: OnCommandChanged);

		public static readonly BindableProperty IconOverrideProperty =
			BindableProperty.Create(nameof(IconOverride), typeof(ImageSource), typeof(BackButtonBehavior), null, BindingMode.OneTime);

		public static readonly BindableProperty IsEnabledProperty =
			BindableProperty.Create(nameof(IsEnabled), typeof(bool), typeof(BackButtonBehavior), true, BindingMode.OneTime);

		public static readonly BindableProperty TextOverrideProperty =
			BindableProperty.Create(nameof(TextOverride), typeof(string), typeof(BackButtonBehavior), null, BindingMode.OneTime);

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

		public ImageSource IconOverride
		{
			get { return (ImageSource)GetValue(IconOverrideProperty); }
			set { SetValue(IconOverrideProperty, value); }
		}

		public bool IsEnabled
		{
			get { return (bool)GetValue(IsEnabledProperty); }
			set { SetValue(IsEnabledProperty, value); }
		}

		public string TextOverride
		{
			get { return (string)GetValue(TextOverrideProperty); }
			set { SetValue(TextOverrideProperty, value); }
		}

		bool IsEnabledCore { set => SetValueCore(IsEnabledProperty, value); }

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
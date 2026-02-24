#nullable disable
using System;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Customizes the appearance and behavior of the back button in a <see cref="Shell"/> application.
	/// </summary>
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
			BindableProperty.Create(nameof(IsEnabled), typeof(bool), typeof(BackButtonBehavior), true, BindingMode.OneWay);

		/// <summary>Bindable property for <see cref="IsVisible"/>.</summary>
		public static readonly BindableProperty IsVisibleProperty =
			BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(BackButtonBehavior), true, BindingMode.OneWay);

		/// <summary>Bindable property for <see cref="TextOverride"/>.</summary>
		public static readonly BindableProperty TextOverrideProperty =
			BindableProperty.Create(nameof(TextOverride), typeof(string), typeof(BackButtonBehavior), null, BindingMode.OneTime);

		/// <summary>
		/// Gets or sets the command to execute when the back button is pressed. This is a bindable property.
		/// </summary>
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <summary>
		/// Gets or sets the parameter to pass to the <see cref="Command"/>. This is a bindable property.
		/// </summary>
		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <summary>
		/// Gets or sets the icon to use instead of the default back button icon. This is a bindable property.
		/// </summary>
		public ImageSource IconOverride
		{
			get { return (ImageSource)GetValue(IconOverrideProperty); }
			set { SetValue(IconOverrideProperty, value); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the back button is enabled. This is a bindable property.
		/// </summary>
		public bool IsEnabled
		{
			get { return (bool)GetValue(IsEnabledProperty); }
			set { SetValue(IsEnabledProperty, value); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the back button is visible. This is a bindable property.
		/// </summary>
		public bool IsVisible
		{
			get { return (bool)GetValue(IsVisibleProperty); }
			set { SetValue(IsVisibleProperty, value); }
		}

		/// <summary>
		/// Gets or sets the text to display instead of the default back button text. This is a bindable property.
		/// </summary>
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

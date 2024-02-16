#nullable disable
using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/CheckBox.xml" path="Type[@FullName='Microsoft.Maui.Controls.CheckBox']/Docs/*" />
	public partial class CheckBox : View, IElementConfiguration<CheckBox>, IBorderElement, IColorElement, ICheckBox, ICommandElement
	{
		readonly Lazy<PlatformConfigurationRegistry<CheckBox>> _platformConfigurationRegistry;
		/// <include file="../../docs/Microsoft.Maui.Controls/CheckBox.xml" path="//Member[@MemberName='IsCheckedVisualState']/Docs/*" />
		public const string IsCheckedVisualState = "IsChecked";

		/// <summary>Bindable property for <see cref="IsChecked"/>.</summary>
		public static readonly BindableProperty IsCheckedProperty =
			BindableProperty.Create(nameof(IsChecked), typeof(bool), typeof(CheckBox), false,
				propertyChanged: (bindable, oldValue, newValue) =>
				{
					if (bindable is not CheckBox checkBox)
					{
						return;
					}

					checkBox.Handler?.UpdateValue(nameof(ICheckBox.Foreground));
					checkBox.CheckedChanged?.Invoke(bindable, new CheckedChangedEventArgs((bool)newValue));
					if (checkBox.Command is not null && checkBox.Command.CanExecute(null))
					{
						checkBox.Command?.Execute(checkBox.CommandParameter);
					}

					checkBox.ChangeVisualState();
				}, defaultBindingMode: BindingMode.TwoWay);


#pragma warning disable RS0016 // Add public types and members to the declared API
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(CheckBox), null, propertyChanging: CommandElement.OnCommandChanging, propertyChanged: CommandElement.OnCommandChanged);

		public ICommand Command
		{
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(CheckBox), null, propertyChanged: CommandElement.OnCommandParameterChanged);

		public object CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}



		/// <summary>Bindable property for <see cref="Color"/>.</summary>
		public static readonly BindableProperty ColorProperty = ColorElement.ColorProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/CheckBox.xml" path="//Member[@MemberName='Color']/Docs/*" />
		public Color Color
		{
			get => (Color)GetValue(ColorProperty);
			set => SetValue(ColorProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/CheckBox.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public CheckBox() => _platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<CheckBox>>(() => new PlatformConfigurationRegistry<CheckBox>(this));

		/// <include file="../../docs/Microsoft.Maui.Controls/CheckBox.xml" path="//Member[@MemberName='IsChecked']/Docs/*" />
		public bool IsChecked
		{
			get => (bool)GetValue(IsCheckedProperty);
			set => SetValue(IsCheckedProperty, value);
		}

		protected internal override void ChangeVisualState()
		{
			if (IsEnabled && IsChecked)
				VisualStateManager.GoToState(this, IsCheckedVisualState);
			else
				base.ChangeVisualState();
		}

		public event EventHandler<CheckedChangedEventArgs> CheckedChanged;

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, CheckBox> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void IBorderElement.OnBorderColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		Color IBorderElement.BorderColor => Colors.Transparent;
		int IBorderElement.CornerRadius => 0;
		double IBorderElement.BorderWidth => 0;
		int IBorderElement.CornerRadiusDefaultValue => 0;
		Color IBorderElement.BorderColorDefaultValue => Colors.Transparent;
		double IBorderElement.BorderWidthDefaultValue => 0;
		bool IBorderElement.IsCornerRadiusSet() => false;
		bool IBorderElement.IsBackgroundColorSet() => IsSet(BackgroundColorProperty);
		bool IBorderElement.IsBackgroundSet() => IsSet(BackgroundProperty);
		bool IBorderElement.IsBorderColorSet() => false;
		bool IBorderElement.IsBorderWidthSet() => false;

		public void CanExecuteChanged(object sender, EventArgs e)
		{
			RefreshIsEnabledProperty();
		}

#pragma warning restore RS0016 // Add public types and members to the declared API
		public Paint Foreground => Color?.AsPaint();

		bool ICheckBox.IsChecked
		{
			get => IsChecked;
			set => SetValue(IsCheckedProperty, value, SetterSpecificity.FromHandler);
		}
	}
}
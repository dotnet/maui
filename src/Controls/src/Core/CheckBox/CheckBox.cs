#nullable disable
using System;
using System.Diagnostics;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/CheckBox.xml" path="Type[@FullName='Microsoft.Maui.Controls.CheckBox']/Docs/*" />
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	public partial class CheckBox : View, IElementConfiguration<CheckBox>, IBorderElement, IColorElement, ICheckBox
	{
		readonly Lazy<PlatformConfigurationRegistry<CheckBox>> _platformConfigurationRegistry;
		/// <include file="../../docs/Microsoft.Maui.Controls/CheckBox.xml" path="//Member[@MemberName='IsCheckedVisualState']/Docs/*" />
		public const string IsCheckedVisualState = "IsChecked";

		/// <summary>Bindable property for <see cref="IsChecked"/>.</summary>
		public static readonly BindableProperty IsCheckedProperty =
			BindableProperty.Create(nameof(IsChecked), typeof(bool), typeof(CheckBox), false,
				propertyChanged: (bindable, oldValue, newValue) =>
				{
					((CheckBox)bindable).Handler?.UpdateValue(nameof(ICheckBox.Foreground));
					((CheckBox)bindable).CheckedChanged?.Invoke(bindable, new CheckedChangedEventArgs((bool)newValue));
					((CheckBox)bindable).ChangeVisualState();
				}, defaultBindingMode: BindingMode.TwoWay);

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
		public Paint Foreground => Color?.AsPaint();

		bool ICheckBox.IsChecked
		{
			get => IsChecked;
			set => SetValue(IsCheckedProperty, value, SetterSpecificity.FromHandler);
		}

		private protected override string GetDebuggerDisplay()
		{
			return $"{base.GetDebuggerDisplay()}, IsChecked = {IsChecked}";
		}
	}
}
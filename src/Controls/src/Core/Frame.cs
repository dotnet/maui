using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(Content))]
	public partial class Frame : ContentView, IElementConfiguration<Frame>, IPaddingElement, IBorderElement
	{
		public static readonly BindableProperty BorderColorProperty = BorderElement.BorderColorProperty;

		public static readonly BindableProperty HasShadowProperty = BindableProperty.Create("HasShadow", typeof(bool), typeof(Frame), true);

		public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float), typeof(Frame), -1.0f,
									validateValue: (bindable, value) => ((float)value) == -1.0f || ((float)value) >= 0f);

		readonly Lazy<PlatformConfigurationRegistry<Frame>> _platformConfigurationRegistry;

		public Frame()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Frame>>(() => new PlatformConfigurationRegistry<Frame>(this));
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator()
		{
			return 20d;
		}

		public bool HasShadow
		{
			get { return (bool)GetValue(HasShadowProperty); }
			set { SetValue(HasShadowProperty, value); }
		}

		public Color BorderColor
		{
			get { return (Color)GetValue(BorderElement.BorderColorProperty); }
			set { SetValue(BorderElement.BorderColorProperty, value); }
		}

		public float CornerRadius
		{
			get { return (float)GetValue(CornerRadiusProperty); }
			set { SetValue(CornerRadiusProperty, value); }
		}

		int IBorderElement.CornerRadius => (int)CornerRadius;

		// not currently used by frame
		double IBorderElement.BorderWidth => -1d;

		int IBorderElement.CornerRadiusDefaultValue => (int)CornerRadiusProperty.DefaultValue;

		Color IBorderElement.BorderColorDefaultValue => (Color)BorderColorProperty.DefaultValue;

		double IBorderElement.BorderWidthDefaultValue => -1d;

		public IPlatformElementConfiguration<T, Frame> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void IBorderElement.OnBorderColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		bool IBorderElement.IsCornerRadiusSet() => IsSet(CornerRadiusProperty);

		bool IBorderElement.IsBackgroundColorSet() => IsSet(BackgroundColorProperty);

		bool IBorderElement.IsBackgroundSet() => IsSet(BackgroundProperty);

		bool IBorderElement.IsBorderColorSet() => IsSet(BorderColorProperty);

		bool IBorderElement.IsBorderWidthSet() => false;
	}
}
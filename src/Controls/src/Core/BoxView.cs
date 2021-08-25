using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class BoxView : View, IColorElement, ICornerElement, IElementConfiguration<BoxView>
	{
		public static readonly BindableProperty ColorProperty = ColorElement.ColorProperty;

		public static readonly BindableProperty CornerRadiusProperty = CornerElement.CornerRadiusProperty;

		readonly Lazy<PlatformConfigurationRegistry<BoxView>> _platformConfigurationRegistry;

		public BoxView()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<BoxView>>(() => new PlatformConfigurationRegistry<BoxView>(this));
		}

		public Color Color
		{
			get => (Color)GetValue(ColorElement.ColorProperty);
			set => SetValue(ColorElement.ColorProperty, value);
		}

		public CornerRadius CornerRadius
		{
			get => (CornerRadius)GetValue(CornerElement.CornerRadiusProperty);
			set => SetValue(CornerElement.CornerRadiusProperty, value);
		}

		public IPlatformElementConfiguration<T, BoxView> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			return new SizeRequest(new Size(40, 40));
		}
	}
}
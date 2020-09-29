using System;
using System.ComponentModel;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_BoxViewRenderer))]
	public class BoxView : View, IColorElement, ICornerElement, IElementConfiguration<BoxView>
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

		[Obsolete("OnSizeRequest is obsolete as of version 2.2.0. Please use OnMeasure instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
		{
			return new SizeRequest(new Size(40, 40));
		}
	}
}
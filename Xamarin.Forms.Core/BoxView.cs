using System;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_BoxViewRenderer))]
	public class BoxView : View, IElementConfiguration<BoxView>
	{
		public static readonly BindableProperty ColorProperty = BindableProperty.Create("Color", typeof(Color), typeof(BoxView), Color.Default);

		readonly Lazy<PlatformConfigurationRegistry<BoxView>> _platformConfigurationRegistry;

		public BoxView()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<BoxView>>(() => new PlatformConfigurationRegistry<BoxView>(this));
		}

		public Color Color
		{
			get { return (Color)GetValue(ColorProperty); }
			set { SetValue(ColorProperty, value); }
		}

		public IPlatformElementConfiguration<T, BoxView> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		[Obsolete("Use OnMeasure")]
		protected override SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
		{
			return new SizeRequest(new Size(40, 40));
		}
	}
}
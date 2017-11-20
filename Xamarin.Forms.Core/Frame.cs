using System;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[ContentProperty("Content")]
	[RenderWith(typeof(_FrameRenderer))]
	public class Frame : ContentView, IElementConfiguration<Frame>, IPaddingElement
	{
		public static readonly BindableProperty OutlineColorProperty = BindableProperty.Create("OutlineColor", typeof(Color), typeof(Frame), Color.Default);

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

		public Color OutlineColor
		{
			get { return (Color)GetValue(OutlineColorProperty); }
			set { SetValue(OutlineColorProperty, value); }
		}

		public float CornerRadius
		{
			get { return (float)GetValue(CornerRadiusProperty); }
			set { SetValue(CornerRadiusProperty, value); }
		}

		public IPlatformElementConfiguration<T, Frame> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}
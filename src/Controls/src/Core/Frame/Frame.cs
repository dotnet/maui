#nullable disable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Frame.xml" path="Type[@FullName='Microsoft.Maui.Controls.Frame']/Docs/*" />
	[ContentProperty(nameof(Content))]
	public partial class Frame : ContentView, IElementConfiguration<Frame>, IPaddingElement, IBorderElement, IView, IContentView
	{
		/// <summary>Bindable property for <see cref="BorderColor"/>.</summary>
		public static readonly BindableProperty BorderColorProperty = BorderElement.BorderColorProperty;

		/// <summary>Bindable property for <see cref="HasShadow"/>.</summary>
		public static readonly BindableProperty HasShadowProperty = BindableProperty.Create(nameof(HasShadow), typeof(bool), typeof(Frame), true);

		/// <summary>Bindable property for <see cref="CornerRadius"/>.</summary>
		public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float), typeof(Frame), -1.0f,
									validateValue: (bindable, value) => ((float)value) == -1.0f || ((float)value) >= 0f);

		readonly Lazy<PlatformConfigurationRegistry<Frame>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/Frame.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Frame()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Frame>>(() => new PlatformConfigurationRegistry<Frame>(this));
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator()
		{
			return 20d;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Frame.xml" path="//Member[@MemberName='HasShadow']/Docs/*" />
		public bool HasShadow
		{
			get { return (bool)GetValue(HasShadowProperty); }
			set { SetValue(HasShadowProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Frame.xml" path="//Member[@MemberName='BorderColor']/Docs/*" />
		public Color BorderColor
		{
			get { return (Color)GetValue(BorderElement.BorderColorProperty); }
			set { SetValue(BorderElement.BorderColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Frame.xml" path="//Member[@MemberName='CornerRadius']/Docs/*" />
		public float CornerRadius
		{
			get { return (float)GetValue(CornerRadiusProperty); }
			set { SetValue(CornerRadiusProperty, value); }
		}

		int IBorderElement.CornerRadius => (int)CornerRadius;

		double IBorderElement.BorderWidth => 1;

		int IBorderElement.CornerRadiusDefaultValue => (int)CornerRadiusProperty.DefaultValue;

		Color IBorderElement.BorderColorDefaultValue => (Color)BorderColorProperty.DefaultValue;

		double IBorderElement.BorderWidthDefaultValue => ((IBorderElement)this).BorderWidth;

		/// <inheritdoc/>
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

		IShadow IView.Shadow
		{
			get
			{
				if (!HasShadow)
					return null;

				if (base.Shadow != null)
					return base.Shadow;

#if IOS
				// The way the shadow is applied in .NET MAUI on iOS is the same way it was applied in Forms
				// so on iOS we just return the shadow that was hard coded into the renderer
				// On Android it sets the elevation on the CardView and on WinUI Forms just ignored HasShadow
				if(HasShadow)
					return new Shadow() { Radius = 5, Opacity = 0.8f, Offset = new Point(0, 0), Brush = Brush.Black };
#endif

				return null;
			}
		}

		// TODO fix iOS/WinUI to work the same as Android
		// once this has been fixed on iOS/WinUI we should centralize 
		// this code and Border into LayoutExtensions
		Size ICrossPlatformLayout.CrossPlatformArrange(Graphics.Rect bounds)
		{
#if !WINDOWS
			bounds = bounds.Inset(((IBorderElement)this).BorderWidth); // Windows' implementation would cause an incorrect double-counting of the inset
#endif
			this.ArrangeContent(bounds);
			return bounds.Size;
		}

		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			var inset = Padding;
#if !WINDOWS
			inset += ((IBorderElement)this).BorderWidth; // Windows' implementation would cause an incorrect double-counting of the inset
#endif
			return this.MeasureContent(inset, widthConstraint, heightConstraint);
		}
	}

	internal static class FrameExtensions
	{
		internal static bool IsClippedToBoundsSet(this Frame frame, bool defaultValue) =>
			frame.IsSet(Compatibility.Layout.IsClippedToBoundsProperty) ? frame.IsClippedToBounds : defaultValue;
	}
}
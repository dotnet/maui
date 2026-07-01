#nullable disable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <summary>An element containing a single child, with some framing options.</summary>
	[ContentProperty(nameof(Content))]
	[Obsolete("Frame is obsolete as of .NET 9. Please use Border instead.")]
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

		/// <summary>Initializes a new instance of the Frame class.</summary>
		/// <remarks>A Frame has a default <see cref="Microsoft.Maui.Controls.Layout.Padding"/> of 20.</remarks>
		public Frame()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Frame>>(() => new PlatformConfigurationRegistry<Frame>(this));
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator()
		{
			return 20d;
		}

		/// <summary>Gets or sets a flag indicating if the Frame has a shadow displayed. This is a bindable property.</summary>
		public bool HasShadow
		{
			get { return (bool)GetValue(HasShadowProperty); }
			set { SetValue(HasShadowProperty, value); }
		}

		/// <summary>Gets or sets the border color for the frame. This is a bindable property.</summary>
		public Color BorderColor
		{
			get { return (Color)GetValue(BorderElement.BorderColorProperty); }
			set { SetValue(BorderElement.BorderColorProperty, value); }
		}

		/// <summary>Gets or sets the corner radius of the frame. This is a bindable property.</summary>
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
			if (BorderColor is not null)
				bounds = bounds.Inset(((IBorderElement)this).BorderWidth); // Windows' implementation would cause an incorrect double-counting of the inset
#endif
			this.ArrangeContent(bounds);
			return bounds.Size;
		}

		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			var inset = Padding;
#if !WINDOWS
			if (BorderColor is not null)
				inset += ((IBorderElement)this).BorderWidth; // Windows' implementation would cause an incorrect double-counting of the inset
#endif
			return this.MeasureContent(inset, widthConstraint, heightConstraint);
		}
	}

	[Obsolete("Frame is obsolete as of .NET 9. Please use Border instead.")]
	internal static class FrameExtensions
	{
		internal static bool IsClippedToBoundsSet(this Frame frame, bool defaultValue) =>
			frame.IsSet(Compatibility.Layout.IsClippedToBoundsProperty) ? frame.IsClippedToBounds : defaultValue;
	}
}
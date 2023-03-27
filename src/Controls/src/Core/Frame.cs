#nullable disable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Frame.xml" path="Type[@FullName='Microsoft.Maui.Controls.Frame']/Docs/*" />
	[ContentProperty(nameof(Content))]
	public partial class Frame : ContentView, IElementConfiguration<Frame>, IPaddingElement, IBorderElement, IContentView
	{
		/// <summary>Bindable property for <see cref="BorderColor"/>.</summary>
		public static readonly BindableProperty BorderColorProperty = BorderElement.BorderColorProperty;

		/// <summary>Bindable property for <see cref="HasShadow"/>.</summary>
		public static readonly BindableProperty HasShadowProperty = BindableProperty.Create("HasShadow", typeof(bool), typeof(Frame), true);

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

		// TODO fix iOS/WinUI to work the same as Android
#if ANDROID
		double IBorderElement.BorderWidth => 1;
#else
		// not currently used by frame
		double IBorderElement.BorderWidth => -1d;
#endif

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


#pragma warning disable RS0016 // Add public types and members to the declared API
		protected override Size ArrangeOverride(Rect bounds)
#pragma warning restore RS0016 // Add public types and members to the declared API
		{
			// This border thickness would need to get adjusted per platform
			// currently just hardcoding it to 1
			bounds = bounds.Inset(1);
			this.ArrangeContent(bounds);
			return base.ArrangeOverride(bounds);
		}

#pragma warning disable RS0016 // Add public types and members to the declared API
		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
#pragma warning restore RS0016 // Add public types and members to the declared API
		{
			// This border thickness would need to get adjusted per platform
			// currently just hardcoding it to 1
			var inset = Padding + 1;
			return this.MeasureContent(inset, widthConstraint, heightConstraint);
			//return base.MeasureOverride(widthConstraint, heightConstraint);
		}

		bool IBorderElement.IsCornerRadiusSet() => IsSet(CornerRadiusProperty);

		bool IBorderElement.IsBackgroundColorSet() => IsSet(BackgroundColorProperty);

		bool IBorderElement.IsBackgroundSet() => IsSet(BackgroundProperty);

		bool IBorderElement.IsBorderColorSet() => IsSet(BorderColorProperty);

		bool IBorderElement.IsBorderWidthSet() => false;

		// TODO fix iOS/WinUI to work the same as Android
		// once this has been fixed on iOS/WinUI we should centralize 
		// this code and Border into LayoutExtensions
		// WinUI being fixed as part of
		// https://github.com/dotnet/maui/issues/13552
#if ANDROID
		Size IContentView.CrossPlatformArrange(Graphics.Rect bounds)
		{
			bounds = bounds.Inset(((IBorderElement)this).BorderWidth);
			this.ArrangeContent(bounds);
			return bounds.Size;
		}

		Size IContentView.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			var inset = Padding + ((IBorderElement)this).BorderWidth;
			return this.MeasureContent(inset, widthConstraint, heightConstraint);
		}
#endif

	}

	internal static class FrameExtensions
	{
		internal static bool IsClippedToBoundsSet(this Frame frame, bool defaultValue) =>
			frame.IsSet(Compatibility.Layout.IsClippedToBoundsProperty) ? frame.IsClippedToBounds : defaultValue;
	}
}
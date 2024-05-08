#nullable disable
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/BoxView.xml" path="Type[@FullName='Microsoft.Maui.Controls.BoxView']/Docs/*" />
	public partial class BoxView : View, IColorElement, ICornerElement, IElementConfiguration<BoxView>, IShapeView, IShape
	{
		/// <summary>Bindable property for <see cref="Color"/>.</summary>
		public static readonly BindableProperty ColorProperty = ColorElement.ColorProperty;

		/// <summary>Bindable property for <see cref="CornerRadius"/>.</summary>
		public static readonly BindableProperty CornerRadiusProperty = CornerElement.CornerRadiusProperty;

		readonly Lazy<PlatformConfigurationRegistry<BoxView>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/BoxView.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public BoxView()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<BoxView>>(() => new PlatformConfigurationRegistry<BoxView>(this));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BoxView.xml" path="//Member[@MemberName='Color']/Docs/*" />
		public Color Color
		{
			get => (Color)GetValue(ColorElement.ColorProperty);
			set => SetValue(ColorElement.ColorProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BoxView.xml" path="//Member[@MemberName='CornerRadius']/Docs/*" />
		public CornerRadius CornerRadius
		{
			get => (CornerRadius)GetValue(CornerElement.CornerRadiusProperty);
			set => SetValue(CornerElement.CornerRadiusProperty, value);
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, BoxView> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			return new SizeRequest(new Size(40, 40));
		}

#nullable enable
		// Todo these shuold be moved to a mapper
		protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == BackgroundColorProperty.PropertyName ||
				propertyName == ColorProperty.PropertyName ||
				propertyName == BackgroundProperty.PropertyName ||
				propertyName == CornerRadiusProperty.PropertyName)
				Handler?.UpdateValue(nameof(IShapeView.Shape));
		}

		IShape? IShapeView.Shape => this;

		PathAspect IShapeView.Aspect => PathAspect.None;

		Paint? IShapeView.Fill => Color?.AsPaint();

		Paint? IStroke.Stroke => null;

		double IStroke.StrokeThickness => 0;

		LineCap IStroke.StrokeLineCap => LineCap.Butt;

		LineJoin IStroke.StrokeLineJoin => LineJoin.Miter;

		float[]? IStroke.StrokeDashPattern => null;

		float IStroke.StrokeDashOffset => 0f;

		float IStroke.StrokeMiterLimit => 0;

		PathF IShape.PathForBounds(Rect bounds)
		{
			var path = new PathF();

			path.AppendRoundedRectangle(
				bounds,
				(float)CornerRadius.TopLeft,
				(float)CornerRadius.TopRight,
				(float)CornerRadius.BottomLeft,
				(float)CornerRadius.BottomRight);

			return path;
		}
#nullable disable

	}
}
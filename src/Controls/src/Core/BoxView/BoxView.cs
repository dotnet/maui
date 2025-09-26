#nullable disable
using System;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A <see cref="View" /> used to draw a solid colored rectangle.
	/// </summary>
	public partial class BoxView : View, IColorElement, ICornerElement, IElementConfiguration<BoxView>, IShapeView, IShape
	{
		WeakBrushChangedProxy _fillProxy = null;
		EventHandler _fillChanged;

		/// <summary>Bindable property for <see cref="Color"/>.</summary>
		public static readonly BindableProperty ColorProperty = ColorElement.ColorProperty;

		/// <summary>Bindable property for <see cref="CornerRadius"/>.</summary>
		public static readonly BindableProperty CornerRadiusProperty = CornerElement.CornerRadiusProperty;

		/// <summary>Bindable property for <see cref="Color"/>.</summary>
		public static readonly BindableProperty FillProperty =
			BindableProperty.Create(nameof(Fill), typeof(Brush), typeof(BoxView), null,
				propertyChanging: (bindable, oldvalue, newvalue) =>
				{
					if (oldvalue != null)
					{
						(bindable as BoxView)?.StopNotifyingFillChanges();
					}
				},
				propertyChanged: (bindable, oldvalue, newvalue) =>
				{
					if (newvalue != null)
					{
						(bindable as BoxView)?.NotifyFillChanges();
					}
				});

		readonly Lazy<PlatformConfigurationRegistry<BoxView>> _platformConfigurationRegistry;

		/// <summary>
		/// Initializes a new instance of the BoxView class.
		/// </summary>
		public BoxView()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<BoxView>>(() => new PlatformConfigurationRegistry<BoxView>(this));
		}

		~BoxView()
		{
			_fillProxy?.Unsubscribe();
		}

		/// <summary>
		/// Gets or sets the color which will fill the rectangle. This is a bindable property.
		/// </summary>
		/// <value>The color that is used to fill the rectangle.</value>
		public Color Color
		{
			get => (Color)GetValue(ColorElement.ColorProperty);
			set => SetValue(ColorElement.ColorProperty, value);
		}

		/// <summary>
		/// Gets or sets the brush that fills the interior of the BoxView.
		/// </summary>
		/// <value>
		/// A <see cref="Brush"/> object that describes how the BoxView's interior is painted.
		/// The default value is <see langword="null"/>.
		/// </value>
		public Brush Fill
		{
			get => (Brush)GetValue(FillProperty);
			set => SetValue(FillProperty, value);
		}

		/// <summary>
		/// Gets or sets the corner radius for the box view.
		/// </summary>
		/// <value>The corner radius for the box view.</value>
		/// <remarks>When specifying corner radii, the order of values is top left, top right, bottom left, and bottom right.</remarks>
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

		[Obsolete("Use MeasureOverride instead")]
		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			return new SizeRequest(new Size(40, 40));
		}

		void NotifyFillChanges()
		{
			var fill = Fill;

			if (fill is ImmutableBrush)
			{
				return;
			}

			if (fill is not null)
			{
				SetInheritedBindingContext(fill, BindingContext);
				_fillChanged ??= (sender, e) => OnPropertyChanged(nameof(Fill));
				_fillProxy ??= new();
				_fillProxy.Subscribe(fill, _fillChanged);

				OnParentResourcesChanged(this.GetMergedResources());
				((IElementDefinition)this).AddResourcesChangedListener(fill.OnParentResourcesChanged);
			}
		}

		void StopNotifyingFillChanges()
		{
			var fill = Fill;

			if (fill is ImmutableBrush)
			{
				return;
			}

			if (fill is not null)
			{
				((IElementDefinition)this).RemoveResourcesChangedListener(fill.OnParentResourcesChanged);

				SetInheritedBindingContext(fill, null);
				_fillProxy?.Unsubscribe();
			}
		}

#nullable enable
		// Todo these shuold be moved to a mapper
		protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == BackgroundColorProperty.PropertyName ||
				propertyName == ColorProperty.PropertyName ||
				propertyName == IsVisibleProperty.PropertyName ||
				propertyName == BackgroundProperty.PropertyName ||
				propertyName == CornerRadiusProperty.PropertyName)
			{
				Handler?.UpdateValue(nameof(IShapeView.Shape));
			}
		}

		IShape? IShapeView.Shape => this;

		PathAspect IShapeView.Aspect => PathAspect.None;

		Paint? IShapeView.Fill => Fill ?? Color?.AsPaint();

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
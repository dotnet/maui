using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A container control that draws a border, background, or both around its child content.
	/// </summary>
	/// <remarks>
	/// Border provides a way to add visual decoration around any content. You can customize the stroke (border), 
	/// background, shape, padding, and more to create visually rich containers.
	/// </remarks>
	[ContentProperty(nameof(Content))]
	[ElementHandler(typeof(BorderHandler))]
	public class Border : View, IContentView, IBorderView, IPaddingElement, ISafeAreaElement, ISafeAreaView2
	{
		float[]? _strokeDashPattern;

		WeakNotifyPropertyChangedProxy? _strokeShapeProxy = null;
		PropertyChangedEventHandler? _strokeShapeChanged;
		WeakNotifyPropertyChangedProxy? _strokeProxy = null;
		PropertyChangedEventHandler? _strokeChanged;

		~Border()
		{
			_strokeShapeProxy?.Unsubscribe();
			_strokeProxy?.Unsubscribe();
		}

		/// <summary>Bindable property for <see cref="Content"/>.</summary>
		public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View),
			typeof(Border), null, propertyChanged: ContentChanged);

		/// <summary>Bindable property for <see cref="Padding"/>.</summary>
		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		/// <summary>Bindable property for <see cref="SafeAreaEdges"/>.</summary>
		public static readonly BindableProperty SafeAreaEdgesProperty = SafeAreaElement.SafeAreaEdgesProperty;

		/// <summary>
		/// Gets or sets the child content that is placed inside the border. This is a bindable property.
		/// </summary>
		/// <value>A <see cref="View"/> that contains the child content, or <see langword="null"/> if no content is set.</value>
		public View? Content
		{
			get { return (View?)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		/// <summary>
		/// Gets or sets the padding inside the border. This is a bindable property.
		/// </summary>
		public Thickness Padding
		{
			get => (Thickness)GetValue(PaddingElement.PaddingProperty);
			set => SetValue(PaddingElement.PaddingProperty, value);
		}

		/// <summary>
		/// Gets or sets the safe area edges to obey for this border.
		/// The default value is SafeAreaEdges.Default (None - edge to edge).
		/// </summary>
		/// <remarks>
		/// This property controls which edges of the border should obey safe area insets.
		/// Use SafeAreaRegions.None for edge-to-edge content, SafeAreaRegions.All to obey all safe area insets, 
		/// SafeAreaRegions.Container for content that flows under keyboard but stays out of bars/notch, or SafeAreaRegions.Keyboard for keyboard-aware behavior.
		/// </remarks>
		public SafeAreaEdges SafeAreaEdges
		{
			get => (SafeAreaEdges)GetValue(SafeAreaElement.SafeAreaEdgesProperty);
			set => SetValue(SafeAreaElement.SafeAreaEdgesProperty, value);
		}

		/// <summary>Bindable property for <see cref="StrokeShape"/>.</summary>
		public static readonly BindableProperty StrokeShapeProperty =
			BindableProperty.Create(nameof(StrokeShape), typeof(IShape), typeof(Border), new Rectangle(),
				propertyChanging: (bindable, oldvalue, newvalue) =>
				{
					if (oldvalue is not null)
						(bindable as Border)?.StopNotifyingStrokeShapeChanges();
				},
				propertyChanged: (bindable, oldvalue, newvalue) =>
				{
					if (newvalue is not null)
						(bindable as Border)?.NotifyStrokeShapeChanges();
				});

		void NotifyStrokeShapeChanges()
		{
			var strokeShape = StrokeShape;

			if (strokeShape is VisualElement visualElement)
			{
				AddLogicalChild(visualElement);
				_strokeShapeChanged ??= (sender, e) =>
				{
					if (e.PropertyName != nameof(Window) &&
						e.PropertyName != nameof(Parent))
					{
						OnPropertyChanged(nameof(StrokeShape));
					}
				};
				_strokeShapeProxy ??= new();
				_strokeShapeProxy.Subscribe(visualElement, _strokeShapeChanged);
			}
		}

		void StopNotifyingStrokeShapeChanges()
		{
			var strokeShape = StrokeShape;

			if (strokeShape is VisualElement visualElement)
			{
				RemoveLogicalChild(visualElement);
				_strokeShapeProxy?.Unsubscribe();
			}
		}

		/// <summary>Bindable property for <see cref="Stroke"/>.</summary>
		public static readonly BindableProperty StrokeProperty =
			BindableProperty.Create(nameof(Stroke), typeof(Brush), typeof(Border), null,
				propertyChanging: (bindable, oldvalue, newvalue) =>
				{
					if (oldvalue is not null)
						(bindable as Border)?.StopNotifyingStrokeChanges();
				},
				propertyChanged: (bindable, oldvalue, newvalue) =>
				{
					if (newvalue is not null)
						(bindable as Border)?.NotifyStrokeChanges();
				});

		void NotifyStrokeChanges()
		{
			var stroke = Stroke;

			if (stroke is ImmutableBrush)
				return;

			if (stroke is not null)
			{
				SetInheritedBindingContext(stroke, BindingContext);
				_strokeChanged ??= (sender, e) => OnPropertyChanged(nameof(Stroke));
				_strokeProxy ??= new();
				_strokeProxy.Subscribe(stroke, _strokeChanged);

				OnParentResourcesChanged(this.GetMergedResources());
				((IElementDefinition)this).AddResourcesChangedListener(stroke.OnParentResourcesChanged);
			}
		}

		void StopNotifyingStrokeChanges()
		{
			var stroke = Stroke;

			if (stroke is ImmutableBrush)
				return;

			if (stroke is not null)
			{
				((IElementDefinition)this).RemoveResourcesChangedListener(stroke.OnParentResourcesChanged);

				SetInheritedBindingContext(stroke, null);
				_strokeProxy?.Unsubscribe();
			}
		}

		/// <summary>Bindable property for <see cref="StrokeThickness"/>.</summary>
		public static readonly BindableProperty StrokeThicknessProperty =
			BindableProperty.Create(nameof(StrokeThickness), typeof(double), typeof(Border), 1.0, propertyChanged: StrokeThicknessChanged);

		/// <summary>Bindable property for <see cref="StrokeDashArray"/>.</summary>
		public static readonly BindableProperty StrokeDashArrayProperty =
			BindableProperty.Create(nameof(StrokeDashArray), typeof(DoubleCollection), typeof(Border), null,
				defaultValueCreator: bindable => new DoubleCollection());

		/// <summary>Bindable property for <see cref="StrokeDashOffset"/>.</summary>
		public static readonly BindableProperty StrokeDashOffsetProperty =
			BindableProperty.Create(nameof(StrokeDashOffset), typeof(double), typeof(Border), 0.0);

		/// <summary>Bindable property for <see cref="StrokeLineCap"/>.</summary>
		public static readonly BindableProperty StrokeLineCapProperty =
			BindableProperty.Create(nameof(StrokeLineCap), typeof(PenLineCap), typeof(Border), PenLineCap.Flat);

		/// <summary>Bindable property for <see cref="StrokeLineJoin"/>.</summary>
		public static readonly BindableProperty StrokeLineJoinProperty =
			BindableProperty.Create(nameof(StrokeLineJoin), typeof(PenLineJoin), typeof(Border), PenLineJoin.Miter);

		/// <summary>Bindable property for <see cref="StrokeMiterLimit"/>.</summary>
		public static readonly BindableProperty StrokeMiterLimitProperty =
			BindableProperty.Create(nameof(StrokeMiterLimit), typeof(double), typeof(Border), 10.0);

		/// <summary>
		/// Gets or sets the shape of the border. This is a bindable property.
		/// </summary>
		/// <value>An <see cref="IShape"/> that defines the border shape, or <see langword="null"/> for the default <see cref="Rectangle"/> shape.</value>
		/// <remarks>
		/// The default value is a <see cref="Rectangle"/>. You can set this to other shapes like <see cref="RoundRectangle"/>, 
		/// <see cref="Ellipse"/>, or any custom <see cref="IShape"/> implementation to change the border's appearance.
		/// </remarks>
		[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
		public IShape? StrokeShape
		{
			set { SetValue(StrokeShapeProperty, value); }
			get { return (IShape?)GetValue(StrokeShapeProperty); }
		}

		/// <summary>
		/// Gets or sets the brush used to paint the border stroke. This is a bindable property.
		/// </summary>
		/// <value>A <see cref="Brush"/> used for the border stroke, or <see langword="null"/> if no stroke is set.</value>
		public Brush? Stroke
		{
			set { SetValue(StrokeProperty, value); }
			get { return (Brush?)GetValue(StrokeProperty); }
		}

		/// <summary>
		/// Gets or sets the thickness of the border stroke. This is a bindable property.
		/// </summary>
		/// <value>The stroke thickness in device-independent units. The default is 1.0.</value>
		public double StrokeThickness
		{
			set { SetValue(StrokeThicknessProperty, value); }
			get { return (double)GetValue(StrokeThicknessProperty); }
		}

		/// <summary>
		/// Gets or sets the pattern of dashes and gaps used to outline the border. This is a bindable property.
		/// </summary>
		/// <value>A <see cref="DoubleCollection"/> containing the dash pattern, or <see langword="null"/> for a solid line.</value>
		public DoubleCollection? StrokeDashArray
		{
			set { SetValue(StrokeDashArrayProperty, value); }
			get { return (DoubleCollection?)GetValue(StrokeDashArrayProperty); }
		}

		/// <summary>
		/// Gets or sets the distance within the dash pattern where a dash begins. This is a bindable property.
		/// </summary>
		/// <value>A <see cref="double"/> representing the offset in device-independent units. The default is 0.0.</value>
		public double StrokeDashOffset
		{
			set { SetValue(StrokeDashOffsetProperty, value); }
			get { return (double)GetValue(StrokeDashOffsetProperty); }
		}

		/// <summary>
		/// Gets or sets the shape at the start and end of the border stroke. This is a bindable property.
		/// </summary>
		/// <value>A <see cref="PenLineCap"/> value. The default is <see cref="PenLineCap.Flat"/>.</value>
		public PenLineCap StrokeLineCap
		{
			set { SetValue(StrokeLineCapProperty, value); }
			get { return (PenLineCap)GetValue(StrokeLineCapProperty); }
		}

		/// <summary>
		/// Gets or sets the type of join that is used at the vertices of the border stroke. This is a bindable property.
		/// </summary>
		/// <value>A <see cref="PenLineJoin"/> value. The default is <see cref="PenLineJoin.Miter"/>.</value>
		public PenLineJoin StrokeLineJoin
		{
			set { SetValue(StrokeLineJoinProperty, value); }
			get { return (PenLineJoin)GetValue(StrokeLineJoinProperty); }
		}

		/// <summary>
		/// Gets or sets the limit on the ratio of the miter length to half the stroke thickness. This is a bindable property.
		/// </summary>
		/// <value>The default is 10.0.</value>
		public double StrokeMiterLimit
		{
			set { SetValue(StrokeMiterLimitProperty, value); }
			get { return (double)GetValue(StrokeMiterLimitProperty); }
		}

		IShape? IBorderStroke.Shape => StrokeShape;

		Paint? IStroke.Stroke => Stroke;

		LineCap IStroke.StrokeLineCap =>
			StrokeLineCap switch
			{
				PenLineCap.Flat => LineCap.Butt,
				PenLineCap.Round => LineCap.Round,
				PenLineCap.Square => LineCap.Square,
				_ => LineCap.Butt
			};

		LineJoin IStroke.StrokeLineJoin =>
			StrokeLineJoin switch
			{
				PenLineJoin.Round => LineJoin.Round,
				PenLineJoin.Bevel => LineJoin.Bevel,
				PenLineJoin.Miter => LineJoin.Miter,
				_ => LineJoin.Round
			};

		/// <summary>
		/// Gets the stroke dash pattern as a float array for platform rendering.
		/// </summary>
		/// <value>A float array containing the dash pattern values, or <see langword="null"/> for a solid line.</value>
		public float[]? StrokeDashPattern
		{
			get
			{
				if (StrokeDashArray is INotifyCollectionChanged oldCollection)
					oldCollection.CollectionChanged -= OnStrokeDashArrayChanged;

				_strokeDashPattern = StrokeDashArray?.ToFloatArray();

				if (StrokeDashArray is INotifyCollectionChanged newCollection)
					newCollection.CollectionChanged += OnStrokeDashArrayChanged;

				return _strokeDashPattern;
			}
		}

		float IStroke.StrokeDashOffset => (float)StrokeDashOffset;

		float IStroke.StrokeMiterLimit => (float)StrokeMiterLimit;


		object? IContentView.Content => Content;

		IView? IContentView.PresentedContent => Content;

		/// <summary>
		/// Arranges the child content within the specified bounds, accounting for stroke thickness.
		/// </summary>
		/// <param name="bounds">The available bounds for the border.</param>
		/// <returns>The actual size used by the border.</returns>
		public Size CrossPlatformArrange(Graphics.Rect bounds)
		{
			var inset = bounds.Inset(StrokeThickness);
			this.ArrangeContent(inset);
			return bounds.Size;
		}

		/// <summary>
		/// Measures the border and its content with the given constraints.
		/// </summary>
		/// <param name="widthConstraint">The available width.</param>
		/// <param name="heightConstraint">The available height.</param>
		/// <returns>The desired size of the border.</returns>
		public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			var inset = Padding + StrokeThickness;
			return this.MeasureContent(inset, widthConstraint, heightConstraint);
		}

		/// <summary>
		/// Called when the <see cref="Content"/> property changes.
		/// </summary>
		/// <param name="bindable">The border instance.</param>
		/// <param name="oldValue">The old content value.</param>
		/// <param name="newValue">The new content value.</param>
		public static void ContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is Border border)
			{
				if (oldValue is Element oldElement)
				{
					border.RemoveLogicalChild(oldElement);
				}

				if (newValue is Element newElement)
				{
					border.AddLogicalChild(newElement);
				}
			}

			((IBorderView)bindable).InvalidateMeasure();
		}

		/// <summary>
		/// Called when the <see cref="StrokeThickness"/> property changes.
		/// </summary>
		/// <param name="bindable">The border instance.</param>
		/// <param name="oldValue">The old stroke thickness value.</param>
		/// <param name="newValue">The new stroke thickness value.</param>
		public static void StrokeThicknessChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((IBorderView)bindable).InvalidateMeasure();
		}

		/// <summary>
		/// Called when the <see cref="Padding"/> property changes.
		/// </summary>
		/// <param name="oldValue">The old padding value.</param>
		/// <param name="newValue">The new padding value.</param>
		public void OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue)
		{
			(this as IBorderView).InvalidateMeasure();
		}

		/// <summary>
		/// Provides the default value for the <see cref="Padding"/> property.
		/// </summary>
		/// <returns>The default padding of <see cref="Thickness.Zero"/>.</returns>
		public Thickness PaddingDefaultValueCreator()
		{
			return Thickness.Zero;
		}

		/// <inheritdoc/>
		protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == StrokeThicknessProperty.PropertyName || propertyName == StrokeShapeProperty.PropertyName)
			{
				UpdateStrokeShape();
				Handler?.UpdateValue(nameof(IBorderStroke.Shape));
			}
			else if (propertyName == StrokeDashArrayProperty.PropertyName)
			{
				Handler?.UpdateValue(nameof(IBorderStroke.StrokeDashPattern));
			}
		}

		void OnStrokeDashArrayChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			Handler?.UpdateValue(nameof(IBorderStroke.StrokeDashPattern));
		}

		void UpdateStrokeShape()
		{
			if (StrokeShape is Shape strokeShape && StrokeThickness == 0)
			{
				strokeShape.StrokeThickness = StrokeThickness;
			}
		}

		/// <inheritdoc cref="ISafeAreaView2.GetSafeAreaRegionsForEdge"/>
		SafeAreaRegions ISafeAreaView2.GetSafeAreaRegionsForEdge(int edge)
		{
			// Use direct property
			var regionForEdge = SafeAreaEdges.GetEdge(edge);

			if (regionForEdge == SafeAreaRegions.Default)
			{
				// If no safe area edges are set, return None
				return SafeAreaRegions.None;
			}

			// For Border, return as-is
			return regionForEdge;
		}

		/// <inheritdoc cref="ISafeAreaView2.SafeAreaInsets"/>
		Thickness ISafeAreaView2.SafeAreaInsets { set { } } // Default no-op implementation for borders

		/// <summary>
		/// Provides the default value for the <see cref="SafeAreaEdges"/> property.
		/// </summary>
		/// <returns>The default safe area edges of <see cref="SafeAreaEdges.None"/>.</returns>
		SafeAreaEdges ISafeAreaElement.SafeAreaEdgesDefaultValueCreator()
		{
			return SafeAreaEdges.None;
		}
	}
}
#nullable enable
using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="Type[@FullName='Microsoft.Maui.Controls.VisualElement']/Docs" />
	public partial class VisualElement : IView
	{
		Semantics _semantics;

		/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Frame']/Docs" />
		public Rectangle Frame
		{
			get => Bounds;
			set
			{
				X = value.X;
				Y = value.Y;
				Width = value.Width;
				Height = value.Height;
			}
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Handler']/Docs" />
		new public IViewHandler? Handler
		{
			get => (IViewHandler?)base.Handler;
			set => base.Handler = value;
		}

		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();

			IsPlatformEnabled = Handler != null;
		}

		Paint? IView.Background
		{
			get
			{
				if (!Brush.IsNullOrEmpty(Background))
					return Background;
				if (BackgroundColor.IsNotDefault())
					return new SolidColorBrush(BackgroundColor);

				return null;
			}
		}

		IShape IView.Clip => Clip;

		IShadow IView.Shadow => Shadow;

		/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='ShadowProperty']/Docs" />
		public static readonly BindableProperty ShadowProperty =
 			BindableProperty.Create(nameof(Shadow), typeof(Shadow), typeof(VisualElement), defaultValue: null,
				propertyChanging: (bindable, oldvalue, newvalue) =>
				{
					if (oldvalue != null)
						(bindable as VisualElement)?.StopNotifyingShadowChanges();
				},
				propertyChanged: (bindable, oldvalue, newvalue) =>
				{
					if (newvalue != null)
						(bindable as VisualElement)?.NotifyShadowChanges();
				});

		/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Shadow']/Docs" />
		public Shadow Shadow
		{
			get { return (Shadow)GetValue(ShadowProperty); }
			set { SetValue(ShadowProperty, value); }
		}

		internal static readonly BindableProperty ZIndexProperty =
			BindableProperty.Create(nameof(ZIndex), typeof(int), typeof(View), default(int),
				propertyChanged: ZIndexPropertyChanged);

		static void ZIndexPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is IView view)
			{
				view.Handler?.Invoke(nameof(IView.ZIndex));
			}
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='ZIndex']/Docs" />
		public int ZIndex
		{
			get { return (int)GetValue(ZIndexProperty); }
			set { SetValue(ZIndexProperty, value); }
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='DesiredSize']/Docs" />
		public Size DesiredSize { get; protected set; }

		/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Arrange']/Docs" />
		public void Arrange(Rectangle bounds)
		{
			Layout(bounds);
		}

		Size IView.Arrange(Rectangle bounds)
		{
			return ArrangeOverride(bounds);
		}

		// ArrangeOverride provides a way to allow subclasses (e.g., ScrollView) to override Arrange even though
		// the interface has to be explicitly implemented to avoid conflict with the old Arrange method
		protected virtual Size ArrangeOverride(Rectangle bounds)
		{
			Frame = this.ComputeFrame(bounds);
			Handler?.NativeArrange(Frame);
			return Frame.Size;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Layout']/Docs" />
		public void Layout(Rectangle bounds)
		{
			Bounds = bounds;
		}

		void IView.InvalidateMeasure()
		{
			InvalidateMeasureOverride();
		}

		// InvalidateMeasureOverride provides a way to allow subclasses (e.g., Layout) to override InvalidateMeasure even though
		// the interface has to be explicitly implemented to avoid conflict with the VisualElement.InvalidateMeasure method
		protected virtual void InvalidateMeasureOverride() => Handler?.Invoke(nameof(IView.InvalidateMeasure));

		void IView.InvalidateArrange()
		{
		}

		Size IView.Measure(double widthConstraint, double heightConstraint)
		{
			return MeasureOverride(widthConstraint, heightConstraint);
		}

		// MeasureOverride provides a way to allow subclasses (e.g., Layout) to override Measure even though
		// the interface has to be explicitly implemented to avoid conflict with the old Measure method
		protected virtual Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			DesiredSize = this.ComputeDesiredSize(widthConstraint, heightConstraint);
			return DesiredSize;
		}

		Maui.FlowDirection IView.FlowDirection
			=> ((IFlowDirectionController)this).EffectiveFlowDirection.ToFlowDirection();

		Primitives.LayoutAlignment IView.HorizontalLayoutAlignment => default;
		Primitives.LayoutAlignment IView.VerticalLayoutAlignment => default;

		Visibility IView.Visibility => IsVisible.ToVisibility();

		Semantics IView.Semantics
		{
			get => _semantics;
		}

		// We don't want to initialize Semantics until someone explicitly 
		// wants to modify some aspect of the semantics class
		internal Semantics SetupSemantics() =>
			_semantics ??= new Semantics();

		static void ValidatePositive(double value, string name)
		{
			if (value < 0)
			{
				throw new InvalidOperationException($"{name} cannot be less than zero.");
			}
		}

		double IView.Width
		{
			get
			{
				if (!IsSet(WidthRequestProperty))
				{
					return Primitives.Dimension.Unset;
				}

				// Access once up front to avoid multiple GetValue calls
				var value = WidthRequest;
				ValidatePositive(value, nameof(IView.Width));
				return value;
			}
		}

		double IView.Height
		{
			get
			{
				if (!IsSet(HeightRequestProperty))
				{
					return Primitives.Dimension.Unset;
				}

				// Access once up front to avoid multiple GetValue calls
				var value = HeightRequest;
				ValidatePositive(value, nameof(IView.Height));
				return value;
			}
		}

		double IView.MinimumWidth
		{
			get
			{
				if (!IsSet(MinimumWidthRequestProperty))
				{
					return Primitives.Dimension.Minimum;
				}

				// Access once up front to avoid multiple GetValue calls
				var value = MinimumWidthRequest;
				ValidatePositive(value, nameof(IView.MinimumWidth));
				return value;
			}
		}

		double IView.MinimumHeight
		{
			get
			{
				if (!IsSet(MinimumHeightRequestProperty))
				{
					return Primitives.Dimension.Minimum;
				}

				// Access once up front to avoid multiple GetValue calls
				var value = MinimumHeightRequest;
				ValidatePositive(value, nameof(IView.MinimumHeight));
				return value;
			}
		}

		double IView.MaximumWidth
		{
			get
			{
				// Access once up front to avoid multiple GetValue calls
				var value = MaximumWidthRequest;
				ValidatePositive(value, nameof(IView.MaximumWidth));
				return value;
			}
		}

		double IView.MaximumHeight
		{
			get
			{
				// Access once up front to avoid multiple GetValue calls
				var value = MaximumHeightRequest;
				ValidatePositive(value, nameof(IView.MaximumHeight));
				return value;
			}
		}

		Thickness IView.Margin => Thickness.Zero;

		IElementHandler? Maui.IElement.Handler
		{
			get => base.Handler;
			set
			{
				if (value != null && value is not IViewHandler)
					throw new InvalidOperationException("Handler must be of type IViewHandler");

				base.Handler = value;
			}
		}

		void NotifyShadowChanges()
		{
			if (Shadow != null)
			{
				Shadow.Parent = this;
				Shadow.PropertyChanged += OnShadowChanged;
			}
		}

		void StopNotifyingShadowChanges()
		{
			if (Shadow != null)
			{
				Shadow.Parent = null;
				Shadow.PropertyChanged -= OnShadowChanged;

			}
		}

		void OnShadowChanged(object? sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(nameof(Shadow));
		}

		void PropagateBindingContextToShadow()
		{
			if (Shadow != null)
				SetInheritedBindingContext(Shadow, BindingContext);
		}
	}
}
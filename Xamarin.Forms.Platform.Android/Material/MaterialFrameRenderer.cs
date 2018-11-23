#if __ANDROID81__
#else
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.FastRenderers;
using Xamarin.Forms.Platform.Android.Material;
using MaterialCardView = Android.Support.Design.Card.MaterialCardView;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;

// this won't go here permanently it's just for testing at this point
[assembly: ExportRenderer(typeof(Xamarin.Forms.Frame), typeof(MaterialFrameRenderer), new[] { typeof(VisualRendererMarker.Material) })]
namespace Xamarin.Forms.Platform.Android.Material
{
	public class MaterialFrameRenderer : MaterialCardView, IVisualElementRenderer, IEffectControlProvider, IViewRenderer, ITabStop
	{
		float _defaultElevation = -1f;
		float _defaultCornerRadius = -1f;
		int? _defaultLabelFor;

		bool _disposed;
		Frame _element;

		VisualElementPackager _visualElementPackager;
		VisualElementTracker _visualElementTracker;

		readonly GestureManager _gestureManager;
		readonly EffectControlProvider _effectControlProvider;
		readonly MotionEventHelper _motionEventHelper = new MotionEventHelper();

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public MaterialFrameRenderer(Context context) : base(new ContextThemeWrapper(context, Resource.Style.XamarinFormsMaterialTheme))
		{
			VisualElement.VerifyVisualFlagEnabled();
			_gestureManager = new GestureManager(this);
			_effectControlProvider = new EffectControlProvider(this);
			UseCompatPadding = true;
		}

		protected CardView Control => this;

		AView ITabStop.TabStop => this;

		protected Frame Element
		{
			get { return _element; }
			set
			{
				if (_element == value)
					return;

				Frame oldElement = _element;
				_element = value;

				OnElementChanged(new ElementChangedEventArgs<Frame>(oldElement, _element));

				_element?.SendViewInitialized(Control);
			}
		}

		VisualElement IVisualElementRenderer.Element => Element;
		ViewGroup IVisualElementRenderer.ViewGroup => this;
		AView IVisualElementRenderer.View => this;

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			Context context = Context;
			return new SizeRequest(new Size(context.ToPixels(20), context.ToPixels(20)));
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			var frame = element as Frame;
			if (frame == null)
				throw new ArgumentException("Element must be of type Frame");
			Element = frame;
			_motionEventHelper.UpdateElement(frame);

			if (!string.IsNullOrEmpty(Element.AutomationId))
				ContentDescription = Element.AutomationId;
		}

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
				_defaultLabelFor = ViewCompat.GetLabelFor(this);

			ViewCompat.SetLabelFor(this, (int)(id ?? _defaultLabelFor));
		}

		VisualElementTracker IVisualElementRenderer.Tracker => _visualElementTracker;

		void IVisualElementRenderer.UpdateLayout()
		{
			VisualElementTracker tracker = _visualElementTracker;
			tracker?.UpdateLayout();
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			_effectControlProvider.RegisterEffect(effect);
		}

		void IViewRenderer.MeasureExactly()
		{
			ViewRenderer.MeasureExactly(this, Element, Context);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				_gestureManager?.Dispose();

				if (_visualElementTracker != null)
				{
					_visualElementTracker.Dispose();
					_visualElementTracker = null;
				}

				if (_visualElementPackager != null)
				{
					_visualElementPackager.Dispose();
					_visualElementPackager = null;
				}

				int count = ChildCount;
				for (var i = 0; i < count; i++)
				{
					AView child = GetChildAt(i);
					child.Dispose();
				}

				if (Element != null)
				{
					Element.PropertyChanged -= OnElementPropertyChanged;

					if (Platform.GetRenderer(Element) == this)
						Element.ClearValue(Platform.RendererProperty);
				}

			}

			base.Dispose(disposing);
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<Frame> e)
		{
			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));

			if (e.OldElement != null)
			{
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (e.NewElement != null)
			{
				this.EnsureId();
				//this.SetBackground(_backgroundDrawable);

				if (_visualElementTracker == null)
				{
					_visualElementTracker = new VisualElementTracker(this);
					_visualElementPackager = new VisualElementPackager(this);
					_visualElementPackager.Load();
				}

				e.NewElement.PropertyChanged += OnElementPropertyChanged;
				UpdateShadow();
				UpdateBackgroundColor();
				UpdateCornerRadius();
				UpdateBorderColor();

				ElevationHelper.SetElevation(this, e.NewElement);
			}
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			if (Element == null)
				return;

			var children = ((IElementController)Element).LogicalChildren;
			for (var i = 0; i < children.Count; i++)
			{
				var visualElement = children[i] as VisualElement;
				if (visualElement == null)
					continue;
				IVisualElementRenderer renderer = Android.Platform.GetRenderer(visualElement);
				renderer?.UpdateLayout();
			}
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (_gestureManager.OnTouchEvent(e) || base.OnTouchEvent(e))
			{
				return true;
			}

			return _motionEventHelper.HandleMotionEvent(Parent, e);
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(this, e);

			if (e.PropertyName == Frame.HasShadowProperty.PropertyName)
				UpdateShadow();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
			else if (e.PropertyName == Frame.CornerRadiusProperty.PropertyName)
				UpdateCornerRadius();
			else if (e.PropertyName == Frame.BorderColorProperty.PropertyName)
				UpdateBorderColor();
		}

		void UpdateBackgroundColor()
		{
			if (_disposed)
				return;

			Color bgColor = Element.BackgroundColor;

			int[][] States =
			{
				new int[0]
			};

			CardBackgroundColor = new global::Android.Content.Res.ColorStateList
				(
					States,
					new int[] { bgColor.IsDefault ? AColor.White : bgColor.ToAndroid() }
				);
			
		}

		void UpdateBorderColor()
		{
			if (_disposed)
				return;

			Color borderColor = Element.BorderColor;
			if (borderColor.IsDefault)
			{
				StrokeColor = AColor.Transparent;
				StrokeWidth = 0;
			}
			else
			{
				StrokeColor = borderColor.ToAndroid();
				StrokeWidth = 25;
			}

		}

		void UpdateShadow()
		{
			if (_disposed)
				return;

			float elevation = _defaultElevation;

			if (elevation == -1f)
				_defaultElevation = elevation = CardElevation;
			 
			if (Element.HasShadow)
				CardElevation = _defaultElevation;
			else
				CardElevation = 0f;
		}

		void UpdateCornerRadius()
		{
			if (_disposed)
				return;

			if (_defaultCornerRadius == -1f)
			{
				_defaultCornerRadius = Radius;
			}

			float cornerRadius = Element.CornerRadius;

			if (cornerRadius == -1f)
				cornerRadius = _defaultCornerRadius;
			else
				cornerRadius = Context.ToPixels(cornerRadius);

			this.Radius = cornerRadius;
		}
	}
}
#endif
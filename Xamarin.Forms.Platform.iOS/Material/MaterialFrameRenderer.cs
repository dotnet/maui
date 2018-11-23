using System;
using System.ComponentModel;
using System.Drawing;
using CoreAnimation;
using MaterialComponents;
using UIKit;
using Xamarin.Forms;
using MCard = MaterialComponents.Card;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Frame), typeof(Xamarin.Forms.Platform.iOS.Material.MaterialFrameRenderer), new[] { typeof(VisualRendererMarker.Material) })]
namespace Xamarin.Forms.Platform.iOS.Material
{
	public class MaterialFrameRenderer : MCard, IVisualElementRenderer
	{
		private VisualElementPackager _packager;
		private VisualElementTracker _tracker;

		public MaterialFrameRenderer()
		{
			VisualElement.VerifyVisualFlagEnabled();
		}

		Xamarin.Forms.Frame FrameElement
		{
			get { return Element as Xamarin.Forms.Frame; }
		}

		public VisualElement Element { get; private set; }

		public UIView NativeView
		{
			get { return this; }
		}

		public UIViewController ViewController
		{
			get { return null; }
		}

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;


		protected virtual void OnElementChanged(VisualElementChangedEventArgs e) => ElementChanged?.Invoke(this, e);

		public void SetElement(VisualElement element)
		{
			var oldElement = Element;
			Element = element;

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (element != null)
			{
				element.PropertyChanged += OnElementPropertyChanged;
				if (_packager == null)
				{
					_packager = new VisualElementPackager(this);
					_packager.Load();

					_tracker = new VisualElementTracker(this);
					_tracker.NativeControlUpdated += OnNativeControlUpdated;
					//_events = new EventTracker(this);
					//	_events.LoadEvents(this);

					//_insetTracker = new KeyboardInsetTracker(this, () => Window, insets => ContentInset = ScrollIndicatorInsets = insets, point =>
					//{
					//	var offset = ContentOffset;
					//	offset.Y += point.Y;
					//	SetContentOffset(offset, true);
					//});
				}

				SetupLayer();
			}

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName ||
				e.PropertyName == Xamarin.Forms.Frame.BorderColorProperty.PropertyName ||
				e.PropertyName == Xamarin.Forms.Frame.HasShadowProperty.PropertyName ||
				e.PropertyName == Xamarin.Forms.Frame.CornerRadiusProperty.PropertyName)
				SetupLayer();
		}


		protected override void Dispose(bool disposing)
		{

			if (disposing)
			{
				if (_packager == null)
					return;

				SetElement(null);

				_packager.Dispose();
				_packager = null;

				_tracker.NativeControlUpdated -= OnNativeControlUpdated;
				_tracker.Dispose();
				_tracker = null;
			}

			base.Dispose(disposing);
		}
		void OnNativeControlUpdated(object sender, EventArgs eventArgs)
		{

		}

		void SetupLayer()
		{
			float cornerRadius = FrameElement.CornerRadius;

			if (cornerRadius == -1f)
				cornerRadius = 5f; // default corner radius

			CornerRadius = cornerRadius;

			if (Element.BackgroundColor == Color.Default)
				BackgroundColor = UIColor.White;
			else
				BackgroundColor = Element.BackgroundColor.ToUIColor();

			if (FrameElement.HasShadow)
			{
				Layer.ShadowRadius = 5;
				SetShadowColor(UIColor.Black, UIControlState.Normal);
				
				SetShadowElevation(3, UIControlState.Normal);				
				//Layer.ShadowColor = UIColor.Black.CGColor;
			}
			else
			{
				//SetShadowColor(UIColor.Black, UIControlState.Normal);
				SetShadowColor(UIColor.Clear, UIControlState.Normal);
			}
			

			if (FrameElement.BorderColor == Color.Default)
				SetBorderColor(UIColor.Clear, UIControlState.Normal);
			else
			{
				SetBorderColor(FrameElement.BorderColor.ToUIColor(), UIControlState.Normal);
				SetBorderWidth(3, UIControlState.Normal);
			}
		}

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return NativeView.GetSizeRequest(widthConstraint, heightConstraint, 44, 44);
		}

		public void SetElementSize(Size size)
		{
			Layout.LayoutChildIntoBoundingRegion(Element, new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}
	}
}
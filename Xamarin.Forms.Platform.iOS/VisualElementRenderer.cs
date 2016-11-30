using System;
using System.Collections.Generic;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;


namespace Xamarin.Forms.Platform.iOS
{
	[Flags]
	public enum VisualElementRendererFlags
	{
		Disposed = 1 << 0,
		AutoTrack = 1 << 1,
		AutoPackage = 1 << 2
	}

	public class VisualElementRenderer<TElement> : UIView, IVisualElementRenderer, IEffectControlProvider where TElement : VisualElement
	{
		readonly UIColor _defaultColor = UIColor.Clear;

		readonly List<EventHandler<VisualElementChangedEventArgs>> _elementChangedHandlers = new List<EventHandler<VisualElementChangedEventArgs>>();

		readonly PropertyChangedEventHandler _propertyChangedHandler;
		EventTracker _events;

		VisualElementRendererFlags _flags = VisualElementRendererFlags.AutoPackage | VisualElementRendererFlags.AutoTrack;

		VisualElementPackager _packager;
		VisualElementTracker _tracker;

		UIVisualEffectView _blur;
		BlurEffectStyle _previousBlur;

		protected VisualElementRenderer() : base(RectangleF.Empty)
		{
			_propertyChangedHandler = OnElementPropertyChanged;
			BackgroundColor = UIColor.Clear;
		}

		// prevent possible crashes in overrides
		public sealed override UIColor BackgroundColor
		{
			get { return base.BackgroundColor; }
			set { base.BackgroundColor = value; }
		}

		public TElement Element { get; private set; }

		protected bool AutoPackage
		{
			get { return (_flags & VisualElementRendererFlags.AutoPackage) != 0; }
			set
			{
				if (value)
					_flags |= VisualElementRendererFlags.AutoPackage;
				else
					_flags &= ~VisualElementRendererFlags.AutoPackage;
			}
		}

		protected bool AutoTrack
		{
			get { return (_flags & VisualElementRendererFlags.AutoTrack) != 0; }
			set
			{
				if (value)
					_flags |= VisualElementRendererFlags.AutoTrack;
				else
					_flags &= ~VisualElementRendererFlags.AutoTrack;
			}
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			var platformEffect = effect as PlatformEffect;
			if (platformEffect != null)
				OnRegisterEffect(platformEffect);
		}

		VisualElement IVisualElementRenderer.Element
		{
			get { return Element; }
		}

		event EventHandler<VisualElementChangedEventArgs> IVisualElementRenderer.ElementChanged
		{
			add { _elementChangedHandlers.Add(value); }
			remove { _elementChangedHandlers.Remove(value); }
		}

		public virtual SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return NativeView.GetSizeRequest(widthConstraint, heightConstraint);
		}

		public UIView NativeView
		{
			get { return this; }
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			SetElement((TElement)element);
		}

		public void SetElementSize(Size size)
		{
			Layout.LayoutChildIntoBoundingRegion(Element, new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}

		public virtual UIViewController ViewController
		{
			get { return null; }
		}

		public event EventHandler<ElementChangedEventArgs<TElement>> ElementChanged;

		public void SetElement(TElement element)
		{
			var oldElement = Element;
			Element = element;

			if (oldElement != null)
				oldElement.PropertyChanged -= _propertyChangedHandler;

			if (element != null)
			{
				if (element.BackgroundColor != Color.Default || (oldElement != null && element.BackgroundColor != oldElement.BackgroundColor))
					SetBackgroundColor(element.BackgroundColor);

				UpdateClipToBounds();

				if (_tracker == null)
				{
					_tracker = new VisualElementTracker(this);
					_tracker.NativeControlUpdated += (sender, e) => UpdateNativeWidget();
				}

				if (AutoPackage && _packager == null)
				{
					_packager = new VisualElementPackager(this);
					_packager.Load();
				}

				if (AutoTrack && _events == null)
				{
					_events = new EventTracker(this);
					_events.LoadEvents(this);
				}

				element.PropertyChanged += _propertyChangedHandler;
			}

			OnElementChanged(new ElementChangedEventArgs<TElement>(oldElement, element));

			if (element != null)
				SendVisualElementInitialized(element, this);

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);

			if (Element != null && !string.IsNullOrEmpty(Element.AutomationId))
				SetAutomationId(Element.AutomationId);
		}

		public override SizeF SizeThatFits(SizeF size)
		{
			return new SizeF(0, 0);
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			if (_blur != null && Superview != null)
			{
				_blur.Frame = Bounds;
				if (_blur.Superview == null)
					Superview.Add(_blur);
			}
		}
		protected override void Dispose(bool disposing)
		{
			if ((_flags & VisualElementRendererFlags.Disposed) != 0)
				return;
			_flags |= VisualElementRendererFlags.Disposed;

			if (disposing)
			{
				if (_events != null)
				{
					_events.Dispose();
					_events = null;
				}
				if (_tracker != null)
				{
					_tracker.Dispose();
					_tracker = null;
				}
				if (_packager != null)
				{
					_packager.Dispose();
					_packager = null;
				}

				Platform.SetRenderer(Element, null);
				SetElement(null);
				Element = null;
			}
			base.Dispose(disposing);
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<TElement> e)
		{
			var args = new VisualElementChangedEventArgs(e.OldElement, e.NewElement);
			for (var i = 0; i < _elementChangedHandlers.Count; i++)
				_elementChangedHandlers[i](this, args);

			var changed = ElementChanged;
			if (changed != null)
				changed(this, e);

			if (e.NewElement != null)
				SetBlur((BlurEffectStyle)e.NewElement.GetValue(PlatformConfiguration.iOSSpecific.VisualElement.BlurEffectProperty));
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				SetBackgroundColor(Element.BackgroundColor);
			else if (e.PropertyName == Layout.IsClippedToBoundsProperty.PropertyName)
				UpdateClipToBounds();
			else if (e.PropertyName == PlatformConfiguration.iOSSpecific.VisualElement.BlurEffectProperty.PropertyName)
				SetBlur((BlurEffectStyle)Element.GetValue(PlatformConfiguration.iOSSpecific.VisualElement.BlurEffectProperty));
		}

		protected virtual void OnRegisterEffect(PlatformEffect effect)
		{
			effect.Container = this;
		}

		protected virtual void SetAutomationId(string id)
		{
			AccessibilityIdentifier = id;
		}

		protected virtual void SetBackgroundColor(Color color)
		{
			if (color == Color.Default)
				BackgroundColor = _defaultColor;
			else
				BackgroundColor = color.ToUIColor();
		}

		protected virtual void SetBlur(BlurEffectStyle blur)
		{
			if (_previousBlur == blur)
				return;

			_previousBlur = blur;

			if (_blur != null)
			{
				_blur.RemoveFromSuperview();
				_blur = null;
			}

			if (blur == BlurEffectStyle.None)
			{
				SetNeedsDisplay();
				return;
			}

			UIBlurEffect blurEffect;
			switch (blur)
			{
				default:
				case BlurEffectStyle.ExtraLight:
					blurEffect = UIBlurEffect.FromStyle(UIBlurEffectStyle.ExtraLight);
					break;
				case BlurEffectStyle.Light:
					blurEffect = UIBlurEffect.FromStyle(UIBlurEffectStyle.Light);
					break;
				case BlurEffectStyle.Dark:
					blurEffect = UIBlurEffect.FromStyle(UIBlurEffectStyle.Dark);
					break;
			}

			_blur = new UIVisualEffectView(blurEffect);
			LayoutSubviews();
		}

		protected virtual void UpdateNativeWidget()
		{
		}

		internal virtual void SendVisualElementInitialized(VisualElement element, UIView nativeView)
		{
			element.SendViewInitialized(nativeView);
		}

		void UpdateClipToBounds()
		{
			var clippableLayout = Element as Layout;
			if (clippableLayout != null)
				ClipsToBounds = clippableLayout.IsClippedToBounds;
		}
	}
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
#if __UNIFIED__
using UIKit;

#else
using MonoTouch.UIKit;
#endif

namespace Xamarin.Forms.Platform.iOS
{
	public class PageRenderer : UIViewController, IVisualElementRenderer
	{
		bool _appeared;
		bool _disposed;
		EventTracker _events;
		VisualElementPackager _packager;
		VisualElementTracker _tracker;

		public PageRenderer()
		{
			if (!Forms.IsiOS7OrNewer)
				WantsFullScreenLayout = true;
		}

		public VisualElement Element { get; private set; }

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return NativeView.GetSizeRequest(widthConstraint, heightConstraint);
		}

		public UIView NativeView
		{
			get { return _disposed ? null : View; }
		}

		public void SetElement(VisualElement element)
		{
			var oldElement = Element;
			Element = element;
			UpdateTitle();

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

			if (Element != null && !string.IsNullOrEmpty(Element.AutomationId))
				SetAutomationId(Element.AutomationId);

			if (element != null)
				element.SendViewInitialized(NativeView);
		}

		public void SetElementSize(Size size)
		{
			Element.Layout(new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}

		public UIViewController ViewController
		{
			get { return _disposed ? null : this; }
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);

			if (_appeared || _disposed)
				return;

			_appeared = true;
			((Page)Element).SendAppearing();
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);

			if (!_appeared || _disposed)
				return;

			_appeared = false;
			((Page)Element).SendDisappearing();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			var uiTapGestureRecognizer = new UITapGestureRecognizer(a => View.EndEditing(true));

			uiTapGestureRecognizer.ShouldRecognizeSimultaneously = (recognizer, gestureRecognizer) => true;
			uiTapGestureRecognizer.ShouldReceiveTouch = OnShouldReceiveTouch;
			uiTapGestureRecognizer.DelaysTouchesBegan = uiTapGestureRecognizer.DelaysTouchesEnded = false;
			View.AddGestureRecognizer(uiTapGestureRecognizer);

			UpdateBackground();

			_packager = new VisualElementPackager(this);
			_packager.Load();

			Element.PropertyChanged += OnHandlePropertyChanged;
			_tracker = new VisualElementTracker(this);

			_events = new EventTracker(this);
			_events.LoadEvents(View);

			Element.SendViewInitialized(View);
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			if (View.Window != null)
				View.Window.EndEditing(true);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				Element.PropertyChanged -= OnHandlePropertyChanged;
				Platform.SetRenderer(Element, null);
				if (_appeared)
					((Page)Element).SendDisappearing();

				_appeared = false;

				if (_events != null)
				{
					_events.Dispose();
					_events = null;
				}

				if (_packager != null)
				{
					_packager.Dispose();
					_packager = null;
				}

				if (_tracker != null)
				{
					_tracker.Dispose();
					_tracker = null;
				}

				Element = null;
				_disposed = true;
			}

			base.Dispose(disposing);
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
			var changed = ElementChanged;
			if (changed != null)
				changed(this, e);
		}

		protected virtual void SetAutomationId(string id)
		{
			if (NativeView != null)
				NativeView.AccessibilityIdentifier = id;
		}

		void OnHandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == Page.BackgroundImageProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == Page.TitleProperty.PropertyName)
				UpdateTitle();
		}

		bool OnShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
		{
			if (ViewAndSuperviewsOfView(touch.View).Any(v => v is UITableView || v is UITableViewCell || v.CanBecomeFirstResponder))
				return false;
			return true;
		}

		void UpdateBackground()
		{
			var bgImage = ((Page)Element).BackgroundImage;
			if (!string.IsNullOrEmpty(bgImage))
			{
				View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromBundle(bgImage));
				return;
			}
			var bgColor = Element.BackgroundColor;
			if (bgColor.IsDefault)
				View.BackgroundColor = UIColor.White;
			else
				View.BackgroundColor = bgColor.ToUIColor();
		}

		void UpdateTitle()
		{
			if (!string.IsNullOrWhiteSpace(((Page)Element).Title))
				Title = ((Page)Element).Title;
		}

		IEnumerable<UIView> ViewAndSuperviewsOfView(UIView view)
		{
			while (view != null)
			{
				yield return view;
				view = view.Superview;
			}
		}
	}
}
using Gtk;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.GTK.Extensions;
using Container = Xamarin.Forms.Platform.GTK.GtkFormsContainer;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
	public abstract class AbstractPageRenderer<TWidget, TPage> : Container, IPageControl, IVisualElementRenderer, IEffectControlProvider
		where TWidget : Widget
		where TPage : Page
	{
		private Gdk.Rectangle _lastAllocation;
		private DateTime _lastAllocationTime;
		protected bool _disposed;
		protected bool _appeared;
		protected readonly PropertyChangedEventHandler _propertyChangedHandler;

		protected AbstractPageRenderer()
		{
			VisibleWindow = true;
			_propertyChangedHandler = OnElementPropertyChanged;
		}

		public Controls.Page Control { get; protected set; }

		public TWidget Widget { get; protected set; }

		public VisualElement Element { get; protected set; }

		public TPage Page => Element as TPage;

		public bool Disposed { get { return _disposed; } }

		public Container Container => this;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		protected IElementController ElementController => Element as IElementController;

		protected IPageController PageController => Element as IPageController;

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			var platformEffect = effect as PlatformEffect;
			if (platformEffect != null)
				platformEffect.SetContainer(Container);
		}

		public virtual void SetElement(VisualElement element)
		{
			VisualElement oldElement = Element;
			Element = element;

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);
		}

		public virtual void SetElementSize(Size newSize)
		{
			if (Element == null)
				return;

			var elementSize = new Size(Element.Bounds.Width, Element.Bounds.Height);

			if (elementSize == newSize)
				return;

			var bounds = new Rectangle(Element.X, Element.Y, newSize.Width, newSize.Height);

			Element.Layout(bounds);
		}

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return Container.GetDesiredSize(widthConstraint, heightConstraint);
		}

		public override void Destroy()
		{
			base.Destroy();

			if (!_disposed)
			{
				if (_appeared)
				{
					ReadOnlyCollection<Element> children = ((IElementController)Element).LogicalChildren;
					for (var i = 0; i < children.Count; i++)
					{
						var visualChild = children[i] as VisualElement;
						visualChild?.Cleanup();
					}

					Page.SendDisappearing();
				}

				_appeared = false;

				Dispose(true);

				_disposed = true;
			}
		}

		protected override void OnShown()
		{
			base.OnShown();

			if (_appeared || _disposed)
				return;

			UpdateBackgroundColor();
			UpdateBackgroundImage();

			_appeared = true;

			PageController.SendAppearing();
		}

		protected override void OnSizeAllocated(Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated(allocation);

			var now = DateTime.Now;
			var diff = now.Subtract(_lastAllocationTime);

			if (_lastAllocation != allocation)
			{
				_lastAllocation = allocation;
				_lastAllocationTime = now;
				SetPageSize(_lastAllocation.Width, _lastAllocation.Height); // Check ToolBar for size calculations.
				PageQueueResize();
			}
			else if (diff > TimeSpan.FromMilliseconds(50)) // Prevent infinite resizing loops for very fast layout changes
			{
				SetPageSize(allocation.Width, allocation.Height);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Element != null)
				{
					Element.PropertyChanged -= OnElementPropertyChanged;
				}

				Platform.SetRenderer(Element, null);

				Control.Destroy();
				Control = null;
				Element = null;
			}
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
			if (e.OldElement != null)
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					Control = new Controls.Page();
					Add(Control);
				}

				e.NewElement.PropertyChanged += OnElementPropertyChanged;
			}

			UpdateBackgroundImage();

			ElementChanged?.Invoke(this, e);
		}

		protected virtual void UpdateBackgroundColor()
		{
			Control.SetBackgroundColor(Element.BackgroundColor);
		}

		protected virtual void UpdateBackgroundImage()
		{
			Control.SetBackgroundImage(Page.BackgroundImageSource);
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
			else if (e.PropertyName == Xamarin.Forms.Page.BackgroundImageSourceProperty.PropertyName)
				UpdateBackgroundImage();
		}

		protected virtual void SetPageSize(int width, int height)
		{
			var finalHeight = height;

			if (Page != null &&
				HasAncestorNavigationPage(Page))
				finalHeight -= GtkToolbarConstants.ToolbarHeight; // Subtract the size of the Toolbar.

			var pageContentSize = new Gdk.Rectangle(0, 0, width, finalHeight);
			var newSize = pageContentSize.ToSize();

			SetElementSize(newSize);
		}

		private void PageQueueResize()
		{
			Control?.Content?.QueueResize();
		}

		private bool HasAncestorNavigationPage(TPage page)
		{
			bool hasParentNavigation = false;
			TPage parent;
			TPage current = page;

			while ((parent = current.Parent as TPage) != null)
			{
				hasParentNavigation = parent is NavigationPage;

				current = parent;

				if (hasParentNavigation) break;
			}
			var hasAncestorNavigationPage = hasParentNavigation && NavigationPage.GetHasNavigationBar(current);
			return hasAncestorNavigationPage;
		}
	}
}

using System;
using AppKit;
using RectangleF = CoreGraphics.CGRect;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.MacOS
{
	public class Platform : BindableObject, IPlatform, IDisposable
	{
		internal static readonly BindableProperty RendererProperty = BindableProperty.CreateAttached("Renderer",
			typeof(IVisualElementRenderer), typeof(Platform), default(IVisualElementRenderer),
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var view = bindable as VisualElement;
				if (view != null)
					view.IsPlatformEnabled = newvalue != null;
			});

		readonly PlatformRenderer PlatformRenderer;

		bool _appeared;
		bool _disposed;

		internal static NativeToolbarTracker NativeToolbarTracker = new NativeToolbarTracker();

		internal Platform()
		{
			PlatformRenderer = new PlatformRenderer(this);

			MessagingCenter.Subscribe(this, Page.AlertSignalName, (Page sender, AlertArguments arguments) =>
			{
				var alert = NSAlert.WithMessage(arguments.Title, arguments.Cancel, arguments.Accept, null, arguments.Message);
				var result = alert.RunSheetModal(PlatformRenderer.View.Window);
				if (arguments.Accept == null)
					arguments.SetResult(result == 1);
				else
					arguments.SetResult(result == 0);
			});

			MessagingCenter.Subscribe(this, Page.ActionSheetSignalName, (Page sender, ActionSheetArguments arguments) =>
			{
				var alert = NSAlert.WithMessage(arguments.Title, arguments.Cancel, arguments.Destruction, null, "");
				if (arguments.Buttons != null)
				{
					int maxScrollHeight = (int)(0.6 * NSScreen.MainScreen.Frame.Height);
					NSView extraButtons = GetExtraButton(arguments);
					if (extraButtons.Frame.Height > maxScrollHeight) {
						NSScrollView scrollView = new NSScrollView();
						scrollView.Frame = new RectangleF(0, 0, extraButtons.Frame.Width, maxScrollHeight);
						scrollView.DocumentView = extraButtons;
						scrollView.HasVerticalScroller = true;
						alert.AccessoryView = scrollView;
					} else {
						alert.AccessoryView = extraButtons;
					}
					alert.Layout();
				}

				var result = (int)alert.RunSheetModal(PlatformRenderer.View.Window);
				var titleResult = string.Empty;
				if (result == 1)
					titleResult = arguments.Cancel;
				else if (result == 0)
					titleResult = arguments.Destruction;
				else if (result > 1 && arguments.Buttons != null && result - 2 <= arguments.Buttons.Count())
					titleResult = arguments.Buttons.ElementAt(result - 2);

				arguments.SetResult(titleResult);
			});
		}

		SizeRequest IPlatform.GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			var renderView = GetRenderer(view);
			if (renderView == null || renderView.NativeView == null)
				return new SizeRequest(Size.Zero);

			return renderView.GetDesiredSize(widthConstraint, heightConstraint);
		}

		Page Page { get; set; }

		Application TargetApplication
		{
			get
			{
				if (Page == null)
					return null;
				return Page.RealParent as Application;
			}
		}

		void IDisposable.Dispose()
		{
			if (_disposed)
				return;
			_disposed = true;

			Page.DescendantRemoved -= HandleChildRemoved;
			MessagingCenter.Unsubscribe<Page, ActionSheetArguments>(this, Page.ActionSheetSignalName);
			MessagingCenter.Unsubscribe<Page, AlertArguments>(this, Page.AlertSignalName);
			MessagingCenter.Unsubscribe<Page, bool>(this, Page.BusySetSignalName);

			DisposeModelAndChildrenRenderers(Page);
			PlatformRenderer.Dispose();
		}

		public static IVisualElementRenderer CreateRenderer(VisualElement element)
		{
			var renderer = Internals.Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(element) ?? new DefaultRenderer();
			renderer.SetElement(element);
			return renderer;
		}

		public static IVisualElementRenderer GetRenderer(VisualElement bindable)
		{
			return (IVisualElementRenderer)bindable.GetValue(RendererProperty);
		}

		public static void SetRenderer(VisualElement bindable, IVisualElementRenderer value)
		{
			bindable.SetValue(RendererProperty, value);
		}

		protected override void OnBindingContextChanged()
		{
			SetInheritedBindingContext(Page, BindingContext);

			base.OnBindingContextChanged();
		}

		internal NSViewController ViewController => PlatformRenderer;

		internal static void DisposeModelAndChildrenRenderers(Element view)
		{
			IVisualElementRenderer renderer;
			foreach (VisualElement child in view.Descendants())
				DisposeModelAndChildrenRenderers(child);

			renderer = GetRenderer((VisualElement)view);
			if (renderer?.ViewController?.ParentViewController != null)
				renderer?.ViewController?.RemoveFromParentViewController();

			renderer?.NativeView?.RemoveFromSuperview();
			renderer?.Dispose();

			view.ClearValue(RendererProperty);
		}

		internal static void DisposeRendererAndChildren(IVisualElementRenderer rendererToRemove)
		{
			if (rendererToRemove == null || rendererToRemove.Element == null)
				return;

			if (GetRenderer(rendererToRemove.Element) == rendererToRemove)
				rendererToRemove.Element.ClearValue(RendererProperty);

			if (rendererToRemove.NativeView != null)
			{
				var subviews = rendererToRemove.NativeView.Subviews;
				for (var i = 0; i < subviews.Length; i++)
				{
					var childRenderer = subviews[i] as IVisualElementRenderer;
					if (childRenderer != null)
						DisposeRendererAndChildren(childRenderer);
				}

				rendererToRemove.NativeView.RemoveFromSuperview();
			}
			rendererToRemove.Dispose();
		}

		internal void LayoutSubviews()
		{
			if (Page == null)
				return;

			var rootRenderer = GetRenderer(Page);

			if (rootRenderer == null)
				return;

			rootRenderer.SetElementSize(new Size(PlatformRenderer.View.Bounds.Width, PlatformRenderer.View.Bounds.Height));
		}

		internal void SetPage(Page newRoot)
		{
			if (newRoot == null)
				return;
			if (Page != null)
				throw new NotImplementedException();
			Page = newRoot;

			if (_appeared == false)
				return;

			Page.Platform = this;
			AddChild(Page);

			Page.DescendantRemoved += HandleChildRemoved;

			TargetApplication.NavigationProxy.Inner = PlatformRenderer.Navigation;
		}

		internal void DidAppear()
		{
			PlatformRenderer.Navigation.AnimateModalPages = false;
			TargetApplication.NavigationProxy.Inner = PlatformRenderer.Navigation;
			PlatformRenderer.Navigation.AnimateModalPages = true;
		}

		internal void WillAppear()
		{
			if (_appeared)
				return;

			Page.Platform = this;
			AddChild(Page);

			Page.DescendantRemoved += HandleChildRemoved;

			_appeared = true;
		}

		static NSView GetExtraButton(ActionSheetArguments arguments)
		{
			var newView = new NSView();
			int height = 50;
			int width = 300;
			int i = 0;
			foreach (var button in arguments.Buttons)
			{
				var btn = new NSButton { Title = button, Tag = i };
				btn.SetButtonType(NSButtonType.MomentaryPushIn);
				btn.Activated +=
					(s, e) =>
					{
						NSApplication.SharedApplication.EndSheet(NSApplication.SharedApplication.MainWindow.AttachedSheet,
							((NSButton)s).Tag + 2);
					};
				btn.Frame = new RectangleF(0, height * i, width, height);
				newView.AddSubview(btn);
				i++;
			}
			newView.Frame = new RectangleF(0, 0, width, height * i);
			return newView;
		}

		void AddChild(VisualElement view)
		{
			if (!Application.IsApplicationOrNull(view.RealParent))
				Console.Error.WriteLine("Tried to add parented view to canvas directly");

			if (GetRenderer(view) == null)
			{
				var viewRenderer = CreateRenderer(view);
				SetRenderer(view, viewRenderer);

				PlatformRenderer.View.AddSubview(viewRenderer.NativeView);
				if (viewRenderer.ViewController != null)
					PlatformRenderer.AddChildViewController(viewRenderer.ViewController);
				viewRenderer.SetElementSize(new Size(PlatformRenderer.View.Bounds.Width, PlatformRenderer.View.Bounds.Height));
			}
			else
				Console.Error.WriteLine("A Renderer was already found, potential view double add");
		}

		void HandleChildRemoved(object sender, ElementEventArgs e)
		{
			var view = e.Element;
			DisposeModelAndChildrenRenderers(view);
		}
	}
}
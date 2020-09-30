using System;
using AppKit;
using RectangleF = CoreGraphics.CGRect;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.MacOS
{
	public class Platform : BindableObject, IDisposable
#pragma warning disable CS0618
		, IPlatform
#pragma warning restore
	{
		internal static readonly BindableProperty RendererProperty = BindableProperty.CreateAttached("Renderer",
			typeof(IVisualElementRenderer), typeof(Platform), default(IVisualElementRenderer),
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var view = bindable as VisualElement;
				if (view != null)
					view.IsPlatformEnabled = newvalue != null;
			});

		readonly PlatformRenderer _renderer;

		bool _appeared;
		bool _disposed;

		internal static NativeToolbarTracker NativeToolbarTracker = new NativeToolbarTracker();

		internal Platform()
		{
			_renderer = new PlatformRenderer(this);

			MessagingCenter.Subscribe(this, Page.AlertSignalName, (Page sender, AlertArguments arguments) =>
			{
				var alert = NSAlert.WithMessage(arguments.Title, arguments.Cancel, arguments.Accept, null, arguments.Message);
				var result = alert.RunSheetModal(_renderer.View.Window);
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

				var result = (int)alert.RunSheetModal(_renderer.View.Window);
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

		public static SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
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

			Page.DisposeModalAndChildRenderers();
			//foreach (var modal in _modals)
				//modal.DisposeModalAndChildRenderers();
			_renderer.Dispose();
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

		internal NSViewController ViewController => _renderer;

		internal void LayoutSubviews()
		{
			if (Page == null)
				return;

			var rootRenderer = GetRenderer(Page);

			if (rootRenderer == null)
				return;

			rootRenderer.SetElementSize(new Size(_renderer.View.Bounds.Width, _renderer.View.Bounds.Height));
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

#pragma warning disable CS0618 // Type or member is obsolete
			// The Platform property is no longer necessary, but we have to set it because some third-party
			// library might still be retrieving it and using it
			Page.Platform = this;
#pragma warning restore CS0618 // Type or member is obsolete

			AddChild(Page);

			Page.DescendantRemoved += HandleChildRemoved;

			TargetApplication.NavigationProxy.Inner = _renderer.Navigation;
		}

		internal void DidAppear()
		{
			_renderer.Navigation.AnimateModalPages = false;
			TargetApplication.NavigationProxy.Inner = _renderer.Navigation;
			_renderer.Navigation.AnimateModalPages = true;
		}

		internal void WillAppear()
		{
			if (_appeared)
				return;

#pragma warning disable CS0618 // Type or member is obsolete
			// The Platform property is no longer necessary, but we have to set it because some third-party
			// library might still be retrieving it and using it
			Page.Platform = this;
#pragma warning restore CS0618 // Type or member is obsolete

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

				_renderer.View.AddSubview(viewRenderer.NativeView);
				if (viewRenderer.ViewController != null)
					_renderer.AddChildViewController(viewRenderer.ViewController);
				viewRenderer.SetElementSize(new Size(_renderer.View.Bounds.Width, _renderer.View.Bounds.Height));
			}
			else
				Console.Error.WriteLine("A Renderer was already found, potential view double add");
		}

		void HandleChildRemoved(object sender, ElementEventArgs e)
		{
			var view = e.Element;
			view.DisposeModalAndChildRenderers();
		}

		#region Obsolete 

		SizeRequest IPlatform.GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			return GetNativeSize(view, widthConstraint, heightConstraint);
		}

		#endregion
	}
}
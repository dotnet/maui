using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms.Internals;
using RectangleF = CoreGraphics.CGRect;

namespace Xamarin.Forms.Platform.iOS
{
	public class Platform : BindableObject, INavigation, IDisposable
#pragma warning disable CS0618
		, IPlatform
#pragma warning restore
	{
		internal static readonly BindableProperty RendererProperty = BindableProperty.CreateAttached("Renderer", typeof(IVisualElementRenderer), typeof(Platform), default(IVisualElementRenderer),
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
#if DEBUG
				if (oldvalue != null && newvalue != null)
				{
					Log.Warning("Renderer", $"{bindable} already has a renderer attached to it: {oldvalue}. Please figure out why and then fix it.");
				}
#endif
				var view = bindable as VisualElement;
				if (view != null)
					view.IsPlatformEnabled = newvalue != null;
			});

		readonly int _alertPadding = 10;

		readonly List<Page> _modals;
		List<Page> _previousModals;
		readonly PlatformRenderer _renderer;
		bool _animateModals = true;
		bool _appeared;

		bool _disposed;

		internal Platform()
		{
			_renderer = new PlatformRenderer(this);
			_modals = new List<Page>();

			SubscribeToAlertsAndActionSheets();
		}

		internal UIViewController ViewController
		{
			get { return _renderer; }
		}

		internal Page Page { get; set; }

		void IDisposable.Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				_renderer.Dispose();
			}
		}

		void INavigation.InsertPageBefore(Page page, Page before)
		{
			throw new InvalidOperationException("InsertPageBefore is not supported globally on iOS, please use a NavigationPage.");
		}

		IReadOnlyList<Page> INavigation.ModalStack
		{
			get 
			{
				if (_disposed)
					return new List<Page>();

				return _modals; 
			}
		}

		IReadOnlyList<Page> INavigation.NavigationStack
		{
			get { return new List<Page>(); }
		}

		Task<Page> INavigation.PopAsync()
		{
			return ((INavigation)this).PopAsync(true);
		}

		Task<Page> INavigation.PopAsync(bool animated)
		{
			throw new InvalidOperationException("PopAsync is not supported globally on iOS, please use a NavigationPage.");
		}

		Task<Page> INavigation.PopModalAsync()
		{
			return ((INavigation)this).PopModalAsync(true);
		}

		async Task<Page> INavigation.PopModalAsync(bool animated)
		{
			var modal = _modals.Last();
			_modals.Remove(modal);
			modal.DescendantRemoved -= HandleChildRemoved;

			var controller = GetRenderer(modal) as UIViewController;

			if (_modals.Count >= 1 && controller != null)
				await controller.DismissViewControllerAsync(animated);
			else
				await _renderer.DismissViewControllerAsync(animated);

			modal.DisposeModalAndChildRenderers();

			return modal;
		}

		Task INavigation.PopToRootAsync()
		{
			return ((INavigation)this).PopToRootAsync(true);
		}

		Task INavigation.PopToRootAsync(bool animated)
		{
			throw new InvalidOperationException("PopToRootAsync is not supported globally on iOS, please use a NavigationPage.");
		}

		Task INavigation.PushAsync(Page root)
		{
			return ((INavigation)this).PushAsync(root, true);
		}

		Task INavigation.PushAsync(Page root, bool animated)
		{
			throw new InvalidOperationException("PushAsync is not supported globally on iOS, please use a NavigationPage.");
		}

		Task INavigation.PushModalAsync(Page modal)
		{
			return ((INavigation)this).PushModalAsync(modal, true);
		}

		Task INavigation.PushModalAsync(Page modal, bool animated)
		{
			EndEditing();

			_modals.Add(modal);

#pragma warning disable CS0618 // Type or member is obsolete
			// The Platform property is no longer necessary, but we have to set it because some third-party
			// library might still be retrieving it and using it
			modal.Platform = this;
#pragma warning restore CS0618 // Type or member is obsolete

			modal.DescendantRemoved += HandleChildRemoved;

			if (_appeared)
				return PresentModal(modal, _animateModals && animated);
			return Task.FromResult<object>(null);
		}

		void INavigation.RemovePage(Page page)
		{
			throw new InvalidOperationException("RemovePage is not supported globally on iOS, please use a NavigationPage.");
		}

		public static SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			Performance.Start(out string reference);

			var renderView = GetRenderer(view);
			if (renderView == null || renderView.NativeView == null)
				return new SizeRequest(Size.Zero);

			Performance.Stop(reference);
			return renderView.GetDesiredSize(widthConstraint, heightConstraint);
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

		internal static UIEdgeInsets SafeAreaInsetsForWindow
		{
			get
			{
				UIEdgeInsets safeAreaInsets;

				if (!Forms.IsiOS11OrNewer)
					safeAreaInsets = new UIEdgeInsets(UIApplication.SharedApplication.StatusBarFrame.Size.Height, 0, 0, 0);
				else if (UIApplication.SharedApplication.KeyWindow != null)
					safeAreaInsets = UIApplication.SharedApplication.KeyWindow.SafeAreaInsets;
				else if (UIApplication.SharedApplication.Windows.Length > 0)
					safeAreaInsets = UIApplication.SharedApplication.Windows[0].SafeAreaInsets;
				else
					safeAreaInsets = UIEdgeInsets.Zero;

				return safeAreaInsets;
			}
		}

		internal void DidAppear()
		{
			_animateModals = false;
			Application.Current.NavigationProxy.Inner = this;
			_animateModals = true;
		}

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

			Application.Current.NavigationProxy.Inner = this;
		}

		internal void WillAppear()
		{
			if (_appeared)
				return;

			_renderer.View.BackgroundColor = UIColor.White;
			_renderer.View.ContentMode = UIViewContentMode.Redraw;

#pragma warning disable CS0618 // Type or member is obsolete
				// The Platform property is no longer necessary, but we have to set it because some third-party
				// library might still be retrieving it and using it
				Page.Platform = this;
#pragma warning restore CS0618 // Type or member is obsolete

			AddChild(Page);

			Page.DescendantRemoved += HandleChildRemoved;

			_appeared = true;
		}

		void AddChild(VisualElement view)
		{
			if (!Application.IsApplicationOrNull(view.RealParent))
				Console.Error.WriteLine("Tried to add parented view to canvas directly");

			if (GetRenderer(view) == null)
			{
				var viewRenderer = CreateRenderer(view);
				SetRenderer(view, viewRenderer);

				var nativeView = viewRenderer.NativeView;

				_renderer.View.AddSubview(nativeView);
				if (viewRenderer.ViewController != null)
					_renderer.AddChildViewController(viewRenderer.ViewController);
				viewRenderer.NativeView.Frame = new RectangleF(0, 0, _renderer.View.Bounds.Width, _renderer.View.Bounds.Height);
				viewRenderer.SetElementSize(new Size(_renderer.View.Bounds.Width, _renderer.View.Bounds.Height));
			}
			else
				Console.Error.WriteLine("Potential view double add");
		}

		static void HandleChildRemoved(object sender, ElementEventArgs e)
		{
			var view = e.Element;
			view?.DisposeModalAndChildRenderers();
		}

		bool PageIsChildOfPlatform(Page page)
		{
			var parent = page.AncestorToRoot();
			return Page == parent || _modals.Contains(parent);
		}

		// Creates a UIAlertAction which includes a call to hide the presenting UIWindow at the end
		UIAlertAction CreateActionWithWindowHide(string text, UIAlertActionStyle style, Action setResult, UIWindow window)
		{
			return UIAlertAction.Create(text, style,
					a =>
					{
						window.Hidden = true;
						setResult();
					});
		}

		void PresentAlert(AlertArguments arguments)
		{
			var window = new UIWindow { BackgroundColor = Color.Transparent.ToUIColor() };

			var alert = UIAlertController.Create(arguments.Title, arguments.Message, UIAlertControllerStyle.Alert);
			var oldFrame = alert.View.Frame;
			alert.View.Frame = new RectangleF(oldFrame.X, oldFrame.Y, oldFrame.Width, oldFrame.Height - _alertPadding * 2);

			if (arguments.Cancel != null)
			{
				alert.AddAction(CreateActionWithWindowHide(arguments.Cancel, UIAlertActionStyle.Cancel,
					() => arguments.SetResult(false), window));
			}

			if (arguments.Accept != null)
			{
				alert.AddAction(CreateActionWithWindowHide(arguments.Accept, UIAlertActionStyle.Default,
					() => arguments.SetResult(true), window));
			}

			PresentPopUp(window, alert);
		}

		void PresentPrompt(PromptArguments arguments)
		{
			var window = new UIWindow { BackgroundColor = Color.Transparent.ToUIColor() };

			var alert = UIAlertController.Create(arguments.Title, arguments.Message, UIAlertControllerStyle.Alert);
			alert.AddTextField(uiTextField =>
			{
				uiTextField.Placeholder = arguments.Placeholder;
				uiTextField.Text = arguments.InitialValue;
				uiTextField.ShouldChangeCharacters = (field, range, replacementString) => arguments.MaxLength <= -1 || field.Text.Length + replacementString.Length - range.Length <= arguments.MaxLength;
				uiTextField.ApplyKeyboard(arguments.Keyboard);
			});
			var oldFrame = alert.View.Frame;
			alert.View.Frame = new RectangleF(oldFrame.X, oldFrame.Y, oldFrame.Width, oldFrame.Height - _alertPadding * 2);

			alert.AddAction(CreateActionWithWindowHide(arguments.Cancel, UIAlertActionStyle.Cancel, () => arguments.SetResult(null), window));
			alert.AddAction(CreateActionWithWindowHide(arguments.Accept, UIAlertActionStyle.Default, () => arguments.SetResult(alert.TextFields[0].Text), window));

			PresentPopUp(window, alert);
		}

		void PresentActionSheet(ActionSheetArguments arguments)
		{
			var alert = UIAlertController.Create(arguments.Title, null, UIAlertControllerStyle.ActionSheet);
			var window = new UIWindow { BackgroundColor = Color.Transparent.ToUIColor() };

			// Clicking outside of an ActionSheet is an implicit cancel on iPads. If we don't handle it, it freezes the app.
			if (arguments.Cancel != null || UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
			{
				alert.AddAction(CreateActionWithWindowHide(arguments.Cancel ?? "", UIAlertActionStyle.Cancel, () => arguments.SetResult(arguments.Cancel), window));
			}

			if (arguments.Destruction != null)
			{
				alert.AddAction(CreateActionWithWindowHide(arguments.Destruction, UIAlertActionStyle.Destructive, () => arguments.SetResult(arguments.Destruction), window));
			}

			foreach (var label in arguments.Buttons)
			{
				if (label == null)
					continue;

				var blabel = label;

				alert.AddAction(CreateActionWithWindowHide(blabel, UIAlertActionStyle.Default, () => arguments.SetResult(blabel), window));
			}

			PresentPopUp(window, alert, arguments);
		}

		static void PresentPopUp(UIWindow window, UIAlertController alert, ActionSheetArguments arguments = null)
		{
			window.RootViewController = new UIViewController();
			window.RootViewController.View.BackgroundColor = Color.Transparent.ToUIColor();
			window.WindowLevel = UIWindowLevel.Alert + 1;
			window.MakeKeyAndVisible();

			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad && arguments != null)
			{
				UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
				var observer = NSNotificationCenter.DefaultCenter.AddObserver(UIDevice.OrientationDidChangeNotification,
					n => { alert.PopoverPresentationController.SourceRect = window.RootViewController.View.Bounds; });

				arguments.Result.Task.ContinueWith(t =>
				{
					NSNotificationCenter.DefaultCenter.RemoveObserver(observer);
					UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();
				}, TaskScheduler.FromCurrentSynchronizationContext());

				alert.PopoverPresentationController.SourceView = window.RootViewController.View;
				alert.PopoverPresentationController.SourceRect = window.RootViewController.View.Bounds;
				alert.PopoverPresentationController.PermittedArrowDirections = 0; // No arrow
			}

			if(!Forms.IsiOS9OrNewer)
			{
				// For iOS 8, we need to explicitly set the size of the window
				window.Frame = new RectangleF(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height);
			}

			window.RootViewController.PresentViewController(alert, true, null);
		}

		async Task PresentModal(Page modal, bool animated)
		{
			var modalRenderer = GetRenderer(modal);
			if (modalRenderer == null)
			{
				modalRenderer = CreateRenderer(modal);
				SetRenderer(modal, modalRenderer);
			}

			var wrapper = new ModalWrapper(modalRenderer);

			if (_modals.Count > 1)
			{
				var topPage = _modals[_modals.Count - 2];
				var controller = GetRenderer(topPage) as UIViewController;
				if (controller != null)
				{
					await controller.PresentViewControllerAsync(wrapper, animated);
					await Task.Delay(5);
					return;
				}
			}

			// One might wonder why these delays are here... well thats a great question. It turns out iOS will claim the 
			// presentation is complete before it really is. It does not however inform you when it is really done (and thus 
			// would be safe to dismiss the VC). Fortunately this is almost never an issue

			await _renderer.PresentViewControllerAsync(wrapper, animated);
			await Task.Delay(5);
		}

		void EndEditing()
		{
			// If any text entry controls have focus, we need to end their editing session
			// so that they are not the first responder; if we don't some things (like the activity indicator
			// on pull-to-refresh) will not work correctly. 

			// The topmost modal on the stack will have the Window; we can use that to end any current
			// editing that's going on 
			if (_modals.Count > 0)
			{
				var uiViewController = GetRenderer(_modals[_modals.Count - 1]) as UIViewController;
				uiViewController?.View?.Window?.EndEditing(true);
				return;
			}

			// If there aren't any modals, then the platform renderer will have the Window
			_renderer.View?.Window?.EndEditing(true);
		}

		internal class DefaultRenderer : VisualElementRenderer<VisualElement>
		{
			public override UIView HitTest(CGPoint point, UIEvent uievent)
			{
				if (!UserInteractionEnabled) 
				{
					// This view can't interact, and neither can its children
					return null;
				}

				// UIview hit testing ignores objects which have an alpha of less than 0.01 
				// (see https://developer.apple.com/reference/uikit/uiview/1622469-hittest)
				// To prevent layouts with low opacity from being implicitly input transparent, 
				// we need to temporarily bump their alpha value during the actual hit testing,
				// then restore it. If the opacity is high enough or user interaction is disabled, 
				// we don't have to worry about it.

				nfloat old = Alpha;
				if (UserInteractionEnabled && old <= 0.01)
				{
					Alpha = (nfloat)0.011;
				}

				var result = base.HitTest(point, uievent);

				if (UserInteractionEnabled && old <= 0.01)
				{
					Alpha = old;
				}

				if (UserInteractionEnabled && Element is Layout layout && !layout.CascadeInputTransparent)
				{
					// This is a Layout with 'InputTransparent = true' and 'InputTransparentInherited = false'
					if (this.Equals(result))
					{
						// If the hit is on the Layout (and not a child control), then ignore it
						return null;
					}
				}

				return result;
			}
		}

		#region Obsolete 

		SizeRequest IPlatform.GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			return GetNativeSize(view, widthConstraint, heightConstraint);
		}

		#endregion

		internal void SubscribeToAlertsAndActionSheets()
		{
			var busyCount = 0;
			MessagingCenter.Subscribe(this, Page.BusySetSignalName, (Page sender, bool enabled) =>
			{
				if (!PageIsChildOfPlatform(sender))
					return;
				busyCount = Math.Max(0, enabled ? busyCount + 1 : busyCount - 1);
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = busyCount > 0;
			});

			MessagingCenter.Subscribe(this, Page.AlertSignalName, (Page sender, AlertArguments arguments) =>
			{
				if (!PageIsChildOfPlatform(sender))
					return;
				PresentAlert(arguments);
			});

			MessagingCenter.Subscribe(this, Page.PromptSignalName, (Page sender, PromptArguments arguments) =>
			{
				if (!PageIsChildOfPlatform(sender))
					return;
				PresentPrompt(arguments);
			});

			MessagingCenter.Subscribe(this, Page.ActionSheetSignalName, (Page sender, ActionSheetArguments arguments) =>
			{
				if (!PageIsChildOfPlatform(sender))
					return;

				PresentActionSheet(arguments);
			});
		}

		internal void UnsubscribeFromAlertsAndActionsSheets()
		{
			MessagingCenter.Unsubscribe<Page, ActionSheetArguments>(this, Page.ActionSheetSignalName);
			MessagingCenter.Unsubscribe<Page, AlertArguments>(this, Page.AlertSignalName);
			MessagingCenter.Unsubscribe<Page, PromptArguments>(this, Page.PromptSignalName);
			MessagingCenter.Unsubscribe<Page, bool>(this, Page.BusySetSignalName);
		}

		internal void MarkForRemoval()
		{
			_previousModals = new List<Page>(_modals);
			_modals.Clear();
		}

		internal void CleanUpPages()
		{
			Page.DescendantRemoved -= HandleChildRemoved;

			Page.DisposeModalAndChildRenderers();

			foreach (var modal in (_previousModals ?? _modals))
				modal.DisposeModalAndChildRenderers();

			_previousModals?.Clear();
			_modals.Clear();

			(Page.Parent as IDisposable)?.Dispose();
		}
	}
}
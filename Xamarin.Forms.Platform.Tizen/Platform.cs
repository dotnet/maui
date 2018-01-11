using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using ElmSharp;
using Tizen.Applications;
using EProgressBar = ElmSharp.ProgressBar;
using EButton = ElmSharp.Button;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen
{
	public class Platform : BindableObject, IPlatform, INavigation, IDisposable
	{
		internal static readonly BindableProperty RendererProperty = BindableProperty.CreateAttached("Renderer", typeof(IVisualElementRenderer), typeof(Platform), default(IVisualElementRenderer),
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var ve = bindable as VisualElement;
				if (ve != null && newvalue == null)
					ve.IsPlatformEnabled = false;
			});

		NavigationModel _navModel = new NavigationModel();
		bool _disposed;
		Native.Dialog _pageBusyDialog;
		int _pageBusyCount;

		internal Platform(CoreUIApplication context, EvasObject parent)
		{
			Forms.NativeParent = parent;
			_pageBusyCount = 0;
			MessagingCenter.Subscribe<Page, bool>(this, Page.BusySetSignalName, BusySetSignalNameHandler);
			MessagingCenter.Subscribe<Page, AlertArguments>(this, Page.AlertSignalName, AlertSignalNameHandler);
			MessagingCenter.Subscribe<Page, ActionSheetArguments>(this, Page.ActionSheetSignalName, ActionSheetSignalNameHandler);

			InternalNaviframe = new Naviframe(Forms.NativeParent)
			{
				PreserveContentOnPop = true,
				DefaultBackButtonEnabled = false,
			};
			InternalNaviframe.SetAlignment(-1, -1);
			InternalNaviframe.SetWeight(1.0, 1.0);
			InternalNaviframe.Show();
			InternalNaviframe.AnimationFinished += NaviAnimationFinished;
		}

		~Platform()
		{
			Dispose(false);
		}

		public Page Page { get; private set; }

		public Naviframe InternalNaviframe { get; private set; }

		public bool HasAlpha { get; set; }

		Action BackPressedAction { get; set; }

		Task CurrentModalNavigationTask { get; set; }
		TaskCompletionSource<bool> CurrentTaskCompletionSource { get; set; }
		IPageController CurrentPageController => _navModel.CurrentPage as IPageController;
		IReadOnlyList<Page> INavigation.ModalStack => _navModel.Modals.ToList();
		IReadOnlyList<Page> INavigation.NavigationStack => new List<Page>();

		public static IVisualElementRenderer GetRenderer(BindableObject bindable)
		{
			return (IVisualElementRenderer)bindable.GetValue(Platform.RendererProperty);
		}

		public static void SetRenderer(BindableObject bindable, IVisualElementRenderer value)
		{
			bindable.SetValue(Platform.RendererProperty, value);
		}

		/// <summary>
		/// Gets the renderer associated with the <c>view</c>. If it doesn't exist, creates a new one.
		/// </summary>
		/// <returns>Renderer associated with the <c>view</c>.</returns>
		/// <param name="view">View for which the renderer is going to be returned.</param>
		public static IVisualElementRenderer GetOrCreateRenderer(VisualElement view)
		{
			return GetRenderer(view) ?? AttachRenderer(view);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void SetPage(Page newRoot)
		{
			if (Page != null)
			{
				var copyOfStack = new List<NaviItem>(InternalNaviframe.NavigationStack);
				for (var i = 0; i < copyOfStack.Count; i++)
				{
					copyOfStack[i].Delete();
				}
				foreach (Page page in _navModel.Roots)
				{
					var renderer = GetRenderer(page);
					(page as IPageController)?.SendDisappearing();
					renderer?.Dispose();
				}
				_navModel = new NavigationModel();
			}

			if (newRoot == null)
				return;

			_navModel.Push(newRoot, null);

			Page = newRoot;
			Page.Platform = this;

			IVisualElementRenderer pageRenderer = AttachRenderer(Page);
			var naviItem = InternalNaviframe.Push(pageRenderer.NativeView);
			naviItem.TitleBarVisible = false;

			// Make naviitem transparent if parent window is transparent.
			// Make sure that this is only for _navModel._naviTree. (not for _navModel._modalStack)
			// In addtion, the style of naviItem is only decided before the naviItem pushed into Naviframe. (not on-demand).
			if (HasAlpha)
			{
				naviItem.Style = "default/transparent";
			}

			((Application)Page.RealParent).NavigationProxy.Inner = this;

			Device.StartTimer(TimeSpan.Zero, () =>
			{
				CurrentPageController?.SendAppearing();
				return false;
			});
		}

		public SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			widthConstraint = widthConstraint <= -1 ? double.PositiveInfinity : widthConstraint;
			heightConstraint = heightConstraint <= -1 ? double.PositiveInfinity : heightConstraint;

			double width = !double.IsPositiveInfinity(widthConstraint) ? widthConstraint : Int32.MaxValue;
			double height = !double.IsPositiveInfinity(heightConstraint) ? heightConstraint : Int32.MaxValue;

			return GetRenderer(view).GetDesiredSize(width, height);
		}

		public bool SendBackButtonPressed()
		{
			bool handled = false;
			if (_navModel.CurrentPage != null)
			{
				if (CurrentModalNavigationTask != null && !CurrentModalNavigationTask.IsCompleted)
				{
					handled = true;
				}
				else
				{
					handled = _navModel.CurrentPage.SendBackButtonPressed();
				}
			}
			return handled;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				SetPage(null);
				InternalNaviframe.Unrealize();
			}
			_disposed = true;
		}

		protected override void OnBindingContextChanged()
		{
			BindableObject.SetInheritedBindingContext(Page, base.BindingContext);
			base.OnBindingContextChanged();
		}

		static IVisualElementRenderer AttachRenderer(VisualElement view)
		{
			IVisualElementRenderer visualElementRenderer = Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(view) ?? new DefaultRenderer();

			visualElementRenderer.SetElement(view);

			return visualElementRenderer;
		}

		void INavigation.InsertPageBefore(Page page, Page before)
		{
			throw new InvalidOperationException("InsertPageBefore is not supported globally on Tizen, please use a NavigationPage.");
		}

		Task<Page> INavigation.PopAsync()
		{
			return ((INavigation)this).PopAsync(true);
		}

		Task<Page> INavigation.PopAsync(bool animated)
		{
			throw new InvalidOperationException("PopAsync is not supported globally on Tizen, please use a NavigationPage.");
		}

		Task INavigation.PopToRootAsync()
		{
			return ((INavigation)this).PopToRootAsync(true);
		}

		Task INavigation.PopToRootAsync(bool animated)
		{
			throw new InvalidOperationException("PopToRootAsync is not supported globally on Tizen, please use a NavigationPage.");
		}

		Task INavigation.PushAsync(Page root)
		{
			return ((INavigation)this).PushAsync(root, true);
		}

		Task INavigation.PushAsync(Page root, bool animated)
		{
			throw new InvalidOperationException("PushAsync is not supported globally on Tizen, please use a NavigationPage.");
		}

		void INavigation.RemovePage(Page page)
		{
			throw new InvalidOperationException("RemovePage is not supported globally on Tizen, please use a NavigationPage.");
		}

		Task INavigation.PushModalAsync(Page modal)
		{
			return ((INavigation)this).PushModalAsync(modal, true);
		}

		async Task INavigation.PushModalAsync(Page modal, bool animated)
		{
			CurrentPageController?.SendDisappearing();

			_navModel.PushModal(modal);

			modal.Platform = this;

			await PushModalInternal(modal, animated);

			// Verify that the modal is still on the stack
			if (_navModel.CurrentPage == modal)
				CurrentPageController.SendAppearing();
		}

		Task<Page> INavigation.PopModalAsync()
		{
			return ((INavigation)this).PopModalAsync(true);
		}

		async Task<Page> INavigation.PopModalAsync(bool animated)
		{
			Page modal = _navModel.PopModal();

			((IPageController)modal).SendDisappearing();

			IVisualElementRenderer modalRenderer = GetRenderer(modal);
			if (modalRenderer != null)
			{
				await PopModalInternal(animated);
			}
			Platform.GetRenderer(modal).Dispose();

			CurrentPageController?.SendAppearing();
			return modal;
		}

		async Task PushModalInternal(Page modal, bool animated)
		{
			TaskCompletionSource<bool> tcs = null;
			if (CurrentModalNavigationTask != null && !CurrentModalNavigationTask.IsCompleted)
			{
				var previousTask = CurrentModalNavigationTask;
				tcs = new TaskCompletionSource<bool>();
				CurrentModalNavigationTask = tcs.Task;
				await previousTask;
			}

			var after = InternalNaviframe.NavigationStack.LastOrDefault();
			NaviItem pushed = null;
			if (animated || after == null)
			{
				pushed = InternalNaviframe.Push(Platform.GetOrCreateRenderer(modal).NativeView, modal.Title);
			}
			else
			{
				pushed = InternalNaviframe.InsertAfter(after, Platform.GetOrCreateRenderer(modal).NativeView, modal.Title);
			}
			pushed.TitleBarVisible = false;

			bool shouldWait = animated && after != null;
			await WaitForCompletion(shouldWait, tcs);
		}

		async Task PopModalInternal(bool animated)
		{
			TaskCompletionSource<bool> tcs = null;
			if (CurrentModalNavigationTask != null && !CurrentModalNavigationTask.IsCompleted)
			{
				var previousTask = CurrentModalNavigationTask;
				tcs = new TaskCompletionSource<bool>();
				CurrentModalNavigationTask = tcs.Task;
				await previousTask;
			}

			if (animated)
			{
				InternalNaviframe.Pop();
			}
			else
			{
				InternalNaviframe.NavigationStack.LastOrDefault()?.Delete();
			}

			bool shouldWait = animated && (InternalNaviframe.NavigationStack.Count != 0);
			await WaitForCompletion(shouldWait, tcs);
		}

		async Task WaitForCompletion(bool shouldWait, TaskCompletionSource<bool> tcs)
		{
			if (shouldWait)
			{
				tcs = tcs ?? new TaskCompletionSource<bool>();
				CurrentTaskCompletionSource = tcs;
				if (CurrentModalNavigationTask == null || CurrentModalNavigationTask.IsCompleted)
				{
					CurrentModalNavigationTask = CurrentTaskCompletionSource.Task;
				}
			}
			else
			{
				tcs?.SetResult(true);
			}

			if (tcs != null)
				await tcs.Task;
		}

		void NaviAnimationFinished(object sender, EventArgs e)
		{
			var tcs = CurrentTaskCompletionSource;
			CurrentTaskCompletionSource = null;
			tcs?.SetResult(true);
		}

		void BusySetSignalNameHandler(Page sender, bool enabled)
		{
			// Verify that the page making the request is child of this platform
			if (!PageIsChildOfPlatform(sender))
				return;

			if (null == _pageBusyDialog)
			{
				_pageBusyDialog = new Native.Dialog(Forms.NativeParent)
				{
					Orientation = PopupOrientation.Top,
				};

				var activity = new EProgressBar(_pageBusyDialog)
				{
					Style = "process_large",
					IsPulseMode = true,
				};
				activity.PlayPulse();
				activity.Show();

				_pageBusyDialog.Content = activity;

			}
			_pageBusyCount = Math.Max(0, enabled ? _pageBusyCount + 1 : _pageBusyCount - 1);
			if (_pageBusyCount > 0)
			{
				_pageBusyDialog.Show();
			}
			else
			{
				_pageBusyDialog.Dismiss();
				_pageBusyDialog = null;
			}
		}

		void AlertSignalNameHandler(Page sender, AlertArguments arguments)
		{
			// Verify that the page making the request is child of this platform 
			if (!PageIsChildOfPlatform(sender))
				return;

			Native.Dialog alert = new Native.Dialog(Forms.NativeParent);
			alert.Title = arguments.Title;
			var message = arguments.Message.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace(Environment.NewLine, "<br>");
			alert.Text = message;

			EButton cancel = new EButton(alert) { Text = arguments.Cancel };
			alert.NegativeButton = cancel;
			cancel.Clicked += (s, evt) =>
			{
				arguments.SetResult(false);
				alert.Dismiss();
			};

			if (arguments.Accept != null)
			{
				EButton ok = new EButton(alert) { Text = arguments.Accept };
				alert.NeutralButton = ok;
				ok.Clicked += (s, evt) =>
				{
					arguments.SetResult(true);
					alert.Dismiss();
				};
			}

			alert.BackButtonPressed += (s, evt) =>
			{
				arguments.SetResult(false);
				alert.Dismiss();
			};

			alert.Show();
		}

		void ActionSheetSignalNameHandler(Page sender, ActionSheetArguments arguments)
		{
			// Verify that the page making the request is child of this platform
			if (!PageIsChildOfPlatform(sender))
				return;

			Native.Dialog alert = new Native.Dialog(Forms.NativeParent);

			alert.Title = arguments.Title;
			Box box = new Box(alert);

			if (null != arguments.Destruction)
			{
				Native.Button destruction = new Native.Button(alert)
				{
					Text = arguments.Destruction,
					TextColor = EColor.Red,
					AlignmentX = -1
				};
				destruction.Clicked += (s, evt) =>
				{
					arguments.SetResult(arguments.Destruction);
					alert.Dismiss();
				};
				destruction.Show();
				box.PackEnd(destruction);
			}

			foreach (string buttonName in arguments.Buttons)
			{
				Native.Button button = new Native.Button(alert)
				{
					Text = buttonName,
					AlignmentX = -1
				};
				button.Clicked += (s, evt) =>
				{
					arguments.SetResult(buttonName);
					alert.Dismiss();
				};
				button.Show();
				box.PackEnd(button);
			}

			box.Show();
			alert.Content = box;

			if (null != arguments.Cancel)
			{
				EButton cancel = new EButton(Forms.NativeParent) { Text = arguments.Cancel };
				alert.NegativeButton = cancel;
				cancel.Clicked += (s, evt) =>
				{
					alert.Dismiss();
				};
			}

			alert.BackButtonPressed += (s, evt) =>
			{
				alert.Dismiss();
			};

			alert.Show();
		}

		bool PageIsChildOfPlatform(Page page)
		{
			while (!Application.IsApplicationOrNull(page.RealParent))
				page = (Page)page.RealParent;

			return Page == page || _navModel.Roots.Contains(page);
		}
	}
}

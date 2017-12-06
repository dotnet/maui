using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using ElmSharp;

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

		Naviframe _naviframe;
		NavigationModel _navModel = new NavigationModel();
		bool _disposed;

		internal Platform(FormsApplication context)
		{
			Forms.Context.MainWindow.BackButtonPressed += (o, e) =>
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
				if (!handled)
					context.Exit();
			};
			_naviframe = new Naviframe(Forms.Context.MainWindow)
			{
				PreserveContentOnPop = true,
				DefaultBackButtonEnabled = false,
			};
			_naviframe.SetAlignment(-1, -1);
			_naviframe.SetWeight(1.0, 1.0);
			_naviframe.Show();
			_naviframe.AnimationFinished += NaviAnimationFinished;
			Forms.Context.BaseLayout.SetContent(_naviframe);
		}

		~Platform()
		{
			Dispose(false);
		}

		public Page Page
		{
			get;
			private set;
		}

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
				var copyOfStack = new List<NaviItem>(_naviframe.NavigationStack);
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
			var naviItem = _naviframe.Push(pageRenderer.NativeView);
			naviItem.TitleBarVisible = false;

			// Make naviitem transparent if Forms.Context.MainWindow is transparent.
			// Make sure that this is only for _navModel._naviTree. (not for _navModel._modalStack)
			// In addtion, the style of naviItem is only decided before the naviItem pushed into Naviframe. (not on-demand).
			if (Forms.Context.MainWindow.Alpha)
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

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				SetPage(null);
				_naviframe.Unrealize();
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

			var after = _naviframe.NavigationStack.LastOrDefault();
			NaviItem pushed = null;
			if (animated || after == null)
			{
				pushed = _naviframe.Push(Platform.GetOrCreateRenderer(modal).NativeView, modal.Title);
			}
			else
			{
				pushed = _naviframe.InsertAfter(after, Platform.GetOrCreateRenderer(modal).NativeView, modal.Title);
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
				_naviframe.Pop();
			}
			else
			{
				_naviframe.NavigationStack.LastOrDefault()?.Delete();
			}

			bool shouldWait = animated && (_naviframe.NavigationStack.Count != 0);
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
	}
}

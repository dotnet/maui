using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElmSharp;
using Xamarin.Forms.Internals;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen
{
	public class LightweightPlatform : ITizenPlatform, INavigation, IDisposable
	{
		NavigationModel _navModel = new NavigationModel();
		Native.Canvas _viewStack;
		readonly PopupManager _popupManager;
		bool _hasAlpha;
		readonly EColor _defaultPlatformColor;

		public LightweightPlatform(EvasObject parent)
		{
			Forms.NativeParent = parent;
			_defaultPlatformColor = Device.Idiom == TargetIdiom.Phone ? EColor.White : EColor.Transparent;
			_viewStack = new Native.Canvas(parent)
			{
				BackgroundColor = _defaultPlatformColor,
			};
			_viewStack.SetAlignment(-1, -1);
			_viewStack.SetWeight(1.0, 1.0);
			_viewStack.LayoutUpdated += OnLayout;
			_viewStack.Show();

			if (Forms.UseMessagingCenter)
			{
				_popupManager = new PopupManager(this);
			}
		}

#pragma warning disable 0067
		public event EventHandler<RootNativeViewChangedEventArgs> RootNativeViewChanged;
#pragma warning restore 0067

		public bool HasAlpha
		{
			get => _hasAlpha;
			set
			{
				_hasAlpha = value;
				_viewStack.BackgroundColor = _hasAlpha ? EColor.Transparent : _defaultPlatformColor;
			}
		}

		IPageController CurrentPageController => _navModel.CurrentPage as IPageController;

		IReadOnlyList<Page> INavigation.ModalStack => _navModel.Modals.ToList();

		IReadOnlyList<Page> INavigation.NavigationStack => new List<Page>();

		public void SetPage(Page page)
		{
			ResetChildren();
			_navModel = new NavigationModel();
			if (page == null)
				return;

			_navModel.Push(page, null);

#pragma warning disable CS0618 // Type or member is obsolete
			page.Platform = this;
#pragma warning restore CS0618 // Type or member is obsolete
			((Application)page.RealParent).NavigationProxy.Inner = this;

			var renderer = Platform.CreateRenderer(page);
			renderer.NativeView.Geometry = _viewStack.Geometry;
			_viewStack.Children.Add(renderer.NativeView);

			CurrentPageController?.SendAppearing();
		}

		public bool SendBackButtonPressed()
		{
			return _navModel?.CurrentPage?.SendBackButtonPressed() ?? false;
		}

		public EvasObject GetRootNativeView()
		{
			return _viewStack;
		}

		public bool PageIsChildOfPlatform(Page page)
		{
			var parent = page.AncestorToRoot();
			return _navModel.Modals.FirstOrDefault() == page || _navModel.Roots.Contains(parent);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_popupManager?.Dispose();
				_viewStack?.Unrealize();
			}
		}

		Task<Page> INavigation.PopModalAsync()
		{
			return (this as INavigation).PopModalAsync(true);
		}

		Task<Page> INavigation.PopModalAsync(bool animated)
		{
			Page page = _navModel.PopModal();
			(page as IPageController)?.SendDisappearing();

			var renderer = Platform.GetRenderer(page);
			_viewStack.Children.Remove(renderer.NativeView);
			renderer.Dispose();

			_viewStack.Children.LastOrDefault()?.Show();

			CurrentPageController?.SendAppearing();
			return Task.FromResult(page);
		}

		Task INavigation.PushModalAsync(Page modal)
		{
			return (this as INavigation).PushModalAsync(modal, true);
		}

		Task INavigation.PushModalAsync(Page page, bool animated)
		{
			var previousPage = CurrentPageController;
			previousPage?.SendDisappearing();

			_navModel.PushModal(page);

			var lastTop = _viewStack.Children.LastOrDefault();

			var renderer = Platform.GetOrCreateRenderer(page);
			renderer.NativeView.Geometry = _viewStack.Geometry;

			_viewStack.Children.Add(renderer.NativeView);
			if (lastTop != null)
			{
				lastTop.Hide();
				renderer.NativeView.StackAbove(lastTop);
			}

			// Verify that the modal is still on the stack
			if (_navModel.CurrentPage == page)
				CurrentPageController.SendAppearing();
			return Task.CompletedTask;
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

		SizeRequest IPlatform.GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			return Platform.GetNativeSize(view, widthConstraint, heightConstraint);
		}

		void OnLayout(object sender, Native.LayoutEventArgs e)
		{
			foreach (var child in _viewStack.Children)
			{
				child.Geometry = _viewStack.Geometry;
			}
		}

		void ResetChildren()
		{
			var children = _viewStack.Children.ToList();
			_viewStack.Children.Clear();
			foreach (var child in children)
			{
				child.Unrealize();
			}
		}
	}
}

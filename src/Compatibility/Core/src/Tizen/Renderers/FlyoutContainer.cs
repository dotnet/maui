using System;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Renderers
{
	public class FlyoutContainer : ElmSharp.Box, IDisposable
	{
		readonly FlyoutPage _parent;
		readonly bool _isFlyout;

		VisualElement _childView;
		bool _disposed;
		bool _hasAppearedToParent;

		IPageController PageController => ChildView as IPageController;

		IFlyoutPageController FlyoutPageController => _parent as IFlyoutPageController;

		public FlyoutContainer(FlyoutPage parentElement, bool isFlyout) : base(Forms.NativeParent)
		{
			_parent = parentElement;
			_isFlyout = isFlyout;

			SetLayoutCallback(OnLayoutUpdated);
			Show();
		}

		~FlyoutContainer()
		{
			Dispose(false);
		}

		public VisualElement ChildView
		{
			get { return _childView; }
			set
			{
				if (_childView == value)
					return;

				if (_childView != null)
				{
					RemoveChildView();
				}

				_childView = value;

				if (_childView == null)
					return;

				AddChildView(_childView);

				if (_hasAppearedToParent)
				{
					Device.BeginInvokeOnMainThread(() =>
					{
						if (!_disposed && _hasAppearedToParent)
							PageController?.SendAppearing();
					});
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void RemoveChildView()
		{
			IVisualElementRenderer childRenderer = Platform.GetRenderer(_childView);
			if (childRenderer != null)
			{
				UnPack(childRenderer.NativeView);
				childRenderer.Dispose();
			}
		}

		protected void AddChildView(VisualElement childView)
		{
			IVisualElementRenderer renderer = Platform.GetOrCreateRenderer(childView);
			this.PackEnd(renderer.NativeView);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (_childView != null)
				{
					RemoveChildView();
				}
				SetLayoutCallback(null);
			}
			_disposed = true;
		}

		void OnLayoutUpdated()
		{
			if (_childView != null)
			{
				if (_isFlyout)
					FlyoutPageController.FlyoutBounds = this.Geometry.ToDP();
				else
					FlyoutPageController.DetailBounds = this.Geometry.ToDP();

				IVisualElementRenderer renderer = Platform.GetRenderer(_childView);
				renderer.NativeView.Geometry = this.Geometry;
			}
		}

		public void SendAppearing()
		{
			if (_hasAppearedToParent)
				return;

			_hasAppearedToParent = true;

			PageController?.SendAppearing();
		}

		public void SendDisappearing()
		{
			if (!_hasAppearedToParent)
				return;

			_hasAppearedToParent = false;

			PageController?.SendDisappearing();
		}
	}
}
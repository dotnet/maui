using System;
using System.Threading.Tasks;
using System.Linq;
using AppKit;
using System.Collections.Generic;

namespace Xamarin.Forms.Platform.MacOS
{
	internal class ModalPageTracker : IDisposable
	{
		NSViewController _renderer;
		List<Page> _modals;
		bool _disposed;

		public ModalPageTracker(NSViewController mainRenderer)
		{
			if (mainRenderer == null)
				throw new ArgumentNullException(nameof(mainRenderer));
			_renderer = mainRenderer;
			_renderer.View.WantsLayer = true;
			_modals = new List<Page>();
		}

		public List<Page> ModalStack => _modals;

		public Task PushAsync(Page modal, bool animated)
		{
			_modals.Add(modal);
			modal.DescendantRemoved += HandleChildRemoved;
			Platform.NativeToolbarTracker.TryHide(modal as NavigationPage);
			return PresentModalAsync(modal, animated);
		}

		public Task<Page> PopAsync(bool animated)
		{
			var modal = _modals.LastOrDefault();
			if (modal == null)
				throw new InvalidOperationException("No Modal pages found in the stack, make sure you pushed a modal page");
			_modals.Remove(modal);
			modal.DescendantRemoved -= HandleChildRemoved;
			return HideModalAsync(modal, animated);
		}

		internal void LayoutSubviews()
		{
			if (_renderer == null || _renderer.View == null)
				return;
			
			foreach(var modal in _modals)
			{
				var modalRenderer = Platform.GetRenderer(modal);
				if(modalRenderer != null)
					modalRenderer.SetElementSize(new Size(_renderer.View.Bounds.Width, _renderer.View.Bounds.Height));
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					foreach (var modal in _modals)
						Platform.DisposeModelAndChildrenRenderers(modal);
					_renderer = null;
				}
				_disposed = true;
			}
		}

		void HandleChildRemoved(object sender, ElementEventArgs e)
		{
			var view = e.Element;
			Platform.DisposeModelAndChildrenRenderers(view);
		}

		Task PresentModalAsync(Page modal, bool animated)
		{
			var modalRenderer = Platform.GetRenderer(modal);
			if (modalRenderer == null)
			{
				modalRenderer = Platform.CreateRenderer(modal);
				Platform.SetRenderer(modal, modalRenderer);
				modalRenderer.SetElementSize(new Size(_renderer.View.Bounds.Width, _renderer.View.Bounds.Height));
			}

			var toViewController = modalRenderer as NSViewController;

			var i = Math.Max(0, _renderer.ChildViewControllers.Length - 1);
			var fromViewController = _renderer.ChildViewControllers[i];

			_renderer.AddChildViewController(toViewController);

			NSViewControllerTransitionOptions option = animated
				? NSViewControllerTransitionOptions.SlideUp
				: NSViewControllerTransitionOptions.None;

			var task = _renderer.HandleAsyncAnimation(fromViewController, toViewController, option,
				() =>
				{
					//Hack: adjust if needed
					toViewController.View.Frame = _renderer.View.Bounds;
					fromViewController.View.Layer.Hidden = true;
				}, true);
			return task;
		}

		Task<Page> HideModalAsync(Page modal, bool animated)
		{
			var controller = Platform.GetRenderer(modal) as NSViewController;

			var i = Math.Max(0, _renderer.ChildViewControllers.Length - 2);
			var toViewController = _renderer.ChildViewControllers[i];

			toViewController.View.Layer.Hidden = false;

			NSViewControllerTransitionOptions option = animated
							? NSViewControllerTransitionOptions.SlideDown
							: NSViewControllerTransitionOptions.None;

			var task = _renderer.HandleAsyncAnimation(controller, toViewController, option,
				() => Platform.DisposeModelAndChildrenRenderers(modal), modal);
			return task;
		}
	}
}
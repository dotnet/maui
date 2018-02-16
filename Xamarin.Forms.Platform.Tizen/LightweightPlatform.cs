using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElmSharp;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Tizen
{
	public class LightweightPlatform : BindableObject, ITizenPlatform
	{
		Page _page;
		EvasObject _rootView;
		bool _disposed;

		internal LightweightPlatform(EvasObject parent)
		{
			Forms.NativeParent = parent;
		}

		public event EventHandler<RootNativeViewChangedEventArgs> RootNativeViewChanged;

		public bool HasAlpha { get => false; set { } }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public EvasObject GetRootNativeView() => _rootView;

		public bool SendBackButtonPressed()
		{
			if (_page == null) return false;
			return _page.SendBackButtonPressed();
		}

		public void SetPage(Page page)
		{
			if (_page == page) return;
			if (_page != null)
			{
				var oldRenderer = Platform.GetRenderer(_page);
				oldRenderer?.Dispose();
			}

			_page = page;

			if (_page == null) return;

			_page.Platform = this;

			var renderer = Platform.AttachRenderer(_page);
			_rootView = renderer.NativeView;
			RootNativeViewChanged?.Invoke(this, new RootNativeViewChangedEventArgs(_rootView));
			_rootView.Show();

			Device.StartTimer(TimeSpan.Zero, () =>
			{
				_page?.SendAppearing();
				return false;
			});
		}

		protected override void OnBindingContextChanged()
		{
			BindableObject.SetInheritedBindingContext(_page, base.BindingContext);
			base.OnBindingContextChanged();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				SetPage(null);
			}
			_disposed = true;
		}

		SizeRequest IPlatform.GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			widthConstraint = widthConstraint <= -1 ? double.PositiveInfinity : widthConstraint;
			heightConstraint = heightConstraint <= -1 ? double.PositiveInfinity : heightConstraint;

			double width = !double.IsPositiveInfinity(widthConstraint) ? widthConstraint : Int32.MaxValue;
			double height = !double.IsPositiveInfinity(heightConstraint) ? heightConstraint : Int32.MaxValue;

			return Platform.GetRenderer(view).GetDesiredSize(width, height);
		}
	}
}

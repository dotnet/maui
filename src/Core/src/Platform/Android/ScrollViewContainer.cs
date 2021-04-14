using Android.Content;
using Android.Views;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public class ScrollViewContainer : ViewGroup
	{
		readonly IMauiContext? _mauiContext;
		IView? _childView;
		bool _isDisposed;

		public ScrollViewContainer(IMauiContext? mauiContext, Context? context) : base(context)
		{
			_mauiContext = mauiContext;
		}

		public IView? ChildView
		{
			get { return _childView; }
			set
			{
				if (_childView == value)
					return;

				RemoveAllViews();

				_childView = value;

				if (_childView == null || _mauiContext == null)
					return;

				AView nativeView = _childView.ToNative(_mauiContext);

				if (nativeView.Parent != null)
					nativeView.RemoveFromParent();

				AddView(nativeView);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			if (disposing)
			{
				while (ChildCount > 0)
				{
					AView? child = GetChildAt(0);

					if (child != null)
					{
						child.RemoveFromParent();
						child.Dispose();
					}
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			
		}
	}
}
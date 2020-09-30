using System;
using Android.Content;
using Android.Runtime;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	internal class PageContainer : ViewGroup
	{
		bool _disposed;

		public PageContainer(Context context, IVisualElementRenderer child, bool inFragment = false) : base(context)
		{
			Id = Platform.GenerateViewId();
			Child = child;
			IsInFragment = inFragment;
			AddView(child.View);
		}

		public IVisualElementRenderer Child { get; set; }

		public bool IsInFragment { get; set; }

		protected PageContainer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			Child.UpdateLayout();
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			Child.View.Measure(widthMeasureSpec, heightMeasureSpec);
			SetMeasuredDimension(Child.View.MeasuredWidth, Child.View.MeasuredHeight);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				if (Child != null)
				{
					RemoveView(Child.View);

					Child = null;
				}
			}

			base.Dispose(disposing);
		}
	}
}
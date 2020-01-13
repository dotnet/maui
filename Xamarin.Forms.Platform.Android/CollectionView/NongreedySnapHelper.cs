#if __ANDROID_29__
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
#else
using Android.Support.V7.Widget;
#endif

namespace Xamarin.Forms.Platform.Android
{
	internal abstract class NongreedySnapHelper : LinearSnapHelper
	{
		// Flag to indicate that the user has scrolled the view, so we can start snapping
		// (otherwise, this would start trying to snap the view as soon as we attached it to the RecyclerView)
		protected bool CanSnap { get; set; }

		bool _disposed;
		RecyclerView _recyclerView;

		public override void AttachToRecyclerView(RecyclerView recyclerView)
		{
			base.AttachToRecyclerView(recyclerView);

			_recyclerView = recyclerView;

			if (_recyclerView != null)
			{
				_recyclerView.ScrollChange += RecyclerViewScrollChange;
			}
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
				if (_recyclerView != null)
				{
					_recyclerView.ScrollChange -= RecyclerViewScrollChange;
				}
			}

			base.Dispose(disposing);
		}

		void RecyclerViewScrollChange(object sender, global::Android.Views.View.ScrollChangeEventArgs e)
		{
			CanSnap = true;
		}
	}
}
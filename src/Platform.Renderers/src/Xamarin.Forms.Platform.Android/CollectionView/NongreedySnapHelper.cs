using AndroidX.RecyclerView.Widget;

namespace Xamarin.Forms.Platform.Android
{
	internal abstract class NongreedySnapHelper : LinearSnapHelper
	{
		// Flag to indicate that the user has scrolled the view, so we can start snapping
		// (otherwise, this would start trying to snap the view as soon as we attached it to the RecyclerView)
		protected bool CanSnap { get; set; }

		bool _disposed;
		RecyclerView _recyclerView;
		InitialScrollListener _initialScrollListener;

		public override void AttachToRecyclerView(RecyclerView recyclerView)
		{
			base.AttachToRecyclerView(recyclerView);

			_recyclerView = recyclerView;

			if (_recyclerView != null)
			{
				StartListeningForScroll();
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
					StopListeningForScroll();
					_initialScrollListener?.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		void StartListeningForScroll()
		{
			_initialScrollListener = new InitialScrollListener(this);
			_recyclerView.AddOnScrollListener(_initialScrollListener);
		}

		void StopListeningForScroll()
		{
			if (_recyclerView != null && _initialScrollListener != null)
			{
				_recyclerView.RemoveOnScrollListener(_initialScrollListener);
			}
		}

		class InitialScrollListener : RecyclerView.OnScrollListener
		{
			readonly NongreedySnapHelper _helper;

			public InitialScrollListener(NongreedySnapHelper helper) => _helper = helper;

			public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
			{
				base.OnScrolled(recyclerView, dx, dy);
				_helper.CanSnap = true;
				_helper.StopListeningForScroll();
			}
		}
	}
}
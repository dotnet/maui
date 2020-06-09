using System.Threading.Tasks;
using ElmSharp;
using ERect = ElmSharp.Rect;
using EScroller = ElmSharp.Scroller;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class Scroller : EScroller
	{
		TaskCompletionSource<bool> _animationTaskComplateSource;
		bool _isAnimation = false;

		public Scroller(EvasObject parent) : base(parent)
		{
		}

		protected Scroller()
		{
		}

		protected override void OnRealized()
		{
			base.OnRealized();
			new SmartEvent(this, RealHandle, "scroll,anim,start").On += (s, e) => _isAnimation = true;
			new SmartEvent(this, RealHandle, "scroll,anim,stop").On += (s, e) =>
			{
				if (_animationTaskComplateSource != null)
				{
					_animationTaskComplateSource.TrySetResult(true);
				}
				_isAnimation = false;
			};
		}

		void CheckTaskCompletionSource()
		{
			if (_animationTaskComplateSource != null)
			{
				if (_animationTaskComplateSource.Task.Status == TaskStatus.Running)
				{
					_animationTaskComplateSource.TrySetCanceled();
				}
			}
			_animationTaskComplateSource = new TaskCompletionSource<bool>();
		}

		public Task ScrollToAsync(int horizontalPageIndex, int verticalPageIndex, bool animated)
		{
			CheckTaskCompletionSource();
			ScrollTo(horizontalPageIndex, verticalPageIndex, animated);
			return animated && _isAnimation ? _animationTaskComplateSource.Task : Task.CompletedTask;
		}

		public Task ScrollToAsync(ERect rect, bool animated)
		{
			CheckTaskCompletionSource();
			ScrollTo(rect, animated);
			return animated && _isAnimation ? _animationTaskComplateSource.Task : Task.CompletedTask;
		}
	}
}
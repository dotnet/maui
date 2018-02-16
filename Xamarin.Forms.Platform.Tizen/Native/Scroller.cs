using System.Threading.Tasks;
using ElmSharp;
using EScroller = ElmSharp.Scroller;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class Scroller : EScroller
	{
		TaskCompletionSource<bool> _animationTaskComplateSource;
		SmartEvent _scrollAnimationStart, _scrollAnimationStop;
		bool _isAnimation = false;

		public Scroller(EvasObject parent) : base(parent)
		{
		}

		protected override void OnRealized()
		{
			base.OnRealized();
			_scrollAnimationStart = new SmartEvent(this, RealHandle, "scroll,anim,start");
			_scrollAnimationStop = new SmartEvent(this, RealHandle, "scroll,anim,stop");
			_scrollAnimationStart.On += (s, e) => _isAnimation = true;
			_scrollAnimationStop.On += (s, e) =>
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

		public Task ScrollToAsync(Rect rect, bool animated)
		{
			CheckTaskCompletionSource();
			ScrollTo(rect, animated);
			return animated && _isAnimation ? _animationTaskComplateSource.Task : Task.CompletedTask;
		}
	}
}
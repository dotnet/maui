using Android.Content;
using Android.Views;
using Android.Widget;

namespace Microsoft.Maui
{
	public partial class WrapperView : FrameLayout
	{
		View? _mainView;

		public WrapperView(Context context)
			: base(context)
		{
			LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
		}

		public View? MainView
		{
			get => _mainView;
			set
			{
				if (_mainView == value)
					return;

				if (_mainView != null)
					RemoveView(_mainView);

				_mainView = value;

				if (_mainView == null)
					return;

				_mainView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
				AddView(_mainView);
			}
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (_mainView == null)
				return;

			_mainView.Measure(widthMeasureSpec, heightMeasureSpec);

			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

			SetMeasuredDimension(_mainView.MeasuredWidth, _mainView.MeasuredHeight);
		}
	}
}
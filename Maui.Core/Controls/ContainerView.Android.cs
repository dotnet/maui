using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using APath = Android.Graphics.Path;

namespace System.Maui.Core.Controls {
	public partial class ContainerView : FrameLayout
	{

		public ContainerView(Context context) : base(context)
		{
			//this.SetWillNotDraw(false);
			//this.SetLayerType(LayerType.Hardware,null);
			this.LayoutParameters = new LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);
		}

		View _mainView;
		public View MainView {
			get => _mainView;
			set
			{
				if (_mainView == value)
					return;
				if (_mainView != null)
				{
					RemoveView(_mainView);
				}

				_mainView = value;
				var parent = _mainView?.Parent as ViewGroup;
				var index = parent?.IndexOfChild(_mainView);
				if (_mainView != null)
				{
					_mainView.LayoutParameters = new ViewGroup.LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);
					AddView(_mainView);
					//parent?.AddView(this, index.Value);

				}
			}
		}
		

		APath currentPath;
		Size lastPathSize;
		protected override void DispatchDraw(Canvas canvas)
		{
			if (ClipShape != null)
			{
				var bounds = new Rectangle(0, 0, canvas.Width, canvas.Height);
				if (lastPathSize != bounds.Size || currentPath == null)
				{
					var path = ClipShape.PathForBounds(bounds);
					currentPath = path.AsAndroidPath();
					lastPathSize = bounds.Size;
				}
				canvas.ClipPath(currentPath);
			}
			base.DispatchDraw(canvas);
		}
		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			_mainView?.Measure(widthMeasureSpec, heightMeasureSpec);
			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
			this.SetMeasuredDimension(_mainView.MeasuredWidth, _mainView.MeasuredHeight);
		}
	}
}

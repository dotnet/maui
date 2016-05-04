using System;
using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	internal class MasterDetailContainer : ViewGroup
	{
		const int DefaultMasterSize = 320;
		const int DefaultSmallMasterSize = 240;
		readonly bool _isMaster;
		readonly MasterDetailPage _parent;
		VisualElement _childView;

		public MasterDetailContainer(MasterDetailPage parent, bool isMaster, Context context) : base(context)
		{
			_parent = parent;
			_isMaster = isMaster;
		}

		public MasterDetailContainer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

		IMasterDetailPageController MasterDetailPageController => _parent as IMasterDetailPageController;

		public VisualElement ChildView
		{
			get { return _childView; }
			set
			{
				if (_childView == value)
					return;

				RemoveAllViews();
				if (_childView != null)
					DisposeChildRenderers();

				_childView = value;

				if (_childView == null)
					return;

				IVisualElementRenderer renderer = Platform.GetRenderer(_childView);
				if (renderer == null)
					Platform.SetRenderer(_childView, renderer = Platform.CreateRenderer(_childView));

				if (renderer.ViewGroup.Parent != this)
				{
					if (renderer.ViewGroup.Parent != null)
						renderer.ViewGroup.RemoveFromParent();
					SetDefaultBackgroundColor(renderer);
					AddView(renderer.ViewGroup);
					renderer.UpdateLayout();
				}
			}
		}

		public int TopPadding { get; set; }

		double DefaultWidthMaster
		{
			get
			{
				double w = Context.FromPixels(Resources.DisplayMetrics.WidthPixels);
				return w < DefaultSmallMasterSize ? w : (w < DefaultMasterSize ? DefaultSmallMasterSize : DefaultMasterSize);
			}
		}

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			bool isShowingPopover = _parent.IsPresented && !MasterDetailPageController.ShouldShowSplitMode;
			if (!_isMaster && isShowingPopover)
				return true;
			return base.OnInterceptTouchEvent(ev);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				RemoveAllViews();
				DisposeChildRenderers();
			}
			base.Dispose(disposing);
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (_childView == null)
				return;

			Rectangle bounds = GetBounds(_isMaster, l, t, r, b);
			if (_isMaster)
				MasterDetailPageController.MasterBounds = bounds;
			else
				MasterDetailPageController.DetailBounds = bounds;

			IVisualElementRenderer renderer = Platform.GetRenderer(_childView);
			renderer.UpdateLayout();
		}

		void DisposeChildRenderers()
		{
			IVisualElementRenderer childRenderer = Platform.GetRenderer(_childView);
			if (childRenderer != null)
				childRenderer.Dispose();
			_childView.ClearValue(Platform.RendererProperty);
		}

		Rectangle GetBounds(bool isMasterPage, int left, int top, int right, int bottom)
		{
			double width = Context.FromPixels(right - left);
			double height = Context.FromPixels(bottom - top);
			double xPos = 0;

			//splitview
			if (MasterDetailPageController.ShouldShowSplitMode)
			{
				//to keep some behavior we have on iPad where you can toggle and it won't do anything 
				bool isDefaultNoToggle = _parent.MasterBehavior == MasterBehavior.Default;
				xPos = isMasterPage ? 0 : (_parent.IsPresented || isDefaultNoToggle ? DefaultWidthMaster : 0);
				width = isMasterPage ? DefaultWidthMaster : _parent.IsPresented || isDefaultNoToggle ? width - DefaultWidthMaster : width;
			}
			else
			{
				//popover make the master smaller
				width = isMasterPage && (Device.Info.CurrentOrientation.IsLandscape() || Device.Idiom == TargetIdiom.Tablet) ? DefaultWidthMaster : width;
			}

			double padding = Context.FromPixels(TopPadding);
			return new Rectangle(xPos, padding, width, height - padding);
		}

		void SetDefaultBackgroundColor(IVisualElementRenderer renderer)
		{
			if (ChildView.BackgroundColor == Color.Default)
			{
				TypedArray colors = Context.Theme.ObtainStyledAttributes(new[] { global::Android.Resource.Attribute.ColorBackground });
				renderer.ViewGroup.SetBackgroundColor(new global::Android.Graphics.Color(colors.GetColor(0, 0)));
			}
		}
	}
}
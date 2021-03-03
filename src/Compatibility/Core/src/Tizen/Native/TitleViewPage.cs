using System;
using ElmSharp;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native
{
	public class TitleViewPage : Native.Box
	{
		Native.Page _page = null;
		View _titleView = null;
		bool _hasNavigationBar = true;

		public TitleViewPage(EvasObject parent, Microsoft.Maui.Controls.Compatibility.Page page, View titleView) : base(parent)
		{
			_page = Platform.GetOrCreateRenderer(page).NativeView as Native.Page;
			_titleView = titleView;
			if (_titleView != null)
			{
				var renderer = Platform.GetOrCreateRenderer(_titleView);
				(renderer as ILayoutRenderer)?.RegisterOnLayoutUpdated();

				this.PackEnd(renderer.NativeView);
			}
			this.PackEnd(_page);
			this.LayoutUpdated += OnLayoutUpdated;
		}

		public bool HasNavigationBar
		{
			get
			{
				return _hasNavigationBar;
			}
			set
			{
				if (_hasNavigationBar != value)
				{
					_hasNavigationBar = value;
					UpdatPageLayout(this, new LayoutEventArgs() { Geometry = this.Geometry });
				}

			}
		}

		void OnLayoutUpdated(object sender, LayoutEventArgs e)
		{
			UpdatPageLayout(sender, e);
		}

		void UpdatPageLayout(object sender, LayoutEventArgs e)
		{
			double dHeight = _titleView.Measure(Forms.ConvertToScaledDP(e.Geometry.Width), Forms.ConvertToScaledDP(e.Geometry.Height)).Request.Height;
			int height = 0;
			if (_hasNavigationBar)
			{
				height = Forms.ConvertToScaledPixel(dHeight);
			}

			var renderer = Platform.GetOrCreateRenderer(_titleView);
			renderer.NativeView.Move(e.Geometry.X, e.Geometry.Y);
			renderer.NativeView.Resize(e.Geometry.Width, height);

			_page.Move(e.Geometry.X, e.Geometry.Y + height);
			_page.Resize(e.Geometry.Width, e.Geometry.Height - height);
		}
	}
}
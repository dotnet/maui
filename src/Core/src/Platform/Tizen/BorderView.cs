using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia.Views;
using Tizen.UIExtensions.Common;

namespace Microsoft.Maui.Platform
{
	public class BorderView : ContentViewGroup//, IWrapperViewCanvas
	{
		//WrapperView _wrapperView;

		public BorderView(IView view) : base(view)
		{
			//_wrapperView = new WrapperView();
			//_wrapperView.Show();
			//Children.Add(_wrapperView);
			//_wrapperView.Lower();
			//Content?.RaiseTop();

			//LayoutUpdated += OnLayout;

		}

		//public IShape? Clip
		//{
		//	get
		//	{
		//		return _wrapperView.Clip;
		//	}
		//	set
		//	{
		//		_wrapperView.Clip = value;
		//	}
		//}


		//public IShadow? Shadow
		//{
		//	get
		//	{
		//		return _wrapperView.Shadow;
		//	}
		//	set
		//	{
		//		_wrapperView.Shadow = value;
		//	}
		//}

		//public IView? Content
		//{
		//	get
		//	{
		//		return _wrapperView.Content;
		//	}
		//	set
		//	{
		//		if (_wrapperView.Content != value)
		//		{
		//			if (_wrapperView.Content != null)
		//			{
		//				Children.Remove(_wrapperView);
		//				_wrapperView.Content = null;
		//			}
		//			_wrapperView.Content = value;
		//			if (_wrapperView.Content != null)
		//			{
		//				Children.Add(_wrapperView);
		//				_wrapperView.RaiseTop();
		//			}
		//		}
		//		_wrapperView.Content = value;
		//	}
		//}

		//public IWrapperViewDrawables Drawables => _wrapperView.Drawables;

		//void OnLayout(object? sender, LayoutEventArgs e)
		//{
		//	//if (Content != null)
		//	//{
		//	//	_wrapperView.Geometry = Geometry;
		//	//}
		//}
	}
}

using System;
using ElmSharp;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia.Views;
using Tizen.UIExtensions.Common;

namespace Microsoft.Maui
{
	public class BorderView : ContentCanvas, IWrapperViewCanvas
	{
		WrapperView _wrapperView;

		public BorderView(EvasObject parent, IView view) : base(parent, view)
		{
			_wrapperView = new WrapperView(parent);
			_wrapperView.Show();
			Children.Add(_wrapperView);
			_wrapperView.Lower();
			Content?.RaiseTop();

			LayoutUpdated += OnLayout;

		}

		public IShape? Clip
		{
			get
			{
				return _wrapperView.Clip;
			}
			set
			{
				_wrapperView.Clip = value;
			}
		}


		public IShadow? Shadow
		{
			get
			{
				return _wrapperView.Shadow;
			}
			set
			{
				_wrapperView.Shadow = value;
			}
		}

		public EvasObject? Content
		{
			get
			{
				return _wrapperView.Content;
			}
			set
			{
				if (_wrapperView.Content != value)
				{
					if (_wrapperView.Content != null)
					{
						Children.Remove(_wrapperView);
						_wrapperView.Content = null;
					}
					_wrapperView.Content = value;
					if (_wrapperView.Content != null)
					{
						Children.Add(_wrapperView);
						_wrapperView.RaiseTop();
					}
				}
				_wrapperView.Content = value;
			}
		}

		public IWrapperViewDrawables Drawables => _wrapperView.Drawables;

		void OnLayout(object? sender, LayoutEventArgs e)
		{
			if (Content != null)
			{
				_wrapperView.Geometry = Geometry;
			}
		}
	}
}

using System;
using ElmSharp;
using Tizen.UIExtensions.ElmSharp;

using Microsoft.Maui.Graphics.Skia.Views;

namespace Microsoft.Maui
{

	public interface IBackgroundCanvas
	{
		public SkiaGraphicsView BackgroundCanvas { get; }
	}

	public partial class WrapperView : Canvas, IBackgroundCanvas
	{
		Lazy<SkiaGraphicsView> _backgroundCanvas;
		EvasObject? _content;

		public WrapperView(EvasObject parent) : base(parent)
		{
			_backgroundCanvas = new Lazy<SkiaGraphicsView>(() =>
			{
				var view = new SkiaGraphicsView(parent);
				view.Show();
				Children.Add(view);
				view.Lower();
				Content?.RaiseTop();
				return view;
			});

			LayoutUpdated += OnLayout;
		}

		void OnLayout(object sender, Tizen.UIExtensions.Common.LayoutEventArgs e)
		{
			if (Content != null)
			{
				Content.Geometry = Geometry;
			}
			if (_backgroundCanvas.IsValueCreated)
			{
				_backgroundCanvas.Value.Geometry = Geometry;
			}
		}

		public EvasObject? Content
		{
			get => _content;
			set
			{
				if (_content != value)
				{
					if (_content != null)
					{
						Children.Remove(_content);
						_content = null;
					}
					_content = value;
					if (_content != null)
					{
						Children.Add(_content);
						_content.RaiseTop();
					}
				}
			}

		}

		public SkiaGraphicsView BackgroundCanvas => _backgroundCanvas.Value;
	}
}

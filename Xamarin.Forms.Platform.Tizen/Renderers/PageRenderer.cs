using System;
using System.Collections.Specialized;
using ElmSharp.Wearable;
using SkiaSharp.Views.Tizen;
using Xamarin.Forms.Platform.Tizen.Native.Watch;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen
{
	/// <summary>
	/// Renderer of ContentPage.
	/// </summary>
	public class PageRenderer : VisualElementRenderer<Page>, SkiaSharp.IBackgroundCanvas
	{
		Native.Page _page;
		Lazy<MoreOption> _moreOption;
		Lazy<SKCanvasView> _backgroundCanvas;

		public SKCanvasView BackgroundCanvas => _backgroundCanvas.Value;

		public PageRenderer()
		{
			RegisterPropertyHandler(Page.BackgroundImageSourceProperty, UpdateBackgroundImage);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
		{
			if (null == _page)
			{
				_page = new Native.Page(Forms.NativeParent);
				_page.LayoutUpdated += OnLayoutUpdated;
				SetNativeView(_page);
			}

			if (Forms.UseSkiaSharp)
			{
				_backgroundCanvas = new Lazy<SKCanvasView>(() =>
				{
					var canvas = new SKCanvasView(Forms.NativeParent);
					canvas.PassEvents = true;
					canvas.PaintSurface += OnBackgroundPaint;
					canvas.Show();
					_page.Children.Add(canvas);
					canvas.Lower();
					return canvas;
				});
			}
			base.OnElementChanged(e);
		}

		protected override void OnElementReady()
		{
			if (Device.Idiom == TargetIdiom.Watch)
			{
				_moreOption = new Lazy<MoreOption>(CreateMoreOption);
				if (Element.ToolbarItems is INotifyCollectionChanged items)
				{
					items.CollectionChanged += OnToolbarCollectionChanged;
				}
				if (Element.ToolbarItems.Count > 0)
				{
					UpdateToolbarItems(true);
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_page != null)
				{
					_page.LayoutUpdated -= OnLayoutUpdated;
				}

				if (Device.Idiom == TargetIdiom.Watch)
				{
					if (Element.ToolbarItems is INotifyCollectionChanged items)
					{
						items.CollectionChanged -= OnToolbarCollectionChanged;
					}

					if (_moreOption.IsValueCreated)
					{
						_moreOption.Value.Clicked -= OnMoreOptionItemClicked;
						_moreOption.Value.Closed -= SendMoreOptionClosed;
						_moreOption.Value.Opened -= SendMoreOptionOpened;
						_moreOption.Value.Items.Clear();
						_moreOption.Value.Unrealize();
					}
				}

				if (Forms.UseSkiaSharp && _backgroundCanvas.IsValueCreated)
				{
					BackgroundCanvas.PaintSurface -= OnBackgroundPaint;
					BackgroundCanvas.Unrealize();
					_backgroundCanvas = null;
				}
			}
			base.Dispose(disposing);
		}

		protected override void UpdateBackgroundColor(bool initialize)
		{
			if (initialize && Element.BackgroundColor.IsDefault)
				return;

			// base.UpdateBackgroundColor() is not called on purpose, we don't want the regular background setting
			if (Element.BackgroundColor.IsDefault || Element.BackgroundColor.A == 0)
				_page.Color = EColor.Transparent;
			else
				_page.Color = Element.BackgroundColor.ToNative();
		}

		protected override void UpdateLayout()
		{
			// empty on purpose
		}

		protected virtual FormsMoreOptionItem CreateMoreOptionItem(ToolbarItem item)
		{
			var moreOptionItem = new FormsMoreOptionItem
			{
				MainText = item.Text,
				ToolbarItem = item
			};
			var icon = item.IconImageSource as FileImageSource;
			if (icon != null)
			{
				var img = new ElmSharp.Image(_moreOption.Value);
				img.Load(ResourcePath.GetPath(icon));
				moreOptionItem.Icon = img;
			}
			return moreOptionItem;
		}

		protected virtual void OnMoreOptionClosed()
		{
		}

		protected virtual void OnMoreOptionOpened()
		{
		}

		void UpdateBackgroundImage(bool initialize)
		{
			if (initialize && Element.BackgroundImageSource.IsNullOrEmpty())
				return;

			// TODO: investigate if we can use the other image source types: stream, font, uri

			var bgImage = Element.BackgroundImageSource as FileImageSource;
			if (bgImage.IsNullOrEmpty())
				_page.File = null;
			else
				_page.File = ResourcePath.GetPath(bgImage);
		}

		void OnLayoutUpdated(object sender, Native.LayoutEventArgs e)
		{
			Element.Layout(e.Geometry.ToDP());

			if (_moreOption != null && _moreOption.IsValueCreated)
			{
				_moreOption.Value.Geometry = _page.Geometry;
			}

			if (_backgroundCanvas != null && _backgroundCanvas.IsValueCreated)
			{
				BackgroundCanvas.Geometry = _page.Geometry;
			}
		}

		void OnBackgroundPaint(object sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			canvas.Clear();

			var bounds = e.Info.Rect;
			var paint = Element.GetBackgroundPaint(bounds);

			if (paint != null)
			{
				using (paint)
				using (var path = bounds.ToPath())
				{
					canvas.DrawPath(path, paint);
				}
			}
		}

		MoreOption CreateMoreOption()
		{
			var moreOption = new MoreOption(_page);
			moreOption.Geometry = _page.Geometry;
			_page.Children.Add(moreOption);
			moreOption.Show();
			moreOption.Clicked += OnMoreOptionItemClicked;
			moreOption.Closed += SendMoreOptionClosed;
			moreOption.Opened += SendMoreOptionOpened;
			return moreOption;
		}

		void SendMoreOptionClosed(object sender, EventArgs e)
		{
			OnMoreOptionClosed();
		}

		void SendMoreOptionOpened(object sender, EventArgs e)
		{
			OnMoreOptionOpened();
		}

		void OnToolbarCollectionChanged(object sender, EventArgs eventArgs)
		{
			UpdateToolbarItems(false);
		}

		void UpdateToolbarItems(bool initialize)
		{
			//clear existing more option items and add toolbar item again on purpose.
			if (!initialize && _moreOption.Value.Items.Count > 0)
			{
				_moreOption.Value.Items.Clear();
			}

			if (Element.ToolbarItems.Count > 0)
			{
				_moreOption.Value.Show();
				foreach (var item in Element.ToolbarItems)
				{
					_moreOption.Value.Items.Add(CreateMoreOptionItem(item));
				}
			}
			else
			{
				_moreOption.Value.Hide();
			}
		}

		void OnMoreOptionItemClicked(object sender, MoreOptionItemEventArgs e)
		{
			var formsMoreOptionItem = e.Item as FormsMoreOptionItem;
			if (formsMoreOptionItem != null)
			{
				((IMenuItemController)formsMoreOptionItem.ToolbarItem)?.Activate();
			}
			_moreOption.Value.IsOpened = false;
		}
	}
}

using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia.Views;
using Tizen.NUI;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;
using TSize = Tizen.UIExtensions.Common.Size;
using TTextAlignment = Tizen.UIExtensions.Common.TextAlignment;
using NColor = Tizen.NUI.Color;

namespace Microsoft.Maui.Platform
{
	public class MauiSearchBar : ViewGroup, IMeasurable
	{
		const double IconSize = 24;
		const double IconMargin = 8;
		const double Radius = 8;

		Entry _entry;
		SkiaGraphicsView _searchButton;
		PointStateType _lastPointState;

		public MauiSearchBar()
		{
			BackgroundColor = new NColor(0.9f, 0.9f, 0.9f, 1);

			_entry = new Entry
			{
				Padding = new Extents(10),
				VerticalTextAlignment = TTextAlignment.Center,
			};
			_entry.KeyEvent += OnKeyEvent;
			_searchButton = new SkiaGraphicsView
			{
				Focusable = true,
				Drawable = new SearchIcon(),
			};
			_searchButton.TouchEvent += OnSearchButtonTouchEvent;
			_searchButton.KeyEvent += OnSearchButtonKeyEvent;

			Children.Add(_entry);
			Children.Add(_searchButton);
			LayoutUpdated += OnLayoutUpdated;
		}

		public Entry Entry => _entry;

		public event EventHandler? SearchButtonPressed;

		protected override void OnEnabled(bool enabled)
		{
			base.OnEnabled(enabled);
			_entry.IsEnabled = enabled;
		}

		TSize IMeasurable.Measure(double availableWidth, double availableHeight)
		{
			if (!string.IsNullOrEmpty(Entry.Text) || !string.IsNullOrEmpty(Entry.Placeholder))
			{
				return new TSize(availableWidth, Math.Max(Entry.NaturalSize.Height, IconSize.ToScaledPixel() + IconMargin.ToScaledPixel()));
			}
			else
			{
				return new TSize(Math.Max(Entry.PixelSize + 10, availableWidth), Math.Max(Entry.PixelSize + 10, IconSize.ToScaledPixel() + IconMargin.ToScaledPixel()));
			}
		}

		void OnSearchButtonClicked(object? sender, EventArgs e)
		{
			SearchButtonPressed?.Invoke(this, EventArgs.Empty);
		}

		bool OnKeyEvent(object source, KeyEventArgs e)
		{
			if (e.Key.IsAcceptKeyEvent())
			{
				SearchButtonPressed?.Invoke(this, EventArgs.Empty);
				return true;
			}
			return false;
		}

		bool OnSearchButtonTouchEvent(object source, TouchEventArgs e)
		{
			var state = e.Touch.GetState(0);

			if (state == PointStateType.Up && _lastPointState == PointStateType.Down)
			{
				SearchButtonPressed?.Invoke(this, EventArgs.Empty);
			}

			_lastPointState = state;
			return state == PointStateType.Up || state == PointStateType.Down;
		}

		bool OnSearchButtonKeyEvent(object source, KeyEventArgs e)
		{
			if (e.Key.IsAcceptKeyEvent())
			{
				SearchButtonPressed?.Invoke(this, EventArgs.Empty);
				return true;
			}
			return false;
		}

		void OnLayoutUpdated(object? sender, LayoutEventArgs e)
		{
			var margin = (float)IconMargin.ToScaledPixel();
			var halfMargin = margin / 2.0f;
			var iconSize = (float)IconSize.ToScaledPixel();
			var iconArea = iconSize + margin;

			CornerRadius = Radius.ToScaledPixel();

			_entry.Position = new Position(halfMargin, 0);
			_entry.SizeHeight = SizeHeight;
			_entry.SizeWidth = SizeWidth - iconArea - halfMargin;
			_searchButton.Position = new Position(_entry.SizeWidth + _entry.Position.X + halfMargin, (float)(SizeHeight - iconSize) / 2.0f);
			_searchButton.SizeHeight = iconSize;
			_searchButton.SizeWidth = iconSize;
		}

		class SearchIcon : IDrawable
		{
			public void Draw(ICanvas canvas, RectF dirtyRect)
			{
				canvas.SaveState();

				var width = dirtyRect.Width;
				var height = dirtyRect.Height;
				var imgWidth = (float)IconSize;
				var imgHeight = (float)IconSize;

				var transX = (width - imgWidth) / 2.0f;
				var transY = (height - imgHeight) / 2.0f;
				canvas.Translate(transX, transY);

				var vBuilder = new PathBuilder();
				var path = vBuilder.BuildPath("M9.5,3A6.5,6.5 0 0,1 16,9.5C16,11.11 15.41,12.59 14.44,13.73L14.71,14H15.5L20.5,19L19,20.5L14,15.5V14.71L13.73,14.44C12.59,15.41 11.11,16 9.5,16A6.5,6.5 0 0,1 3,9.5A6.5,6.5 0 0,1 9.5,3M9.5,5C7,5 5,7 5,9.5C5,12 7,14 9.5,14C12,14 14,12 14,9.5C14,7 12,5 9.5,5Z");

				canvas.FillColor = Colors.Black;
				canvas.FillPath(path);
				canvas.RestoreState();
			}
		}
	}
}

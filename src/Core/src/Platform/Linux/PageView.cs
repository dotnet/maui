using System;
using Gtk;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{

	public class PageView : Gtk.Box
	{

		public PageView() : base(Orientation.Horizontal, 0) { }

		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }

		internal Func<Rectangle, Size>? CrossPlatformArrange { get; set; }

		Widget? _content;

		public Widget? Content
		{
			get => _content;
			set
			{
				if (_content != null && value != null)
				{
					this.ReplaceChild(_content, value);

				}
				else if (value != null)
				{
					PackStart(value, true, true, 0);
				}

				_content = value;

			}
		}

		protected override void OnGetPreferredHeightForWidth(int width, out int minimumHeight, out int naturalHeight)
		{
			base.OnGetPreferredHeightForWidth(width, out minimumHeight, out naturalHeight);

			if (Content == null) return;

			Content.GetPreferredHeightForWidth(width, out var childMinimumHeight, out var childNaturalHeight);
			minimumHeight = Math.Max(Math.Max(minimumHeight, childMinimumHeight), hr);
			naturalHeight = Math.Max(Math.Max(naturalHeight, childNaturalHeight), hr);
			hr = 0;
			
		}

		int hr = 0;

		protected override void OnGetPreferredWidthForHeight(int height, out int minimumWidth, out int naturalWidth)
		{
			base.OnGetPreferredWidthForHeight(height, out minimumWidth, out naturalWidth);
			hr = Math.Max(hr, height);

			if (Content == null) return;

			Content.GetPreferredWidthForHeight(height, out var childMinimumWidth, out var childNaturalWidth);
			minimumWidth = Math.Max(minimumWidth, childMinimumWidth);
			naturalWidth = Math.Max(naturalWidth, childNaturalWidth);
		}

	}

}
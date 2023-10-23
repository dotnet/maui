using System;
using Gtk;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{

	public class ContentView : Gtk.Box
	{

		public ContentView() : base(Orientation.Horizontal, 0) { }

		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }

		internal Func<Rect, Size>? CrossPlatformArrange { get; set; }

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

	}

}
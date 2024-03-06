using System;
using Gtk;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{

	public class ContentView : EventBox, ICrossPlatformLayoutBacking
	{
		public ContentView() : base() { }

		public ICrossPlatformLayout? CrossPlatformLayout { get; set; }

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
					//PackStart(value, true, false, 0);
					Child = value;
				}

				_content = value;
			}
		}
		
		Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return CrossPlatformLayout?.CrossPlatformMeasure(widthConstraint, heightConstraint) ?? Size.Zero;
		}

		Size CrossPlatformArrange(Rect bounds)
		{
			return CrossPlatformLayout?.CrossPlatformArrange(bounds) ?? Size.Zero;
		}
	}

}
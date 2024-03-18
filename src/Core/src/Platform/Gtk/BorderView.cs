using Gdk;
using Gtk;

namespace Microsoft.Maui.Platform;

// https://docs.gtk.org/gtk3/class.Frame.html

public class BorderView : Frame, ICrossPlatformLayoutBacking
{

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
				Child = value;
				Child.Expand = true;
			}

			_content = value;
		}
	}

}
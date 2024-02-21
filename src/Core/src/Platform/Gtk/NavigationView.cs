using Gtk;

namespace Microsoft.Maui.Platform
{

	public class NavigationView : Gtk.Box
	{

		public NavigationView() : base()
		{
			Orientation = Orientation.Horizontal;
		}

		Widget? _content;

		public Widget? Content
		{
			get => _content;
			set
			{
				if (_content == value)
					return;

				if (_content is { })
					Remove(_content);

				// _content?.Unparent();

				_content = value;

				if (_content is { })
				{
					PackStart(_content, true, true, 0);
				}
			}
		}

	}

}
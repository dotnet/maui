using Microsoft.Maui;

namespace Microsoft.Maui.Controls
{
	public class ControlsWindow : VisualElement, IWindow
	{
		private Page _page;

		public ControlsWindow(IPage page)
		{
			Page = (Page)page;
		}

		public Page Page
		{
			get => _page;
			set
			{
				if (_page != null)
					OnChildRemoved(_page, 0);

				_page = value;

				if (value != null)
					OnChildAdded(value);
			}
		}

		IView IWindow.View
		{
			get => (IView)Page;
			set => Page = (Page)value;
		}
	}
}
using ElmSharp;
using Tizen.UIExtensions.ElmSharp;
using EToolbar = ElmSharp.Toolbar;

namespace Tizen.UIExtensions.Shell
{
	public class Tabs : EToolbar, ITabs
	{
		TabsType _type;

		public Tabs(EvasObject parent) : base(parent)
		{
			Style = ElmSharp.ThemeConstants.Toolbar.Styles.Material;
			SelectionMode = ToolbarSelectionMode.Always;
		}

		public TabsType Scrollable
		{
			get => _type;
			set
			{
				switch (value)
				{
					case TabsType.Fixed:
						this.ShrinkMode = ToolbarShrinkMode.Expand;
						break;
					case TabsType.Scrollable:
						this.ShrinkMode = ToolbarShrinkMode.Scroll;
						break;
				}
				_type = value;
			}
		}
	}
}

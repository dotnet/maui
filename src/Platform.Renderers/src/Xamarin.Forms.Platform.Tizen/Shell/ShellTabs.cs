using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ShellTabs : Toolbar, IShellTabs
	{

		ShellTabsType _type;
		public ShellTabs(EvasObject parent) : base(parent)
		{
			Style = ThemeConstants.Toolbar.Styles.Material;
			SelectionMode = ToolbarSelectionMode.Always;
		}

		public ShellTabsType Scrollable
		{
			get => _type;
			set
			{
				switch (value)
				{
					case ShellTabsType.Fixed:
						this.ShrinkMode = ToolbarShrinkMode.Expand;
						break;
					case ShellTabsType.Scrollable:
						this.ShrinkMode = ToolbarShrinkMode.Scroll;
						break;
				}
				_type = value;
			}
		}

		public EvasObject NativeView
		{
			get
			{
				return this;
			}
		}
	}
}

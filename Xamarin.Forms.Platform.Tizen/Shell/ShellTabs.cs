using System;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ShellTabs : Toolbar, IShellTabs
	{
		ShellTabsType _type;
		public ShellTabs(EvasObject parent) : base(parent)
		{
			Style = "material";
		}

		public ShellTabsType Type
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

		public EvasObject TargetView
		{
			get
			{
				return this;
			}
		}
	}
}

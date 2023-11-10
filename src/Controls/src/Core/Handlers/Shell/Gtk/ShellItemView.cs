using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Maui.Controls.Handlers.Items;
using Gtk;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Controls.Platform
{

	public class ShellItemView : NotImplementedView
	{

		public ShellItemView(object view) : base(nameof(ShellItemView)) { }

		[MissingMapper]
		public void UpdateTabBar(bool b)
		{ }

		[MissingMapper]
		public void UpdateCurrentItem(object current)
		{ }

		[MissingMapper]
		public void UpdateBottomTabBarColors(Color tabBarBackgroudColor, Color tabBarTitleColor, Color tabBarUnselectedColor)
		{ }

	}

}
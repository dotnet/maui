using System;
using ElmSharp;
using EColor = ElmSharp.Color;
using EToolbarItemEventArgs = ElmSharp.ToolbarItemEventArgs;

namespace Tizen.UIExtensions.Shell
{
	public interface ITabs
	{
		TabsType Scrollable { get; set; }

		EColor BackgroundColor { get; set; }

		ToolbarItem SelectedItem { get; }

		event EventHandler<EToolbarItemEventArgs> Selected;

		ToolbarItem Append(string label, string icon);

		ToolbarItem Append(string label);

		ToolbarItem InsertBefore(ToolbarItem before, string label, string icon);
	}

	public enum TabsType
	{
		Fixed,
		Scrollable
	}
}

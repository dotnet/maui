using System;
using ElmSharp;
using EColor = ElmSharp.Color;
using EToolbarItem = ElmSharp.ToolbarItem;

namespace System.Maui.Platform.Tizen
{
	public interface IShellTabs
	{
		ShellTabsType Type { get; set; }

		EvasObject TargetView { get; }

		EColor BackgroundColor { get; set; }

		EToolbarItem SelectedItem { get; }

		event EventHandler<ToolbarItemEventArgs> Selected;

		EToolbarItem Append(string label, string icon);

		EToolbarItem InsertBefore(EToolbarItem before, string label, string icon);
	}

	public enum ShellTabsType
	{
		Fixed,
		Scrollable
	}
}

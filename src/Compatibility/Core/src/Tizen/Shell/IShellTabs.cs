using System;
using ElmSharp;
using EColor = ElmSharp.Color;
using EToolbarItem = ElmSharp.ToolbarItem;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public interface IShellTabs
	{
		ShellTabsType Scrollable { get; set; }

		EvasObject NativeView { get; }

		EColor BackgroundColor { get; set; }

		EToolbarItem SelectedItem { get; }

		event EventHandler<ToolbarItemEventArgs> Selected;

		EToolbarItem Append(string label, string icon);
		EToolbarItem Append(string label);

		EToolbarItem InsertBefore(EToolbarItem before, string label, string icon);
	}

	public enum ShellTabsType
	{
		Fixed,
		Scrollable
	}
}

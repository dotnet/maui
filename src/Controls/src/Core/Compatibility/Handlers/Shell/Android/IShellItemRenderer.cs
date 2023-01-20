#nullable disable
using System;
using AndroidX.Fragment.App;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellItemRenderer : IDisposable
	{
		Fragment Fragment { get; }

		ShellItem ShellItem { get; set; }

		event EventHandler Destroyed;
	}
}
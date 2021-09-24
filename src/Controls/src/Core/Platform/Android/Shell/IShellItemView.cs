using System;
using AndroidX.Fragment.App;

namespace Microsoft.Maui.Controls.Platform
{
	public interface IShellItemView : IDisposable
	{
		Fragment Fragment { get; }

		ShellItem ShellItem { get; set; }

		event EventHandler Destroyed;
	}
}
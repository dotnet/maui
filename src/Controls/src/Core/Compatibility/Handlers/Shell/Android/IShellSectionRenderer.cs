#nullable disable
using System;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellSectionRenderer : IShellObservableFragment, IDisposable
	{
		ShellSection ShellSection { get; set; }
	}
}
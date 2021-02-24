using System;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public interface IShellSectionRenderer : IShellObservableFragment, IDisposable
	{
		ShellSection ShellSection { get; set; }
	}
}
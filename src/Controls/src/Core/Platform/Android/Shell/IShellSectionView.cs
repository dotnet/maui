using System;

namespace Microsoft.Maui.Controls.Platform
{
	public interface IShellSectionView : IShellObservableFragment, IDisposable
	{
		ShellSection ShellSection { get; set; }
	}
}
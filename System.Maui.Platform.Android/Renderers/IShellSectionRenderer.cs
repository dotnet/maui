using System;

namespace System.Maui.Platform.Android
{
	public interface IShellSectionRenderer : IShellObservableFragment, IDisposable
	{
		ShellSection ShellSection { get; set; }
	}
}
using System;

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellSectionRenderer : IShellObservableFragment, IDisposable
	{
		ShellSection ShellSection { get; set; }
	}
}
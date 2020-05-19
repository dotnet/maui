using System;
using UIKit;

namespace System.Maui.Platform.iOS
{
	public interface IShellSectionRenderer : IDisposable
	{
		bool IsInMoreTab { get; set; }
		ShellSection ShellSection { get; set; }
		UIViewController ViewController { get; }
	}
}
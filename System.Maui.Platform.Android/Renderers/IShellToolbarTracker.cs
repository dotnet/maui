using System;

namespace System.Maui.Platform.Android
{
	public interface IShellToolbarTracker : IDisposable
	{
		Page Page { get; set; }

		bool CanNavigateBack { get; set; }

		Color TintColor { get; set; }
	}
}
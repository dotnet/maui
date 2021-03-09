using System;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public interface IShellToolbarTracker : IDisposable
	{
		Page Page { get; set; }

		bool CanNavigateBack { get; set; }

		Color TintColor { get; set; }
	}
}
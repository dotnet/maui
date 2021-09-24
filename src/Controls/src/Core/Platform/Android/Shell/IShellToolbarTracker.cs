using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Platform
{
	public interface IShellToolbarTracker : IDisposable
	{
		Page Page { get; set; }

		bool CanNavigateBack { get; set; }

		Color TintColor { get; set; }
	}
}
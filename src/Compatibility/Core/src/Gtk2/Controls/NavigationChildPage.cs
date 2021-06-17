using System;

namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK.Controls
{
	public class NavigationChildPage : IDisposable
	{
		bool _disposed;

		public NavigationChildPage(Microsoft.Maui.Controls.Compatibility.Page page)
		{
			Page = page;
			Identifier = Guid.NewGuid().ToString();
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				Page = null;
			}
		}

		public string Identifier { get; set; }

		public Microsoft.Maui.Controls.Compatibility.Page Page { get; private set; }
	}
}

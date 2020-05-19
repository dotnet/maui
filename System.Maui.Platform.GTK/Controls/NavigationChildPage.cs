using System;

namespace System.Maui.Platform.GTK.Controls
{
	public class NavigationChildPage : IDisposable
	{
		bool _disposed;

		public NavigationChildPage(System.Maui.Page page)
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

		public System.Maui.Page Page { get; private set; }
	}
}

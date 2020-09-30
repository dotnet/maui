using System;

namespace Xamarin.Forms.Platform.GTK.Controls
{
	public class NavigationChildPage : IDisposable
	{
		bool _disposed;

		public NavigationChildPage(Xamarin.Forms.Page page)
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

		public Xamarin.Forms.Page Page { get; private set; }
	}
}

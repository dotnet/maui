using System;
using System.Diagnostics;

namespace System.Maui
{

	[DebuggerDisplay("Location = {Location}")]
	public class ShellNavigationState
	{
		Uri _fullLocation;

		internal Uri FullLocation
		{
			get => _fullLocation;
			set
			{
				_fullLocation = value;
				Location = Routing.Remove(value, true, true);
			}
		}

		public Uri Location
		{
			get;
			private set;
		}

		public ShellNavigationState() { }
		public ShellNavigationState(string location)
		{
			var uri = ShellUriHandler.CreateUri(location);

			if (uri.IsAbsoluteUri)
				uri = new Uri($"/{uri.PathAndQuery}", UriKind.Relative);

			FullLocation = uri;
		}

		public ShellNavigationState(Uri location) => FullLocation = location;
		public static implicit operator ShellNavigationState(Uri uri) => new ShellNavigationState(uri);
		public static implicit operator ShellNavigationState(string value) => new ShellNavigationState(value);
	}
}

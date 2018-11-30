using System;

namespace Xamarin.Forms
{
	public class ShellNavigationState
	{
		public Uri Location { get; set; }

		public ShellNavigationState() { }
		public ShellNavigationState(string location) => Location = new Uri(location);
		public ShellNavigationState(Uri location) => Location = location;
		public static implicit operator ShellNavigationState(Uri uri) => new ShellNavigationState(uri);
		public static implicit operator ShellNavigationState(string value) => new ShellNavigationState(value);
	}
}
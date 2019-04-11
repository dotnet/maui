using System;
using System.Diagnostics;

namespace Xamarin.Forms
{

	[DebuggerDisplay("Location = {Location}")]
	public class ShellNavigationState
	{
		public Uri Location { get; set; }

		public ShellNavigationState() { }
		public ShellNavigationState(string location) => Location = new Uri(location, UriKind.RelativeOrAbsolute);
		public ShellNavigationState(Uri location) => Location = location;
		public static implicit operator ShellNavigationState(Uri uri) => new ShellNavigationState(uri);
		public static implicit operator ShellNavigationState(string value) => new ShellNavigationState(value);
	}
}
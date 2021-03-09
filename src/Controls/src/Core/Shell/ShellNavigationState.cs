using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Maui.Controls
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
			}
		}

		public Uri Location
		{
			get;
			private set;
		}

		public ShellNavigationState() { }
		public ShellNavigationState(string location) : this(location, true)
		{
		}

		internal ShellNavigationState(string location, bool trimForUser)
		{
			var uri = ShellUriHandler.CreateUri(location);

			if (uri.IsAbsoluteUri)
				uri = new Uri($"/{uri.PathAndQuery}", UriKind.Relative);

			FullLocation = uri;

			if (trimForUser)
				Location = TrimDownImplicitAndDefaultPaths(FullLocation);
			else
				Location = FullLocation;
		}

		public ShellNavigationState(Uri location)
		{
			FullLocation = location;
			Location = TrimDownImplicitAndDefaultPaths(FullLocation);
		}

		public static implicit operator ShellNavigationState(Uri uri) => new ShellNavigationState(uri);
		public static implicit operator ShellNavigationState(string value) => new ShellNavigationState(value);

		static Uri TrimDownImplicitAndDefaultPaths(Uri uri)
		{
			uri = ShellUriHandler.FormatUri(uri, null);

			// don't trim relative pushes
			if (!uri.OriginalString.StartsWith($"{Routing.PathSeparator}{Routing.PathSeparator}"))
				return uri;

			string[] parts = uri.OriginalString.TrimEnd(Routing.PathSeparator[0]).Split(Routing.PathSeparator[0]);

			List<string> toKeep = new List<string>();

			// iterate over the shellitem/section/content
			for (int i = 2; i < 5 && i < parts.Length; i++)
			{
				if (!(Routing.IsDefault(parts[i])) && !(Routing.IsImplicit(parts[i])))
				{
					toKeep.Add(parts[i]);
				}
				else if (i == 4)
				{
					// if all the routes are default then just put the last
					// shell content page as the route
					if (toKeep.Count == 0)
						toKeep.Add(parts[i]);
				}
			}

			// Always include pushed pages
			for (int i = 5; i < parts.Length; i++)
			{
				toKeep.Add(parts[i]);
			}

			toKeep.Insert(0, "");
			toKeep.Insert(0, "");
			return new Uri(string.Join(Routing.PathSeparator, toKeep), UriKind.Relative);
		}
	}
}

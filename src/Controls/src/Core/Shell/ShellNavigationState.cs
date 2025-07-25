#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Represents the current navigation state of a <see cref="Shell"/>, expressed as a URI.
	/// </summary>
	[DebuggerDisplay("Location = {Location}")]
	[TypeConverter(typeof(ShellNavigationStateTypeConverter))]
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

		/// <summary>
		/// Gets the URI representing the current navigation location in the Shell.
		/// </summary>
		public Uri Location
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ShellNavigationState"/> class.
		/// </summary>
		public ShellNavigationState() { }
		/// <summary>
		/// Initializes a new instance of the <see cref="ShellNavigationState"/> class with the specified location string.
		/// </summary>
		/// <param name="location">A string representing the navigation location URI.</param>
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

		/// <summary>
		/// Initializes a new instance of the <see cref="ShellNavigationState"/> class with the specified URI.
		/// </summary>
		/// <param name="location">A <see cref="Uri"/> representing the navigation location.</param>
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
			if (!uri.OriginalString.StartsWith("//", StringComparison.Ordinal))
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
#nullable enable
		private sealed class ShellNavigationStateTypeConverter : TypeConverter
		{
			public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => false;
			public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? cultureInfo, object? value, Type? destinationType) => throw new NotSupportedException();

			public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
				=> sourceType == typeof(string) || sourceType == typeof(Uri);

			public override object? ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
				=> value switch
				{
					string str => (ShellNavigationState)str,
					Uri uri => (ShellNavigationState)uri,
					_ => throw new NotSupportedException(),
				};
		}
	}
}
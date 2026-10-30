using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>Builds the URIs handed to the desktop's default handlers.</summary>
static class LaunchUris
{
	/// <summary>RFC 6068 <c>mailto:</c> URI for a message (attachments are not representable).</summary>
	internal static string Mailto(EmailMessage message)
	{
		var sb = new StringBuilder("mailto:");
		sb.Append(string.Join(",", (message.To ?? new List<string>()).Select(EscapeAddress)));

		var query = new List<string>();
		AddList(query, "cc", message.Cc);
		AddList(query, "bcc", message.Bcc);
		if (!string.IsNullOrEmpty(message.Subject))
			query.Add("subject=" + Uri.EscapeDataString(message.Subject));
		if (!string.IsNullOrEmpty(message.Body))
			query.Add("body=" + Uri.EscapeDataString(message.Body));

		if (query.Count > 0)
			sb.Append('?').Append(string.Join("&", query));

		return sb.ToString();
	}

	/// <summary>RFC 5870 <c>geo:</c> URI; a name is passed as <c>?q=lat,lon(name)</c>, which GNOME Maps and Marble understand.</summary>
	internal static string Geo(double latitude, double longitude, string? name = null)
	{
		var coordinates = Format(latitude) + "," + Format(longitude);
		return string.IsNullOrEmpty(name)
			? "geo:" + coordinates
			: "geo:" + coordinates + "?q=" + coordinates + Uri.EscapeDataString("(" + name + ")");
	}

	/// <summary>A <c>geo:0,0?q=</c> search URI from the textual parts of a placemark, or null when it has none.</summary>
	internal static string? GeoQuery(Placemark placemark)
	{
		var parts = new[]
		{
			placemark.FeatureName, placemark.Thoroughfare is null ? null : (placemark.SubThoroughfare + " " + placemark.Thoroughfare).Trim(),
			placemark.PostalCode, placemark.Locality, placemark.AdminArea, placemark.CountryName,
		};

		var text = string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct());
		return text.Length == 0 ? null : "geo:0,0?q=" + Uri.EscapeDataString(text);
	}

	/// <summary>The <c>file://</c> URI of a local path.</summary>
	internal static string FileUri(string path) =>
		new Uri(System.IO.Path.GetFullPath(path)).AbsoluteUri;

	static string Format(double value) =>
		value.ToString("0.#######", CultureInfo.InvariantCulture);

	static string EscapeAddress(string address) =>
		Uri.EscapeDataString(address).Replace("%40", "@", StringComparison.Ordinal);

	static void AddList(List<string> query, string name, List<string>? addresses)
	{
		if (addresses is { Count: > 0 })
			query.Add(name + "=" + string.Join(",", addresses.Select(EscapeAddress)));
	}
}

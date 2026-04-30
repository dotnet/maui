using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Maui.Windows.Run;

/// <summary>
/// Reads identity and application information from a generated AppxManifest.xml.
/// </summary>
internal sealed class AppxManifestReader
{
	static readonly XNamespace FoundationNs = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";

	public string IdentityName { get; }
	public string Publisher { get; }
	public string Version { get; }
	public string ProcessorArchitecture { get; }
	public string ApplicationId { get; }
	public string Executable { get; }

	AppxManifestReader(string identityName, string publisher, string version, string arch, string appId, string executable)
	{
		IdentityName = identityName;
		Publisher = publisher;
		Version = version;
		ProcessorArchitecture = arch;
		ApplicationId = appId;
		Executable = executable;
	}

	public static AppxManifestReader Load(string manifestPath)
	{
		if (!File.Exists(manifestPath))
			throw new FileNotFoundException($"AppxManifest.xml not found at: {manifestPath}");

		var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit };
		using var reader = XmlReader.Create(manifestPath, settings);
		var doc = XDocument.Load(reader);
		var root = doc.Root ?? throw new InvalidOperationException("AppxManifest.xml has no root element.");

		var identity = root.Element(FoundationNs + "Identity")
			?? throw new InvalidOperationException("AppxManifest.xml missing <Identity> element.");

		var identityName = identity.Attribute("Name")?.Value
			?? throw new InvalidOperationException("AppxManifest.xml <Identity> missing Name attribute.");
		var publisher = identity.Attribute("Publisher")?.Value
			?? throw new InvalidOperationException("AppxManifest.xml <Identity> missing Publisher attribute.");
		var version = identity.Attribute("Version")?.Value ?? "0.0.0.0";
		var arch = identity.Attribute("ProcessorArchitecture")?.Value ?? "neutral";

		var applications = root.Element(FoundationNs + "Applications")
			?? throw new InvalidOperationException("AppxManifest.xml missing <Applications> element.");
		var app = applications.Element(FoundationNs + "Application")
			?? throw new InvalidOperationException("AppxManifest.xml missing <Application> element.");

		var appId = app.Attribute("Id")?.Value
			?? throw new InvalidOperationException("AppxManifest.xml <Application> missing Id attribute.");
		var executable = app.Attribute("Executable")?.Value ?? string.Empty;

		return new AppxManifestReader(identityName, publisher, version, arch, appId, executable);
	}
}

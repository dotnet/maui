#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Maui.Resizetizer
{
	public class GenerateTizenManifest : Task
	{
		[Required]
		public string IntermediateOutputPath { get; set; } = null!;

		[Required]
		public string TizenManifestFile { get; set; } = TizenManifestFileName;

		public string GeneratedFilename { get; set; } = TizenManifestFileName;

		public string? ApplicationId { get; set; }

		public string? ApplicationDisplayVersion { get; set; }

		public string? ApplicationVersion { get; set; }

		public string? ApplicationTitle { get; set; }

		public ITaskItem[]? AppIcon { get; set; }

		public ITaskItem[]? SplashScreen { get; set; }

		[Output]
		public ITaskItem GeneratedTizenManifest { get; set; } = null!;

		const string TizenManifestFileName = "tizen-manifest.xml";

		const string IconDefaultDpiType = "xhdpi";

		const string SharedResourcePath = "shared/res/";

		const string UiApplicationName = "ui-application";

		const string PackageName = "package";

		const string AppidName = "appid";

		const string VersionName = "version";

		const string LabelName = "label";

		const string IconName= "icon";

		const string SplashScreensName = "splash-screens";

		const string SplashScreenName = "splash-screen";

		const string DpiName = "dpi";

		bool _shouldUpdateOriginalManifest;

		string? _tizenManifestFilePath;

		public override bool Execute()
		{
			try
			{
				Directory.CreateDirectory(IntermediateOutputPath);

				_tizenManifestFilePath = Path.Combine(Environment.CurrentDirectory, TizenManifestFile);

				var targetFilename = Path.Combine(IntermediateOutputPath, GeneratedFilename);

				var manifest = XDocument.Load(_tizenManifestFilePath);

				UpdateManifest(manifest);

				manifest.Save(targetFilename);

				GeneratedTizenManifest = new TaskItem(targetFilename);
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);
			}

			return !Log.HasLoggedErrors;
		}

		void UpdateManifest(XDocument tizenManifest)
		{
			var xmlns = tizenManifest.Root!.GetDefaultNamespace();
			var manifest = tizenManifest.Root;
			var uiApplication = manifest.Element(xmlns + UiApplicationName);

			if (manifest == null || uiApplication == null)
			{
				return;
			}

			UpdateSharedManifest(xmlns, manifest);

			UpdateOriginalManifest(tizenManifest);

			UpdateSharedResources(xmlns, manifest);
		}

		void UpdateSharedManifest(XNamespace xmlns ,XElement manifest)
		{
			var uiApplication = manifest.Element(xmlns + UiApplicationName);

			if (!string.IsNullOrEmpty(ApplicationId))
			{
				UpdateElementAttribute(manifest, PackageName, ApplicationId);
				UpdateElementAttribute(uiApplication, AppidName, ApplicationId);
			}

			if (!string.IsNullOrEmpty(ApplicationDisplayVersion))
			{
				if (TryMergeVersionNumbers(ApplicationDisplayVersion, out var finalVersion))
				{
					UpdateElementAttribute(manifest, VersionName, finalVersion);
				}
				else
				{
					Log.LogWarning($"ApplicationDisplayVersion '{ApplicationDisplayVersion}' was not a valid version for Tizen");
				}

			}

			if (!string.IsNullOrEmpty(ApplicationTitle))
			{
				var label = uiApplication.Element(xmlns + LabelName);
				UpdateElementValue(label, ApplicationTitle);
			}
		}

		void UpdateOriginalManifest(XDocument tizenManifest)
		{
			if (_shouldUpdateOriginalManifest)
			{
				tizenManifest.Save(_tizenManifestFilePath);
			}
		}

		void UpdateSharedResources(XNamespace xmlns, XElement menifestElement)
		{
			var uiApplicationElement = menifestElement.Element(xmlns + UiApplicationName);
			var appIconInfo = AppIcon?.Length > 0 ? ResizeImageInfo.Parse(AppIcon[0]) : null;

			if (appIconInfo != null)
			{
				var xiconName = xmlns + IconName;
				var iconElements = uiApplicationElement.Elements(xiconName);
				if (iconElements != null)
				{
					var elementsToRemove = iconElements.Where(d => d.Attribute(DpiName) == null || d.Attribute(DpiName)?.Value == "hdpi" || d.Attribute(DpiName)?.Value == "xhdpi");
					elementsToRemove?.Remove();
				}

				foreach (var dpiPath in DpiPath.Tizen.AppIcon)
				{
					var dpiType = dpiPath.Path.Replace(SharedResourcePath, "");
					var iconElement = new XElement(xmlns + IconName);
					iconElement.SetAttributeValue(DpiName, dpiType);
					iconElement.Value = dpiType + "/" + appIconInfo.OutputName + dpiPath.FileSuffix + ".png";
					uiApplicationElement.AddFirst(iconElement);
				}
				var defaultIconElement = new XElement(xmlns + IconName);
				var defaultDpi = DpiPath.Tizen.AppIcon.Where(n => n.Path.EndsWith(IconDefaultDpiType)).FirstOrDefault();
				defaultIconElement.Value = IconDefaultDpiType + "/" + appIconInfo.OutputName + defaultDpi.FileSuffix + ".png";
				uiApplicationElement.AddFirst(defaultIconElement);
			}

			var splashInfo = SplashScreen?.Length > 0 ? ResizeImageInfo.Parse(SplashScreen[0]) : null;

			if (splashInfo != null)
			{
				var splashscreensElement = uiApplicationElement.Element(xmlns + SplashScreensName);
				if (splashscreensElement == null)
				{
					splashscreensElement = new XElement(xmlns + SplashScreensName);
					uiApplicationElement.Add(splashscreensElement);
				}
				else
				{
					var elementsToRemove = splashscreensElement.Elements(xmlns + SplashScreenName).Where(d => d.Attribute(DpiName)?.Value == "mdpi" || d.Attribute(DpiName)?.Value == "hdpi");
					elementsToRemove?.Remove();
				}

				foreach (var image in TizenSplashUpdater.splashDpiMap)
				{
					var splashscreenElement = new XElement(xmlns + SplashScreenName);
					splashscreenElement.SetAttributeValue("src", image.Value);
					splashscreenElement.SetAttributeValue("type", "img");
					splashscreenElement.SetAttributeValue(DpiName, image.Key.Resolution);
					splashscreenElement.SetAttributeValue("orientation", image.Key.Orientation);
					splashscreenElement.SetAttributeValue("indicator-display", "false");
					splashscreensElement.Add(splashscreenElement);
				}
			}
		}

		void UpdateElementAttribute(XElement element, XName attrName, string? value)
		{
			var attr = element.Attribute(attrName);
			if (attr == null || string.IsNullOrEmpty(attr.Value) || attr.Value != value)
			{
				element.SetAttributeValue(attrName, value);
				_shouldUpdateOriginalManifest = true;
			}
		}

		void UpdateElementValue(XElement element, string? value)
		{
			if (element != null && !string.IsNullOrEmpty(value) && element.Value != value)
			{
				element.Value = value;
				_shouldUpdateOriginalManifest = true;
			}
		}

		public static bool TryMergeVersionNumbers(string? displayVersion, out string? finalVersion)
		{
			displayVersion = displayVersion?.Trim();
			finalVersion = null;

			var parts = displayVersion?.Split('.') ?? Array.Empty<string>();
			if (parts.Length > 3)
				return false;

			var v = new int[3];
			for (var i = 0; i < 3 && i < parts.Length; i++)
			{
				if (!int.TryParse(parts[i], out var parsed))
					return false;

				v[i] = parsed;
			}

			if (!VerifyTizenVersion(v[0], v[1], v[2]))
				return false;

			finalVersion = $"{v[0]:0}.{v[1]:0}.{v[2]:0}";
			return true;
		}

		static bool VerifyTizenVersion(int x, int y, int z)
		{
			return (x < 0 || x > 255 || y < 0 || y > 255 || z < 0 || z > 65535) ? false : true;
		}
	}
}
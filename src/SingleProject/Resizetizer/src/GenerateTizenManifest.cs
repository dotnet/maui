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
		const string ApplicationIdPlaceholder = "maui-application-id-placeholder";
		const string LabelPlaceholder = "maui-application-title-placeholder";
		const string ManifestVersionPlaceholder = "0.0.0";
		const string AppIconPlaceholder = "maui-appicon-placeholder";
		const string TizenManifestFileName = "tizen-manifest.xml";
		const string IconDefaultDpiType = "xhdpi";
		const string IconImageExtension = ".png";
		const string UiApplicationName = "ui-application";
		const string PackageName = "package";
		const string AppidName = "appid";
		const string VersionName = "version";
		const string LabelName = "label";
		const string IconName = "icon";
		const string SplashScreensName = "splash-screens";
		const string SplashScreenName = "splash-screen";
		const string DpiName = "dpi";

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

			UpdateSharedResources(xmlns, manifest);
		}

		void UpdateSharedManifest(XNamespace xmlns, XElement manifest)
		{
			var uiApplication = manifest.Element(xmlns + UiApplicationName);

			if (!string.IsNullOrEmpty(ApplicationId))
			{
				UpdateElementAttribute(manifest, PackageName, ApplicationId, ApplicationIdPlaceholder);
				UpdateElementAttribute(uiApplication, AppidName, ApplicationId, ApplicationIdPlaceholder);
			}

			if (!string.IsNullOrEmpty(ApplicationDisplayVersion))
			{
				if (TryMergeVersionNumbers(ApplicationDisplayVersion, out var finalVersion))
				{
					UpdateElementAttribute(manifest, VersionName, finalVersion, ManifestVersionPlaceholder);
				}
				else
				{
					Log.LogWarning($"ApplicationDisplayVersion '{ApplicationDisplayVersion}' was not a valid version for Tizen");
				}

			}

			if (!string.IsNullOrEmpty(ApplicationTitle))
			{
				var label = uiApplication.Element(xmlns + LabelName);
				if (label == null)
				{
					label = new XElement(xmlns + LabelName);
					uiApplication.AddFirst(label);
				}
				UpdateElementValue(label, ApplicationTitle, LabelPlaceholder);
			}
		}

		void UpdateSharedResources(XNamespace xmlns, XElement manifestElement)
		{
			var uiApplicationElement = manifestElement.Element(xmlns + UiApplicationName);
			var appIconInfo = AppIcon?.Length > 0 ? ResizeImageInfo.Parse(AppIcon[0]) : null;

			if (appIconInfo != null)
			{
				var xiconName = xmlns + IconName;
				var iconElements = uiApplicationElement.Elements(xiconName);

				var iconPlaceholderElements = iconElements.Where(d => d.Value == AppIconPlaceholder);
				foreach (var icon in iconPlaceholderElements)
				{
					if (icon.Attribute(DpiName) == null)
					{
						var defaultDpi = DpiPath.Tizen.AppIcon.Where(n => n.Path.EndsWith(IconDefaultDpiType)).FirstOrDefault();
						icon.Value = IconDefaultDpiType + "/" + appIconInfo.OutputName + defaultDpi.FileSuffix + IconImageExtension;
					}
					else
					{
						string dpiValue = icon.Attribute(DpiName).Value;
						string fileSuffix = dpiValue == IconDefaultDpiType ? "xhigh" : "high";
						icon.Value = dpiValue + "/" + appIconInfo.OutputName + fileSuffix + IconImageExtension;
					}
				}
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

				foreach (var image in TizenSplashUpdater.splashDpiMap)
				{
					var splashElements = splashscreensElement.Elements(xmlns + SplashScreenName).Where(
						d => d.Attribute("type")?.Value == "img"
						&& d.Attribute(DpiName)?.Value == image.Key.Resolution
						&& d.Attribute("orientation")?.Value == image.Key.Orientation
						&& d.Attribute("indicator-display")?.Value == "false");
					if (splashElements.Count() == 0)
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
		}

		void UpdateElementAttribute(XElement element, XName attrName, string? value, string? placeholder)
		{
			var attr = element.Attribute(attrName);
			if (attr == null || string.IsNullOrEmpty(attr.Value) || attr.Value == placeholder)
			{
				element.SetAttributeValue(attrName, value);
			}
		}

		void UpdateElementValue(XElement element, string? value, string? placeholder)
		{
			if (string.IsNullOrEmpty(element.Value) || element.Value == placeholder)
			{
				element.Value = value;
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
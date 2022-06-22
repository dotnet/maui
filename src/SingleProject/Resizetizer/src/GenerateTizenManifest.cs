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
		public string TizenManifestFile { get; set; } = "tizen-manifest.xml";

		public string IntermediateImagesOutputPath { get; set; } = null!;

		public string GeneratedFilename { get; set; } = "tizen-manifest.xml";

		public string? ApplicationId { get; set; }

		public string? ApplicationDisplayVersion { get; set; }

		public string? ApplicationVersion { get; set; }

		public string? ApplicationTitle { get; set; }

		public ITaskItem[]? AppIcon { get; set; }

		public ITaskItem[]? SplashScreen { get; set; }

		[Output]
		public ITaskItem GeneratedTizenManifest { get; set; } = null!;

		const string iconDefaultDpiType = "xhdpi";

		const string sharedResourcePath = "shared/res/";

		public override bool Execute()
		{
			try
			{
				Directory.CreateDirectory(IntermediateOutputPath);

				var tizenManifestFilePath = Path.Combine(Environment.CurrentDirectory, TizenManifestFile);

				var targetFilename = Path.Combine(IntermediateOutputPath, GeneratedFilename);

				var manifest = XDocument.Load(tizenManifestFilePath);

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
			var xuiApplication = xmlns + "ui-application";
			var uiApplication = tizenManifest.Root.Element(xuiApplication);

			if (manifest == null || uiApplication == null)
			{
				return;
			}

			if (!string.IsNullOrEmpty(ApplicationId) || !string.IsNullOrEmpty(ApplicationDisplayVersion) || !string.IsNullOrEmpty(ApplicationVersion))
			{
				if (!string.IsNullOrEmpty(ApplicationId))
				{
					var xpackageName = "package";
					var packageAttr = manifest.Attribute(xpackageName);
					if (packageAttr != null)
					{
						manifest.SetAttributeValue(xpackageName, ApplicationId);
					}

					var xappidName= "appid";
					var appidAttr = uiApplication.Attribute(xappidName);
					if (appidAttr != null)
					{
						uiApplication.SetAttributeValue(xappidName, ApplicationId);
					}
				}

				if (!string.IsNullOrEmpty(ApplicationDisplayVersion) || !string.IsNullOrEmpty(ApplicationVersion))
				{
					var xversionName = "version";
					var versionAttr = manifest.Attribute(xversionName);
					if (versionAttr != null)
					{
						if (!TryMergeVersionNumbers(ApplicationDisplayVersion, ApplicationVersion, out var finalVersion))
						{
							Log.LogWarning($"ApplicationDisplayVersion '{ApplicationDisplayVersion}' was not a valid version for Tizen");
							return;
						}
						manifest.SetAttributeValue(xversionName, finalVersion);
					}
				}
			}

			if (!string.IsNullOrEmpty(ApplicationTitle))
			{
				var xlabelName = xmlns + "label";
				var labelElement = uiApplication.Element(xlabelName);
				if (labelElement != null)
				{
					labelElement.Value = ApplicationTitle;
				}
			}

			var appIconInfo = AppIcon?.Length > 0 ? ResizeImageInfo.Parse(AppIcon[0]) : null;
			if (appIconInfo != null)
			{
				var xiconName = xmlns + "icon";
				var iconElements = uiApplication.Elements(xiconName);
				if (iconElements != null)
				{
					var elementsToRemove = iconElements.Where(d => d.Attribute("dpi") == null || d.Attribute("dpi")?.Value == "hdpi" || d.Attribute("dpi")?.Value == "xhdpi");
					elementsToRemove?.Remove();
				}

				foreach (var dpi in DpiPath.Tizen.AppIcon)
				{
					var dpiType = dpi.Path.Replace(sharedResourcePath, "");
					var iconElement = new XElement(xmlns + "icon");
					iconElement.SetAttributeValue("dpi", dpiType);
					iconElement.Value = dpiType + "/" + appIconInfo.OutputName + dpi.FileSuffix + ".png";
					uiApplication.AddFirst(iconElement);
				}
				var defaultIconElement = new XElement(xmlns + "icon");
				var defaultDpi = DpiPath.Tizen.AppIcon.Where(n => n.Path.EndsWith(iconDefaultDpiType)).FirstOrDefault();
				defaultIconElement.Value = iconDefaultDpiType + "/" + appIconInfo.OutputName + defaultDpi.FileSuffix + ".png";
				uiApplication.AddFirst(defaultIconElement);
			}

			var splashInfo = SplashScreen?.Length > 0 ? ResizeImageInfo.Parse(SplashScreen[0]) : null;
			if (splashInfo != null)
			{
				var splashscreens = uiApplication.Element(xmlns + "splash-screens");
				if (splashscreens == null)
				{
					splashscreens = new XElement(xmlns + "splash-screens");
					uiApplication.Add(splashscreens);
				}
				else
				{
					var elementsToRemove = splashscreens.Elements(xmlns + "splash-screen").Where(d => d.Attribute("dpi")?.Value == "mdpi" || d.Attribute("dpi")?.Value == "hdpi");
					elementsToRemove?.Remove();
				}

				foreach (var image in TizenSplashUpdater.splashDpiMap)
				{
					var splashscreenElement = new XElement(xmlns + "splash-screen");
					splashscreenElement.SetAttributeValue("src", image.Value);
					splashscreenElement.SetAttributeValue("type", "img");
					splashscreenElement.SetAttributeValue("dpi", image.Key.Resolution);
					splashscreenElement.SetAttributeValue("orientation", image.Key.Orientation);
					splashscreenElement.SetAttributeValue("indicator-display", "false");
					splashscreens.Add(splashscreenElement);
				}
			}
		}

		public static bool TryMergeVersionNumbers(string? displayVersion, string? version, out string? finalVersion)
		{
			displayVersion = displayVersion?.Trim();
			version = version?.Trim();
			finalVersion = null;

			var parts = displayVersion?.Split('.') ?? Array.Empty<string>();
			if (parts.Length > 3 && !string.IsNullOrEmpty(version))
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
			if (x < 0 || x > 255 || y < 0 || y > 255)
				return false;
			if (z < 0 || z > 65535)
				return false;
			return true;
		}
	}
}
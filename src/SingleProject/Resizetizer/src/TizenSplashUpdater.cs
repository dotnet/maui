using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SkiaSharp;

namespace Microsoft.Maui.Resizetizer
{
	public class TizenSplashUpdater : Task
	{
		[Required]
		public ITaskItem[] MauiSplashScreen { get; set; }

		[Required]
		public string IntermediateOutputPath { get; set; }

		public string ManifestFile { get; set; } = "tizen-manifest.xml";

		public ILogger Logger { get; private set; }

		public override bool Execute()
		{
			if (UpdateSplashImage())
				UpdateManifest();

			return !Log.HasLoggedErrors;
		}

		const string namespaceURI = "http://tizen.org/ns/packages";
		const string splashDirectoryName = "splash";
		List<string> orientations = new List<string>() { "portrait", "landscape" };
		Size hdSize = new Size(720, 1080);
		Size fhdSize = new Size(1080, 1920);
		Dictionary<(string Resolution, string Orientation), string> splashDpiMap = new Dictionary<(string, string), string>();

		public bool UpdateSplashImage()
		{
			var splash = MauiSplashScreen[0];

			var info = ResizeImageInfo.Parse(splash);

			var image = info.OutputName + ".png";
			var sharedResFullPath = Path.GetFullPath(Path.Combine(IntermediateOutputPath, "shared/res/"));
			var splashFullPath = Path.Combine(sharedResFullPath, splashDirectoryName);

			if (Directory.Exists(splashFullPath))
				Directory.Delete(splashFullPath, true);
			Directory.CreateDirectory(splashFullPath);

			foreach (var dpi in DpiPath.Tizen.SplashScreen)
			{
				var imageOutputPath = Path.GetFullPath(Path.Combine(IntermediateOutputPath, dpi.Path));
				var imageFullPath = Path.Combine(imageOutputPath, image);

				if (File.Exists(imageFullPath))
				{
					var resolution = dpi.Path.Split('-')[1].ToLower();
					foreach (var orientation in orientations)
					{
						var newImage = $"{info.OutputName}.{resolution}.{orientation}.png";

						splashDpiMap.Add((resolution, orientation), $"{splashDirectoryName}/{newImage}");
						UpdateColorAndMoveFile(GetScreenSize(resolution, orientation), imageFullPath, Path.Combine(splashFullPath, newImage));
					}
				}
				else
				{
					Log.LogWarning($"Unable to find splash image at {imageFullPath}.");
					return false;
				}
			}

			return true;
		}

		Size GetScreenSize(string resolution, string orientation) =>
			resolution switch
			{
				"mdpi" => orientation == "portrait" ? hdSize : new Size(hdSize.Height, hdSize.Width),
				_ => orientation == "portrait" ? fhdSize : new Size(fhdSize.Height, fhdSize.Width)
			};

		public void UpdateColorAndMoveFile(Size screenSize, string sourceFilePath, string destFilePath)
		{
			var splash = MauiSplashScreen[0];

			var splashInfo = ResizeImageInfo.Parse(splash);

			var color = splashInfo.Color ?? SKColors.White;

			using var bmp = SKBitmap.Decode(sourceFilePath);
			SKImageInfo info = new SKImageInfo(screenSize.Width, screenSize.Height);

			using var surface = SKSurface.Create(info);
			SKCanvas canvas = surface.Canvas;
			canvas.Clear(color);

			using var paint = new SKPaint
			{
				IsAntialias = true,
				FilterQuality = SKFilterQuality.High
			};

			var left = screenSize.Width <= bmp.Width ? 0 : (screenSize.Width - bmp.Width) / 2;
			var top = screenSize.Height <= bmp.Height ? 0 : (screenSize.Height - bmp.Height) / 2;
			var right = screenSize.Width <= bmp.Width ? left + screenSize.Width : left + bmp.Width;
			var bottom = screenSize.Height <= bmp.Height ? top + screenSize.Height : top + bmp.Height;

			canvas.DrawBitmap(bmp, new SKRect(left, top, right, bottom), paint);
			canvas.Flush();

			var updatedsplash = surface.Snapshot();

			using var data = updatedsplash.Encode(SKEncodedImageFormat.Png, 100);
			using var stream = File.Create(destFilePath);
			data.SaveTo(stream);
		}

		public void UpdateManifest()
		{
			XmlDocument doc = new XmlDocument();
			var xmlPath = Path.Combine(Environment.CurrentDirectory, ManifestFile);
			try
			{
				doc.Load(xmlPath);
			}
			catch
			{
				Log.LogWarning($"Failed to load tizen-manifest.xml");
				return;
			}

			var nsmgr = new XmlNamespaceManager(doc.NameTable);
			nsmgr.AddNamespace("manifest", namespaceURI);
			var uiApplicationNode = doc.SelectSingleNode("//manifest:ui-application", nsmgr);
			if (uiApplicationNode == null)
			{
				Log.LogWarning($"Failed to find <ui-application>");
				return;
			}

			var splashScreensNodeList = doc.SelectNodes("//manifest:splash-screens", nsmgr);
			XmlNode splashScreensNode;
			if (splashScreensNodeList.Count == 0)
			{
				splashScreensNode = doc.CreateElement("splash-screens", namespaceURI);
				uiApplicationNode.AppendChild(splashScreensNode);
			}
			else
			{
				splashScreensNode = splashScreensNodeList[0];
				List<XmlNode> nodesToRemove = new List<XmlNode>();
				foreach (XmlNode splashScreenNode in splashScreensNode.ChildNodes)
				{
					var dpiValue = splashScreenNode.Attributes.GetNamedItem("dpi")?.Value;
					if (dpiValue == "mdpi" || dpiValue == "hdpi")
						nodesToRemove.Add(splashScreenNode);
				}
				foreach (XmlNode node in nodesToRemove)
				{
					splashScreensNode.RemoveChild(node);
				}
			}

			foreach (var image in splashDpiMap)
			{
				var splashScreenNode = doc.CreateElement("splash-screen", namespaceURI);
				splashScreenNode.SetAttribute("src", image.Value);
				splashScreenNode.SetAttribute("type", "img");
				splashScreenNode.SetAttribute("dpi", image.Key.Resolution);
				splashScreenNode.SetAttribute("orientation", image.Key.Orientation);
				splashScreenNode.SetAttribute("indicator-display", "false");
				splashScreensNode.AppendChild(splashScreenNode);
			}

			doc.Save(xmlPath);
		}
	}
}

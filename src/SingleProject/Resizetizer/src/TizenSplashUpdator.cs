using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SkiaSharp;

namespace Microsoft.Maui.Resizetizer
{
	public class TizenSplashUpdator : Task
	{
		[Required]
		public ITaskItem[] MauiSplashScreen { get; set; }

		[Required]
		public string IntermediateOutputPath { get; set; }

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
		Dictionary<string, string> splashDpiMap = new Dictionary<string, string>();

		public bool UpdateSplashImage()
		{
			var splash = MauiSplashScreen[0];
			var image = Path.GetFileNameWithoutExtension(splash.ItemSpec) + ".png";
			var sharedResFullPath = Path.GetFullPath(Path.Combine(IntermediateOutputPath, "shared/res/"));
			var splashFullPath = Path.Combine(sharedResFullPath, splashDirectoryName);

			if (Directory.Exists(splashFullPath))
			{
				Directory.Delete(splashFullPath, true);
			}
			Directory.CreateDirectory(splashFullPath);

			foreach (var dpi in DpiPath.Tizen)
			{
				var imageOutputPath = Path.GetFullPath(Path.Combine(IntermediateOutputPath, dpi.Path));
				var imageFullPath = Path.Combine(imageOutputPath, image);
				if (File.Exists(imageFullPath))
				{
					var resolution = dpi.Path.Split('-')[1].ToLower();
					var newImage = Path.GetFileNameWithoutExtension(splash.ItemSpec) + "." + resolution + ".png";
					splashDpiMap.Add(resolution, $"{splashDirectoryName}/{ newImage }");
					UpdateColorAndMoveFile(imageFullPath, Path.Combine(splashFullPath, newImage));
				}
				else
				{
					Log.LogWarning($"Unable to find splash image at {imageFullPath}.");
					return false;
				}
			}
			return true;
		}

		public void UpdateColorAndMoveFile(string sourceFilePath, string destFilePath)
		{
			var splash = MauiSplashScreen[0];
			var colorMetadata = splash.GetMetadata("Color");
			var color = Utils.ParseColorString(colorMetadata);
			if (color == null)
			{
				if (!string.IsNullOrEmpty(colorMetadata))
				{
					Log.LogWarning($"Unable to parse color value '{colorMetadata}' for '{splash.ItemSpec}'.");
				}
				color = SKColors.White;
			}

			using (SKBitmap bmp = SKBitmap.Decode(sourceFilePath))
			{ 
				SKImageInfo info = new SKImageInfo(bmp.Width, bmp.Height);
				using (SKSurface surface = SKSurface.Create(info))
				{
					SKCanvas canvas = surface.Canvas;
					canvas.Clear(color.Value);
					using SKPaint paint = new SKPaint
					{
						IsAntialias = true,
						FilterQuality = SKFilterQuality.High
					};
					canvas.DrawBitmap(bmp, info.Rect, paint);
					canvas.Flush();

					var updatedsplash = surface.Snapshot();
					using (var data = updatedsplash.Encode(SKEncodedImageFormat.Png, 100))
					{
						using (var stream = File.Create(destFilePath))
						{
							data.SaveTo(stream);
						}
					}
				}
			}
			File.Delete(sourceFilePath);
		}

		public void UpdateManifest()
		{
			XmlDocument doc = new XmlDocument();
			var xmlPath = Environment.CurrentDirectory + "\\tizen-manifest.xml";
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
			if (splashScreensNodeList != null)
			{
				foreach (XmlNode node in splashScreensNodeList)
				{
					uiApplicationNode.RemoveChild(node);
				}
			}

			var splashScreensNode = doc.CreateElement("splash-screens", namespaceURI);
			uiApplicationNode.AppendChild(splashScreensNode);

			foreach(var image in splashDpiMap)
			{
				foreach (var orientation in orientations)
				{
					var splashScreenNode = doc.CreateElement("splash-screen", namespaceURI);
					splashScreenNode.SetAttribute("src", image.Value);
					splashScreenNode.SetAttribute("type", "img");
					splashScreenNode.SetAttribute("dpi", image.Key);
					splashScreenNode.SetAttribute("orientation", orientation);
					splashScreenNode.SetAttribute("indicator-display", "false");
					splashScreenNode.SetAttribute("app-control-operation", "http://tizen.org/appcontrol/operation/default");
					splashScreensNode.AppendChild(splashScreenNode);
				}
			}

			doc.Save(xmlPath);
		}
	}
}

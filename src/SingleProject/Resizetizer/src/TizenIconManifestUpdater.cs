using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace Microsoft.Maui.Resizetizer
{
	internal class TizenIconManifestUpdater
	{
		const string namespaceURI = "http://tizen.org/ns/packages";
		const string tizenManifestFile = "tizen-manifest.xml";
		const string resourcePath = "shared/res/";
		const string defaultDpiType = "xhdpi";

		public TizenIconManifestUpdater(string appIconName, DpiPath[] dpis, ILogger logger)
		{
			AppIconName = appIconName;
			Dpis = dpis;
			Logger = logger;
		}

		public string AppIconName { get; private set; }

		public DpiPath[] Dpis { get; }

		public ILogger Logger { get; private set; }

		public void Update()
		{
			XmlDocument doc = new XmlDocument();
			var xmlPath = Path.Combine(Environment.CurrentDirectory, "platforms", "Tizen", tizenManifestFile);
			try
			{
				doc.Load(xmlPath);
			}
			catch
			{
				Logger.Log($"Failed to load tizen-manifest.xml");
				return;
			}

			var nsmgr = new XmlNamespaceManager(doc.NameTable);
			nsmgr.AddNamespace("manifest", namespaceURI);
			var uiApplicationNode = doc.SelectSingleNode("//manifest:ui-application", nsmgr);
			if (uiApplicationNode == null)
			{
				Logger.Log($"Failed to find <ui-application>");
				return;
			}

			var iconNodes = doc.SelectNodes("//manifest:icon", nsmgr);
			foreach (XmlElement node in iconNodes)
			{
				if (node.HasAttribute("dpi") == false)
				{
					uiApplicationNode.RemoveChild(node);
				}
				else
				{
					foreach (var dpi in Dpis)
					{
						var dpiType = dpi.Path.Replace(resourcePath, "");
						if (node.Attributes["dpi"].Value == dpiType)
						{
							uiApplicationNode.RemoveChild(node);
						}
					}
				}
			}

			foreach (var dpi in Dpis)
			{
				var dpiType = dpi.Path.Replace(resourcePath, "");
				var iconNode = doc.CreateElement("icon", namespaceURI);
				iconNode.SetAttribute("dpi", dpiType);
				iconNode.InnerText = dpiType + "/" + AppIconName + dpi.FileSuffix + ".png";
				uiApplicationNode.PrependChild(iconNode);
			}
			var defaultIconNode = doc.CreateElement("icon", namespaceURI);
			var defaultDpi = Dpis.Where(n => n.Path.EndsWith(defaultDpiType)).FirstOrDefault();
			defaultIconNode.InnerText = defaultDpiType + "/" + AppIconName + defaultDpi.FileSuffix + ".png";
			uiApplicationNode.PrependChild(defaultIconNode);

			doc.Save(xmlPath);
		}
	}
}

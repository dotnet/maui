using System;
using System.Xml;

namespace Microsoft.Maui.Resizetizer
{
	internal class TizenIconManifestUpdator
	{
		const string namespaceURI = "http://tizen.org/ns/packages";

		public TizenIconManifestUpdator(string appIconName, DpiPath[] dpis, ILogger logger)
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
			var xmlPath = Environment.CurrentDirectory + "\\tizen-manifest.xml";
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
			var IconNode = doc.SelectSingleNode("//manifest:icon", nsmgr);
			if (IconNode == null)
			{
				IconNode = doc.CreateElement("icon", namespaceURI);
				uiApplicationNode.AppendChild(IconNode);
			}
			IconNode.InnerText = AppIconName + Dpis[1].FileSuffix + ".png";

			doc.Save(xmlPath);
		}
	}
}

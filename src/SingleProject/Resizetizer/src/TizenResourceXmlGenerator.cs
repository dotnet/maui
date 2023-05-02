using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Microsoft.Maui.Resizetizer
{
	internal class TizenResourceXmlGenerator
	{
		static readonly IDictionary<string, string> resolutionMap = new Dictionary<string, string>
		{
			{ "LDPI", "from 0 to 240" },
			{ "MDPI", "from 241 to 300" },
			{ "HDPI", "from 301 to 380" },
			{ "XHDPI", "from 381 to 480" },
			{ "XXHDPI", "from 481 to 600" },
		};

		const string NamespaceURI = "http://tizen.org/ns/rm";

		public TizenResourceXmlGenerator(string intermediateOutputPath, ILogger logger)
		{
			Logger = logger;
			IntermediateOutputPath = intermediateOutputPath;
		}

		public string IntermediateOutputPath { get; private set; }

		public ILogger Logger { get; private set; }

		public ResizedImageInfo Generate()
		{
			XmlDocument doc = new XmlDocument();
			XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
			doc.AppendChild(docNode);

			XmlNode rootNode = doc.CreateElement("res", NamespaceURI);
			doc.AppendChild(rootNode);

			XmlElement groupImageNode = doc.CreateElement("group-image", NamespaceURI);
			groupImageNode.SetAttribute("folder", "contents");
			rootNode.AppendChild(groupImageNode);

			XmlElement groupLayoutNode = doc.CreateElement("group-layout", NamespaceURI);
			groupLayoutNode.SetAttribute("folder", "contents");
			rootNode.AppendChild(groupLayoutNode);

			XmlElement groupSoundNode = doc.CreateElement("group-sound", NamespaceURI);
			groupSoundNode.SetAttribute("folder", "contents");
			rootNode.AppendChild(groupSoundNode);

			XmlElement groupBinNode = doc.CreateElement("group-bin", NamespaceURI);
			groupBinNode.SetAttribute("folder", "contents");
			rootNode.AppendChild(groupBinNode);

			string outputResourceDir = Path.Combine(IntermediateOutputPath, "res");
			string outputContentsDir = Path.Combine(outputResourceDir, "contents");
			string destination = Path.Combine(outputResourceDir, "res.xml");

			var contentsDirInfo = new DirectoryInfo(outputContentsDir);
			if (!contentsDirInfo.Exists)
			{
				Logger.Log("No 'res/contents/' directory to generate res.xml.");
				return null;
			}
			foreach (DirectoryInfo subDir in contentsDirInfo.GetDirectories())
			{
				if (subDir.Name.Contains("-"))
				{
					var resolution = subDir.Name.Split('-')[1];
					if (resolutionMap.TryGetValue(resolution, out string dpiRange))
					{
						foreach (XmlNode groupNode in rootNode)
						{
							XmlElement node = doc.CreateElement("node", NamespaceURI);
							node.SetAttribute("folder", $"contents/{subDir.Name}");
							node.SetAttribute("screen-dpi-range", dpiRange);
							groupNode.AppendChild(node);

							Logger.Log($"Add {subDir.Name} to {groupNode.Name}");
						}
					}
				}
			}
			doc.Save(destination);
			Logger.Log($"res.xml file has been saved in {outputResourceDir}");
			return new ResizedImageInfo() { Dpi = DpiPath.Tizen.Original, Filename = destination };
		}
	}
}

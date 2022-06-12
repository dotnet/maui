#nullable enable
using System;
using System.IO;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Maui.Resizetizer
{
	public class GenerateAndroidManifestXml : Task
	{
		static readonly XNamespace xmlnsAndroid = "http://schemas.android.com/apk/res/android";

		[Required]
		public ITaskItem Manifest { get; set; } = null!;

		[Required]
		public string IntermediateOutputPath { get; set; } = null!;

		public string? GeneratedFilename { get; set; }

		public ITaskItem[]? AppIcon { get; set; }

		public override bool Execute()
		{
			try
			{
				var xml = XDocument.Load(Manifest.ItemSpec);
				if (UpdateManifest(xml))
				{
					Directory.CreateDirectory(IntermediateOutputPath);

					var outFilename = Path.Combine(IntermediateOutputPath, GeneratedFilename ?? "AndroidManifest.xml");
					xml.Save(outFilename);
				}
				else
				{
					Log.LogMessage("No changes were made to the AndroidManifest.xml");
				}
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);
			}

			return !Log.HasLoggedErrors;
		}

		bool UpdateManifest(XDocument manifest)
		{
			var madeChanges = false;

			var appIconInfo = AppIcon?.Length > 0 ? ResizeImageInfo.Parse(AppIcon[0]) : null;

			var xmlns = manifest.Root!.GetDefaultNamespace();

			// <application android:icon="" android:roundIcon="" />
			if (appIconInfo is not null)
			{
				// <application>
				var xapplication = xmlns + "application";
				var application = manifest.Root.Element(xapplication);
				if (application is null)
				{
					application = new XElement(xapplication);
					manifest.Root.Add(application);
					madeChanges = true;
				}

				// android:icon=""
				{
					var xname = xmlnsAndroid + "icon";
					var attr = application.Attribute(xname);
					if (attr is null || string.IsNullOrEmpty(attr.Value))
					{
						application.SetAttributeValue(xname, "@mipmap/" + appIconInfo.OutputName);
						madeChanges = true;
					}
				}

				// android:roundIcon=""
				{
					var xname = xmlnsAndroid + "roundIcon";
					var attr = application.Attribute(xname);
					if (attr is null || string.IsNullOrEmpty(attr.Value))
					{
						application.SetAttributeValue(xname, "@mipmap/" + appIconInfo.OutputName + "_round");
						madeChanges = true;
					}
				}
			}

			return madeChanges;
		}
	}
}
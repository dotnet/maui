using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics;

[assembly: Microsoft.Maui.Controls.XmlnsDefinition(
		xmlNamespace: "http://example.com/maui-controls", target: "Microsoft.Maui.Controls",
		AssemblyName = "Microsoft.Maui.Controls")]

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class App : Application
	{
		public const string AppName = "CompatibilityGalleryControls";
	}
}

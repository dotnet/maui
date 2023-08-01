using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics;

[assembly: Microsoft.Maui.Controls.XmlnsDefinition ("http://example.com/maui-controls", "Microsoft.Maui.Controls")]

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class App : Application
	{
		public const string AppName = "CompatibilityGalleryControls";
	}
}

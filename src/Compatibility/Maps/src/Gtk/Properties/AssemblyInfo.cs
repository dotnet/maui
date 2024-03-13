using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Maps.Gtk;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Maps;

[assembly: ComVisible(false)]

[assembly: ExportRenderer(typeof(Map), typeof(MapRenderer))]
[assembly: Preserve]
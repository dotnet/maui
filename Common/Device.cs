using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public static partial class Device
	{
		public const string iOS = "iOS";
		public const string Android = "Android";
		public const string UWP = "UWP";
		public const string macOS = "macOS";
		public const string GTK = "GTK";
		public const string Tizen = "Tizen";
		public const string WPF = "WPF";

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool IsInvokeRequired
		{
			get { return PlatformServices.IsInvokeRequired; }
		}
	}
}

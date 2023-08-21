//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using Android.Content.Res;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public interface IDeviceInfoProvider
	{
		Resources Resources { get; }

		event EventHandler ConfigurationChanged;
	}
}
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
using System.Collections.Generic;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public partial class XamlNativeViews : ContentPage
	{
		public XamlNativeViews()
		{
			InitializeComponent();
			BindingContext = new VM { NativeText = "Text set to Native view using native binding" };
		}
	}

	[Preserve(AllMembers = true)]
	public class VM
	{
		public string NativeText { get; set; }
	}
}
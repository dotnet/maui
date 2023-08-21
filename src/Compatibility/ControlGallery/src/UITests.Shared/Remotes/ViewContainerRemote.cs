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
using Xamarin.UITest;

namespace Microsoft.Maui.Controls.Compatibility.UITests
{
	using IApp = Xamarin.UITest.IApp;
	internal sealed class ViewContainerRemote : BaseViewContainerRemote
	{
		public ViewContainerRemote(Xamarin.UITest.IApp app, Enum formsType, string platformViewType)
			: base(app, formsType, platformViewType) { }
	}
}
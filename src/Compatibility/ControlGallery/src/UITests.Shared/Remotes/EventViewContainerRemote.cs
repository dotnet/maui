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
using System.Linq;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Microsoft.Maui.Controls.Compatibility.UITests
{
	using IApp = Xamarin.UITest.IApp;
	internal sealed class EventViewContainerRemote : BaseViewContainerRemote
	{
		public EventViewContainerRemote(Xamarin.UITest.IApp app, Enum formsType, string platformViewType)
			: base(app, formsType, platformViewType)
		{
		}

		public AppResult GetEventLabel()
		{
			App.WaitForElement(q => q.Raw(EventLabelQuery));
			return App.Query(q => q.Raw(EventLabelQuery)).First();
		}
	}
}
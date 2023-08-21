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
using Android.Content;
using Microsoft.Maui.Controls.ControlGallery.Issues;

namespace Microsoft.Maui.Controls.ControlGallery.Android
{
	public class MultiWindowService : IMultiWindowService
	{
		public void OpenWindow(Type type)
		{
			throw new NotImplementedException();
			//if (type == typeof(Issue10182))
			//{
			//	var context = DependencyService.Resolve<Context>();
			//	Intent intent = new Intent(context, typeof(Issue10182Activity));
			//	context.StartActivity(intent);
			//}
		}
	}
}
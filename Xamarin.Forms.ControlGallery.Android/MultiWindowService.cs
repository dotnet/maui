using Android.Content;
using Xamarin.Forms.Controls.Issues;
using System;

namespace Xamarin.Forms.ControlGallery.Android
{
	public class MultiWindowService : IMultiWindowService
	{
		public void OpenWindow(Type type)
		{
			if (type == typeof(Issue10182))
			{
				var context = DependencyService.Resolve<Context>();
				Intent intent = new Intent(context, typeof(Issue10182Activity));
				context.StartActivity(intent);
			}
		}
	}
}
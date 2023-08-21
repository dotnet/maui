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

using System.Collections.Generic;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal static class ViewGroupExtensions
	{
		internal static IEnumerable<T> GetChildrenOfType<T>(this AViewGroup self) where T : AView
		{
			for (var i = 0; i < self.ChildCount; i++)
			{
				AView child = self.GetChildAt(i);
				var typedChild = child as T;
				if (typedChild != null)
					yield return typedChild;

				if (child is AViewGroup)
				{
					IEnumerable<T> myChildren = (child as AViewGroup).GetChildrenOfType<T>();
					foreach (T nextChild in myChildren)
						yield return nextChild;
				}
			}
		}
	}
}
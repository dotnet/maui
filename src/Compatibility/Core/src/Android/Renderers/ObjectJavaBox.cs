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

using Java.Lang;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal sealed class ObjectJavaBox<T> : Object
	{
		public ObjectJavaBox(T instance)
		{
			Instance = instance;
		}

		public T Instance { get; set; }
	}
}
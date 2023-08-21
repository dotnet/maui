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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class ProgressBarGallery : ContentPage
	{
		readonly StackLayout _stack;

		public ProgressBarGallery()
		{
			_stack = new StackLayout();

			var normal = new ProgressBar
			{
				Progress = 0.24
			};

			Content = _stack;

			_stack.Children.Add(normal);
		}
	}
}

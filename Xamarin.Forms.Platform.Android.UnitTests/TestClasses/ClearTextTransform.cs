using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.UnitTests;

[assembly: ExportRenderer(typeof(ClearTextTransform), typeof(ClearTextTransformRenderer))]
namespace Xamarin.Forms.Platform.Android.UnitTests
{
	public class ClearTextTransform : Button
	{

	}

	public class ClearTextTransformRenderer : Xamarin.Forms.Platform.Android.FastRenderers.ButtonRenderer
	{
		public ClearTextTransformRenderer(Context context) : base(context)
		{
			this.TransformationMethod = null;
		}
	}
}
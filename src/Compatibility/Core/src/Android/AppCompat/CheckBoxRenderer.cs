using System;
using Android.Content;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class CheckBoxRenderer : CheckBoxRendererBase
	{
		public CheckBoxRenderer(Context context) : base(context)
		{
		}
	}
}

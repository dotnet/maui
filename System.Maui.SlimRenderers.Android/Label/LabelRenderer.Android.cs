//using System;
//using Android.Content;
//using System.ComponentModel;
//using Android.Content.Res;
//using Android.Graphics;
//#if __ANDROID_29__
//using AndroidX.Core.View;
//#else
//using Android.Support.V4.View;
//#endif
//using Android.Text;
//using Android.Views;
//using Android.Widget;
//using System.Maui.Platform;
//using Xamarin.Forms.Platform.Android;

//namespace System.Maui.Platform
//{
//	public partial class LabelRenderer : AbstractViewRenderer<ILabel, TextView>
//	{
//		public LabelRenderer(PropertyMapper mapper) : base(mapper)
//		{
//		}

//		public static void MapPropertyText(IMauiRenderer renderer, ILabel view)
//		{
//			//var control =
//			//	renderer.NativeView as Platform.Android.FastRenderers.LabelRenderer;

//			//FormsTextView.UpdateText(view, control);
//		}

//		protected override TextView CreateView()
//		{
//			return new FormsTextView(Context);
//		}
//	}
//}

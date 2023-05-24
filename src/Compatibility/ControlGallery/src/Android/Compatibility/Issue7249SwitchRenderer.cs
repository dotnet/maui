//using System;
//using Android.Content;
//using Android.Graphics;
//using Android.OS;
//using Microsoft.Maui.Controls.ControlGallery.Issues;
//using Microsoft.Maui.Controls.Compatibility.Platform.Android;
//using Microsoft.Maui.Controls.Platform;
//using Microsoft.Maui.Platform;
//using static Android.Widget.CompoundButton;

//namespace Microsoft.Maui.Controls.ControlGallery.Android
//{
//	public class Issue7249SwitchRenderer : Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat.SwitchRenderer
//	{
//		Issue7249Switch _view;

//		public Issue7249SwitchRenderer(Context context) : base(context)
//		{

//		}

//		protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
//		{
//			base.OnElementChanged(e);

//			if (e.OldElement != null || e.NewElement == null)
//				return;

//			_view = (Issue7249Switch)Element;

//			if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
//			{
//				if (Control != null)
//				{
//					if (Control.Checked)
//					{
//						Control.TrackDrawable.SetColorFilter(_view.SwitchOnColor, FilterMode.SrcAtop);
//					}
//					else
//					{
//						Control.TrackDrawable.SetColorFilter(_view.SwitchOffColor, FilterMode.SrcAtop);
//					}

//					Control.TrackDrawable.SetColorFilter(_view.SwitchThumbColor, FilterMode.Multiply);
//					Control.CheckedChange += OnCheckedChange;
//				}
//			}
//		}

//		protected override void Dispose(bool disposing)
//		{
//			Control.CheckedChange -= OnCheckedChange;
//			base.Dispose(disposing);
//		}

//		void OnCheckedChange(object sender, CheckedChangeEventArgs e)
//		{
//			if (OperatingSystem.IsAndroidVersionAtLeast(29))
//			{
//				if (Control.Checked)
//				{

//					Control.TrackDrawable.SetColorFilter(new BlendModeColorFilter(_view.SwitchOffColor.ToAndroid(), BlendMode.SrcAtop));
//				}
//				else
//				{
//					Control.TrackDrawable.SetColorFilter(new BlendModeColorFilter(_view.SwitchOffColor.ToAndroid(), BlendMode.SrcAtop));
//				}
//			}
//		}
//	}
//}

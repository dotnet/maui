//using System;
//using System.Collections.Generic;
//using System.Text;
//using Android.Views;
//using Android.Widget;
//using AndroidX.AppCompat.Widget;

//namespace Microsoft.Maui.Handlers
//{
//	public partial class ImageButtonHandler : ViewHandler<IImageButton, AppCompatImageButton>, IImageButtonHandler
//	{
//		ImageView IImageHandler.NativeView => this.NativeView;

//		ButtonHandler.ButtonClickListener IButtonHandler.ClickListener { get; } = new ButtonHandler.ButtonClickListener();

//		ButtonHandler.ButtonTouchListener IButtonHandler.TouchListener { get; } = new ButtonHandler.ButtonTouchListener();

//		protected override AppCompatImageButton CreateNativeView()
//		{
//			return new AppCompatImageButton(Context);
//		}
//	}
//}

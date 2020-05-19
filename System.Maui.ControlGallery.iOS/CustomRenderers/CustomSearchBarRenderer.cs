using System;
using UIKit;
using System.Maui;
using System.Maui.ControlGallery.iOS.CustomRenderers;
using System.Maui.Platform.iOS;

// REMARK: Test renderer to validate that Virtual UpdateCancelButton works

//[assembly: ExportRenderer(typeof(SearchBar), typeof(CustomSearchBarRenderer))]
//namespace System.Maui.ControlGallery.iOS.CustomRenderers
//{
//	public class CustomSearchBarRenderer : SearchBarRenderer
//	{
//		public override void UpdateCancelButton()
//		{
//			base.UpdateCancelButton();			
//			Control.ShowsCancelButton = false;
//		}
//	}
//}
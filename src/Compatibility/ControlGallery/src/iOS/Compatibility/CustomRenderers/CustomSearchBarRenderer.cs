using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.ControlGallery.iOS.CustomRenderers;
using ObjCRuntime;
using UIKit;

// REMARK: Test renderer to validate that Virtual UpdateCancelButton works

//[assembly: ExportRenderer(typeof(SearchBar), typeof(CustomSearchBarRenderer))]
//namespace Microsoft.Maui.Controls.ControlGallery.iOS.CustomRenderers
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
using System;
using UIKit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS.CustomRenderers;
using Microsoft.Maui.Controls.Compatibility;

// REMARK: Test renderer to validate that Virtual UpdateCancelButton works

//[assembly: ExportRenderer(typeof(SearchBar), typeof(CustomSearchBarRenderer))]
//namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS.CustomRenderers
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
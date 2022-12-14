using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class AccessibilityCheck
	{
		public AccessibilityCheck()
		{
			InitializeComponent();

			Loaded += AccessibilityCheck_Loaded;
		}

		Microsoft.Maui.Controls.Window _window;
		AccessibilityDiagnosticsOverlay _overlay;
		//double _firstPosX;
		//double _firstPosY;
		//double _currentMousePosX;
		//double _currentMousePosY;
		
		//bool _tappedWithoutMove;

		//Microsoft.Maui.Controls.View[] _allChildren = null;

		private void AccessibilityCheck_Loaded(object sender, EventArgs e)
		{
			_window = this.GetParentWindow();
			
			_overlay = new AccessibilityDiagnosticsOverlay(_window);
			//_overlay.EnableElementSelector = true;

			//System.Diagnostics.Debug.WriteLine($"_overlay.EnableElementSelector == {_overlay.EnableElementSelector}");

			_window.AddOverlay(_overlay);

			//_allChildren = this.GetVisualTreeDescendants()
			//	.OfType<Microsoft.Maui.Controls.View>()
			//	.Where(x => x != RectangleSelectionCheckBox)
			//	.ToArray();

//#if ANDROID
//			RectangleSelectionCheckBox.IsEnabled = false;
//			_overlay.Tapped += DoHandleOverlayTapped;

//			void DoHandleOverlayTapped(object sender, WindowOverlayTappedEventArgs e)
//			{
//				var p = e.Point;
//				Debug.Print($"{sender.GetType().Name} tapped! ({p.X};{p.Y})");
//				_tappedWithoutMove = false; // No mouse move on iOS/Android
//				HandleTapped(p.X, p.Y);
//			}
//#endif
		}
	}
}
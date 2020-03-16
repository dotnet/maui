using Foundation;
using System;
using System.Collections.Generic;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal class PageContainer : UIView, IUIAccessibilityContainer
	{
		readonly IAccessibilityElementsController _parent;
		List<NSObject> _accessibilityElements = null;
		bool _disposed;

		public PageContainer(IAccessibilityElementsController parent)
		{
			AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			_parent = parent;
		}

		public PageContainer()
		{
			IsAccessibilityElement = false;
		}

		List<NSObject> AccessibilityElements
		{
			get
			{
				// lazy-loading this list so that the expensive call to GetAccessibilityElements only happens when VoiceOver is on.
				if (_accessibilityElements == null || _accessibilityElements.Count == 0)
				{
					_accessibilityElements = _parent.GetAccessibilityElements();
				}
				return _accessibilityElements;
			}
		}

		public void ClearAccessibilityElements()
		{
			_accessibilityElements = null;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				ClearAccessibilityElements();
				_disposed = true;
			}
			base.Dispose(disposing);
		}

		[Export("accessibilityElementCount")]
		nint AccessibilityElementCount()
		{
			if (AccessibilityElements == null || AccessibilityElements.Count == 0)
				return 0;

			// Note: this will only be called when VoiceOver is enabled
			return AccessibilityElements.Count;
		}

		[Export("accessibilityElementAtIndex:")]
		NSObject GetAccessibilityElementAt(nint index)
		{
			if (AccessibilityElements == null || AccessibilityElements.Count == 0)
				return NSNull.Null;

			// Note: this will only be called when VoiceOver is enabled
			return AccessibilityElements[(int)index];
		}

		[Export("indexOfAccessibilityElement:")]
		int GetIndexOfAccessibilityElement(NSObject element)
		{
			if (AccessibilityElements == null || AccessibilityElements.Count == 0)
				return int.MaxValue;

			// Note: this will only be called when VoiceOver is enabled
			return AccessibilityElements.IndexOf(element);
		}
	}
}
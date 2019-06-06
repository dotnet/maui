using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal class PageContainer : UIView, IUIAccessibilityContainer
	{
		readonly IAccessibilityElementsController _parent;
		List<NSObject> _accessibilityElements = new List<NSObject>();
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
				if (_accessibilityElements == null)
				{
					_accessibilityElements = _parent.GetAccessibilityElements();
					if (_accessibilityElements == null)
					{
						NSObject defaultElements = AccessibilityContainer.GetAccessibilityElements();
						if (defaultElements != null)
							_accessibilityElements = NSArray.ArrayFromHandle<NSObject>(defaultElements.Handle).ToList();
					}
				}
				return _accessibilityElements;
			}
		}

		IUIAccessibilityContainer AccessibilityContainer => this;

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
			// Note: this will only be called when VoiceOver is enabled
			return AccessibilityElements.Count;
		}

		[Export("accessibilityElementAtIndex:")]
		NSObject GetAccessibilityElementAt(nint index)
		{
			// Note: this will only be called when VoiceOver is enabled
			return AccessibilityElements[(int)index];
		}

		[Export("indexOfAccessibilityElement:")]
		int GetIndexOfAccessibilityElement(NSObject element)
		{
			// Note: this will only be called when VoiceOver is enabled
			return AccessibilityElements.IndexOf(element);
		}
	}
}
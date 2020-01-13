using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.OS;
#if __ANDROID_29__
using AndroidX.Core.Content;
using AndroidX.AppCompat.Widget;
#else
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
#endif
using Android.Views;
using Android.Views.Accessibility;
using AColor = Android.Graphics.Color;
using AColorRes = Android.Resource.Color;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public class PageRenderer : VisualElementRenderer<Page>, IOrderedTraversalController
	{
		public PageRenderer(Context context) : base(context)
		{
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use PageRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public PageRenderer()
		{
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			base.OnTouchEvent(e);

			return true;
		}

		IPageController PageController => Element as IPageController;

		IOrderedTraversalController OrderedTraversalController => this;

		double _previousHeight;
		bool _isDisposed = false;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				PageController?.SendDisappearing();
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			var pageContainer = Parent as PageContainer;
			if (pageContainer != null && (pageContainer.IsInFragment || pageContainer.Visibility == ViewStates.Gone))
				return;
			PageController.SendAppearing();
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			var pageContainer = Parent as PageContainer;
			if (pageContainer != null && pageContainer.IsInFragment)
				return;
			PageController.SendDisappearing();
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
		{
			Page view = e.NewElement;
			base.OnElementChanged(e);

			if (Id == NoId)
			{
				Id = Platform.GenerateViewId();
			}

			UpdateBackground(false);

			Clickable = true;
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == Page.BackgroundImageSourceProperty.PropertyName)
				UpdateBackground(true);
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackground(false);
			else if (e.PropertyName == VisualElement.HeightProperty.PropertyName)
				UpdateHeight();
		}

		void UpdateHeight()
		{
			// Handle size changes because of the soft keyboard (there's probably a more elegant solution to this)

			// This is only necessary if:
			// - we're navigating back from a page where the soft keyboard was open when the user hit the Navigation Bar 'back' button
			// - the Application's content height has changed because WindowSoftInputModeAdjust was set to Resize
			// - the height has increased (in other words, the last layout was with the keyboard open, and now it's closed)
			var newHeight = Element.Height;

			if (_previousHeight > 0 && newHeight > _previousHeight)
			{
				var nav = Element.Navigation;

				// This update check will fire for all the pages on the stack, but we only need to request a layout for the top one
				if (nav?.NavigationStack != null && nav.NavigationStack.Count > 0 && Element == nav.NavigationStack[nav.NavigationStack.Count - 1])
				{
					// The Forms layout stuff is already correct, we just need to force Android to catch up
					RequestLayout();
				}
			}

			// Cache the height for next time
			_previousHeight = newHeight;
		}

		void UpdateBackground(bool setBkndColorEvenWhenItsDefault)
		{
			Page page = Element;

			_ = this.ApplyDrawableAsync(page, Page.BackgroundImageSourceProperty, Context, drawable =>
			{
				if (drawable != null)
				{
					this.SetBackground(drawable);
				}
				else
				{
					Color bkgndColor = page.BackgroundColor;
					bool isDefaultBkgndColor = bkgndColor.IsDefault;

					// A TabbedPage has no background. See Github6384.
					bool isInShell = page.Parent is BaseShellItem
					|| (page.Parent is TabbedPage && page.Parent?.Parent is BaseShellItem);
					if (isInShell && isDefaultBkgndColor)
					{
						var color = Forms.IsMarshmallowOrNewer ?
							Context.Resources.GetColor(AColorRes.BackgroundLight, Context.Theme) :
							new AColor(ContextCompat.GetColor(Context, global::Android.Resource.Color.BackgroundLight));
						SetBackgroundColor(color);
					}
					else if (!isDefaultBkgndColor || setBkndColorEvenWhenItsDefault)
					{
						SetBackgroundColor(bkgndColor.ToAndroid());
					}
				}
			});
		}

		void IOrderedTraversalController.UpdateTraversalOrder()
		{
			// traversal order wasn't added until API 22
			if ((int)Forms.SdkInt < 22)
				return;

			// since getting and updating the traversal order is expensive, let's only do it when a screen reader is active
			// note that this does NOT get auto updated when you enable TalkBack, so the page will need to be reloaded to enable this path 
			var am = AccessibilityManager.FromContext(Context);
			if (!am.IsEnabled)
				return;

			SortedDictionary<int, List<ITabStopElement>> tabIndexes = null;
			foreach (var child in Element.LogicalChildren)
			{
				if (!(child is VisualElement ve))
					continue;

				tabIndexes = ve.GetSortedTabIndexesOnParentPage(out _);
				break;
			}

			if (tabIndexes == null)
				return;

			AView prevControl = null;
			foreach (var idx in tabIndexes?.Keys)
			{
				var tabGroup = tabIndexes[idx];
				foreach (var child in tabGroup)
				{
					if (child is Layout || 
						!(
							child is VisualElement ve && ve.IsTabStop
							&& AutomationProperties.GetIsInAccessibleTree(ve) != false // accessible == true
							&& ve.GetRenderer()?.View is ITabStop tabStop)
						 )
						continue;

					var thisControl = tabStop.TabStop;

					if (thisControl == null)
						continue;

					// this element should be the first thing focused after the root
					if (prevControl == null)
					{
						thisControl.AccessibilityTraversalAfter = NoId;
					}
					else
					{
						if (thisControl != prevControl)
							thisControl.AccessibilityTraversalAfter = prevControl.Id;
					}

					prevControl = thisControl;
				}
			}
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			base.OnLayout(changed, l, t, r, b);
			OrderedTraversalController.UpdateTraversalOrder();
		}
	}
}

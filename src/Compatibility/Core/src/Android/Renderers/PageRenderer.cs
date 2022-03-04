using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.Views;
using Android.Views.Accessibility;
using AndroidX.Core.Content;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using AColorRes = Android.Resource.Color;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public class PageRenderer : VisualElementRenderer<Page>
	{
		public PageRenderer(Context context) : base(context)
		{
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			base.OnTouchEvent(e);

			return true;
		}

		IPageController PageController => Element as IPageController;

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
			base.OnElementChanged(e);

			if (Id == NoId)
			{
				Id = Platform.GenerateViewId();
			}

			UpdateBackground(false);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Page.BackgroundImageSourceProperty.PropertyName)
				UpdateBackground(true);
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackground(false);
			else if (e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
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
					Brush background = Element.Background;

					if (!Brush.IsNullOrEmpty(background))
						this.UpdateBackground(background);
					else
					{
						Color backgroundColor = page.BackgroundColor;
						bool isDefaultBackgroundColor = backgroundColor == null;

						// A TabbedPage has no background. See Github6384.
						bool isInShell = page.Parent is BaseShellItem ||
						(page.Parent is TabbedPage && page.Parent?.Parent is BaseShellItem);

						if (isInShell && isDefaultBackgroundColor)
						{
							var color = Forms.IsMarshmallowOrNewer ?
								Context.Resources.GetColor(AColorRes.BackgroundLight, Context.Theme) :
								new AColor(ContextCompat.GetColor(Context, AColorRes.BackgroundLight));
							SetBackgroundColor(color);
						}
						else if (!isDefaultBackgroundColor || setBkndColorEvenWhenItsDefault)
						{
							SetBackgroundColor(backgroundColor.ToAndroid());
						}
					}
				}
			});
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			var deviceIndependentLeft = Context.FromPixels(l);
			var deviceIndependentTop = Context.FromPixels(t);
			var deviceIndependentRight = Context.FromPixels(r);
			var deviceIndependentBottom = Context.FromPixels(b);

			var destination = Rect.FromLTRB(deviceIndependentLeft, deviceIndependentTop,
				deviceIndependentRight, deviceIndependentBottom);

			(Element as IView)?.Arrange(destination);
			base.OnLayout(changed, l, t, r, b);
		}
	}
}

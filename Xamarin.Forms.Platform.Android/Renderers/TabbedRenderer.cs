using System;
using System.ComponentModel;
using Android.Content;
using AButton = Android.Widget.Button;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public class TabbedRenderer : VisualElementRenderer<TabbedPage>
	{
		public TabbedRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use TabbedRenderer(Context) instead.")]
		public TabbedRenderer()
		{
			AutoPackage = false;
		}

		IPageController PageController => Element as IPageController;

		protected override void Dispose(bool disposing)
		{
			if (disposing && Element != null && Element.Children.Count > 0)
			{
				RemoveAllViews();
				foreach (Page pageToRemove in Element.Children)
				{
					IVisualElementRenderer pageRenderer = Platform.GetRenderer(pageToRemove);
					if (pageRenderer != null)
						pageRenderer.Dispose();
					pageToRemove.ClearValue(Platform.RendererProperty);
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			PageController.SendAppearing();
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			PageController.SendDisappearing();
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TabbedPage> e)
		{
			base.OnElementChanged(e);

			TabbedPage tabs = e.NewElement;
			if (tabs.CurrentPage != null)
				SwitchContent(tabs.CurrentPage);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == "CurrentPage")
				SwitchContent(Element.CurrentPage);
		}

		protected virtual void SwitchContent(Page view)
		{
			Context.HideKeyboard(this);

			RemoveAllViews();

			if (view == null)
				return;

			if (Platform.GetRenderer(view) == null)
				Platform.SetRenderer(view, Platform.CreateRenderer(view, Context));

			AddView(Platform.GetRenderer(view).View);
		}
	}
}
using System.ComponentModel;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public class PageRenderer : VisualElementRenderer<Page>
	{
		public override bool OnTouchEvent(MotionEvent e)
		{
			base.OnTouchEvent(e);

			return true;
		}

		IPageController PageController => Element as IPageController;

		protected override void Dispose(bool disposing)
		{
			PageController?.SendDisappearing();
			base.Dispose(disposing);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			var pageContainer = Parent as PageContainer;
			if (pageContainer != null && pageContainer.IsInFragment)
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

			UpdateBackgroundColor(view);
			UpdateBackgroundImage(view);

			Clickable = true;
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == Page.BackgroundImageProperty.PropertyName)
				UpdateBackgroundImage(Element);
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor(Element);
		}

		void UpdateBackgroundColor(Page view)
		{
			if (view.BackgroundColor != Color.Default)
				SetBackgroundColor(view.BackgroundColor.ToAndroid());
		}

		void UpdateBackgroundImage(Page view)
		{
			if (!string.IsNullOrEmpty(view.BackgroundImage))
				this.SetBackground(Context.Resources.GetDrawable(view.BackgroundImage));
		}
	}
}
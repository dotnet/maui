using System;
using System.ComponentModel;
using Xamarin.Forms.Platform.WPF.Controls;

namespace Xamarin.Forms.Platform.WPF
{
	public class MasterDetailPageRenderer : FlyoutPageRenderer
	{

	}

	public class FlyoutPageRenderer : VisualPageRenderer<FlyoutPage, FormsFlyoutPage>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<FlyoutPage> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new FormsFlyoutPage() { ContentLoader = new FormsContentLoader() });

					DependencyPropertyDescriptor.FromProperty(FormsFlyoutPage.IsPresentedProperty, typeof(FormsFlyoutPage)).AddValueChanged(Control, OnIsPresentedChanged);
				}
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == FlyoutPage.IsPresentedProperty.PropertyName) // || e.PropertyName == FlyoutPage.FlyoutLayoutBehaviorProperty.PropertyName)
				UpdateIsPresented();
			else if (e.PropertyName == "Master")
				UpdateMasterPage();
			else if (e.PropertyName == "Detail")
				UpdateDetailPage();
		}

		protected override void Appearing()
		{
			base.Appearing();
			UpdateIsPresented();
			UpdateMasterPage();
			UpdateDetailPage();
		}

		void UpdateIsPresented()
		{
			Control.IsPresented = Element.IsPresented;
		}

		void UpdateMasterPage()
		{
			Control.FlyoutPage = Element.Flyout;
		}

		void UpdateDetailPage()
		{
			Control.DetailPage = Element.Detail;
		}

		private void OnIsPresentedChanged(object sender, EventArgs arg)
		{
			((IElementController)Element).SetValueFromRenderer(FlyoutPage.IsPresentedProperty, Control.IsPresented);
		}

		bool _isDisposed;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if (Control != null)
				{
					DependencyPropertyDescriptor.FromProperty(FormsFlyoutPage.IsPresentedProperty, typeof(FormsFlyoutPage))
						.RemoveValueChanged(Control, OnIsPresentedChanged);
				}
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}
	}
}

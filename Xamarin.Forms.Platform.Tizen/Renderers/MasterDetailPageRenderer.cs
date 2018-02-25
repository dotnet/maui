using System;
using Xamarin.Forms.Platform.Tizen.Renderers;

namespace Xamarin.Forms.Platform.Tizen
{
	public class MasterDetailPageRenderer : VisualElementRenderer<MasterDetailPage>
	{
		Native.MasterDetailPage _mdpage;
		MasterDetailContainer _masterContainer = null;
		MasterDetailContainer _detailContainer = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public MasterDetailPageRenderer()
		{
			RegisterPropertyHandler(nameof(Element.Master), UpdateMasterPage);
			RegisterPropertyHandler(nameof(Element.Detail), UpdateDetailPage);
			RegisterPropertyHandler(MasterDetailPage.IsPresentedProperty,
				UpdateIsPresented);
			RegisterPropertyHandler(MasterDetailPage.MasterBehaviorProperty,
				UpdateMasterBehavior);
			RegisterPropertyHandler(MasterDetailPage.IsGestureEnabledProperty,
				UpdateIsGestureEnabled);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<MasterDetailPage> e)
		{
			if (_mdpage == null)
			{
				_mdpage = new Native.MasterDetailPage(Forms.NativeParent)
				{
					IsPresented = e.NewElement.IsPresented,
					Master = _masterContainer = new MasterDetailContainer(Element, true),
					Detail = _detailContainer = new MasterDetailContainer(Element, false),
				};

				_mdpage.IsPresentedChanged += (sender, ev) =>
				{
					Element.IsPresented = ev.IsPresent;
				};
				_mdpage.UpdateIsPresentChangeable += (sender, ev) =>
				{
					(Element as IMasterDetailPageController).CanChangeIsPresented = ev.CanChange;
				};
				SetNativeView(_mdpage);
			}

			if (e.OldElement != null)
			{
				(e.OldElement as IMasterDetailPageController).BackButtonPressed -= OnBackButtonPressed;
				e.OldElement.Appearing -= OnMasterDetailAppearing;
				e.OldElement.Disappearing -= OnMasterDetailDisappearing;
			}

			if (e.NewElement != null)
			{
				(e.NewElement as IMasterDetailPageController).BackButtonPressed += OnBackButtonPressed;
				e.NewElement.Appearing += OnMasterDetailAppearing;
				e.NewElement.Disappearing += OnMasterDetailDisappearing;
			}

			UpdateMasterBehavior();
			base.OnElementChanged(e);
		}

		void OnMasterDetailDisappearing(object sender, EventArgs e)
		{
			_masterContainer?.SendDisappearing();
			_detailContainer?.SendDisappearing();
		}

		void OnMasterDetailAppearing(object sender, EventArgs e)
		{
			_masterContainer?.SendAppearing();
			_detailContainer?.SendAppearing();
		}

		protected override void OnElementReady()
		{
			base.OnElementReady();
			UpdateMasterPage(false);
			UpdateDetailPage(false);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_masterContainer != null)
				{
					_masterContainer.Dispose();
					_masterContainer = null;
				}

				if (_detailContainer != null)
				{
					_detailContainer.Dispose();
					_detailContainer = null;
				}

				if (Element != null)
				{
					Element.Appearing -= OnMasterDetailAppearing;
					Element.Disappearing -= OnMasterDetailDisappearing;
				}
			}

			base.Dispose(disposing);
		}

		protected void UpdateMasterPageRatio(double popoverRatio, double splitRatio)
		{
			_mdpage.PopoverRatio = popoverRatio;
			_mdpage.SplitRatio = splitRatio;
		}

		void OnBackButtonPressed(object sender, BackButtonPressedEventArgs e)
		{
			if ((Element != null) && Element.IsPresented && !_mdpage.IsSplit)
			{
				Element.IsPresented = false;
				e.Handled = true;
			}
		}

		void UpdateMasterBehavior()
		{
			_mdpage.MasterBehavior = Element.MasterBehavior;
		}

		void UpdateMasterPage(bool isInit)
		{
			if (!isInit)
				_masterContainer.ChildView = Element.Master;
		}

		void UpdateDetailPage(bool isInit)
		{
			if (!isInit)
				_detailContainer.ChildView = Element.Detail;
		}

		void UpdateIsPresented()
		{
			_mdpage.IsPresented = Element.IsPresented;
		}

		void UpdateIsGestureEnabled()
		{
			_mdpage.IsGestureEnabled = Element.IsGestureEnabled;
		}
	}
}

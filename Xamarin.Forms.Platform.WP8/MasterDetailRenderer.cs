using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class MasterDetailRenderer : VisualElementRenderer<MasterDetailPage, FrameworkElement>
	{
		readonly SlideTransition _inTransition = new SlideTransition { Mode = SlideTransitionMode.SlideUpFadeIn };
		readonly SlideTransition _outTransition = new SlideTransition { Mode = SlideTransitionMode.SlideDownFadeOut };
		readonly Border _popup = new Border();
		IVisualElementRenderer _detailRenderer;
		IVisualElementRenderer _masterRenderer;

		ITransition _toggleTransition;

		public MasterDetailRenderer()
		{
			AutoPackage = false;
		}

		IMasterDetailPageController MasterDetailPageController => Element as IMasterDetailPageController;

		public bool Visible { get; private set; }

		protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
		{
			UpdateSizes(finalSize.Width, finalSize.Height);
			return base.ArrangeOverride(finalSize);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<MasterDetailPage> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
				((IMasterDetailPageController)e.OldElement).BackButtonPressed -= HandleBackButtonPressed;

			if (e.NewElement != null)
				((IMasterDetailPageController)e.NewElement).BackButtonPressed += HandleBackButtonPressed;

			LoadDetail();
			LoadMaster();

			UpdateSizes(ActualWidth, ActualHeight);

			Loaded += (sender, args) =>
			{
				if (Element.IsPresented)
					Toggle();
				Element.SendAppearing();
			};
			Unloaded += (sender, args) =>
			{
				Element.SendDisappearing();
				if (Visible)
				{
					var platform = (Platform)Element.Platform;
					Canvas container = platform.GetCanvas();

					container.Children.Remove(_popup);
				}
			};
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == "Detail")
			{
				LoadDetail();
				UpdateSizes(ActualWidth, ActualHeight);
			}
			else if (e.PropertyName == "Master")
			{
				LoadMaster();
				UpdateSizes(ActualWidth, ActualHeight);
			}
			else if (e.PropertyName == MasterDetailPage.IsPresentedProperty.PropertyName)
			{
				if (Visible == Element.IsPresented)
					return;
				Toggle();
			}
		}

		internal void Toggle()
		{
			var platform = Element.Platform as Platform;
			Canvas container = platform.GetCanvas();

			if (_toggleTransition != null)
				return;

			if (Visible)
			{
				_toggleTransition = _outTransition.GetTransition(_popup);
				_toggleTransition.Begin();
				_toggleTransition.Completed += (sender, args) =>
				{
					_toggleTransition.Stop();
					container.Children.Remove(_popup);
					_toggleTransition = null;
				};
			}
			else
			{
				_popup.Child = _masterRenderer.ContainerElement;
				container.Children.Add(_popup);

				_toggleTransition = _inTransition.GetTransition(_popup);
				_toggleTransition.Begin();

				_toggleTransition.Completed += (sender, args) =>
				{
					_toggleTransition.Stop();
					_toggleTransition = null;
				};
			}

			Visible = !Visible;

			((IElementController)Element).SetValueFromRenderer(MasterDetailPage.IsPresentedProperty, Visible);
		}

		void HandleBackButtonPressed(object sender, BackButtonPressedEventArgs e)
		{
			if (Visible)
			{
				Toggle();
				e.Handled = true;
			}
		}

		void LoadDetail()
		{
			if (_detailRenderer != null)
				Children.Remove(_detailRenderer.ContainerElement);

			Page detail = Element.Detail;
			if (Platform.GetRenderer(detail) == null)
				Platform.SetRenderer(detail, Platform.CreateRenderer(detail));

			_detailRenderer = Platform.GetRenderer(detail);

			Children.Clear();
			if (_detailRenderer != null)
				Children.Add(_detailRenderer.ContainerElement);
		}

		void LoadMaster()
		{
			if (_masterRenderer != null && _popup != null)
				_popup.Child = null;

			Page master = Element.Master;
			if (Platform.GetRenderer(master) == null)
				Platform.SetRenderer(master, Platform.CreateRenderer(master));

			_masterRenderer = Platform.GetRenderer(master);
			var control = _masterRenderer as Panel;
			if (control != null && master.BackgroundColor == Color.Default)
				control.Background = Color.Black.ToBrush();
		}

		void UpdateSizes(double width, double height)
		{
			if (width <= 0 || height <= 0)
				return;

			var platform = Element.Platform as Platform;
			Size screenSize = platform.Size;
			MasterDetailPageController.MasterBounds = new Rectangle(0, 0, screenSize.Width - 20, screenSize.Height - 20);
			MasterDetailPageController.DetailBounds = new Rectangle(0, 0, width, height);

			_popup.Width = width - 20;
			_popup.Height = height - 20;

			Canvas.SetLeft(_popup, 10);
			Canvas.SetTop(_popup, 10);
		}
	}
}
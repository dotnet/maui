using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WinPhone
{
	internal class BackgroundTracker<T> : VisualElementTracker<Page, T> where T : FrameworkElement
	{
		readonly DependencyProperty _backgroundProperty;

		bool _backgroundNeedsUpdate = true;

		public BackgroundTracker(DependencyProperty backgroundProperty)
		{
			if (backgroundProperty == null)
				throw new ArgumentNullException("backgroundProperty");

			_backgroundProperty = backgroundProperty;
		}

		protected override void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName || e.PropertyName == Page.BackgroundImageProperty.PropertyName)
				UpdateBackground();

			base.HandlePropertyChanged(sender, e);
		}

		protected override void UpdateNativeControl()
		{
			base.UpdateNativeControl();

			if (_backgroundNeedsUpdate)
				UpdateBackground();
		}

		void UpdateBackground()
		{
			if (Model == null || Element == null)
				return;

			if (Model.BackgroundImage != null)
			{
				Element.SetValue(_backgroundProperty, new ImageBrush { ImageSource = new BitmapImage(new Uri(Model.BackgroundImage, UriKind.Relative)) });
			}
			else if (Model.BackgroundColor != Color.Default)
				Element.SetValue(_backgroundProperty, Model.BackgroundColor.ToBrush());

			_backgroundNeedsUpdate = false;
		}
	}

	public class CarouselPagePresenter : System.Windows.Controls.ContentPresenter
	{
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			DependencyObject parent = VisualTreeHelper.GetParent(this);
			while (parent != null && !(parent is PanoramaItem))
				parent = VisualTreeHelper.GetParent(parent);

			var panoramaItem = parent as PanoramaItem;
			if (panoramaItem == null)
				throw new Exception("No parent PanoramaItem found for carousel page");

			var element = (FrameworkElement)VisualTreeHelper.GetChild(panoramaItem, 0);
			element.SizeChanged += (s, e) =>
			{
				if (element.ActualWidth > 0 && element.ActualHeight > 0)
				{
					var carouselItem = (Page)DataContext;
					((IPageController)carouselItem.RealParent).ContainerArea = new Rectangle(0, 0, element.ActualWidth, element.ActualHeight);
				}
			};
		}
	}

	public class CarouselPageRenderer : Panorama, IVisualElementRenderer
	{
		static readonly System.Windows.DataTemplate PageTemplate;

		static readonly BindableProperty PageContainerProperty = BindableProperty.CreateAttached("PageContainer", typeof(PanoramaItem), typeof(CarouselPageRenderer), null);

		CarouselPage _page;
		BackgroundTracker<Control> _tracker;

		static CarouselPageRenderer()
		{
			PageTemplate = (System.Windows.DataTemplate)System.Windows.Application.Current.Resources["CarouselPage"];
		}

		public CarouselPageRenderer()
		{
			SetBinding(TitleProperty, new System.Windows.Data.Binding("Title"));
		}

		public UIElement ContainerElement
		{
			get { return this; }
		}

		public VisualElement Element
		{
			get { return _page; }
		}

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return new SizeRequest(new Size(widthConstraint, heightConstraint));
		}

		public void SetElement(VisualElement element)
		{
			CarouselPage oldElement = _page;
			_page = (CarouselPage)element;
			_tracker = new BackgroundTracker<Control>(BackgroundProperty) { Model = _page, Element = this };

			DataContext = _page;

			SelectionChanged += OnSelectionChanged;

			OnPagesChanged(_page, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

			_page.PagesChanged += OnPagesChanged;
			_page.PropertyChanged += OnPropertyChanged;

			Loaded += (sender, args) => ((IPageController)_page).SendAppearing();
			Unloaded += (sender, args) => ((IPageController)_page).SendDisappearing();

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
			EventHandler<VisualElementChangedEventArgs> changed = ElementChanged;
			if (changed != null)
				changed(this, e);
		}

		static PanoramaItem GetPageContainer(BindableObject bindable)
		{
			return (PanoramaItem)bindable.GetValue(PageContainerProperty);
		}

		void InsertItem(object item, int index, bool newItem)
		{
			DependencyObject pageContent = PageTemplate.LoadContent();

			var pageItem = (Page)item;
			PanoramaItem container = GetPageContainer(pageItem);
			if (container == null)
			{
				container = new PanoramaItem { DataContext = item, Content = pageContent };

				SetPageContainer(pageItem, container);
			}

			Items.Insert(index, container);
		}

		void OnPagesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			e.Apply(InsertItem, RemoveItem, Reset);
		}

		void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "CurrentPage")
			{
				ContentPage current = _page.CurrentPage;
				if (current == null)
					return;

				SetValue(SelectedItemProperty, GetPageContainer(current));
				OnItemsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var panoramaItem = (PanoramaItem)SelectedItem;
			if (panoramaItem == null)
				_page.CurrentPage = null;
			else
				_page.CurrentPage = (ContentPage)panoramaItem.DataContext;
		}

		void RemoveItem(object item, int index)
		{
			Items.RemoveAt(index);
		}

		void Reset()
		{
			Items.Clear();

			var i = 0;
			foreach (Page pageItem in _page.Children)
				InsertItem(pageItem, i++, true);

			ContentPage current = _page.CurrentPage;
			if (current != null)
				SetValue(SelectedItemProperty, GetPageContainer(current));
		}

		static void SetPageContainer(BindableObject bindable, PanoramaItem container)
		{
			bindable.SetValue(PageContainerProperty, container);
		}
	}
}
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using WFlipView = Windows.UI.Xaml.Controls.FlipView;
using WBinding = Windows.UI.Xaml.Data.Binding;
using WApp = Windows.UI.Xaml.Application;
using WSize = Windows.Foundation.Size;
using WDataTemplate = Windows.UI.Xaml.DataTemplate;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class CarouselViewRenderer : ViewRenderer<CarouselView, FrameworkElement>
	{
		WFlipView _flipView;

		bool _leftAdd;

		ICarouselViewController Controller
		{
			get { return Element; }
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CarouselView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				_flipView.SelectionChanged -= SelectionChanged;
				_flipView.ItemsSource = null;
				Element.CollectionChanged -= CollectionChanged;
			}

			if (e.NewElement != null)
			{
				if (_flipView == null)
				{
					_flipView = new FlipView 
					{
						IsSynchronizedWithCurrentItem = false,
						ItemTemplate = (WDataTemplate)WApp.Current.Resources["ItemTemplate"]
					};
				}

				_flipView.ItemsSource = Element.ItemsSource;
				_flipView.SelectedIndex = Element.Position;
				_flipView.SelectionChanged += SelectionChanged;
				Element.CollectionChanged += CollectionChanged;
			}

			if (_flipView != Control)
				SetNativeControl(_flipView);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Position" && _flipView.SelectedIndex != Element.Position)
			{
				if (!_leftAdd)
					_flipView.SelectedIndex = Element.Position;
				_leftAdd = false;
			}

			base.OnElementPropertyChanged(sender, e);
		}

		void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Controller.SendSelectedPositionChanged(_flipView.SelectedIndex);

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (e.NewStartingIndex <= Element.Position)
					{
						_leftAdd = true;
						int position = Element.Position + e.NewItems.Count;
						PositionChanged(position);
					}
					break;

				case NotifyCollectionChangedAction.Move:
					break;

				case NotifyCollectionChangedAction.Remove:
					if (Controller.Count == 0)
						throw new InvalidOperationException("CarouselView must retain a least one item.");

					if (e.OldStartingIndex < Element.Position)
						PositionChanged(Element.Position - e.OldItems.Count);
					break;

				case NotifyCollectionChangedAction.Replace:
					break;

				case NotifyCollectionChangedAction.Reset:
					break;

				default:
					throw new Exception($"Enum value '{(int)e.Action}' is not a member of NotifyCollectionChangedAction enumeration.");
			}
		}

		void PositionChanged(int position)
		{
			if (!_leftAdd)
				_flipView.SelectedIndex = position;
			Element.Position = position;
			Controller.SendSelectedPositionChanged(position);
		}

		void SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			object[] addedItems = e.AddedItems.ToArray();
			object[] removedItems = e.RemovedItems.ToArray();

			object addedItem = addedItems.SingleOrDefault();
			if (addedItem != null)
			{
				PositionChanged(_flipView.SelectedIndex);
				Controller.SendSelectedItemChanged(addedItems.Single());
			}
		}
	}

	public class ItemControl : ContentControl
	{
		CarouselView _carouselView;
		object _item;
		View _view;

		public ItemControl()
		{
			DataContextChanged += OnDataContextChanged;
		}

		CarouselView CarouselView => LoadCarouselView();

		IItemViewController Controller => CarouselView;

		protected override WSize ArrangeOverride(WSize finalSize)
		{
			_view.Layout(new Rectangle(0, 0, CarouselView.Width, CarouselView.Height));
			return base.ArrangeOverride(finalSize);
		}

		protected override WSize MeasureOverride(WSize availableSize)
		{
			LoadCarouselView();

			if (_item != null)
			{
				SetDataContext(_item);
				_item = null;
			}

			return base.MeasureOverride(availableSize);
		}

		CarouselView LoadCarouselView()
		{
			if (_carouselView != null)
				return _carouselView;

			DependencyObject parent = VisualTreeHelper.GetParent(this);
			CarouselViewRenderer renderer = default(CarouselViewRenderer);

			do
			{
				if (parent == null)
					return null;

				renderer = parent as CarouselViewRenderer;
				if (renderer != null)
					break;

				parent = VisualTreeHelper.GetParent(parent);
			} while (true);

			_carouselView = renderer.Element;
			return _carouselView;
		}

		void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			object item = args.NewValue;

			if (_carouselView != null)
				SetDataContext(item);

			else if (item != null)
				_item = item;
		}

		void SetDataContext(object item)
		{
			// type item
			object type = Controller.GetItemType(item);

			// activate item
			_view = Controller.CreateView(type);
			_view.Parent = CarouselView;
			_view.Layout(new Rectangle(0, 0, CarouselView.Width, CarouselView.Height));

			// render item
			IVisualElementRenderer renderer = Platform.CreateRenderer(_view);
			Platform.SetRenderer(_view, renderer);
			Content = renderer;

			// bind item
			Controller.BindView(_view, item);
		}
	}
}
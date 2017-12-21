using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfLightToolkit.Controls;
using WpfLightToolkit.Extensions;

namespace Xamarin.Forms.Platform.WPF
{
	public class VisualPageRenderer<TElement, TNativeElement> : ViewRenderer<TElement, TNativeElement>
		where TElement : Page
		where TNativeElement : LightPage
	{
		protected override void OnElementChanged(ElementChangedEventArgs<TElement> e)
		{
			if (e.OldElement != null) // Clear old element event
			{
				((ObservableCollection<ToolbarItem>)e.OldElement.ToolbarItems).CollectionChanged -= VisualPageRenderer_CollectionChanged;
			}

			if (e.NewElement != null)
			{
				// Update control property 
				UpdateTitle();
				UpdateBackButton();
				UpdateBackButtonTitle();
				UpdateNavigationBarVisible();
				UpdateToolbar();

				// Suscribe element event
				((ObservableCollection<ToolbarItem>)Element.ToolbarItems).CollectionChanged += VisualPageRenderer_CollectionChanged;
			}

			base.OnElementChanged(e);
		}
		
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Page.BackgroundImageProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == Page.TitleProperty.PropertyName)
				UpdateTitle();
			else if (e.PropertyName == NavigationPage.HasBackButtonProperty.PropertyName)
				UpdateBackButton();
			else if (e.PropertyName == NavigationPage.BackButtonTitleProperty.PropertyName)
				UpdateBackButtonTitle();
			else if (e.PropertyName == NavigationPage.HasNavigationBarProperty.PropertyName)
				UpdateNavigationBarVisible();

		}

		void UpdateTitle()
		{
			if (!string.IsNullOrWhiteSpace(Element.Title))
				Control.Title = Element.Title;
		}

		void UpdateBackButton()
		{
			this.Control.HasBackButton = NavigationPage.GetHasBackButton(Element);
		}

		void UpdateBackButtonTitle()
		{
			this.Control.BackButtonTitle = NavigationPage.GetBackButtonTitle(Element);
		}

		void UpdateNavigationBarVisible()
		{
			this.Control.HasNavigationBar = NavigationPage.GetHasNavigationBar(Element);
		}

		protected override void UpdateBackground()
		{
			string bgImage = Element.BackgroundImage;
			if (!string.IsNullOrEmpty(bgImage))
			{
				ImageBrush imgBrush = new ImageBrush()
				{
					ImageSource = new BitmapImage(new Uri(bgImage, UriKind.RelativeOrAbsolute))
				};
				Control.Background = imgBrush;
			}
			else
			{
				base.UpdateBackground();
			}
		}
		
		void UpdateToolbar()
		{
			Control.PrimaryTopBarCommands.Clear();
			Control.SecondaryTopBarCommands.Clear();

			foreach (var item in Element.ToolbarItems)
			{
				LightAppBarButton appBar = new LightAppBarButton()
				{
					Label = item.Text,
					Tag = item
				};

				appBar.SetValue(FrameworkElementAttached.PriorityProperty, item.Priority);

				if(item.Icon != null)
				{
					Symbol symbol;
					Geometry geometry;

					if (Enum.TryParse(item.Icon.File, true, out symbol))
						appBar.Icon = new LightSymbolIcon() { Symbol = symbol };
					else if (TryParseGeometry(item.Icon.File, out geometry))
						appBar.Icon = new LightPathIcon() { Data = geometry };
					else if (Path.GetExtension(item.Icon.File) != null)
						appBar.Icon = new LightBitmapIcon() { UriSource = new Uri(item.Icon.File, UriKind.RelativeOrAbsolute) };
				}
				
				appBar.Click += (sender, e) =>
				{
					if (appBar.Tag != null && appBar.Tag is ToolbarItem)
					{
						(appBar.Tag as ToolbarItem).Activate();
					}
				};

				if (item.Order == ToolbarItemOrder.Default || item.Order == ToolbarItemOrder.Primary)
					Control.PrimaryTopBarCommands.Add(appBar);
				else
					Control.SecondaryTopBarCommands.Add(appBar);
			}
		}

		protected override void Appearing()
		{
			base.Appearing();
			Element?.SendAppearing();
		}

		protected override void Disappearing()
		{
			Element?.SendDisappearing();
			base.Disappearing();
		}

		private void VisualPageRenderer_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			UpdateToolbar();
		}
		
		private bool TryParseGeometry(string value, out Geometry geometry)
		{
			geometry = null;
			try
			{
				geometry = Geometry.Parse(value);
				return true;
			}
			catch(Exception)
			{
				return false;
			}
		}

		bool _isDisposed;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if (Element != null)
				{
					((ObservableCollection<ToolbarItem>)Element.ToolbarItems).CollectionChanged -= VisualPageRenderer_CollectionChanged;
				}
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}
	}
}

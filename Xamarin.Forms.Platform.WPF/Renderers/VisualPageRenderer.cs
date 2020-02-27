using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xamarin.Forms.Platform.WPF.Controls;
using Xamarin.Forms.Platform.WPF.Converters;
using Xamarin.Forms.Platform.WPF.Extensions;

namespace Xamarin.Forms.Platform.WPF
{
	public class VisualPageRenderer<TElement, TNativeElement> : ViewRenderer<TElement, TNativeElement>
		where TElement : Page
		where TNativeElement : FormsPage
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

			if (e.PropertyName == Page.BackgroundImageSourceProperty.PropertyName)
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

		protected override async void UpdateBackground()
		{
			var bgImage = Element.BackgroundImageSource;
			if (bgImage == null || bgImage.IsEmpty)
			{
				base.UpdateBackground();
				return;
			}

			var img = await bgImage.ToWindowsImageSourceAsync();
			Control.Background = new ImageBrush { ImageSource = img };
		}
		
		void UpdateToolbar()
		{
			Control.PrimaryTopBarCommands.Clear();
			Control.SecondaryTopBarCommands.Clear();

			foreach (var item in Element.ToolbarItems)
			{
				var appBar = new FormsAppBarButton() { DataContext = item };

				var iconBinding = new System.Windows.Data.Binding(nameof(item.IconImageSource))
				{
					Converter = new IconConveter()
				};

				appBar.SetBinding(FormsAppBarButton.IconProperty, iconBinding);
				appBar.SetBinding(FormsAppBarButton.LabelProperty, nameof(item.Text));
				appBar.SetBinding(FormsAppBarButton.IsEnabledProperty, nameof(item.IsEnabled));
				appBar.SetValue(FrameworkElementAttached.PriorityProperty, item.Priority);

				appBar.Click += (sender, e) =>
				{
					if (appBar.DataContext is ToolbarItem toolbarItem)
					{
						((IMenuItemController)toolbarItem).Activate();
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

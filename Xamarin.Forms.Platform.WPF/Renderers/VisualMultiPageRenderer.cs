using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfLightToolkit.Controls;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WPF
{
	public class VisualMultiPageRenderer<TElement, TContainer, TNativeElement> : VisualPageRenderer<TElement, TNativeElement>
		where TElement : MultiPage<TContainer>
		where TNativeElement : LightMultiPage
		where TContainer : Page
	{

		protected override void OnElementChanged(ElementChangedEventArgs<TElement> e)
		{
			if (e.OldElement != null) // Clear old element event
			{
				((INotifyCollectionChanged)e.OldElement.Children).CollectionChanged -= OnPagesChanged;
			}

			if (e.NewElement != null)
			{
				// Subscribe control event
				Control.SelectionChanged += Control_SelectionChanged;
				
				// Subscribe element event
				((INotifyCollectionChanged)Element.Children).CollectionChanged += OnPagesChanged;
			}

			base.OnElementChanged(e);
		}

		protected override void Appearing()
		{
			base.Appearing();
			OnPagesChanged(Element.Children, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			UpdateCurrentPage();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == nameof(MultiPage<TContainer>.CurrentPage))
				UpdateCurrentPage();
		}
		
		void OnPagesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			e.Apply(Element.Children, Control.ItemsSource);
		}

		void UpdateCurrentPage()
		{
			Control.SelectedItem = Element.CurrentPage;
		}
		
		private void Control_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Element.CurrentPage = e.NewElement as TContainer;
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
					Control.SelectionChanged -= Control_SelectionChanged;
				}

				if (Element != null)
				{
					((INotifyCollectionChanged)Element.Children).CollectionChanged -= OnPagesChanged;
				}
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}
	}
}

using System;
using System.ComponentModel;
using ElmSharp;
using EColor = ElmSharp.Color;
using NSearchBar = Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native.SearchBar;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public class SearchHandlerRenderer : IDisposable
	{
		bool disposedValue;
		SearchResultList _searchResultList;

		public SearchHandlerRenderer(SearchHandler searchHandler)
		{
			Element = searchHandler;
			Control = new NSearchBar(Forms.NativeParent)
			{
				IsSingleLine = true,
			};
			Control.Show();
			Control.SetInputPanelReturnKeyType(InputPanelReturnKeyType.Search);
			Control.TextChanged += OnTextChanged;
			Control.Activated += OnActivated;
			Control.Focused += OnFocused;
			Control.Unfocused += OnFocused;

			Element.FocusChangeRequested += OnFocusChangedRequested;
			Element.PropertyChanged += OnElementPropertyChanged;
			(Element as ISearchHandlerController).ListProxyChanged += OnSearchResultListChanged;

			UpdateKeyboard();
			UpdatePlaceholder();
			UpdatePlaceholderColor();
			UpdateHorizontalTextAlignment();
			UpdateTextColor();
			UpdateFontAttributes();
			UpdateFontFamily();
			UpdateFontSize();
			UpdateBackgroundColor();
			UpdateQuery();
			UpdateIsSearchEnabled();

			UpdateSearchResult();

		}

		public SearchHandler Element { get; }
		public EvasObject NativeView => Control;
		ISearchHandlerController SearchHandlerController => Element;
		NSearchBar Control { get; }

		~SearchHandlerRenderer()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Element.FocusChangeRequested -= OnFocusChangedRequested;
					Element.PropertyChanged -= OnElementPropertyChanged;
					(Element as ISearchHandlerController).ListProxyChanged -= OnSearchResultListChanged;
					Control.TextChanged -= OnTextChanged;
					Control.Activated -= OnActivated;
					Control.Focused -= OnFocused;
					Control.Unfocused -= OnFocused;
					Control.Unrealize();
				}
				disposedValue = true;
			}
		}

		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Element.Keyboard))
			{
				UpdateKeyboard();
			}
			else if (e.PropertyName == nameof(Element.Placeholder))
			{
				UpdatePlaceholder();
			}
			else if (e.PropertyName == nameof(Element.PlaceholderColor))
			{
				UpdatePlaceholderColor();
			}
			else if (e.PropertyName == nameof(Element.HorizontalTextAlignment))
			{
				UpdateHorizontalTextAlignment();
			}
			else if (e.PropertyName == nameof(Element.TextColor))
			{
				UpdateTextColor();
			}
			else if (e.PropertyName == nameof(Element.FontAttributes))
			{
				UpdateFontAttributes();
			}
			else if (e.PropertyName == nameof(Element.FontFamily))
			{
				UpdateFontFamily();
			}
			else if (e.PropertyName == nameof(Element.FontSize))
			{
				UpdateFontSize();
			}
			else if (e.PropertyName == nameof(Element.BackgroundColor))
			{
				UpdateBackgroundColor();
			}
			else if (e.PropertyName == nameof(Element.Query))
			{
				UpdateQuery();
			}
			else if (e.PropertyName == nameof(Element.IsSearchEnabled))
			{
				UpdateIsSearchEnabled();
			}
			else if (e.PropertyName == nameof(Element.ShowsResults))
			{
				UpdateSearchResult();
			}
		}

		void OnSearchResultListChanged(object sender, ListProxyChangedEventArgs e)
		{
			UpdateSearchResult();
		}

		void InitializeSearchResultList()
		{
			if (_searchResultList != null)
			{
				return;
			}
			_searchResultList = new SearchResultList();
			_searchResultList.Show();
			_searchResultList.ItemSelected += OnResultItemSelected;
		}

		void OnResultItemSelected(object sender, GenListItemEventArgs e)
		{
			var data = (e.Item.Data as View).BindingContext;
			SearchHandlerController.ItemSelected(data);
			Device.BeginInvokeOnMainThread(() =>
			{
				DeinitializeSearchResultList();
			});
		}

		void DeinitializeSearchResultList()
		{
			if (_searchResultList == null)
			{
				return;
			}
			_searchResultList.ItemSelected -= OnResultItemSelected;
			_searchResultList.Unrealize();
			_searchResultList = null;
		}

		void UpdateSearchResult()
		{
			if (SearchHandlerController == null)
				return;

			if (!Element.ShowsResults)
			{
				DeinitializeSearchResultList();
				return;
			}

			if (Control.IsFocused && SearchHandlerController.ListProxy != null &&
				SearchHandlerController.ListProxy.Count > 0 &&
				Element.ItemTemplate != null)
			{
				InitializeSearchResultList();
				_searchResultList.ItemTemplate = Element.ItemTemplate;
				_searchResultList.ItemsSource = SearchHandlerController.ListProxy;
				UpdateSearchResultLayout();
			}
			else
			{
				DeinitializeSearchResultList();
			}
		}

		void UpdateIsSearchEnabled()
		{
			Control.IsEnabled = Element.IsSearchEnabled;
		}

		void UpdateQuery()
		{
			Control.Text = Element.Query;
		}

		void UpdateFontAttributes()
		{
			Control.FontAttributes = Element.FontAttributes;
		}

		void UpdateFontFamily()
		{
			Control.FontFamily = Element.FontFamily;
		}

		void UpdateFontSize()
		{
			Control.FontSize = Element.FontSize;
		}

		void UpdateBackgroundColor()
		{
			var color = Element.BackgroundColor.ToNative();
			Control.BackgroundColor = color == EColor.Default ? EColor.White : color;
		}

		void UpdateTextColor()
		{
			Control.TextColor = Element.TextColor.ToNative();
		}

		void UpdateHorizontalTextAlignment()
		{
			Control.HorizontalTextAlignment = Element.HorizontalTextAlignment.ToNative();
		}

		void OnFocusChangedRequested(object sender, VisualElement.FocusRequestArgs e)
		{
			Control.SetFocus(e.Focus);
			e.Result = true;
		}

		void UpdateKeyboard()
		{
			Control.Keyboard = Element.Keyboard.ToNative();
		}

		void UpdatePlaceholder()
		{
			Control.Placeholder = Element.Placeholder;
		}
		void UpdatePlaceholderColor()
		{
			Control.PlaceholderColor = Element.PlaceholderColor.ToNative();
		}

		void OnFocused(object sender, EventArgs e)
		{
			Element.SetIsFocused(Control.IsFocused);
			if (Control.IsFocused)
			{
				UpdateSearchResult();
			}
			else
			{
				if (_searchResultList != null)
				{
					_searchResultList.Hide();
				}
				Device.BeginInvokeOnMainThread(() =>
				{
					Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
					{
						DeinitializeSearchResultList();
						return false;
					});
				});
			}
		}

		void OnActivated(object sender, EventArgs e)
		{
			Control.HideInputPanel();
			(Element as ISearchHandlerController).QueryConfirmed();
		}

		void OnTextChanged(object sender, TextChangedEventArgs e)
		{
			Element.SetValueCore(SearchHandler.QueryProperty, Control.Text);
		}

		void UpdateSearchResultLayout()
		{
			if (_searchResultList != null)
			{
				var bound = NativeView.Geometry;
				bound.Y += NativeView.Geometry.Height;
				bound.Height = Math.Min(_searchResultList.Height, bound.Width);
				_searchResultList.Geometry = bound;
				_searchResultList.UpdateLayout();
			}
		}
	}
}

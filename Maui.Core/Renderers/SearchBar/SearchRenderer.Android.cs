using AView = Android.Views.View;
using Android.Text;
using System.Linq;
using Android.Widget;
using Android.Views;
using System.Collections.Generic;

namespace System.Maui.Platform
{
	public partial class SearchRenderer : AbstractViewRenderer<ISearch, SearchView>
	{
		EditText _editText;
		TextColorSwitcher _textColorSwitcher;
		TextColorSwitcher _hintColorSwitcher;
		QueryListener _queryListener;

		protected override SearchView CreateView()
		{
			_queryListener = new QueryListener(VirtualView, this);

			var context = (Context as ContextThemeWrapper).BaseContext ?? Context;
			var searchView = new SearchView(context);

			searchView.SetIconifiedByDefault(false);
			searchView.Iconified = false;
			_editText ??= searchView.GetChildrenOfType<EditText>().FirstOrDefault();

			if (_editText != null)
			{
				_textColorSwitcher = new TextColorSwitcher(_editText);
				_hintColorSwitcher = new TextColorSwitcher(_editText);
			}

			searchView.SetOnQueryTextListener(_queryListener);

			return searchView;
		}

		public static void MapPropertyColor(IViewRenderer renderer, ITextInput entry)
		{
			(renderer as SearchRenderer)?.UpdateTextColor(entry.Color);
		}

		public static void MapPropertyPlaceholder(IViewRenderer renderer, ITextInput entry)
		{
			(renderer as SearchRenderer)?.UpdatePlaceholder();
		}

		public static void MapPropertyPlaceholderColor(IViewRenderer renderer, ITextInput entry)
		{
			(renderer as SearchRenderer)?.UpdatePlaceholderColor(entry.PlaceholderColor);
		}

		public static void MapPropertyText(IViewRenderer renderer, ITextInput entry)
		{
			(renderer as SearchRenderer)?.UpdateText();
		}

		public static void MapPropertyCancelColor(IViewRenderer renderer, ISearch search)
		{
			(renderer as SearchRenderer)?.UpdateCancelColor();
		}

		public static void MapPropertyMaxLength(IViewRenderer renderer, ITextInput view)
		{
			(renderer as SearchRenderer)?.UpdateMaxLength();
		}

		protected virtual void UpdateTextColor(Color color)
		{
			_textColorSwitcher?.UpdateTextColor(color);
		}

		protected virtual void UpdatePlaceholderColor(Color color)
		{
			_hintColorSwitcher?.UpdateHintTextColor(color);
		}

		protected virtual void UpdateMaxLength()
		{
			if (_editText == null)
				return;

			var maxLength = VirtualView.MaxLength;

			//default
			if (maxLength == -1)
				return;

			var currentFilters = new List<IInputFilter>(_editText?.GetFilters() ?? new IInputFilter[0]);

			for (var i = 0; i < currentFilters.Count; i++)
			{
				if (currentFilters[i] is InputFilterLengthFilter)
				{
					currentFilters.RemoveAt(i);
					break;
				}
			}

			currentFilters.Add(new InputFilterLengthFilter(maxLength));

			_editText?.SetFilters(currentFilters.ToArray());

			var currentControlText = TypedNativeView.Query;

			if (currentControlText.Length > maxLength)
				TypedNativeView.SetQuery(currentControlText.Substring(0, maxLength), false);
		}

		protected virtual void UpdatePlaceholder()
		{
			TypedNativeView.SetQueryHint(VirtualView.Placeholder);
		}

		protected virtual void UpdateText()
		{
			string query = TypedNativeView.Query;
			var text = VirtualView.Text;
			if (query != text)
				TypedNativeView.SetQuery(text, false);
		}

		protected virtual void UpdateCancelColor()
		{
			int searchViewCloseButtonId = TypedNativeView.Resources.GetIdentifier("android:id/search_close_btn", null, null);
			if (searchViewCloseButtonId != 0)
			{
				var image =  TypedNativeView.FindViewById<ImageView>(searchViewCloseButtonId);
				if (image != null && image.Drawable != null)
				{
					var cancelColor = VirtualView.CancelColor;

					if (!cancelColor.IsDefault)
						image.Drawable.SetColorFilter(cancelColor, FilterMode.SrcIn);
					else
						image.Drawable.ClearColorFilter();
				}
			}
		}

		internal void ClearFocus()
		{
			TypedNativeView?.ClearFocus();
		}
	}

	class QueryListener : Java.Lang.Object, SearchView.IOnQueryTextListener
	{
		ISearch _search;
		SearchRenderer _searchRenderer;

		public QueryListener(ISearch search, SearchRenderer searchRenderer)
		{
			_searchRenderer = searchRenderer;
			_search = search;
		}

		bool SearchView.IOnQueryTextListener.OnQueryTextChange(string newText)
		{
			_search.Text = newText;
			return true;
		}

		bool SearchView.IOnQueryTextListener.OnQueryTextSubmit(string query)
		{
			_search.Search();
			_searchRenderer.ClearFocus();
			return true;
		}
	}
}

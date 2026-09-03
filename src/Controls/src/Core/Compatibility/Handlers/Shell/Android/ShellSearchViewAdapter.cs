#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellSearchViewAdapter : BaseAdapter, IFilterable
	{
		public const string DoNotUpdateMarker = "__DO_NOT_UPDATE__";

		SearchHandler _searchHandler;
		IShellContext _shellContext;
		DataTemplate _defaultTemplate;
		Filter _filter;
		IReadOnlyList<object> _emptyList = new List<object>();
		IReadOnlyList<object> ListProxy => SearchController.ListProxy ?? _emptyList;
		bool _disposed;
		protected IMauiContext MauiContext => _shellContext.Shell.Handler.MauiContext;

		public ShellSearchViewAdapter(SearchHandler searchHandler, IShellContext shellContext)
		{
			_searchHandler = searchHandler ?? throw new ArgumentNullException(nameof(searchHandler));
			_shellContext = shellContext ?? throw new ArgumentNullException(nameof(shellContext));
			SearchController.ListProxyChanged += OnListPropxyChanged;
			_searchHandler.PropertyChanged += OnSearchHandlerPropertyChanged;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				SearchController.ListProxyChanged -= OnListPropxyChanged;
				_searchHandler.PropertyChanged -= OnSearchHandlerPropertyChanged;
				_filter?.Dispose();
			}

			_filter = null;
			_shellContext = null;
			_searchHandler = null;
			_defaultTemplate = null;

			base.Dispose(disposing);
		}

		public Filter Filter => _filter ?? (_filter = new CustomFilter(this));

		public override int Count => ListProxy.Count;

		DataTemplate DefaultTemplate
		{
			get
			{
				if (_defaultTemplate == null)
				{
					_defaultTemplate = new DataTemplate(() =>
					{
						var label = new Label();

						if (RuntimeFeature.IsShellSearchResultsRendererDisplayMemberNameSupported)
						{
#pragma warning disable CS0618
#if NET8_0
#pragma warning disable IL2026 // FeatureGuardAttribute is not supported on .NET 8
#endif
							label.SetBinding(Label.TextProperty, _searchHandler.DisplayMemberName ?? ".");
#if NET8_0
#pragma warning restore IL2026 // FeatureGuardAttribute is not supported on .NET 8
#endif
#pragma warning restore CS0618
						}
						else
						{
#pragma warning disable CS0618
							if (_searchHandler.DisplayMemberName is not null)
							{
								Application.Current?.FindMauiContext()?.CreateLogger<ShellSearchViewAdapter>()?.LogError(TrimmerConstants.SearchHandlerDisplayMemberNameNotSupportedWarning);
								throw new InvalidOperationException(TrimmerConstants.SearchHandlerDisplayMemberNameNotSupportedWarning);
							}
#pragma warning restore CS0618
						}

						label.HorizontalTextAlignment = TextAlignment.Center;
						label.VerticalTextAlignment = TextAlignment.Center;

						return label;
					});
				}
				return _defaultTemplate;
			}
		}

		ISearchHandlerController SearchController => _searchHandler;

		public override Java.Lang.Object GetItem(int position)
		{
			return new ObjectWrapper(ListProxy[position]);
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override AView GetView(int position, AView convertView, ViewGroup parent)
		{
			var item = ListProxy[position];

			ContainerView result = null;
			if (convertView != null)
			{
				result = convertView as ContainerView;
				result.View.BindingContext = item;
			}
			else
			{
				var template = _searchHandler.ItemTemplate ?? DefaultTemplate;
				var view = (View)template.CreateContent(item, _shellContext.Shell);
				view.BindingContext = item;

				result = new ContainerView(parent.Context, view, MauiContext);
				result.MatchWidth = true;
				result.MeasureHeight = true;
			}

			return result;
		}

		protected virtual void OnSearchHandlerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == SearchHandler.ItemTemplateProperty.PropertyName)
			{
				NotifyDataSetChanged();
			}
		}

		void OnListPropxyChanged(object sender, ListProxyChangedEventArgs e)
		{
			NotifyDataSetChanged();
		}

		class CustomFilter : Filter
		{
			private readonly BaseAdapter _adapter;

			public CustomFilter(BaseAdapter adapter)
			{
				_adapter = adapter;
			}

			protected override FilterResults PerformFiltering(ICharSequence constraint)
			{
				var results = new FilterResults();

				results.Count = 100;
				return results;
			}

			protected override void PublishResults(ICharSequence constraint, FilterResults results)
			{
				_adapter.NotifyDataSetChanged();
			}
		}

		class ObjectWrapper : Java.Lang.Object
		{
			public ObjectWrapper(object obj)
			{
				Object = obj;
			}

			object Object { get; set; }

			public override string ToString() => DoNotUpdateMarker;
		}
	}
}
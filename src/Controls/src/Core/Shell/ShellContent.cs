#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Represents the content displayed within a <see cref="ShellSection"/> tab.
	/// </summary>
	[ContentProperty(nameof(Content))]
	[TypeConverter(typeof(ShellContentConverter))]
	public class ShellContent : BaseShellItem, IShellContentController, IVisualTreeElement
	{
		static readonly BindablePropertyKey MenuItemsPropertyKey =
			BindableProperty.CreateReadOnly(nameof(MenuItems), typeof(MenuItemCollection), typeof(ShellContent), null,
				defaultValueCreator: bo => new MenuItemCollection());

		/// <summary>Bindable property for <see cref="MenuItems"/>.</summary>
		public static readonly BindableProperty MenuItemsProperty = MenuItemsPropertyKey.BindableProperty;

		/// <summary>Bindable property for <see cref="Content"/>.</summary>
		public static readonly BindableProperty ContentProperty =
			BindableProperty.Create(nameof(Content), typeof(object), typeof(ShellContent), null, BindingMode.OneTime, propertyChanged: OnContentChanged);

		/// <summary>Bindable property for <see cref="ContentTemplate"/>.</summary>
		public static readonly BindableProperty ContentTemplateProperty =
			BindableProperty.Create(nameof(ContentTemplate), typeof(DataTemplate), typeof(ShellContent), null, BindingMode.OneTime);

		/// <summary>Bindable property for <see cref="QueryString"/>.</summary>
		public static readonly BindableProperty QueryStringProperty =
			BindableProperty.Create(nameof(QueryString), typeof(string), typeof(ShellContent), default(string), propertyChanged: OnQueryStringChanged);

		internal static readonly BindableProperty QueryAttributesProperty =
			BindableProperty.CreateAttached("QueryAttributes", typeof(ShellRouteParameters), typeof(ShellContent), defaultValue: null, propertyChanged: OnQueryAttributesPropertyChanged);

		/// <summary>
		/// Gets the collection of menu items associated with this content. This is a bindable property.
		/// </summary>
		public MenuItemCollection MenuItems => (MenuItemCollection)GetValue(MenuItemsProperty);

		/// <summary>
		/// Gets or sets the page content to display. This is a bindable property.
		/// </summary>
		public object Content
		{
			get => GetValue(ContentProperty);
			set => SetValue(ContentProperty, value);
		}

		/// <summary>
		/// Gets or sets a template used to create the content page. This is a bindable property.
		/// </summary>
		public DataTemplate ContentTemplate
		{
			get => (DataTemplate)GetValue(ContentTemplateProperty);
			set => SetValue(ContentTemplateProperty, value);
		}

		/// <summary>
		/// Gets or sets an encoded query string supplied to this content's page when the content is selected.
		/// </summary>
		/// <remarks>
		/// The leading <c>?</c> is optional. The query string is URI-decoded once. Values from
		/// <see cref="QueryParameters"/> take precedence over values in this query string. Parameters supplied
		/// to Shell navigation take precedence for that navigation; selecting the content again reapplies its
		/// declarative parameters. For compatibility, existing Shell navigation retains its historical decoding
		/// and culture behavior, so moving the same encoded text from navigation to this property can produce
		/// a different result.
		/// </remarks>
#nullable enable
		public string? QueryString
		{
			get => (string?)GetValue(QueryStringProperty);
			set => SetValue(QueryStringProperty, value);
		}
#nullable disable

		/// <summary>
		/// Gets the query parameters supplied to this content's page when the content is selected.
		/// </summary>
		/// <remarks>
		/// Changes to this collection or its parameters are applied immediately when this content is selected,
		/// or the next time it is selected otherwise. Values in this collection take precedence over
		/// <see cref="QueryString"/>. Parameters supplied to Shell navigation take precedence for that
		/// navigation; selecting the content again reapplies its declarative parameters. Parameters are logical
		/// children of this content, so they support inherited binding contexts, dynamic resources, relative-source
		/// ancestor bindings, and parent-scoped XAML references. A parameter instance can occur more than once in
		/// this collection, but it cannot be shared with another <see cref="ShellContent"/>.
		/// </remarks>
		public IList<ShellContentQueryParameter> QueryParameters { get; }

		Page IShellContentController.Page => ContentCache;

		EventHandler _isPageVisibleChanged;
		event EventHandler IShellContentController.IsPageVisibleChanged { add => _isPageVisibleChanged += value; remove => _isPageVisibleChanged -= value; }

		bool _createdViaService;
		Page IShellContentController.GetOrCreateContent()
		{
			var template = ContentTemplate;
			var content = Content;

			Page result = null;
			if (template is null)
			{
				if (content is Page page)
					result = page;
			}
			else
			{
				if (template.Type is not null)
				{
					template.LoadTemplate = () =>
					{
						var services = Parent?.FindMauiContext()?.Services;
						if (services is not null)
						{
							var result = services.GetService(template.Type);
							if (result is not null)
							{
								_createdViaService = true;
								return result;
							}
						}

						_createdViaService = false;
						return Extensions.DependencyInjection.ActivatorUtilities.CreateInstance(services, template.Type);
					};
				}

				result = ContentCache ?? (Page)template.CreateContent(content, this);

				if (GetValue(QueryAttributesProperty) is ShellRouteParameters delayedQueryParams)
				{
					result?.SetValue(QueryAttributesProperty, delayedQueryParams);
				}

				ContentCache = result;
			}

			if (result is null)
				throw new InvalidOperationException($"No Content found for {nameof(ShellContent)}, Title:{Title}, Route {Route}");

			if (result is TabbedPage)
				throw new NotSupportedException($"Shell is currently not compatible with TabbedPage. Please use TabBar, Tab or switch to using NavigationPage for your {Application.Current}.MainPage");

			if (result is FlyoutPage)
				throw new NotSupportedException("Shell is currently not compatible with FlyoutPage.");

			if (result is NavigationPage)
				throw new NotSupportedException("Shell is currently not compatible with NavigationPage. Shell has Navigation built in and doesn't require a NavigationPage.");

			return result;
		}

		void IShellContentController.RecyclePage(Page page)
		{
		}

		Page _contentCache;
		readonly HashSet<ShellContentQueryParameter> _trackedQueryParameters = new();
		readonly HashSet<string> _lastAppliedContentParameterNames = new(StringComparer.Ordinal);
		bool _hasAppliedQueryParameters;

		sealed class ShellContentQueryParameterCollection : ObservableCollection<ShellContentQueryParameter>
		{
			readonly ShellContent _owner;

			public ShellContentQueryParameterCollection(ShellContent owner)
			{
				_owner = owner;
			}

			protected override void InsertItem(int index, ShellContentQueryParameter item)
			{
				ValidateOwner(item);
				base.InsertItem(index, item);
			}

			protected override void SetItem(int index, ShellContentQueryParameter item)
			{
				ValidateOwner(item);
				base.SetItem(index, item);
			}

			void ValidateOwner(ShellContentQueryParameter parameter)
			{
				if (parameter?.Parent is Element parent && parent != _owner)
					throw new InvalidOperationException($"{nameof(ShellContentQueryParameter)} instances cannot be shared across {nameof(ShellContent)} objects.");
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ShellContent"/> class.
		/// </summary>
		public ShellContent()
		{
			((INotifyCollectionChanged)MenuItems).CollectionChanged += MenuItemsCollectionChanged;
			QueryParameters = new ShellContentQueryParameterCollection(this);
			((INotifyCollectionChanged)QueryParameters).CollectionChanged += QueryParametersCollectionChanged;
		}

		internal bool IsVisibleContent =>
			IsSelectedContent &&
			(
				Navigation?.ModalStack is null ||
				Navigation?.ModalStack?.Count == 0
			);

		bool IsSelectedContent =>
			Parent is ShellSection shellSection &&
			shellSection.IsVisibleSection &&
			shellSection.CurrentItem == this;

		internal override void SendDisappearing()
		{
			base.SendDisappearing();
			((ContentCache ?? Content) as Page)?.SendDisappearing();
		}

		internal override void SendAppearing()
		{
			// only fire Appearing when the Content Page exists on the ShellContent
			var content = ContentCache ?? Content;
			if (content == null)
				return;

			base.SendAppearing();

			SendPageAppearing((ContentCache ?? Content) as Page);
		}

		void SendPageAppearing(Page page)
		{
			if (page == null)
				return;

			if (page.Parent == null)
			{
				page.ParentSet += OnPresentedPageParentSet;
				void OnPresentedPageParentSet(object sender, EventArgs e)
				{
					this.FindParentOfType<Shell>().SendPageAppearing(page);
					(sender as Page).ParentSet -= OnPresentedPageParentSet;
				}
			}
			else if (IsVisibleContent && page.IsVisible)
			{
				this.FindParentOfType<Shell>().SendPageAppearing(page);
			}
		}

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);
			if (child is Page page)
			{
				page.PropertyChanged += OnPagePropertyChanged;
				_isPageVisibleChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		protected override void OnChildRemoved(Element child, int oldLogicalIndex)
		{
			base.OnChildRemoved(child, oldLogicalIndex);
			if (child is Page page)
			{
				page.PropertyChanged -= OnPagePropertyChanged;
			}
		}


		void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.IsVisibleProperty.PropertyName)
				_isPageVisibleChanged?.Invoke(this, EventArgs.Empty);
		}

		Page ContentCache
		{
			get => _contentCache;
			set
			{
				if (_contentCache == value)
					return;

				var oldCache = _contentCache;
				_contentCache = value;
				if (oldCache != null)
				{
					RemoveLogicalChild(oldCache);
					oldCache.Unloaded -= OnPageUnloaded;
				}

				if (value is not null && value.Parent != this)
				{
					AddLogicalChild(value);

					if (_createdViaService)
					{
						value.Unloaded += OnPageUnloaded;
					}
				}

				if (value is not null && GetValue(QueryAttributesProperty) is ShellRouteParameters query)
					value.SetValue(QueryAttributesProperty, query);

				if (Parent is not null)
				{
					((ShellSection)Parent).UpdateDisplayedPage();
				}
			}
		}

		internal void EvaluateDisconnect()
		{
			if (!_createdViaService)
				return;

			// If the user has set the IsVisible property on this shell content to false
			bool disconnect = true;

			Shell shell = null;

			if (Parent is ShellSection shellSection &&
					  shellSection.Parent is ShellItem shellItem &&
					  shellItem.Parent is Shell shellInstance)
			{
				shell = shellInstance;
				disconnect =
					!this.IsVisible || // user has set the IsVisible property to false
					(_contentCache is not null && !_contentCache.IsVisible) || // user has set IsVisible on the Page to false
					shell.CurrentItem != shellItem || // user has navigated to a different TabBar or a different FlyoutItem
					!shellSection.IsVisible || // user has set IsVisible on the ShellSection to false
					this.Window is null; // user has set the main page to a different shell instance
			}

			if (!disconnect)
			{
				shell?.NotifyFlyoutBehaviorObservers();
				return;
			}

			if (_contentCache is not null)
			{
				_contentCache.Unloaded -= OnPageUnloaded;
				RemoveLogicalChild(_contentCache);
			}

			_contentCache = null;
		}

		protected override void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == WindowProperty.PropertyName)
			{
				if (_contentCache?.IsLoaded == true)
				{
					return;
				}

				EvaluateDisconnect();
			}
			else if (propertyName == TitleProperty.PropertyName)
			{
				// Propagate child Title change to parent ShellSection's handler
				// so the mapper can update platform tab titles.
				if (Parent is ShellSection section)
				{
					section.Handler?.UpdateValue(nameof(Title));
				}
			}
		}

		void OnPageUnloaded(object sender, EventArgs e) => EvaluateDisconnect();

		public static implicit operator ShellContent(TemplatedPage page)
		{
			if (page.Parent != null)
			{
				return (ShellContent)page.Parent;
			}

			var shellContent = new ShellContent();

			var pageRoute = Routing.GetRoute(page);

			shellContent.Route = Routing.GenerateImplicitRoute(pageRoute);

			shellContent.Content = page;
			shellContent.SetBinding(TitleProperty, static (TemplatedPage page) => page.Title, BindingMode.OneWay, source: page);
			shellContent.SetBinding(IconProperty, static (TemplatedPage page) => page.IconImageSource, BindingMode.OneWay, source: page);
			shellContent.SetBinding(FlyoutIconProperty, static (TemplatedPage page) => page.IconImageSource, BindingMode.OneWay, source: page);

			return shellContent;
		}

		static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var shellContent = (ShellContent)bindable;
			shellContent._createdViaService = false;
			// This check is wrong but will work for testing
			if (shellContent.ContentTemplate == null)
			{
				// deparent old item
				if (oldValue is Page oldElement)
				{
					shellContent.ContentCache = null;
				}

				if (newValue is Page newElement)
				{
					shellContent.ContentCache = newElement;
				}
				else if (newValue != null)
				{
					throw new InvalidOperationException($"{nameof(ShellContent)} {nameof(Content)} should be of type {nameof(Page)}. Title {shellContent?.Title}, Route {shellContent?.Route} ");
				}
			}

			if (shellContent.Parent?.Parent is ShellItem shellItem)
				shellItem.SendStructureChanged();
		}

		void MenuItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
				foreach (Element el in e.NewItems)
					OnChildAdded(el);

			if (e.OldItems != null)
				for (var i = 0; i < e.OldItems.Count; i++)
				{
					var el = (Element)e.OldItems[i];
					OnChildRemoved(el, e.OldStartingIndex + i);
				}
		}

		void QueryParametersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				foreach (var parameter in new List<ShellContentQueryParameter>(_trackedQueryParameters))
					UntrackQueryParameter(parameter);
			}
			else if (e.OldItems is not null)
			{
				// An instance can appear more than once or be moved. Only detach it once it is absent.
				var currentParameters = new HashSet<ShellContentQueryParameter>(QueryParameters);
				foreach (ShellContentQueryParameter parameter in e.OldItems)
					if (parameter is not null && !currentParameters.Contains(parameter))
						UntrackQueryParameter(parameter);
			}

			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				foreach (var parameter in QueryParameters)
					TrackQueryParameterIfNeeded(parameter);
			}
			else if (e.NewItems is not null)
				foreach (ShellContentQueryParameter parameter in e.NewItems)
					TrackQueryParameterIfNeeded(parameter);

			ApplyQueryParametersIfVisible();
		}

		void TrackQueryParameterIfNeeded(ShellContentQueryParameter parameter)
		{
			if (parameter is not null && _trackedQueryParameters.Add(parameter))
				TrackQueryParameter(parameter);
		}

		void TrackQueryParameter(ShellContentQueryParameter parameter)
		{
			parameter.PropertyChanged += OnQueryParameterPropertyChanged;
			AddLogicalChild(parameter);
		}

		void UntrackQueryParameter(ShellContentQueryParameter parameter)
		{
			_trackedQueryParameters.Remove(parameter);
			parameter.PropertyChanged -= OnQueryParameterPropertyChanged;
			RemoveLogicalChild(parameter);
		}

		void OnQueryParameterPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellContentQueryParameter.NameProperty.PropertyName ||
				e.PropertyName == ShellContentQueryParameter.ValueProperty.PropertyName)
			{
				ApplyQueryParametersIfVisible();
			}
		}

		void ApplyQueryParametersIfVisible()
		{
			if (IsSelectedContent)
				ApplyQueryAttributesFromParameterChange();
		}

		static void OnQueryStringChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((ShellContent)bindable).ApplyQueryParametersIfVisible();
		}

		internal void ApplyQueryAttributesFromSelection()
		{
			if (this.FindParentOfType<Shell>()?.NavigationManager.AccumulateNavigatedEvents == true)
				return;

			ApplyQueryAttributesFromSelectionCore(requireSelected: true);
		}

		internal void ApplyQueryAttributesFromIntermediateNavigation()
		{
			ApplyQueryAttributesFromSelectionCore(requireSelected: false);
		}

		void ApplyQueryAttributesFromSelectionCore(bool requireSelected)
		{
			if (requireSelected && !IsSelectedContent)
				return;

			var query = CreateSelectionQueryParameters(out bool hasQueryParameters);
			if (!hasQueryParameters && !_hasAppliedQueryParameters)
				return;

			ApplyQueryAttributesCore(query);
			_hasAppliedQueryParameters = hasQueryParameters;
		}

		void ApplyQueryAttributesFromParameterChange()
		{
			var query = CreateParameterChangeQueryParameters(out bool hasQueryParameters);
			if (!hasQueryParameters && !_hasAppliedQueryParameters)
				return;

			var currentQuery = GetValue(QueryAttributesProperty) as ShellRouteParameters;
			var currentContentQuery = (ContentCache as BindableObject)?.GetValue(QueryAttributesProperty) as ShellRouteParameters;
			if (query.IsEquivalentTo(currentQuery) &&
				(ContentCache is null || query.IsEquivalentTo(currentContentQuery)))
			{
				return;
			}

			ApplyQueryAttributesCore(query);
			_hasAppliedQueryParameters = hasQueryParameters;
		}

		internal override void ApplyQueryAttributes(ShellRouteParameters query)
		{
			base.ApplyQueryAttributes(query);

			var mergedQuery = CreateMergedQueryParameters(query, out bool hasQueryParameters);
			ApplyQueryAttributesCore(mergedQuery);
			_hasAppliedQueryParameters = hasQueryParameters;
		}

		void ApplyQueryAttributesCore(ShellRouteParameters query)
		{
			// If the query parameters are empty and this attribute wasn't previously set
			// That means there's no work to be done here.
			// An empty query is only valid if we've previously propagated
			// something to this bindable property
			if (query.Count == 0 && !this.IsSet(QueryAttributesProperty))
				return;

			SetValue(QueryAttributesProperty, query);

			if (ContentCache is BindableObject bindable)
				bindable.SetValue(QueryAttributesProperty, query);
		}

		ShellRouteParameters CreateMergedQueryParameters(ShellRouteParameters navigationQuery, out bool hasQueryParameters)
		{
			var contentQuery = GetContentQueryParameters();
			hasQueryParameters = contentQuery.Count > 0;

			if (!hasQueryParameters)
			{
				_lastAppliedContentParameterNames.Clear();
				return navigationQuery;
			}

			var query = new ShellRouteParameters(navigationQuery);
			var appliedNames = new HashSet<string>(StringComparer.Ordinal);

			foreach (var parameter in contentQuery)
			{
				if (!query.ContainsKey(parameter.Key) || query.IsShellContentParameter(parameter.Key))
				{
					SetContentQueryParameter(query, parameter.Key, parameter.Value);
					appliedNames.Add(parameter.Key);
				}
			}

			UpdateLastAppliedContentParameterNames(appliedNames);
			return query;
		}

		ShellRouteParameters CreateSelectionQueryParameters(out bool hasQueryParameters)
		{
			var existing = GetValue(QueryAttributesProperty) as ShellRouteParameters;
			var query = existing is null ? new ShellRouteParameters() : new ShellRouteParameters(existing);
			var contentQuery = GetContentQueryParameters();

			foreach (var name in _lastAppliedContentParameterNames)
				query.RemoveShellContentQueryParameter(name);

			foreach (var parameter in contentQuery)
				SetContentQueryParameter(query, parameter.Key, parameter.Value);

			hasQueryParameters = contentQuery.Count > 0;
			UpdateLastAppliedContentParameterNames(contentQuery.Keys);
			return query;
		}

		ShellRouteParameters CreateParameterChangeQueryParameters(out bool hasQueryParameters)
		{
			var existing = GetValue(QueryAttributesProperty) as ShellRouteParameters;
			var query = existing is null ? new ShellRouteParameters() : new ShellRouteParameters(existing);
			var contentQuery = GetContentQueryParameters();
			var previouslyAppliedNames = new HashSet<string>(_lastAppliedContentParameterNames, StringComparer.Ordinal);
			var appliedNames = new HashSet<string>(StringComparer.Ordinal);

			foreach (var name in previouslyAppliedNames)
				query.RemoveShellContentQueryParameter(name);

			foreach (var parameter in contentQuery)
			{
				if (previouslyAppliedNames.Contains(parameter.Key) || !query.ContainsKey(parameter.Key))
				{
					SetContentQueryParameter(query, parameter.Key, parameter.Value);
					appliedNames.Add(parameter.Key);
				}
			}

			hasQueryParameters = contentQuery.Count > 0;
			UpdateLastAppliedContentParameterNames(appliedNames);
			return query;
		}

		Dictionary<string, ContentQueryParameter> GetContentQueryParameters()
		{
			var contentQuery = new Dictionary<string, ContentQueryParameter>(StringComparer.Ordinal);
			var queryStringParameters = new ShellRouteParameters();
			queryStringParameters.SetQueryStringParameters(QueryString);

			foreach (var parameter in queryStringParameters)
			{
				if (!string.IsNullOrEmpty(parameter.Key))
					contentQuery[parameter.Key] = new ContentQueryParameter(parameter.Value, isQueryString: true);
			}

			foreach (var parameter in QueryParameters)
			{
				if (!string.IsNullOrEmpty(parameter?.Name))
					contentQuery[parameter.Name] = new ContentQueryParameter(parameter.Value, isQueryString: false);
			}

			return contentQuery;
		}

		static void SetContentQueryParameter(ShellRouteParameters query, string name, ContentQueryParameter parameter)
		{
			if (parameter.IsQueryString)
				query.SetShellContentQueryStringParameter(name, parameter.Value);
			else
				query.SetShellContentQueryParameter(name, parameter.Value);
		}

		void UpdateLastAppliedContentParameterNames(IEnumerable<string> names)
		{
			_lastAppliedContentParameterNames.Clear();
			foreach (var name in names)
				_lastAppliedContentParameterNames.Add(name);
		}

		readonly struct ContentQueryParameter
		{
			public ContentQueryParameter(object value, bool isQueryString)
			{
				Value = value;
				IsQueryString = isQueryString;
			}

			public object Value { get; }
			public bool IsQueryString { get; }
		}

		static void OnQueryAttributesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			ApplyQueryAttributes(bindable, newValue as ShellRouteParameters, oldValue as ShellRouteParameters);
		}

		static void ApplyQueryAttributes(object content, ShellRouteParameters query, ShellRouteParameters oldQuery)
		{
			query = query ?? new ShellRouteParameters();
			oldQuery = oldQuery ?? new ShellRouteParameters();

			if (content is IQueryAttributable attributable)
			{
				attributable
					.ApplyQueryAttributes(query.ToReadOnlyIfUsingShellNavigationQueryParameters());
			}

			if (content is BindableObject bindable && bindable.BindingContext != null && content != bindable.BindingContext)
				ApplyQueryAttributes(bindable.BindingContext, query, oldQuery);

			if (RuntimeFeature.IsQueryPropertyAttributeSupported)
			{
				var type = content.GetType();
				var queryPropertyAttributes = type.GetCustomAttributes(typeof(QueryPropertyAttribute), true);
				if (queryPropertyAttributes.Length == 0)
				{
					ClearQueryIfAppliedToPage(query, content);
					return;
				}

				foreach (QueryPropertyAttribute attrib in queryPropertyAttributes)
				{
					if (query.TryGetValue(attrib.QueryId, out var value))
					{
						PropertyInfo prop = type.GetRuntimeProperty(attrib.Name);

						if (prop != null && prop.CanWrite && prop.SetMethod.IsPublic)
						{
							if (prop.PropertyType == typeof(string))
							{
								if (value != null)
								{
									if (query.IsShellContentQueryParameter(attrib.QueryId))
										value = Convert.ToString(value, global::System.Globalization.CultureInfo.InvariantCulture);
									else if (!query.IsShellContentQueryStringParameter(attrib.QueryId))
										value = global::System.Net.WebUtility.UrlDecode((string)value);
								}

								prop.SetValue(content, value);
							}
							else
							{
								// Handle nullable types
								Type targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

								if (value == null)
								{
									prop.SetValue(content, null);
								}
								else
								{
									var culture = query.IsShellContentParameter(attrib.QueryId)
										? global::System.Globalization.CultureInfo.InvariantCulture
										: global::System.Globalization.CultureInfo.CurrentCulture;
									var castValue = Convert.ChangeType(value, targetType, culture);
									prop.SetValue(content, castValue);
								}
							}
						}
					}
					else if (oldQuery.TryGetValue(attrib.QueryId, out var oldValue))
					{
						PropertyInfo prop = type.GetRuntimeProperty(attrib.Name);

						if (prop != null && prop.CanWrite && prop.SetMethod.IsPublic)
						{
							object defaultValue = prop.PropertyType.IsValueType &&
								Nullable.GetUnderlyingType(prop.PropertyType) is null
									? Activator.CreateInstance(prop.PropertyType)
									: null;
							prop.SetValue(content, defaultValue);
						}
					}
				}
			}

			ClearQueryIfAppliedToPage(query, content);

			static void ClearQueryIfAppliedToPage(ShellRouteParameters query, object content)
			{
				// Once we've applied the attributes to ContentPage lets remove the 
				// parameters used during navigation
				if (content is ContentPage)
					query.ResetToQueryParameters();
			}
		}
#nullable enable
		private sealed class ShellContentConverter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
				=> sourceType == typeof(TemplatedPage);

			public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
				=> false;

			public override object? ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
			{
				if (value is TemplatedPage templatedPage)
				{
					return (ShellContent)templatedPage;
				}

				throw new NotSupportedException();
			}

			public override object? ConvertTo(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object? value, Type destinationType)
			{
				throw new NotSupportedException();
			}
		}
	}
}

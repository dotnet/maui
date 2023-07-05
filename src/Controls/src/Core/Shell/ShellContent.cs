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
	/// <include file="../../../docs/Microsoft.Maui.Controls/ShellContent.xml" path="Type[@FullName='Microsoft.Maui.Controls.ShellContent']/Docs/*" />
	[ContentProperty(nameof(Content))]
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

		internal static readonly BindableProperty QueryAttributesProperty =
			BindableProperty.CreateAttached("QueryAttributes", typeof(ShellRouteParameters), typeof(ShellContent), defaultValue: null, propertyChanged: OnQueryAttributesPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellContent.xml" path="//Member[@MemberName='MenuItems']/Docs/*" />
		public MenuItemCollection MenuItems => (MenuItemCollection)GetValue(MenuItemsProperty);

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellContent.xml" path="//Member[@MemberName='Content']/Docs/*" />
		public object Content
		{
			get => GetValue(ContentProperty);
			set => SetValue(ContentProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellContent.xml" path="//Member[@MemberName='ContentTemplate']/Docs/*" />
		public DataTemplate ContentTemplate
		{
			get => (DataTemplate)GetValue(ContentTemplateProperty);
			set => SetValue(ContentTemplateProperty, value);
		}

		Page IShellContentController.Page => ContentCache;

		EventHandler _isPageVisibleChanged;
		event EventHandler IShellContentController.IsPageVisibleChanged { add => _isPageVisibleChanged += value; remove => _isPageVisibleChanged -= value; }

		Page IShellContentController.GetOrCreateContent()
		{
			var template = ContentTemplate;
			var content = Content;

			Page result = null;
			if (template == null)
			{
				if (content is Page page)
					result = page;
			}
			else
			{
				if (template.Type != null)
				{
					template.LoadTemplate = () =>
					{
						var services = Parent?.FindMauiContext()?.Services;
						if (services != null)
						{
							return services.GetService(template.Type) ?? Activator.CreateInstance(template.Type);
						}
						return Activator.CreateInstance(template.Type);
					};
				}
				result = ContentCache ?? (Page)template.CreateContent(content, this);
				ContentCache = result;
			}

			if (result == null)
				throw new InvalidOperationException($"No Content found for {nameof(ShellContent)}, Title:{Title}, Route {Route}");

			if (result is TabbedPage)
				throw new NotSupportedException($"Shell is currently not compatible with TabbedPage. Please use TabBar, Tab or switch to using NavigationPage for your {Application.Current}.MainPage");

			if (result is FlyoutPage)
				throw new NotSupportedException("Shell is currently not compatible with FlyoutPage.");

			if (result is NavigationPage)
				throw new NotSupportedException("Shell is currently not compatible with NavigationPage. Shell has Navigation built in and doesn't require a NavigationPage.");

			if (GetValue(QueryAttributesProperty) is ShellRouteParameters delayedQueryParams)
				result.SetValue(QueryAttributesProperty, delayedQueryParams);

			return result;
		}

		void IShellContentController.RecyclePage(Page page)
		{
		}

		Page _contentCache;

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellContent.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ShellContent() => ((INotifyCollectionChanged)MenuItems).CollectionChanged += MenuItemsCollectionChanged;

		internal bool IsVisibleContent => Parent is ShellSection shellSection && shellSection.IsVisibleSection && shellSection.CurrentItem == this;

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
					RemoveLogicalChild(oldCache);

				if (value != null && value.Parent != this)
				{
					AddLogicalChild(value);
				}

				if (Parent != null)
					((ShellSection)Parent).UpdateDisplayedPage();
			}
		}

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
			shellContent.SetBinding(TitleProperty, new Binding(nameof(Title), BindingMode.OneWay, source: page));
			shellContent.SetBinding(IconProperty, new Binding(nameof(Icon), BindingMode.OneWay, source: page));
			shellContent.SetBinding(FlyoutIconProperty, new Binding(nameof(FlyoutIcon), BindingMode.OneWay, source: page));

			return shellContent;
		}

		static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var shellContent = (ShellContent)bindable;
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

		internal override void ApplyQueryAttributes(ShellRouteParameters query)
		{
			base.ApplyQueryAttributes(query);

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
								value = global::System.Net.WebUtility.UrlDecode((string)value);

							prop.SetValue(content, value);
						}
						else
						{
							var castValue = Convert.ChangeType(value, prop.PropertyType);
							prop.SetValue(content, castValue);
						}
					}
				}
				else if (oldQuery.TryGetValue(attrib.QueryId, out var oldValue))
				{
					PropertyInfo prop = type.GetRuntimeProperty(attrib.Name);

					if (prop != null && prop.CanWrite && prop.SetMethod.IsPublic)
						prop.SetValue(content, null);
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
	}
}

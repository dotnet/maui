using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

#if NETSTANDARD1_0
using System.Linq;
#endif

using System.Reflection;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[ContentProperty(nameof(Content))]
	public class ShellContent : BaseShellItem, IShellContentController
	{
		static readonly BindablePropertyKey MenuItemsPropertyKey =
			BindableProperty.CreateReadOnly(nameof(MenuItems), typeof(MenuItemCollection), typeof(ShellContent), null,
				defaultValueCreator: bo => new MenuItemCollection());

		public static readonly BindableProperty MenuItemsProperty = MenuItemsPropertyKey.BindableProperty;

		public static readonly BindableProperty ContentProperty =
			BindableProperty.Create(nameof(Content), typeof(object), typeof(ShellContent), null, BindingMode.OneTime, propertyChanged: OnContentChanged);

		public static readonly BindableProperty ContentTemplateProperty =
			BindableProperty.Create(nameof(ContentTemplate), typeof(DataTemplate), typeof(ShellContent), null, BindingMode.OneTime);

		internal static readonly BindableProperty QueryAttributesProperty =
			BindableProperty.CreateAttached("QueryAttributes", typeof(IDictionary<string, string>), typeof(ShellContent), defaultValue: null, propertyChanged: OnQueryAttributesPropertyChanged);

		public MenuItemCollection MenuItems => (MenuItemCollection)GetValue(MenuItemsProperty);

		public object Content {
			get => GetValue(ContentProperty);
			set => SetValue(ContentProperty, value);
		}

		public DataTemplate ContentTemplate {
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
				result = ContentCache ?? (Page)template.CreateContent(content, this);
				ContentCache = result;
			}

			if (result == null)
				throw new InvalidOperationException($"No Content found for {nameof(ShellContent)}, Title:{Title}, Route {Route}");

			if (GetValue(QueryAttributesProperty) is IDictionary<string, string> delayedQueryParams)
				result.SetValue(QueryAttributesProperty, delayedQueryParams);

			return result;
		}

		void IShellContentController.RecyclePage(Page page)
		{
		}

		Page _contentCache;
		IList<Element> _logicalChildren = new List<Element>();
		ReadOnlyCollection<Element> _logicalChildrenReadOnly;

		public ShellContent() => ((INotifyCollectionChanged)MenuItems).CollectionChanged += MenuItemsCollectionChanged;

		internal bool IsVisibleContent => Parent is ShellSection shellSection && shellSection.IsVisibleSection && shellSection.CurrentItem == this;
		internal override ReadOnlyCollection<Element> LogicalChildrenInternal => _logicalChildrenReadOnly ?? (_logicalChildrenReadOnly = new ReadOnlyCollection<Element>(_logicalChildren));

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
					page.SendAppearing();
					(sender as Page).ParentSet -= OnPresentedPageParentSet;
				}
			}
			else
			{
				page.SendAppearing();
			}
		}

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);
			if (child is Page page)
			{
				if (IsVisibleContent && page.IsVisible)
				{
					SendAppearing();
					SendPageAppearing(page);
				}

				page.PropertyChanged += OnPagePropertyChanged;
				_isPageVisibleChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		protected override void OnChildRemoved(Element child)
		{
			base.OnChildRemoved(child);
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
				_contentCache = value;
				if (value != null && value.Parent != this)
				{
					OnChildAdded(value);
				}

				if (Parent != null)
					((ShellSection)Parent).UpdateDisplayedPage();
			}
		}
		
		public static implicit operator ShellContent(TemplatedPage page)
		{
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
					shellContent.OnChildRemoved(oldElement);
					shellContent.ContentCache = null;
				}

				// make sure LogicalChildren collection stays consisten
				shellContent._logicalChildren.Clear();
				if (newValue is Page newElement)
				{
					shellContent._logicalChildren.Add((Element)newValue);
					shellContent.ContentCache = newElement;
				}
				else if(newValue != null)
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
				foreach (Element el in e.OldItems)
					OnChildRemoved(el);
		}

		internal override void ApplyQueryAttributes(IDictionary<string, string> query)
		{
			base.ApplyQueryAttributes(query);
			SetValue(QueryAttributesProperty, query);

			if (Content is BindableObject bindable)
				bindable.SetValue(QueryAttributesProperty, query);
		}

		static void OnQueryAttributesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (newValue is IDictionary<string, string> query)
				ApplyQueryAttributes(bindable, query);
		}

		static void ApplyQueryAttributes(object content, IDictionary<string, string> query)
		{
			if (content is IQueryAttributable attributable)
				attributable.ApplyQueryAttributes(query);

			if (content is BindableObject bindable && bindable.BindingContext != null && content != bindable.BindingContext)
				ApplyQueryAttributes(bindable.BindingContext, query);

			var type = content.GetType();
			var typeInfo = type.GetTypeInfo();
#if NETSTANDARD1_0
			var queryPropertyAttributes = typeInfo.GetCustomAttributes(typeof(QueryPropertyAttribute), true).ToArray();
#else
			var queryPropertyAttributes = typeInfo.GetCustomAttributes(typeof(QueryPropertyAttribute), true);
#endif

			if (queryPropertyAttributes.Length == 0)
				return;

			foreach (QueryPropertyAttribute attrib in queryPropertyAttributes) {
				if (query.TryGetValue(attrib.QueryId, out var value)) {
					PropertyInfo prop = type.GetRuntimeProperty(attrib.Name);

					if (prop != null && prop.CanWrite && prop.SetMethod.IsPublic)
						prop.SetValue(content, value);
				}
			}
		}
	}
}

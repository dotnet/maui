#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="MultiPage{T}"/> that displays an array of tabs across the top of the screen, each of which loads content onto the screen.</summary>
	[ContentProperty(nameof(Children))]
#if IOS || MACCATALYST
	[ElementHandler(typeof(Handlers.Compatibility.TabbedRenderer))]
#elif WINDOWS || ANDROID || TIZEN
	[ElementHandler(typeof(TabbedViewHandler))]
#endif
	public partial class TabbedPage : MultiPage<Page>, IBarElement, IElementConfiguration<TabbedPage>, ITabbedView
	{
		/// <summary>Bindable property for <see cref="BarBackgroundColor"/>.</summary>
		public static readonly BindableProperty BarBackgroundColorProperty = BarElement.BarBackgroundColorProperty;

		/// <summary>Bindable property for <see cref="BarBackground"/>.</summary>
		public static readonly BindableProperty BarBackgroundProperty = BarElement.BarBackgroundProperty;

		/// <summary>Bindable property for <see cref="BarTextColor"/>.</summary>
		public static readonly BindableProperty BarTextColorProperty = BarElement.BarTextColorProperty;

		/// <summary>Bindable property for <see cref="UnselectedTabColor"/>.</summary>
		public static readonly BindableProperty UnselectedTabColorProperty = BindableProperty.Create(nameof(UnselectedTabColor), typeof(Color), typeof(TabbedPage), default(Color));

		/// <summary>Bindable property for <see cref="SelectedTabColor"/>.</summary>
		public static readonly BindableProperty SelectedTabColorProperty = BindableProperty.Create(nameof(SelectedTabColor), typeof(Color), typeof(TabbedPage), default(Color));

		/// <summary>Bindable property for attached property <c>BadgeText</c>.</summary>
		public static readonly BindableProperty BadgeTextProperty =
			BindableProperty.CreateAttached("BadgeText", typeof(string), typeof(TabbedPage), null);

		/// <summary>Bindable property for attached property <c>BadgeColor</c>.</summary>
		public static readonly BindableProperty BadgeColorProperty =
			BindableProperty.CreateAttached("BadgeColor", typeof(Color), typeof(TabbedPage), null);

		/// <summary>Bindable property for attached property <c>BadgeTextColor</c>.</summary>
		public static readonly BindableProperty BadgeTextColorProperty =
			BindableProperty.CreateAttached("BadgeTextColor", typeof(Color), typeof(TabbedPage), null);

		readonly Lazy<PlatformConfigurationRegistry<TabbedPage>> _platformConfigurationRegistry;

		// Stores the collection change args from OnPagesChanged so MapItemsSource
		// can handle Add/Remove incrementally instead of full rebuild.
		internal NotifyCollectionChangedEventArgs _pendingPagesChangedArgs;

		// Stores the page and property that changed so the mapper can refresh
		// only that page's tab bar item instead of all children.
		internal Page _pendingPropertyChangedPage;
		internal string _pendingPropertyChangedPropertyName;

		// Tracks pages with active PropertyChanged subscriptions so they can be
		// unsubscribed on Reset (where Children is already empty and e.OldItems is null).
		HashSet<Page> _subscribedPages;

		/// <summary>Gets or sets the background color of the tab bar. This is a bindable property.</summary>
		public Color BarBackgroundColor
		{
			get => (Color)GetValue(BarElement.BarBackgroundColorProperty);
			set => SetValue(BarElement.BarBackgroundColorProperty, value);
		}

		/// <summary>Gets or sets the brush used for the tab bar background. This is a bindable property.</summary>
		public Brush BarBackground
		{
			get => (Brush)GetValue(BarElement.BarBackgroundProperty);
			set => SetValue(BarElement.BarBackgroundProperty, value);
		}

		/// <summary>Gets or sets the color of the tab bar text. This is a bindable property.</summary>
		public Color BarTextColor
		{
			get => (Color)GetValue(BarElement.BarTextColorProperty);
			set => SetValue(BarElement.BarTextColorProperty, value);
		}

		/// <summary>Gets or sets the color of unselected tabs. This is a bindable property.</summary>
		public Color UnselectedTabColor
		{
			get => (Color)GetValue(UnselectedTabColorProperty);
			set => SetValue(UnselectedTabColorProperty, value);
		}
		/// <summary>Gets or sets the color of the selected tab. This is a bindable property.</summary>
		public Color SelectedTabColor
		{
			get => (Color)GetValue(SelectedTabColorProperty);
			set => SetValue(SelectedTabColorProperty, value);
		}

		/// <summary>Gets the badge text displayed for a page in a <see cref="TabbedPage"/>.</summary>
		/// <remarks>
		/// A non-null, non-empty value displays a badge containing the value. An empty string displays
		/// a dot indicator, and <see langword="null"/> hides the badge. On Windows, non-numeric text
		/// displays as a dot indicator.
		/// </remarks>
		public static string GetBadgeText(BindableObject page) =>
			(string)page.GetValue(BadgeTextProperty);

		/// <summary>Sets the badge text displayed for a page in a <see cref="TabbedPage"/>.</summary>
		/// <param name="page">The page whose tab badge is updated.</param>
		/// <param name="value">The badge text, an empty string for a dot, or <see langword="null"/> to hide the badge.</param>
		public static void SetBadgeText(BindableObject page, string value) =>
			page.SetValue(BadgeTextProperty, value);

		/// <summary>Gets the background color of the badge displayed for a page in a <see cref="TabbedPage"/>.</summary>
		/// <remarks>
		/// On iOS and Mac Catalyst, this maps to <c>UITabBarItem.BadgeColor</c>. System tab bars
		/// introduced in iOS 18 and Mac Catalyst 18 may retain this value but render the system
		/// badge color instead.
		/// </remarks>
		public static Color GetBadgeColor(BindableObject page) =>
			(Color)page.GetValue(BadgeColorProperty);

		/// <summary>Sets the background color of the badge displayed for a page in a <see cref="TabbedPage"/>.</summary>
		public static void SetBadgeColor(BindableObject page, Color value) =>
			page.SetValue(BadgeColorProperty, value);

		/// <summary>Gets the text color of the badge displayed for a page in a <see cref="TabbedPage"/>.</summary>
		/// <remarks>
		/// On iOS and Mac Catalyst, this maps to <c>UITabBarItem.SetBadgeTextAttributes</c>. System
		/// tab bars introduced in iOS 18 and Mac Catalyst 18 may retain these attributes but render
		/// the system badge text color instead.
		/// </remarks>
		public static Color GetBadgeTextColor(BindableObject page) =>
			(Color)page.GetValue(BadgeTextColorProperty);

		/// <summary>Sets the text color of the badge displayed for a page in a <see cref="TabbedPage"/>.</summary>
		public static void SetBadgeTextColor(BindableObject page, Color value) =>
			page.SetValue(BadgeTextColorProperty, value);

		protected override Page CreateDefault(object item)
		{
			var page = new Page();
			if (item != null)
				page.Title = item.ToString();

			return page;
		}

		/// <summary>Initializes a new instance of the <see cref="TabbedPage"/> class.</summary>
		public TabbedPage()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<TabbedPage>>(() => new PlatformConfigurationRegistry<TabbedPage>(this));
		}

		/// <inheritdoc/>
		public new IPlatformElementConfiguration<T, TabbedPage> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		[Obsolete("Use ArrangeOverride instead")]
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			// We don't want forcelayout to call the legacy
			// Page.LayoutChildren code
		}

		partial void OnHandlerChangingPartial(HandlerChangingEventArgs args);
		private protected override void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			base.OnHandlerChangingCore(args);

			if (args.NewHandler == null)
			{
				PagesChanged -= OnPagesChanged;
				WireUnwireChanges(false);
			}
			else if (args.OldHandler == null)
			{
				PagesChanged += OnPagesChanged;
				WireUnwireChanges(true);
			}

			OnHandlerChangingPartial(args);
			void OnPagesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
			{
				WireUnwireChanges(false);

				// Unsubscribe removed pages — they're no longer in Children after mutation.
				// On Reset, e.OldItems is null and Children is already empty, so use _subscribedPages.
				if (e.OldItems is not null)
				{
					foreach (var item in e.OldItems)
					{
						if (item is Page page)
						{
							page.PropertyChanged -= OnPagePropertyChanged;
							_subscribedPages?.Remove(page);
						}
					}
				}
				else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset
					&& _subscribedPages is not null)
				{
					// Reset path: Children is already empty, e.OldItems is null.
					// Unsubscribe all previously tracked pages.
					foreach (var page in _subscribedPages)
					{
						page.PropertyChanged -= OnPagePropertyChanged;
					}
					_subscribedPages.Clear();
				}

				_pendingPagesChangedArgs = e;
				Handler?.UpdateValue(TabbedPage.ItemsSourceProperty.PropertyName);

				// Clear after UpdateValue — iOS mapper consumes it synchronously during the call above.
				// On other platforms the mapper doesn't use it, so clear to avoid retaining removed pages.
				_pendingPagesChangedArgs = null;
				WireUnwireChanges(true);
			}

			void WireUnwireChanges(bool wire)
			{
				foreach (var page in Children)
				{
					if (wire)
					{
						page.PropertyChanged += OnPagePropertyChanged;
						_subscribedPages ??= new HashSet<Page>();
						_subscribedPages.Add(page);
					}
					else
					{
						page.PropertyChanged -= OnPagePropertyChanged;
						_subscribedPages?.Remove(page);
					}
				}
			}

			void OnPagePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
			{
				if (e.PropertyName == Page.TitleProperty.PropertyName ||
					e.PropertyName == Page.IconImageSourceProperty.PropertyName ||
					e.PropertyName == BadgeTextProperty.PropertyName ||
					e.PropertyName == BadgeColorProperty.PropertyName ||
					e.PropertyName == BadgeTextColorProperty.PropertyName)
				{
					_pendingPropertyChangedPage = sender as Page;
					_pendingPropertyChangedPropertyName = e.PropertyName;
					Handler?.UpdateValue(TabbedPage.ItemsSourceProperty.PropertyName);
					_pendingPropertyChangedPage = null;
					_pendingPropertyChangedPropertyName = null;
				}
			}
		}

		/// <summary>
		/// Adapts a <see cref="Page"/> to the <see cref="ITab"/> interface for ITabbedView consumption.
		/// </summary>
		internal sealed class PageTabAdapter : ITab
		{
			public PageTabAdapter(Page page) => Page = page;
			internal Page Page { get; }
			public string Title => Page.Title;
			public IImageSource Icon => Page.IconImageSource;
			public bool IsEnabled => Page.IsEnabled;
		}
	}
}
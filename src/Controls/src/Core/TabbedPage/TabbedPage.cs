#nullable disable
using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="MultiPage{T}"/> that displays an array of tabs across the top of the screen, each of which loads content onto the screen.</summary>
	[ContentProperty(nameof(Children))]
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

		readonly Lazy<PlatformConfigurationRegistry<TabbedPage>> _platformConfigurationRegistry;

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
				Handler?.UpdateValue(TabbedPage.ItemsSourceProperty.PropertyName);
				WireUnwireChanges(true);
			}

			void WireUnwireChanges(bool wire)
			{
				foreach (var page in Children)
				{
					if (wire)
						page.PropertyChanged += OnPagePropertyChanged;
					else
						page.PropertyChanged -= OnPagePropertyChanged;
				}
			}

			void OnPagePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
			{
				if (e.PropertyName == Page.TitleProperty.PropertyName)
					Handler?.UpdateValue(TabbedPage.ItemsSourceProperty.PropertyName);
			}
		}
	}
}
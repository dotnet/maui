using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/TabbedPage.xml" path="Type[@FullName='Microsoft.Maui.Controls.TabbedPage']/Docs/*" />
	[ContentProperty(nameof(Children))]
	public partial class TabbedPage : MultiPage<Page>, IBarElement, IElementConfiguration<TabbedPage>
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/TabbedPage.xml" path="//Member[@MemberName='BarBackgroundColorProperty']/Docs/*" />
		public static readonly BindableProperty BarBackgroundColorProperty = BarElement.BarBackgroundColorProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/TabbedPage.xml" path="//Member[@MemberName='BarBackgroundProperty']/Docs/*" />
		public static readonly BindableProperty BarBackgroundProperty = BarElement.BarBackgroundProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/TabbedPage.xml" path="//Member[@MemberName='BarTextColorProperty']/Docs/*" />
		public static readonly BindableProperty BarTextColorProperty = BarElement.BarTextColorProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/TabbedPage.xml" path="//Member[@MemberName='UnselectedTabColorProperty']/Docs/*" />
		public static readonly BindableProperty UnselectedTabColorProperty = BindableProperty.Create(nameof(UnselectedTabColor), typeof(Color), typeof(TabbedPage), default(Color));

		/// <include file="../../docs/Microsoft.Maui.Controls/TabbedPage.xml" path="//Member[@MemberName='SelectedTabColorProperty']/Docs/*" />
		public static readonly BindableProperty SelectedTabColorProperty = BindableProperty.Create(nameof(SelectedTabColor), typeof(Color), typeof(TabbedPage), default(Color));

		readonly Lazy<PlatformConfigurationRegistry<TabbedPage>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/TabbedPage.xml" path="//Member[@MemberName='BarBackgroundColor']/Docs/*" />
		public Color BarBackgroundColor
		{
			get => (Color)GetValue(BarElement.BarBackgroundColorProperty);
			set => SetValue(BarElement.BarBackgroundColorProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TabbedPage.xml" path="//Member[@MemberName='BarBackground']/Docs/*" />
		public Brush BarBackground
		{
			get => (Brush)GetValue(BarElement.BarBackgroundProperty);
			set => SetValue(BarElement.BarBackgroundProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TabbedPage.xml" path="//Member[@MemberName='BarTextColor']/Docs/*" />
		public Color BarTextColor
		{
			get => (Color)GetValue(BarElement.BarTextColorProperty);
			set => SetValue(BarElement.BarTextColorProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TabbedPage.xml" path="//Member[@MemberName='UnselectedTabColor']/Docs/*" />
		public Color UnselectedTabColor
		{
			get => (Color)GetValue(UnselectedTabColorProperty);
			set => SetValue(UnselectedTabColorProperty, value);
		}
		/// <include file="../../docs/Microsoft.Maui.Controls/TabbedPage.xml" path="//Member[@MemberName='SelectedTabColor']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/TabbedPage.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public TabbedPage()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<TabbedPage>>(() => new PlatformConfigurationRegistry<TabbedPage>(this));
		}

		/// <inheritdoc/>
		public new IPlatformElementConfiguration<T, TabbedPage> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}
using System.Collections.Generic;
using System.Windows;
using Xamarin.Forms.Platform.WPF.Interfaces;
using WBrush = System.Windows.Media.Brush;

namespace Xamarin.Forms.Platform.WPF.Controls
{
	public class FormsMasterDetailPage : FormsFlyoutPage
	{
		public object MasterPage
		{
			get => base.FlyoutPage;
			set => base.FlyoutPage = value;
		}

	}

	public class FormsFlyoutPage : FormsPage
	{
		FormsContentControl lightFlyoutContentControl;
		FormsContentControl lightDetailContentControl;

		public static readonly DependencyProperty FlyoutPageProperty = DependencyProperty.Register(nameof(FlyoutPage), typeof(object), typeof(FormsFlyoutPage));
		public static readonly DependencyProperty DetailPageProperty = DependencyProperty.Register("DetailPage", typeof(object), typeof(FormsFlyoutPage));
		public static readonly DependencyProperty ContentLoaderProperty = DependencyProperty.Register("ContentLoader", typeof(IContentLoader), typeof(FormsFlyoutPage), new PropertyMetadata(new DefaultContentLoader()));
		public static readonly DependencyProperty IsPresentedProperty = DependencyProperty.Register("IsPresented", typeof(bool), typeof(FormsFlyoutPage));

		public object FlyoutPage
		{
			get { return (object)GetValue(FlyoutPageProperty); }
			set { SetValue(FlyoutPageProperty, value); }
		}

		public object DetailPage
		{
			get { return (object)GetValue(DetailPageProperty); }
			set { SetValue(DetailPageProperty, value); }
		}

		public bool IsPresented
		{
			get { return (bool)GetValue(IsPresentedProperty); }
			set { SetValue(IsPresentedProperty, value); }
		}

		public IContentLoader ContentLoader
		{
			get { return (IContentLoader)GetValue(ContentLoaderProperty); }
			set { SetValue(ContentLoaderProperty, value); }
		}

		public FormsFlyoutPage()
		{
			this.DefaultStyleKey = typeof(FormsFlyoutPage);
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			lightFlyoutContentControl = Template.FindName("PART_Master", this) as FormsContentControl;
			lightDetailContentControl = Template.FindName("PART_Detail_Content", this) as FormsContentControl;
		}

		public override string GetTitle()
		{
			if (lightDetailContentControl != null && lightDetailContentControl.Content is FormsPage page)
			{
				return page.GetTitle();
			}
			return this.Title;
		}

		public override WBrush GetTitleBarBackgroundColor()
		{
			if (lightDetailContentControl != null && lightDetailContentControl.Content is FormsPage page)
			{
				return page.GetTitleBarBackgroundColor();
			}
			return this.TitleBarBackgroundColor;
		}

		public override WBrush GetTitleBarTextColor()
		{
			if (lightDetailContentControl != null && lightDetailContentControl.Content is FormsPage page)
			{
				return page.GetTitleBarTextColor();
			}
			return this.TitleBarTextColor;
		}

		public override IEnumerable<FrameworkElement> GetPrimaryTopBarCommands()
		{
			List<FrameworkElement> frameworkElements = new List<FrameworkElement>();
			frameworkElements.AddRange(this.PrimaryTopBarCommands);

			if (lightFlyoutContentControl != null && lightFlyoutContentControl.Content is FormsPage flyoutPage)
			{
				frameworkElements.AddRange(flyoutPage.GetPrimaryTopBarCommands());
			}

			if (lightDetailContentControl != null && lightDetailContentControl.Content is FormsPage page)
			{
				frameworkElements.AddRange(page.GetPrimaryTopBarCommands());
			}

			return frameworkElements;
		}

		public override IEnumerable<FrameworkElement> GetSecondaryTopBarCommands()
		{
			List<FrameworkElement> frameworkElements = new List<FrameworkElement>();
			frameworkElements.AddRange(this.SecondaryTopBarCommands);

			if (lightFlyoutContentControl != null && lightFlyoutContentControl.Content is FormsPage flyoutPage)
			{
				frameworkElements.AddRange(flyoutPage.GetSecondaryTopBarCommands());
			}

			if (lightDetailContentControl != null && lightDetailContentControl.Content is FormsPage page)
			{
				frameworkElements.AddRange(page.GetSecondaryTopBarCommands());
			}

			return frameworkElements;
		}

		public override IEnumerable<FrameworkElement> GetPrimaryBottomBarCommands()
		{
			List<FrameworkElement> frameworkElements = new List<FrameworkElement>();
			frameworkElements.AddRange(this.PrimaryBottomBarCommands);

			if (lightFlyoutContentControl != null && lightFlyoutContentControl.Content is FormsPage flyoutPage)
			{
				frameworkElements.AddRange(flyoutPage.GetPrimaryBottomBarCommands());
			}

			if (lightDetailContentControl != null && lightDetailContentControl.Content is FormsPage page)
			{
				frameworkElements.AddRange(page.GetPrimaryBottomBarCommands());
			}

			return frameworkElements;
		}

		public override IEnumerable<FrameworkElement> GetSecondaryBottomBarCommands()
		{
			List<FrameworkElement> frameworkElements = new List<FrameworkElement>();
			frameworkElements.AddRange(this.SecondaryBottomBarCommands);

			if (lightFlyoutContentControl != null && lightFlyoutContentControl.Content is FormsPage flyoutPage)
			{
				frameworkElements.AddRange(flyoutPage.GetSecondaryBottomBarCommands());
			}

			if (lightDetailContentControl != null && lightDetailContentControl.Content is FormsPage page)
			{
				frameworkElements.AddRange(page.GetSecondaryBottomBarCommands());
			}

			return frameworkElements;
		}
	}
}

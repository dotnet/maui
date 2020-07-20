using System.Collections.Generic;
using System.Windows;
using Xamarin.Forms.Platform.WPF.Interfaces;
using WBrush = System.Windows.Media.Brush;

namespace Xamarin.Forms.Platform.WPF.Controls
{
	public class FormsMasterDetailPage : FormsPage
	{
		FormsContentControl lightMasterContentControl;
		FormsContentControl lightDetailContentControl;

		public static readonly DependencyProperty MasterPageProperty = DependencyProperty.Register("MasterPage", typeof(object), typeof(FormsMasterDetailPage));
		public static readonly DependencyProperty DetailPageProperty = DependencyProperty.Register("DetailPage", typeof(object), typeof(FormsMasterDetailPage));
		public static readonly DependencyProperty ContentLoaderProperty = DependencyProperty.Register("ContentLoader", typeof(IContentLoader), typeof(FormsMasterDetailPage), new PropertyMetadata(new DefaultContentLoader()));
		public static readonly DependencyProperty IsPresentedProperty = DependencyProperty.Register("IsPresented", typeof(bool), typeof(FormsMasterDetailPage));

		public object MasterPage
		{
			get { return (object)GetValue(MasterPageProperty); }
			set { SetValue(MasterPageProperty, value); }
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

		public FormsMasterDetailPage()
		{
			this.DefaultStyleKey = typeof(FormsMasterDetailPage);
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			lightMasterContentControl = Template.FindName("PART_Master", this) as FormsContentControl;
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

			if (lightMasterContentControl != null && lightMasterContentControl.Content is FormsPage masterPage)
			{
				frameworkElements.AddRange(masterPage.GetPrimaryTopBarCommands());
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

			if (lightMasterContentControl != null && lightMasterContentControl.Content is FormsPage masterPage)
			{
				frameworkElements.AddRange(masterPage.GetSecondaryTopBarCommands());
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

			if (lightMasterContentControl != null && lightMasterContentControl.Content is FormsPage masterPage)
			{
				frameworkElements.AddRange(masterPage.GetPrimaryBottomBarCommands());
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

			if (lightMasterContentControl != null && lightMasterContentControl.Content is FormsPage masterPage)
			{
				frameworkElements.AddRange(masterPage.GetSecondaryBottomBarCommands());
			}

			if (lightDetailContentControl != null && lightDetailContentControl.Content is FormsPage page)
			{
				frameworkElements.AddRange(page.GetSecondaryBottomBarCommands());
			}

			return frameworkElements;
		}
	}
}

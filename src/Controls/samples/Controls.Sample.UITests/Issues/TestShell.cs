using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	public abstract class TestShell : Shell
	{
		protected TestShell() : base()
		{
			Init();
		}

		protected ContentPage DisplayedPage
		{
			get
			{
				return (ContentPage)(CurrentItem.CurrentItem as IShellSectionController).PresentedPage;
			}
		}

		public ContentPage AddTopTab(string title, string icon = null, ShellSection root = null)
		{
			var page = new ContentPage()
			{
				Title = title
			};

			AddTopTab(page, title, icon, root);
			return page;
		}


		public void AddTopTab(ContentPage page, string title = null, string icon = null, ShellSection root = null)
		{
			if (Items.Count == 0)
			{
				var item = AddContentPage(page);
				item.Items[0].Items[0].Title = title ?? page.Title;
				return;
			}

			root = Items[0].Items[0];
			title ??= page.Title;
			var content = new ShellContent()
			{
				Title = title,
				Content = page,
				Icon = icon,
				AutomationId = title
			};

			root.Items.Add(content);

			if (!string.IsNullOrWhiteSpace(content.Title))
				content.Route = content.Title;
		}

		public ContentPage AddBottomTab(ContentPage page, string title, string icon = null)
		{
			if (Items.Count == 0)
			{
				var item = AddContentPage(page);
				item.Items[0].Items[0].Title = title ?? page.Title;
				item.Items[0].Title = title ?? page.Title;
				return page;
			}

			Items[0].Items.Add(new ShellSection()
			{
				AutomationId = title,
				Route = title,
				Title = title,
				Icon = icon,
				Items =
 				{
 					new ShellContent()
 					{
 						ContentTemplate = new DataTemplate(() => page),
 						Title = title
 					}
 				}
			});

			return page;
		}

		public ContentPage AddBottomTab(string title, string icon = null)
		{
			ContentPage page = new ContentPage();

			if (Items.Count == 0)
			{
				var item = AddContentPage(page, title);
				item.Items[0].Items[0].Title = title ?? page.Title;
				item.Items[0].Title = title ?? page.Title;
				return page;
			}

			Items[0].Items.Add(new ShellSection()
			{
				AutomationId = title,
				Route = title,
				Title = title,
				Icon = icon,
				Items =
				{
					new ShellContent()
					{
						ContentTemplate = new DataTemplate(() => page),
						Title = title
					}
				}
			});

			return page;
		}

		public ContentPage CreateContentPage<TShellItem>(string title)
			where TShellItem : ShellItem
		{
			ContentPage page = new ContentPage() { Title = title };
			AddContentPage<TShellItem, Tab>(page, title);
			return page;
		}

		public FlyoutItem AddFlyoutItem(string title)
		{
			return AddContentPage<FlyoutItem, Tab>(new ContentPage(), title);
		}

		public FlyoutItem AddFlyoutItem(ContentPage page, string title)
		{
			return AddContentPage<FlyoutItem, Tab>(page, title);
		}

		public ContentPage CreateContentPage(string shellItemTitle = null)
			=> CreateContentPage<ShellItem, ShellSection>(shellItemTitle);

		public ContentPage CreateContentPage<TShellItem, TShellSection>(string shellItemTitle = null)
			where TShellItem : ShellItem
			where TShellSection : ShellSection
		{
			shellItemTitle ??= $"Item: {Items.Count}";
			ContentPage page = new ContentPage();

			TShellItem item = Activator.CreateInstance<TShellItem>();
			item.Title = shellItemTitle;

			TShellSection shellSection = Activator.CreateInstance<TShellSection>();
			shellSection.Title = shellItemTitle;

			shellSection.Items.Add(new ShellContent()
			{
				ContentTemplate = new DataTemplate(() => page)
			});

			item.Items.Add(shellSection);

			Items.Add(item);
			return page;
		}

		public ShellItem AddContentPage(ContentPage contentPage = null, string title = null)
			=> AddContentPage<ShellItem, ShellSection>(contentPage, title);

		public TShellItem AddContentPage<TShellItem, TShellSection>(ContentPage contentPage = null, string title = null)
			where TShellItem : ShellItem
			where TShellSection : ShellSection
		{
			title ??= contentPage?.Title;
			contentPage ??= new ContentPage();
			TShellItem item = Activator.CreateInstance<TShellItem>();
			item.Title = title;
			TShellSection shellSection = Activator.CreateInstance<TShellSection>();
			Items.Add(item);
			item.Items.Add(shellSection);
			shellSection.Title = title;

			var content = new ShellContent()
			{
				ContentTemplate = new DataTemplate(() => contentPage),
				Title = title
			};

			shellSection.Items.Add(content);

			if (!string.IsNullOrWhiteSpace(title))
			{
				content.Route = title;
			}

			return item;
		}

		protected abstract void Init();
	}
}

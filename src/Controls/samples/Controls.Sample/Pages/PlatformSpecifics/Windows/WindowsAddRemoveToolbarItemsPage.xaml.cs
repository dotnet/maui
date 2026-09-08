using System;
using System.Linq;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class WindowsAddRemoveToolbarItemsPage : ContentPage
	{
		public static readonly BindableProperty ParentPageProperty = BindableProperty.Create("ParentPage", typeof(Page), typeof(WindowsAddRemoveToolbarItemsPage), null);

		public Page ParentPage
		{
			get { return (Page)GetValue(ParentPageProperty); }
			set { SetValue(ParentPageProperty, value); }
		}

		readonly Action? _action;

		public WindowsAddRemoveToolbarItemsPage()
		{
			InitializeComponent();
#if WINDOWS
			_action = () => ParentPage.DisplayAlertAsync(WindowsPlatformSpecificsHelpers.Title, WindowsPlatformSpecificsHelpers.Message, WindowsPlatformSpecificsHelpers.Dismiss);
#else
			_action = null;
#endif
		}

		void OnAddPrimaryButtonClicked(object sender, EventArgs e)
		{
			int index = ParentPage.ToolbarItems.Count(item => item.Order == ToolbarItemOrder.Primary) + 1;
			ParentPage.ToolbarItems.Add(new ToolbarItem(string.Format("Primary {0}", index), "calculator.png", _action, ToolbarItemOrder.Primary));
		}

		void OnAddSecondaryButtonClicked(object sender, EventArgs e)
		{
			int index = ParentPage.ToolbarItems.Count(item => item.Order == ToolbarItemOrder.Secondary) + 1;
			ParentPage.ToolbarItems.Add(new ToolbarItem(string.Format("Secondary {0}", index), "calculator.png", _action, ToolbarItemOrder.Secondary));
		}

		void OnRemoveButtonClicked(object sender, EventArgs e)
		{
			if (ParentPage.ToolbarItems.Any())
			{
				ParentPage.ToolbarItems.RemoveAt(0);
			}
		}
	}
}

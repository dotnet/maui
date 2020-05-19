using System.Maui.CustomAttributes;
using System.Maui.Internals;
using System;

#if UITEST
using Xamarin.UITest;
using System.Maui.Core.UITests;
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Shell)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4756, "Cannot prevent flyout menu list from scrolling", PlatformAffected.Default)]
	public class Issue4756 : TestShell
	{
		protected override void Init()
		{
			FlowDirection = FlowDirection.RightToLeft;
			FlyoutHeader = new StackLayout();
			FlyoutVerticalScrollMode = ScrollMode.Disabled;
			for (int i = 0; i < 10; i++)
				Items.Add(GenerateItem(i.ToString()));
		}

		ShellItem GenerateItem(string title)
		{
			var picker = new Picker
			{
				ItemsSource = Enum.GetNames(typeof(ScrollMode)),
				Title = "FlyoutVerticalScrollMode",
				SelectedItem = FlyoutVerticalScrollMode.ToString()
			};
			picker.SelectedIndexChanged += (_, e) => FlyoutVerticalScrollMode = (ScrollMode)picker.SelectedIndex;

			var section = new ShellSection
			{
				Items =
				{
					new System.Maui.ShellContent
					{
						Content = new ContentPage
						{
							Content = new StackLayout
							{
								Children = {
									new Button
									{
										Text = "Add ShellItem",
										Command = new Command(() => Items.Add(GenerateItem(Items.Count.ToString())))
									},
									new Button
									{
										Text = "Remove ShellItem",
										Command = new Command(() => {
											if (Items.Count > 1)
												Items.RemoveAt(0);
										})
									},
									new Label
									{
										Text = "FlyoutVerticalScrollMode"
									},
									picker
								}
							}
						}
					}
				}
			};
			var item = new ShellItem
			{
				Title = title,
				Route = title,
				Items =
				{
					section
				}
			};
			item.CurrentItem = section;
			return item;
		}
	}
}
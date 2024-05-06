using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 5555, "Memory leak when SwitchCell or EntryCell", PlatformAffected.iOS)]
	public class Issue5555 : TestContentPage
	{
		public static Label DestructorCount = new Label() { Text = "0" };
		protected override void Init()
		{
			var instructions = new Label
			{
				FontSize = 16,
				Text = "Click 'Push page' twice"
			};

			var result = new Label
			{
				Text = "Success",
				AutomationId = "SuccessLabel",
				IsVisible = false
			};

			var list = new List<WeakReference>();

			var checkButton = new Button
			{
				Text = "Check Result",
				AutomationId = "CheckResult",
				IsVisible = false,
				Command = new Command(async () =>
				{
					if (list.Count < 2)
					{
						instructions.Text = "Click 'Push page' again";
						return;
					}

					try
					{
						await GarbageCollectionHelper.WaitForGC(2500, list.ToArray());
						result.Text = "Success";
						result.IsVisible = true;
						instructions.Text = "";
					}
					catch (Exception)
					{
						instructions.Text = "Failed";
						result.IsVisible = false;
						return;
					}
				})
			};

			Content = new StackLayout
			{
				Children = {
					DestructorCount,
					instructions,
					result,
					new Button
					{
						Text = "Push page",
						AutomationId = "PushPage",
						Command = new Command(async() => {
							if (list.Count >= 2)
								list.Clear();

							var wref = new WeakReference(new LeakPage());

							await Navigation.PushAsync(wref.Target as Page);
							await (wref.Target as Page).Navigation.PopAsync();

							list.Add(wref);
							if (list.Count > 1)
							{
								checkButton.IsVisible = true;
								instructions.Text = "You can check result";
							}
							else
							{
								instructions.Text = "Again";
							}
						})
					},
					checkButton
				}
			};
		}

		class LeakPage : ContentPage
		{
			public LeakPage()
			{
				Content = new StackLayout
				{
					Children = {
					new Entry { Text = "LeakPage" },
					new TableView
					{
						Root = new TableRoot
						{
							new TableSection
							{
								new SwitchCell { Text = "switch cell", On = true },
								new EntryCell { Text = "entry cell" }
							}
						}
					}
				}
				};
			}

			~LeakPage()
			{
				System.Diagnostics.Debug.WriteLine("LeakPage Finalized");
			}
		}
	}
}
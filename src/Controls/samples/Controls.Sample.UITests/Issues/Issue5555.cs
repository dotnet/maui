using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
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
								//new SwitchCell { Text = "switch cell", On = true },
								new EntryCell { Text = "entry cell" }
							}
						}
					}
				}
			};
		}
		~LeakPage(){
			System.Diagnostics.Debug.WriteLine("Finalized");			
		}
	}

	[Issue(IssueTracker.None, 5555, "Memory leak when SwitchCell or EntryCell", PlatformAffected.iOS)]
	public class Issue5555 : TestContentPage
	{
		public static Label DestructorCount = new Label() {Text = "0"};
		protected override void Init()
		{
			var result = new Label
			{
				FontSize = 16,
				Text = "Click 'Push page' twice"
			};

			var list = new List<WeakReference>();

			var checkButton = new Button
			{
				Text = "Check Result",
				IsEnabled = false,
				Command = new Command(async () =>
				{
					if (list.Count < 2)
					{
						result.Text = "Click 'Push page' again";
						return;
					}


					try{
						await GarbageCollectionHelper.WaitForGC(2500, list.ToArray());
						result.Text = "Success";
					}
					catch(Exception){
						result.Text = "Failed";
						return;
					}
					
					/*result.Text = "Success";
					foreach(var weakRef in list)
					{
						if (weakRef.IsAlive)
						{
							result.Text = "Failed";
							break;
						}
					}*/
				})
			};

			Content = new StackLayout
			{
				Children = {
					DestructorCount,
					result,
					new Button
					{
						Text = "Push page",
						Command = new Command(async() => {
							if (list.Count >= 2)
								list.Clear();

							var wref = new WeakReference(new LeakPage());

							await Navigation.PushAsync(wref.Target as Page);
							await (wref.Target as Page).Navigation.PopAsync();

							list.Add(wref);
							if (list.Count > 1)
							{
								checkButton.IsEnabled = true;
								result.Text = "You can check result";
							}
							else
							{
								result.Text = "Again";
							}
						})
					},
					checkButton
				}
			};
		}
	}
}
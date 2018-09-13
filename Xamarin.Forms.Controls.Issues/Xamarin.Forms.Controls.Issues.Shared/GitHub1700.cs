using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1700, "Desktop: TabStop/TabIndex support (for multiple Views)", PlatformAffected.All)]
	public class GitHub1700 : TestContentPage
	{
		IList<View> listViews;

		void IndexDesc()
		{
			int index = 100500;
			foreach (var item in listViews)
			{
				if (item is Button but && but.Text.StartsWith("TabIndex"))
					continue;
				item.TabIndex = index--;
			}
		}

		void IndexNegative()
		{
			int index = -100;
			foreach (var item in listViews)
			{
				if (item is Button but && but.Text.StartsWith("TabIndex"))
					continue;
				item.TabIndex = index++;
			}
		}

		protected override void Init()
		{
			var actionGrid = new Grid()
			{
				Padding = new Thickness(10),
				BackgroundColor = Color.Aquamarine
			};
			actionGrid.AddChild(new Button()
			{
				Text = "Index desc",
				Command = new Command(() => IndexDesc())
			}, 0, 0);
			actionGrid.AddChild(new Button()
			{
				Text = "All indexes equal 0",
				Command = new Command(() => listViews.ForEach(c => c.TabIndex = 0))
			}, 1, 0);
			actionGrid.AddChild(new Button()
			{
				Text = "Negative indexes",
				Command = new Command(() => IndexNegative())
			}, 2, 0);
			actionGrid.AddChild(new Button()
			{
				Text = "TabStops = True",
				Command = new Command(() => listViews.ForEach(c => c.IsTabStop = true))
			}, 0, 1);
			actionGrid.AddChild(new Button()
			{
				Text = "TabStops = False",
				Command = new Command(() => listViews.ForEach(c => c.IsTabStop = false))
			}, 1, 1);
			actionGrid.AddChild(new Button()
			{
				Text = "TabStops every second",
				Command = new Command(() =>
				{
					for (int i = 0; i < listViews.Count; i++)
						listViews[i].IsTabStop = i % 2 == 0;
				})
			}, 2, 1);

			var pickerStopped = new Picker
			{
				Title = $"[+] Picker - Tab stop enable",
				IsTabStop = true
			};
			var pickerNotStopped = new Picker
			{
				Title = "[-] Picker - Tab stop disable",
				IsTabStop = false
			};
			for (var i = 1; i < 3; i++) {
				pickerNotStopped.Items.Add("Sample Option " + i);
				pickerStopped.Items.Add("Sample Option " + i);
			}

			var stack = new StackLayout
			{
				Children =
				{
					actionGrid,
					pickerStopped,
					pickerNotStopped,
					new Button
					{
						Text = $"TabIndex 90",
						IsTabStop = true,
						TabIndex = 90
					},
					new Button
					{
						Text = $"TabIndex 100",
						IsTabStop = true,
						TabIndex = 100
					},
					new Button
					{
						Text = $"TabIndex 100",
						IsTabStop = true,
						TabIndex = 100
					},
					new Button
					{
						Text = $"TabIndex 90",
						IsTabStop = true,
						TabIndex = 90
					},
					new Button
					{
						Text = $"[+] Button - TabStop enable",
						IsTabStop = true
					},
					new Button
					{
						Text = "Button - Non stop",
						IsTabStop = false
					},
					new DatePicker
					{
						IsTabStop = true
					},
					new DatePicker
					{
						IsTabStop = false
					},
					new Editor
					{
						Text = $"[+] Editor - Tab stop enable",
						IsTabStop = true
					},
					new Editor
					{
						Text = "Editor - Non stop",
						IsTabStop = false
					},
					new Entry
					{
						Text = $"[+] Entry - Tab stop enable",
						IsTabStop = true
					},
					new Entry
					{
						Text = "Entry - Non stop",
						IsTabStop = false
					},
					new ProgressBar
					{
						IsTabStop = true,
						HeightRequest = 40,
						Progress = 80
					},
					new ProgressBar
					{
						IsTabStop = false,
						HeightRequest = 40,
						Progress = 40
					},
					new SearchBar
					{
						Text = $"[+] SearchBar - TabStop enable",
						IsTabStop = true
					},
					new SearchBar
					{
						Text = "SearchBar - TabStop disable",
						IsTabStop = false
					},
					new Slider
					{
						IsTabStop = true
					},
					new Slider
					{
						IsTabStop = false
					},
					new Stepper
					{
						IsTabStop = true
					},
					new Stepper
					{
						IsTabStop = false
					},
					new Switch
					{
						IsTabStop = true
					},
					new Switch
					{
						IsTabStop = false
					},
					new TimePicker
					{
						IsTabStop = true
					},
					new TimePicker
					{
						IsTabStop = false
					},
				}
			};

			listViews = stack.Children;

			foreach (var item in listViews)
			{
				item.Focused += (_, e) =>
				{
					BackgroundColor = e.VisualElement.IsTabStop ? Color.Transparent : Color.OrangeRed;
					Title = $"{e.VisualElement.TabIndex} - " + (e.VisualElement.IsTabStop ? "[+]" : "WRONG");
					e.VisualElement.Scale = 0.7;
				};
				item.Unfocused += (_, e) =>
				{
					BackgroundColor = Color.Transparent;
					Title = string.Empty;
					e.VisualElement.Scale = 1;
				};
			}

			IndexDesc();

			Content = new ScrollView()
			{
				Content = stack
			};
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3454, "Picker accessibility text is wrong", PlatformAffected.Android)]
	public class Issue3454 : TestContentPage
	{
		protected override void Init()
		{
			var pickers = new List<Picker>();
			var grid = new Grid();
			int row = 0, col = 0;
			grid.AddChild(new Label { Text = "Default Style" }, col, row++);
#if APP
			void AddPicker(string title, Func<Picker> getPicker)
			{
				grid.AddChild(new Label { Text = title }, col, row++);
				var picker = getPicker();
				picker.ItemsSource = Enumerable.Range(1, 10).Select(i => $"item {i}").ToList();
				pickers.Add(picker);
				grid.AddChild(picker, col, row++);
			}

			AddPicker("AutomationProperties", () =>
			{
				var picker = new Picker();
				picker.SetAutomationPropertiesName("First accessibility");
				picker.SetAutomationPropertiesHelpText("This is the accessibility text");
				return picker;
			});
			AddPicker("Default", () => new Picker());
			AddPicker("Default + Title", () => new Picker { Title = "Title1" });
			AddPicker("AutomationProperties + Title", () =>
			{
				var picker = new Picker { Title = "Title2" };
				picker.SetAutomationPropertiesName("Last accessibility");
				picker.SetAutomationPropertiesHelpText("This is the accessibility text");
				return picker;
			});

			row = 0;
			col++;
			grid.AddChild(new Label { Text = "Material Style" }, col, row++);
			AddPicker("AutomationProperties", () =>
			{
				var picker = new Picker { Visual = VisualMarker.Material };
				picker.SetAutomationPropertiesName("First accessibility");
				picker.SetAutomationPropertiesHelpText("This is the accessibility text");
				return picker;
			});
			AddPicker("Default", () => new Picker { Visual = VisualMarker.Material });
			AddPicker("Default + Title", () => new Picker { Title = "Title1", Visual = VisualMarker.Material });
			AddPicker("AutomationProperties + Title", () =>
			{
				var picker = new Picker { Title = "Title2", Visual = VisualMarker.Material };
				picker.SetAutomationPropertiesName("Last accessibility");
				picker.SetAutomationPropertiesHelpText("This is the accessibility text");
				return picker;
			});
#endif

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						grid,
						new Button
						{
							Text = "Clear pickers",
							Command = new Command(() => pickers.ForEach(p => p.SelectedItem = null))
						}
					}
				}
			};
		}
	}
}

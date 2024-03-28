using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 2681, "[UWP] Label inside Listview gets stuck inside infinite loop",
	PlatformAffected.UWP)]
public class XFIssue2681 : TestContentPage
{
	const string NavigateToPage = "Click Me.";
	protected override void Init()
	{
		Content = new Button()
		{
			Text = NavigateToPage,
			AutomationId = "ClickMe",
			Command = new Command(async () => await this.Navigation.PushAsync(new FreezeMe()))
		};
	}

	public partial class FreezeMe : ContentPage
	{
		public List<int> Items { get; set; }

		public FreezeMe()
		{
			this.BindingContext = this;
			var lv = new ListView()
			{
				Margin = new Thickness(20, 5, 5, 5)
			};

			lv.ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label() { Text = "sassifrass" };
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(Label.AutomationIdProperty, ".");
				return new ViewCell() { View = label };
			});

			lv.SetBinding(ListView.ItemsSourceProperty, "Items");

			this.Content = new ScrollView()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label(){ Text = "If page is not frozen this test has passed" },
						new StackLayout()
						{
							Orientation = StackOrientation.Horizontal,
							Children =
							{
								lv
							}
						},
						new Button(){
							Text = "Go Back",
							AutomationId = "GoBack",
							Command = new Command(async () =>
							{
								await Navigation.PopAsync();
							})
						}
					}
				}
			};

			this.Appearing += (s, e) =>
			{
				this.Items = new List<int> { 1, 2, 3 };
				this.OnPropertyChanged("Items");
			};
		}
	}
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2681, "[UWP] Label inside Listview gets stuck inside infinite loop",
		PlatformAffected.UWP)]
	public class Issue2681 : TestNavigationPage
	{
		const string NavigateToPage = "Click Me.";
		protected override void Init()
		{
			PushAsync(new ContentPage() { Title = "Freeze Test", Content = new Button() { Text = NavigateToPage, Command = new Command(() => this.PushAsync(new FreezeMe())) } });
		}

		[Preserve(AllMembers = true)]
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
								Children = {lv  }
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

#if UITEST
		[Test]
		public void ListViewDoesntFreezeApp()
		{
			RunningApp.Tap(x => x.Marked(NavigateToPage));
			RunningApp.WaitForElement("3");
		}
#endif
	}
}

using System;

using Xamarin.Forms.CustomAttributes;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39499, "CarouselViewTest")]
	public sealed class CarouselViewContentPage : TestContentPage // or TestMasterDetailPage, etc ...
	{
		[Preserve(AllMembers = true)]
		public sealed class Item
		{
			static int s_id = 0;

			int id;

			internal Item()
			{
				id = s_id++;
			}

			public int Id => id;

			public string Text => $"Item {Id}";
		}

		[Preserve(AllMembers = true)]
		public sealed class ItemView : ContentView
		{
			public ItemView()
			{
				var idLabel = new Label() { StyleId = "id", TextColor = Color.White };
				idLabel.SetBinding(Label.TextProperty, nameof(Item.Text));

				var stackLayout = new StackLayout
				{
					Children = {
						new Label { Text = "Target" },
						new Label { Text = "Stack" }
					},
					BackgroundColor = Color.Red
				};

				var button = CreateButton("Hide Target Stack", () =>
				{
					stackLayout.IsVisible = false;
				});

				var buttonImage = CreateButton("AddImage", () =>
				{
					stackLayout.IsVisible = true;
					stackLayout.Children.Add(new Image { Source = "menuIcon.png" });
				});
				Content = new StackLayout
				{
					Children = {
						idLabel,
						button,
						buttonImage,
						stackLayout,
					}
				};
			}

			Button CreateButton(string text, Action clicked)
			{
				var button = new Button();
				button.Text = text;
				button.Clicked += (s, e) =>
				{
					clicked();
				};
				return button;
			}
		}

		static readonly IList<Item> Items = new ObservableCollection<Item>() {
			new Item(),
			new Item(),
		};

		Button CreateButton(string text, Action onClicked = null)
		{
			var button = new Button
			{
				Text = text
			};

			if (onClicked != null)
				button.Clicked += (s, e) => onClicked();

			return button;
		}

		protected override void Init()
		{
			BackgroundColor = Color.Blue;

			var carouselView = new CarouselView
			{
				BackgroundColor = Color.Purple,
				ItemsSource = Items,
				ItemTemplate = new DataTemplate(typeof(ItemView)),
				Position = 0
			};

			var moveBar = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children =
				{
					CreateButton("+", () => Items.Add(new Item())),
					CreateButton("<<", () => carouselView.Position = 0),
					CreateButton("<", () =>
					{
						try
						{
							carouselView.Position--;
						}
						catch
						{
						}
					}),
					CreateButton(">", () =>
					{
						try
						{
							carouselView.Position++;
						}
						catch
						{
						}
					}),
					CreateButton(">>", () => carouselView.Position = Items.Count - 1)
				}
			};

			Content = new StackLayout
			{
				Children =
				{
					carouselView,
					moveBar
				}
			};
		}

#if UITEST
		//[Test]
		public void CarouselViewTest()
		{
			var app = RunningApp;
			app.WaitForElement(q => q.Marked("Item 0"));
			app.SwipeRightToLeft();
			app.WaitForElement(q => q.Marked("Item 1"));
			app.Tap(c => c.Marked("<"));
			app.WaitForElement(q => q.Marked("Item 0"));
		}

		[Test]
		public void CarouselViewTestAddItem()
		{
			var app = RunningApp;
			app.WaitForElement(q => q.Marked("Hide Target Stack"));
			app.Tap(c => c.Marked("+"));
			app.SwipeRightToLeft();
			app.SwipeRightToLeft();
			app.WaitForElement(q => q.Marked("Item 2"));
			app.Screenshot("I see the Item 2");
		}
#endif
	}
}

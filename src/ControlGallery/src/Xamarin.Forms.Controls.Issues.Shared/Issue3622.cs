using System;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3622, "Android TalkBack reads elements behind modal pages", PlatformAffected.Android, NavigationBehavior.PushModalAsync)]
	public class Issue3622 : TestContentPage // or TestFlyoutPage, etc ...
	{
		[Preserve(AllMembers = true)]
		public class Contact
		{
			public string Name { get; set; }

			public int Age { get; set; }

			public string Occupation { get; set; }

			public string Country { get; set; }

			public override string ToString()
			{
				return Name;
			}
		}

		[Preserve(AllMembers = true)]
		public class DetailPageCS : ContentPage
		{
			public DetailPageCS()
			{
				var nameLabel = new Label
				{
					Style = Device.Styles.TitleStyle
				};
				nameLabel.SetBinding(Label.TextProperty, "Name");

				var ageLabel = new Label
				{
					Style = Device.Styles.CaptionStyle

				};
				ageLabel.SetBinding(Label.TextProperty, "Age");

				var occupationLabel = new Label
				{
					Style = Device.Styles.BodyStyle
				};
				occupationLabel.SetBinding(Label.TextProperty, "Occupation");

				var countryLabel = new Label
				{
					FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
					FontAttributes = FontAttributes.Bold
				};
				countryLabel.SetBinding(Label.TextProperty, "Country");

				var dismissButton = new Button { Text = "Dismiss" };
				dismissButton.Clicked += OnDismissButtonClicked;

				Content = new StackLayout
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Children = {
					new StackLayout {
						Orientation = StackOrientation.Horizontal,
						Children = {
							new Label {
								Text = "Name:",
								FontSize = Device.GetNamedSize (NamedSize.Medium, typeof(Label)),
								HorizontalOptions = LayoutOptions.FillAndExpand
							},
							nameLabel
						}
					},
					new StackLayout {
						Orientation = StackOrientation.Horizontal,
						Children = {
							new Label {
								Text = "Age:",
								FontSize = Device.GetNamedSize (NamedSize.Medium, typeof(Label)),
								HorizontalOptions = LayoutOptions.FillAndExpand
							},
							ageLabel
						}
					},
					new StackLayout {
						Orientation = StackOrientation.Horizontal,
						Children = {
							new Label {
								Text = "Occupation:",
								FontSize = Device.GetNamedSize (NamedSize.Medium, typeof(Label)),
								HorizontalOptions = LayoutOptions.FillAndExpand
							},
							occupationLabel
						}
					},
					new StackLayout {
						Orientation = StackOrientation.Horizontal,
						Children = {
							new Label {
								Text = "Country:",
								FontSize = Device.GetNamedSize (NamedSize.Medium, typeof(Label)),
								HorizontalOptions = LayoutOptions.FillAndExpand
							},
							countryLabel
						}
					},
					dismissButton
				}
				};
			}

			async void OnDismissButtonClicked(object sender, EventArgs args)
			{
				await Navigation.PopModalAsync();
			}
		}

		ListView listView;
		List<Contact> contacts;

		protected override void Init()
		{
			SetupData();

			listView = new ListView
			{
				Header = new Label { Text = "Enable TalkBack. Make sure Android reads this list, when you tap an item make sure it reads the details page and not this list" },
				ItemsSource = contacts
			};
			listView.ItemSelected += OnItemSelected;

			Thickness padding;
			switch (Device.RuntimePlatform)
			{
				case Device.iOS:
					padding = new Thickness(0, 40, 0, 0);
					break;
				default:
					padding = new Thickness();
					break;
			}

			Padding = padding;
			Content = new StackLayout
			{
				Children = {
					listView
				}
			};
		}

		async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			if (listView.SelectedItem != null)
			{
				var detailPage = new DetailPageCS();
				detailPage.BindingContext = e.SelectedItem as Contact;
				listView.SelectedItem = null;
				await Navigation.PushModalAsync(detailPage);
			}
		}

		void SetupData()
		{
			contacts = new List<Contact>();
			contacts.Add(new Contact
			{
				Name = "Jane Doe",
				Age = 30,
				Occupation = "Developer",
				Country = "USA"
			});
			contacts.Add(new Contact
			{
				Name = "John Doe",
				Age = 34,
				Occupation = "Tester",
				Country = "USA"
			});
			contacts.Add(new Contact
			{
				Name = "John Smith",
				Age = 52,
				Occupation = "PM",
				Country = "UK"
			});
			contacts.Add(new Contact
			{
				Name = "Kath Smith",
				Age = 55,
				Occupation = "Business Analyst",
				Country = "UK"
			});
			contacts.Add(new Contact
			{
				Name = "Steve Smith",
				Age = 19,
				Occupation = "Junior Developer",
				Country = "UK"
			});
		}
	}
}
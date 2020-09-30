using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using WindowsOS = Xamarin.Forms.PlatformConfiguration.Windows;

#if UITEST
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 59718, "Multiple issues with listview and navigation in UWP", PlatformAffected.UWP)]
	public class Bugzilla59718 : TestContentPage
	{
		const string GoBackButtonId = "GoBackButtonId";
		const string Target1 = "Label with TapGesture Cricket";
		const string Target1b = "Label with TapGesture Cricket Tapped!";
		const string Target2 = "Label with no TapGesture Cricket";
		const string Target3 = "You came here from Cricket.";

		Label _ItemTappedLabel;
		Label _LabelTappedLabel;
		ListView _list;

		class Grouping<K, T> : ObservableCollection<T>
		{
			public K Key { get; private set; }

			public Grouping(K key, IEnumerable<T> items)
			{
				Key = key;
				foreach (var item in items)
					this.Items.Add(item);

			}
		}

		protected override void Init()
		{
			_LabelTappedLabel = new Label { TextColor = Color.Red };
			_ItemTappedLabel = new Label { TextColor = Color.Purple };

			_list = new ListView
			{
				IsGroupingEnabled = true,
				GroupDisplayBinding = new Binding("Key"),
				ItemTemplate = new DataTemplate(() =>
				{
					var tapLabel = new Label();
					tapLabel.SetBinding(Label.TextProperty, ".", stringFormat: "Label with TapGesture {0}");

					var tap = new TapGestureRecognizer();
					tap.Tapped += (s, e) =>
					{
						_LabelTappedLabel.Text = $"{tapLabel.Text} Tapped!";
					};

					tapLabel.GestureRecognizers.Add(tap);

					var noTap = new Label();
					noTap.SetBinding(Label.TextProperty, ".", stringFormat: "Label with no TapGesture {0}");

					var view = new ViewCell { View = new StackLayout { Children = { noTap, tapLabel } } };
					return view;
				})
			};

			_list.On<WindowsOS>().SetSelectionMode(Xamarin.Forms.PlatformConfiguration.WindowsSpecific.ListViewSelectionMode.Inaccessible);

			_list.ItemTapped += ListView_ItemTapped;

			Content = new StackLayout { Children = { _LabelTappedLabel, _ItemTappedLabel, _list } };
		}

		protected override void OnAppearing()
		{
			_list.ItemsSource = new ObservableCollection<Grouping<string, string>>
			{
				new Grouping<string, string>("Sports", new string[] {"Cricket", "Football" }),
				new Grouping<string, string>("Mobile", new string[] {"Samsung", "Apple" }),
				new Grouping<string, string>("Microsoft", new string[] {"Office", "Windows" }),
				new Grouping<string, string>("Games", new string[] {"Online", "Offline" }),
				new Grouping<string, string>("Test", new string[] {"test1", "test2" }),
				new Grouping<string, string>("Variable", new string[] {"String", "Int" }),
			};
			;

			base.OnAppearing();
		}

		async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
		{
			_ItemTappedLabel.Text = $"{e.Item}";

			await Navigation.PushAsync(new NextPage(_ItemTappedLabel.Text));

			((ListView)sender).SelectedItem = null;
		}

		class NextPage : ContentPage
		{
			public NextPage(string source)
			{
				var button = new Button { Text = "Go back", AutomationId = GoBackButtonId };
				button.Clicked += Button_Clicked;
				Content = new StackLayout
				{
					Children = {
						new Label { Text = $"You came here from {source}." },
						button
					}
				};
			}

			async void Button_Clicked(object sender, System.EventArgs e)
			{
				await Navigation.PopAsync();
			}
		}

#if UITEST
		[Test]
		public void Bugzilla59718Test()
		{
			RunningApp.WaitForElement(q => q.Marked(Target1));
			RunningApp.Tap(q => q.Marked(Target1));

			RunningApp.WaitForElement(q => q.Marked(Target1b));

			RunningApp.WaitForElement(q => q.Marked(Target2));
			RunningApp.Tap(q => q.Marked(Target2));

			RunningApp.WaitForElement(q => q.Marked(Target3));

			RunningApp.WaitForElement(q => q.Marked(GoBackButtonId));
			RunningApp.Tap(q => q.Marked(GoBackButtonId));

			RunningApp.WaitForElement(q => q.Marked(Target1));
		}
#endif
	}
}
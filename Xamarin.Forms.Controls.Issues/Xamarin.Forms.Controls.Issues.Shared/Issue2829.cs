using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Xamarin.Forms.Controls.Effects;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2829, "[Android] Renderers associated with ListView cells are occasionaly not being disposed of which causes left over events to propagate to disposed views",
		PlatformAffected.Android)]
	public class Issue2829 : TestNavigationPage
	{
		AttachedStateEffectList attachedStateEffectList = new AttachedStateEffectList();
		const string kScrollMe = "kScrollMe";
		const string kSuccess = "SUCCESS";
		const string kCreateListViewButton = "kCreateListViewButton";
		StackLayout layout = null;

		protected override void Init()
		{
			var label = new Label() { Text = "Click the button then click back" };

			layout = new StackLayout()
			{
				Children =
					{
						label,
						new Button()
						{
							AutomationId = kCreateListViewButton,
							Text    = "Create ListView",
							Command = new Command(() =>
							{
								attachedStateEffectList.ToList().ForEach(x=> attachedStateEffectList.Remove(x));
								label.Text = "FAILURE";
								Navigation.PushAsync(CreateListViewPage());
							})
						}
					}
			};

			var page = new ContentPage()
			{
				Content = layout
			};


			PushAsync(page);
			attachedStateEffectList.AllEventsDetached += (_, __) =>
			{
				label.Text = kSuccess;
			};
		}

		ListView CreateListView()
		{
			ListView view = new ListView(ListViewCachingStrategy.RecycleElement);
			view.ItemTemplate = new DataTemplate(() =>
			{
				ViewCell cell = new ViewCell();
				AttachedStateEffectLabel label = new AttachedStateEffectLabel();
				label.TextColor = Color.Black;
				label.BackgroundColor = Color.White;
				label.SetBinding(Label.TextProperty, "Text");
				attachedStateEffectList.Add(label);
				label.BindingContextChanged += (_, __) =>
				{
					if (label.AutomationId == null)
						label.AutomationId = ((Data)label.BindingContext).Text.ToString();
				};

				cell.View = new ContentView()
				{
					Content = new StackLayout()
					{
						Orientation = StackOrientation.Horizontal,
						Children =
						{
							label,
							new Image{ Source = "coffee.png"}
						}
					},
					HeightRequest = 40
				};

				return cell;
			});
			var data = new ObservableCollection<Data>(Enumerable.Range(0, 72).Select(index => new Data() { Text = index.ToString() }));
			view.ItemsSource = data;

			return view;
		}

		Page CreateListViewPage()
		{
			var view = CreateListView();
			var data = view.ItemsSource as ObservableCollection<Data>;

			Button scrollMe = new Button()
			{
				Text = "Scroll ListView",
				AutomationId = kScrollMe,
				Command = new Command(() =>
				{
					view.ScrollTo(data.Last(), ScrollToPosition.MakeVisible, true);
				})
			};

			return new ContentPage()
			{
				Content = new StackLayout()
				{
					Children = { scrollMe, view }
				}
			};
		}

		[Preserve(AllMembers = true)]
		public class Data : INotifyPropertyChanged
		{
			private string _text;

			public string Text
			{
				get => _text;
				set
				{
					_text = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;
		}

#if UITEST && __ANDROID__
		[Test]
		public void ViewCellsAllDisposed()
		{
			RunningApp.Tap(kCreateListViewButton);
			RunningApp.WaitForElement("0");
			RunningApp.Tap(kScrollMe);
			RunningApp.WaitForElement("70");
			RunningApp.Back();
			RunningApp.WaitForElement(kSuccess);
		}
#endif
	}
}

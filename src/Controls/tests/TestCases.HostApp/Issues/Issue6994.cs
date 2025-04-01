using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 6994, "Regression in Xamarin.Forms 4.2.0-pre1 (Java.Lang.NullPointerException when using FastRenderers)", PlatformAffected.Android)]
	public class Issue6994 : TestContentPage
	{
		protected override void Init()
		{
			var source = new ObservableCollection<ItemViewModel>(Enumerable.Range(0, 100).Select(i => new ItemViewModel(i.ToString(), false)).ToList());
			var button = new Button { AutomationId = "Click me", Text = "Click me" };

			var listView = new ListView
			{
				ItemsSource = source,
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();

					var trigger = new DataTrigger(typeof(Label)) { Binding = new Binding(nameof(ItemViewModel.Pink)), Value = true };
					trigger.Setters.Add(new Setter { Value = Colors.Magenta, Property = Label.TextColorProperty });
					label.Triggers.Add(trigger);

					label.SetBinding(Label.TextProperty, nameof(ItemViewModel.Id));
					return new ViewCell { View = label };
				})
			};

			var instructions = new Label { FormattedText = "Enable Fast Renderers. Click the button. If the app crashes, this test has failed." };

			var stackLayout = new StackLayout
			{
				Children = {
					instructions,
					button,
					listView,
				}
			};
			Content = stackLayout;

			button.Clicked += (s, e) =>
			{
				//This seems needlessly complicated, but this test requires both removing a Label (to trigger Dispose) and updating the TextColor of another set of Labels.
				instructions.Text = "Please wait...";

				Parallel.Invoke(
					() =>
					{
						for (int j = 0; j < source.Count; j++)
						{
							var i = source[j];
							i.Pink = int.TryParse(i.Id, out int o) && o % 2 == 0;
						}
					},
					async () =>
					{
						await Task.Delay(20);
						source.Clear();
						await Task.Delay(2000);
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
						await Device.InvokeOnMainThreadAsync(() => instructions.Text = "Success");
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
					});

				stackLayout.Children.Remove(instructions);
				instructions.TextColor = Colors.Green;
				stackLayout.Children.Insert(0, instructions);
			};
		}


		public class ItemViewModel : INotifyPropertyChanged
		{
			string _id;
			bool _pink;

			public ItemViewModel(string i, bool pink)
			{
				Pink = pink;
				Id = i;
			}

			public event PropertyChangedEventHandler PropertyChanged;

			public string Id
			{
				get
				{
					return _id;
				}
				set
				{
					if (_id == value)
					{
						return;
					}

					_id = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id)));
				}
			}

			public bool Pink
			{
				get
				{
					return _pink;
				}
				set
				{
					if (_pink == value)
					{
						return;
					}

					_pink = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Pink)));
				}
			}
		}
	}
}
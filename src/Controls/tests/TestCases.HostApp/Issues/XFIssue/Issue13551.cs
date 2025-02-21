using System.Collections.ObjectModel;
using System.Globalization;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 13551, "[Bug] [iOS] CollectionView does not display items if `IsVisible` modified via a binding/trigger", PlatformAffected.iOS)]
	public class Issue13551 : TestContentPage
	{
		const string Success1 = "Success1";
		const string Success2 = "Success2";

		public ObservableCollection<Item> Source1 { get; } = new ObservableCollection<Item>();
		public ObservableCollection<Item> Source2 { get; } = new ObservableCollection<Item>();

		CollectionView BindingWithConverter()
		{
			var cv = new CollectionView
			{
				IsVisible = true,

				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding(nameof(Item.Text)));
					return label;
				})
			};

			cv.SetBinding(CollectionView.ItemsSourceProperty, new Binding("Source1"));
			cv.SetBinding(VisualElement.IsVisibleProperty, new Binding("Source1.Count", converter: new IntToBoolConverter()));

			return cv;
		}

		CollectionView WithTrigger()
		{
			var cv = new CollectionView
			{
				IsVisible = true,

				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding(nameof(Item.Text)));
					return label;
				})
			};

			cv.SetBinding(CollectionView.ItemsSourceProperty, new Binding("Source2"));

			var trigger = new DataTrigger(typeof(CollectionView));
			trigger.Value = 0;
			trigger.Setters.Add(new Setter() { Property = VisualElement.IsVisibleProperty, Value = false });
			trigger.Binding = new Binding("Source2.Count");

			cv.Triggers.Add(trigger);

			return cv;
		}

		protected override void Init()
		{
			BindingContext = this;

			var cv1 = BindingWithConverter();
			var cv2 = WithTrigger();

			var grid = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition() { Height = GridLength.Star },
					new RowDefinition() { Height = GridLength.Star },
				}
			};

			grid.Children.Add(cv1);
			grid.Children.Add(cv2);
			Grid.SetRow(cv2, 1);

			Content = grid;

#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			Device.StartTimer(TimeSpan.FromMilliseconds(300), () =>
			{
				Device.BeginInvokeOnMainThread(() =>
				{
					Source1.Add(new Item { Text = Success1 });
					Source2.Add(new Item { Text = Success2 });
				});

				return false;
			});
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
		}

		class IntToBoolConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return value is int val && val > 0;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				throw new NotImplementedException();
			}
		}

		public class Item
		{
			public string Text { get; set; }
		}
	}
}

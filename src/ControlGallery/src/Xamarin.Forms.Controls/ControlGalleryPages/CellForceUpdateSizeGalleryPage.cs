using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	[Preserve(AllMembers = true)]
	public class CellForceUpdateSizeGalleryPage : NavigationPage
	{
		[Preserve(AllMembers = true)]
		public class MyViewCell : ViewCell
		{
			Label _Label = new Label();
			VariableHeightItem _DataItem => BindingContext as VariableHeightItem;

			public MyViewCell()
			{
				_Label.SetBinding(Label.TextProperty, nameof(_DataItem.Text));
				_Label.PropertyChanged += UpdateCell;

				View = new StackLayout { Children = { _Label } };
			}

			void UpdateCell(object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == Label.TextProperty.PropertyName)
				{
					ForceUpdateSize();
				}
			}
		}

		[Preserve(AllMembers = true)]
		public class MyImageCell : ImageCell
		{
			VariableHeightItem _DataItem => BindingContext as VariableHeightItem;

			public MyImageCell()
			{
				SetBinding(TextProperty, new Binding(nameof(_DataItem.Text)));
				PropertyChanged += UpdateCell;
			}

			void UpdateCell(object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == ImageCell.TextProperty.PropertyName)
				{
					(sender as ImageCell).Height += 100;
					ForceUpdateSize();
				}
			}
		}

		[Preserve(AllMembers = true)]
		public class MyTextCell : TextCell
		{
			VariableHeightItem _DataItem => BindingContext as VariableHeightItem;

			public MyTextCell()
			{
				SetBinding(TextProperty, new Binding(nameof(_DataItem.Text)));
				PropertyChanged += UpdateCell;
			}

			void UpdateCell(object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == TextCell.TextProperty.PropertyName)
				{
					(sender as TextCell).Height += 100;
					ForceUpdateSize();
				}
			}
		}

		[Preserve(AllMembers = true)]
		public class MyEntryCell : EntryCell
		{
			VariableHeightItem _DataItem => BindingContext as VariableHeightItem;

			public MyEntryCell()
			{
				SetBinding(TextProperty, new Binding(nameof(_DataItem.Text)));
				PropertyChanged += UpdateCell;
			}

			void UpdateCell(object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == EntryCell.TextProperty.PropertyName)
				{
					(sender as EntryCell).Height += 100;
					ForceUpdateSize();
				}
			}
		}

		[Preserve(AllMembers = true)]
		public class MySwitchCell : SwitchCell
		{
			VariableHeightItem _DataItem => BindingContext as VariableHeightItem;

			public MySwitchCell()
			{
				SetBinding(TextProperty, new Binding(nameof(_DataItem.Text)));
				PropertyChanged += UpdateCell;
			}

			void UpdateCell(object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == SwitchCell.TextProperty.PropertyName)
				{
					(sender as SwitchCell).Height += 100;
					ForceUpdateSize();
				}
			}
		}

		public class MyPage<T> : ContentPage where T : Cell
		{
			private List<VariableHeightItem> _DataSource;
			public MyPage(ListViewCachingStrategy strategy)
			{
				_DataSource = Enumerable.Range(0, 10).Select(n => new VariableHeightItem()).ToList();
				var listView = new ListView(strategy) { HasUnevenRows = true, ItemsSource = _DataSource, ItemTemplate = new DataTemplate(typeof(T)) };
				var button = new Button { Text = "Click me" };
				button.Clicked += async (sender, e) =>
				{
					for (int i = 0; i < _DataSource.Count; i++)
					{
						var target = _DataSource[i];

						if (Device.RuntimePlatform == Device.iOS)
						{
							if (typeof(T) == typeof(MyViewCell))
								target.Text = "I am an exceptionally long string that should cause the label to wrap, thus increasing the size of the cell such that the entirety of the string is readable by human eyes. Hurrah.";
							else if (strategy == ListViewCachingStrategy.RetainElement)
								target.Text = "Look, I'm taller!";
							else
								target.Text = $"I'm only taller in {ListViewCachingStrategy.RetainElement} mode. :(";
						}
						else
						{
							if (typeof(T) == typeof(MyViewCell))
								target.Text = "I am an exceptionally long string that should cause the label to wrap, thus increasing the size of the cell such that the entirety of the string is readable by human eyes. Hurrah.";
							else
								target.Text = "Look, I'm taller!";
						}

						await Task.Delay(1000);
					}
				};

				Content = new StackLayout { Children = { button, listView } };
				Title = $"{typeof(T).Name} {strategy}";
			}
		}

		class VariableHeightItem : ViewModelBase
		{
			public VariableHeightItem()
			{
				Text = "This is a line of text.";
			}

			public string Text
			{
				get { return GetProperty<string>(); }
				set { SetProperty(value); }
			}
		}

		public CellForceUpdateSizeGalleryPage()
		{
			var stackLayout = new StackLayout();

			stackLayout.Children.Add(GetButton<MyViewCell>(ListViewCachingStrategy.RetainElement));
			stackLayout.Children.Add(GetButton<MyImageCell>(ListViewCachingStrategy.RetainElement));
			stackLayout.Children.Add(GetButton<MyEntryCell>(ListViewCachingStrategy.RetainElement));
			stackLayout.Children.Add(GetButton<MyTextCell>(ListViewCachingStrategy.RetainElement));
			stackLayout.Children.Add(GetButton<MySwitchCell>(ListViewCachingStrategy.RetainElement));

			stackLayout.Children.Add(GetButton<MyViewCell>(ListViewCachingStrategy.RecycleElement));
			stackLayout.Children.Add(GetButton<MyImageCell>(ListViewCachingStrategy.RecycleElement));
			stackLayout.Children.Add(GetButton<MyEntryCell>(ListViewCachingStrategy.RecycleElement));
			stackLayout.Children.Add(GetButton<MyTextCell>(ListViewCachingStrategy.RecycleElement));
			stackLayout.Children.Add(GetButton<MySwitchCell>(ListViewCachingStrategy.RecycleElement));

			Navigation.PushAsync(new ContentPage { Content = stackLayout });
		}

		Button GetButton<T>(ListViewCachingStrategy strategy) where T : Cell
		{
			return new Button
			{
				Text = $"{typeof(T).Name} {strategy}",
				Command = new Command(async () => await Navigation.PushAsync(new MyPage<T>(strategy)))
			};
		}
	}
}

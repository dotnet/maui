using System;
using System.Linq;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.ComponentModel;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44525, "Listview Row Height Does Not Adapt In iOS")]
	public class Bugzilla44525 : TestContentPage
	{
		List<Person> _DataSource;

		[Preserve(AllMembers = true)]
		class CustomCell : ViewCell
		{
			public CustomCell()
			{
				Label age = new Label();
				Label name = new Label();
				StackLayout cellWrapper = new StackLayout();

				age.SetBinding(Label.TextProperty, "Age");
				name.SetBinding(Label.TextProperty, "Name");

				age.PropertyChanged += UpdateCell;
				name.PropertyChanged += UpdateCell;

				cellWrapper.Children.Add(age);
				cellWrapper.Children.Add(name);

				View = cellWrapper;
			}

			void UpdateCell(object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == Label.TextProperty.PropertyName)
				{
					ForceUpdateSize();
				}
			}
		}

		class Person : ViewModelBase
		{
			private string _Name;
			public string Name
			{
				get
				{
					return _Name;
				}
				set
				{
					if (_Name == value)
						return;

					_Name = value;
					OnPropertyChanged();
				}
			}

			private string _Age;
			public string Age
			{
				get
				{
					return _Age;
				}
				set
				{
					if (_Age == value)
						return;

					_Age = value;
					OnPropertyChanged();
				}
			}
		}

		protected override void Init()
		{
			_DataSource = Enumerable.Range(1, 100).Select(c => new Person { Name = $"Person {c}", Age = $"{c} year(s) old" }).ToList();

			var listView = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				ItemTemplate = new DataTemplate(typeof(CustomCell)),
				ItemsSource = _DataSource,
				HasUnevenRows = true
			};

			var button = new Button { Text = "Click me" };
			button.Clicked += (sender, e) =>
			{
				var target = _DataSource[1];
				target.Name = "I am an exceptionally long string that should cause the label to wrap, thus increasing the size of the ViewCell such that the entirety of the string is readable by human eyes. Hurrah.";
			};

			Content = new StackLayout { Children = { button, listView } };
		}
	}
}

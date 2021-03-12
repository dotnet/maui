using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8672, "CollectionView crashes on iOS 12.4 for repeated adds", PlatformAffected.iOS)]
	public class Issue8672 : TestContentPage
	{
		protected override void Init()
		{
			var layout = new StackLayout();

			var indications = new Label
			{
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "If the CollectionView below loads items, the test has passed."
			};

			var collectionView = new CollectionView
			{
				ItemTemplate = CreateDataTemplate()
			};

			collectionView.SetBinding(ItemsView.ItemsSourceProperty, "Collection");

			layout.Children.Add(indications);
			layout.Children.Add(collectionView);

			Content = layout;
			BindingContext = new Issue8672ViewModel();
		}

		DataTemplate CreateDataTemplate()
		{
			var template = new DataTemplate(() =>
			{
				var layout = new StackLayout();

				var id = new Label();
				id.SetBinding(Label.TextProperty, "Id");
				layout.Children.Add(id);

				var name = new Label();
				name.SetBinding(Label.TextProperty, "Name");
				layout.Children.Add(name);

				return layout;
			});

			return template;
		}
	}

	public class Issue8672Model
	{
		public string Id { get; set; }

		public string Name { get; set; }
	}

	public class Issue8672ViewModel
	{
		public Issue8672ViewModel()
		{
			Task.Run(() =>
			{
				Device.BeginInvokeOnMainThread(() =>
				{
					for (int i = 0; i < 10; i++)
					{
						Collection.Add(new Issue8672Model() { Id = i.ToString(), Name = $"Item {i}" });
					}
				});
			});
		}

		public ObservableCollection<Issue8672Model> Collection { get; } = new ObservableCollection<Issue8672Model>();
	}
}

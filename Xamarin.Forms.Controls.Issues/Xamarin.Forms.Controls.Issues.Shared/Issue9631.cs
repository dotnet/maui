using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
using System.Linq;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9631, "[CollectionView] How to scroll in Horizontal CollectionView",
		PlatformAffected.UWP)]
	public partial class Issue9631 : TestContentPage
	{

		protected override void Init()
		{
			CollectionView collectionView = new CollectionView { ItemsLayout = LinearItemsLayout.Horizontal, };
			collectionView.ItemTemplate = new DataTemplate(() =>
			{
				var stacklayout = new StackLayout
				{
					Orientation = StackOrientation.Vertical,
					Spacing = 5,
					Padding = 10,
					BackgroundColor = Color.Beige
				};


				Image image = new Image { Aspect = Aspect.AspectFill, HeightRequest = 60, WidthRequest = 60 };
				image.SetBinding(Image.SourceProperty, "Source");

				Label nameLabel = new Label { FontAttributes = FontAttributes.Bold };
				nameLabel.SetBinding(Label.TextProperty, "Text");

				nameLabel.SetBinding(Label.AutomationIdProperty, "AutomationId");


				stacklayout.Children.Add(image);
				stacklayout.Children.Add(nameLabel);

				return stacklayout;
			});
			Content = collectionView;
			collectionView.SetBinding(ItemsView.ItemsSourceProperty, "Items");


			BindingContext = new ViewModel9631();
		}



		[Preserve(AllMembers = true)]
		public class ViewModel9631
		{
			public ObservableCollection<Model9631> Items { get; set; }

			public ViewModel9631()
			{
				var collection = new ObservableCollection<Model9631>();
				var pageSize = 50;

				for (var i = 0; i < pageSize; i++)
				{
					collection.Add(new Model9631
					{
						Text = "Image" + i,
						Source = "coffee.png",
						AutomationId = "Image" + i
					});
				}

				Items = collection;
			}
		}

		[Preserve(AllMembers = true)]
		public class Model9631
		{
			public string Text { get; set; }

			public string Source { get; set; }

			public string AutomationId { get; set; }

			public Model9631()
			{

			}
		}
	}
}
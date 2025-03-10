using System.Collections.ObjectModel;
 using Microsoft.Maui.Layouts;

 namespace Maui.Controls.Sample.Issues
 {

 	[Issue(IssueTracker.Github, 20855, "Grouped CollectionView items not rendered properly on Android, works on Windows", PlatformAffected.Android)]
 	public class Issue20855 : TestContentPage
 	{

 		protected override void Init()
 		{
 		ObservableCollection<ItemGroup>	ItemGroups = new ObservableCollection<ItemGroup>();
 		var group1 = new ItemGroup { Name = "Group 1"};
 		group1.Add(new Item { Name = "Item 1", CreateDate = new DateTime(2025, 2, 20) });
 		group1.Add(new Item { Name = "Item 2", CreateDate = new DateTime(2025, 2, 19) });
 		ItemGroups.Add(group1);
 		var group2 = new ItemGroup { Name = "Group 2"};
 		group2.Add(new Item { Name = "Item 3", CreateDate = new DateTime(2025, 2, 18) });
 		group2.Add(new Item { Name = "Item 4", CreateDate = new DateTime(2025, 2, 17) });
 		ItemGroups.Add(group2);

 		var collectionView = new CollectionView
         {
             ItemsSource = ItemGroups,
             IsGrouped = true,
             ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem
         };
         collectionView.GroupHeaderTemplate = new DataTemplate(() =>
         {
             var label = new Label
             {
                 FontSize = 18,
                 BackgroundColor = Colors.DarkGray,
                 TextColor = Colors.White,
                 Padding = new Thickness(10, 0)
             };
             label.SetBinding(Label.TextProperty, "Name");

             return  label;
         });
         collectionView.ItemTemplate = new DataTemplate(() =>
         {
             var flexLayout = new FlexLayout
             {
                 JustifyContent =  FlexJustify.SpaceBetween,
                 AlignItems = FlexAlignItems.Center,
                 HeightRequest = 100
             };

             var nameLabel = new Label
             {
                 Padding = new Thickness(8, 0),
                 HeightRequest = 80
             };
             nameLabel.SetBinding(Label.TextProperty, "Name");

             var dateLabel = new Label
             {
                 Padding = new Thickness(8, 0),
                 HeightRequest = 85
             };
             dateLabel.SetBinding(Label.TextProperty, new Binding("CreateDate", stringFormat: "{0:MMM d}"));

             flexLayout.Children.Add(nameLabel);
             flexLayout.Children.Add(dateLabel);

             return flexLayout;
         });
         var grid = new Grid();
         grid.Children.Add(collectionView);
         Content = grid;
 		}
 	}

 public class Item
 {
     public string Name { get; set; }
     public DateTime CreateDate { get; set; }
 }

 public class ItemGroup : ObservableCollection<Item>
 {
     public string Name { get; set; }
 }
 }
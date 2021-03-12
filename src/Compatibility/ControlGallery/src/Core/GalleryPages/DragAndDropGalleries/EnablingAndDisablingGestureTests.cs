using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.DragAndDropGalleries
{


	[Preserve(AllMembers = true)]
	public class EnablingAndDisablingGestureTests : ContentPage
	{
		public EnablingAndDisablingGestureTests()
		{
			Title = "Enabling and Disabling Gestures";
			StackLayout stackLayout = new StackLayout();
			CollectionView collectionView = new CollectionView();
			collectionView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;
			ObservableCollection<string> observableCollection = new ObservableCollection<string>();
			collectionView.ItemsSource = observableCollection;

			Image imageSource = new Image()
			{
				Source = "coffee.png",
				BackgroundColor = Color.Green
			};

			Image imageDestination = new Image()
			{
				BackgroundColor = Color.Purple,
				HeightRequest = 50,
				WidthRequest = 50
			};

			Button addRemoveDragGesture = new Button()
			{
				Text = "Add/Remove Drag Gesture",
				Command = new Command(() =>
				{
					var dragGestureRecognizer = imageSource.GestureRecognizers.OfType<DragGestureRecognizer>()
						.FirstOrDefault();

					if (dragGestureRecognizer != null)
						imageSource.GestureRecognizers.Remove(dragGestureRecognizer);
					else
					{
						var dragGesture = new DragGestureRecognizer()
						{
							CanDrag = true
						};

						dragGesture.DragStarting += (_, args) =>
						{
							observableCollection.Insert(0, $"DragStarting");
						};

						dragGesture.DropCompleted += (_, args) =>
						{
							observableCollection.Insert(0, $"DropCompleted");
						};

						imageSource.GestureRecognizers.Add(dragGesture);
					}
				})
			};

			Button toggleCanDrag = new Button()
			{
				Text = "Toggle Can Drag",
				Command = new Command(() =>
				{
					var dragGestureRecognizer = imageSource.GestureRecognizers.OfType<DragGestureRecognizer>()
						.FirstOrDefault();

					if (dragGestureRecognizer != null)
						dragGestureRecognizer.CanDrag = !dragGestureRecognizer.CanDrag;
				})
			};



			Button addRemoveDropGesture = new Button()
			{
				Text = "Add/Remove Drop Gesture",
				Command = new Command(() =>
				{
					var dropGestureRecognizer = imageDestination.GestureRecognizers.OfType<DropGestureRecognizer>()
						.FirstOrDefault();

					if (dropGestureRecognizer != null)
						imageDestination.GestureRecognizers.Remove(dropGestureRecognizer);
					else
					{
						var dropGesture = new DropGestureRecognizer()
						{
							AllowDrop = true
						};

						dropGesture.Drop += (_, args) =>
						{
							observableCollection.Insert(0, $"Drop");
						};

						dropGesture.DragOver += (_, args) =>
						{
							observableCollection.Insert(0, $"DragOver");
							args.AcceptedOperation = DataPackageOperation.Copy;
						};

						imageDestination.GestureRecognizers.Add(dropGesture);
					}
				})
			};

			Button toggleCanDrop = new Button()
			{
				Text = "Toggle Can Drop",
				Command = new Command(() =>
				{
					var dropGestureRecognizer = imageDestination.GestureRecognizers.OfType<DropGestureRecognizer>()
						.FirstOrDefault();

					if (dropGestureRecognizer != null)
						dropGestureRecognizer.AllowDrop = !dropGestureRecognizer.AllowDrop;
				})
			};

			stackLayout.Children.Add(imageSource);
			stackLayout.Children.Add(addRemoveDragGesture);
			stackLayout.Children.Add(toggleCanDrag);

			stackLayout.Children.Add(imageDestination);
			stackLayout.Children.Add(addRemoveDropGesture);
			stackLayout.Children.Add(toggleCanDrop);

			stackLayout.Children.Add(collectionView);
			Content = stackLayout;
		}
	}
}

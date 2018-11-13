using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class ItemReplacer : CollectionModifier
	{
		public ItemReplacer(CollectionView cv) : base(cv, "Replace")
		{
		}

		protected override void ModifyCollection(ObservableCollection<CollectionViewGalleryTestItem> observableCollection, params int[] indexes)
		{
			var index = indexes[0];

			if (index > -1 && index < observableCollection.Count)
			{
				var replacement = new CollectionViewGalleryTestItem(DateTime.Now, "Replacement", "coffee.png", index);
				observableCollection[index] = replacement;
			}
		}
	}

	internal class ItemMover : CollectionModifier
	{
		public ItemMover(CollectionView cv) : base(cv, "Move")
		{
			Entry.Keyboard = Keyboard.Default;
		}

		static int ParseToken(string value)
		{
			if (!int.TryParse(value, out int index))
			{
				return -1;
			}

			return index;
		}

		protected override bool ParseIndexes(out int[] indexes)
		{
			var text = Entry.Text;

			indexes = text.Split(',').Select(v => ParseToken(v.Trim())).ToArray();

			return indexes.Length == 2;
		}

		protected override void ModifyCollection(ObservableCollection<CollectionViewGalleryTestItem> observableCollection, params int[] indexes)
		{
			if (indexes.Length < 2)
			{
				return;
			}

			var index1 = indexes[0];
			var index2 = indexes[1];

			if (index1 > -1 && index2 > -1 && index1 < observableCollection.Count &&
				index2 < observableCollection.Count)
			{
				observableCollection.Move(index1, index2);
			}
		}
	}
}
using ElmSharp;
using ERect = ElmSharp.Rect;
using ESize = ElmSharp.Size;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public interface ICollectionViewLayoutManager
	{
		ICollectionViewController CollectionView { get; set; }

		bool IsHorizontal { get; }

		void SizeAllocated(ESize size);

		ESize GetScrollCanvasSize();

		void LayoutItems(ERect bound, bool force = false);

		ERect GetItemBound(int index);

		void ItemInserted(int index);

		void ItemRemoved(int index);

		void ItemUpdated(int index);

		void ItemSourceUpdated();

		void Reset();

		void ItemMeasureInvalidated(int index);

		int GetVisibleItemIndex(int x, int y);

		int GetScrollBlockSize();

		void SetHeader(EvasObject header, ESize size);

		void SetFooter(EvasObject footer, ESize size);
	}
}

using System;
using Android.Support.V7.Widget;

namespace Xamarin.Forms.Platform.Android
{
	public class SnapManager : IDisposable
	{
		readonly RecyclerView _recyclerView;
		readonly ItemsView _itemsView;
		SnapHelper _snapHelper;

		public SnapManager(ItemsView itemsView, RecyclerView recyclerView)
		{
			CollectionView.VerifyCollectionViewFlagEnabled(nameof(SnapManager));
			_recyclerView = recyclerView;
			_itemsView = itemsView;
		}

		internal void UpdateSnapBehavior()
		{
			if (!(_itemsView.ItemsLayout is ItemsLayout itemsLayout))
			{
				return;
			}

			var snapPointsType = itemsLayout.SnapPointsType;

			// Clear our the existing snap helper (if any)
			_snapHelper?.AttachToRecyclerView(null);
			_snapHelper = null;

			if (snapPointsType == SnapPointsType.None)
			{
				return;
			}

			var alignment = itemsLayout.SnapPointsAlignment;

			// Create a new snap helper
			_snapHelper = CreateSnapHelper(snapPointsType, alignment);
			
			// And attach it to this RecyclerView
			_snapHelper.AttachToRecyclerView(_recyclerView);
		}

		protected virtual SnapHelper CreateSnapHelper(SnapPointsType snapPointsType, SnapPointsAlignment alignment)
		{
			if (snapPointsType == SnapPointsType.Mandatory)
			{
				switch (alignment)
				{
					case SnapPointsAlignment.Start:
						return new StartSnapHelper();
					case SnapPointsAlignment.Center:
						return new LinearSnapHelper();
					case SnapPointsAlignment.End:
						return new EndSnapHelper();
					default:
						throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
				}
			}

			if (snapPointsType == SnapPointsType.MandatorySingle)
			{
				switch (alignment)
				{
					case SnapPointsAlignment.Start:
						return new StartPagerSnapHelper();
					case SnapPointsAlignment.Center:
						return new PagerSnapHelper();
					case SnapPointsAlignment.End:
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
				}
			}

			// The default LinearSnapHelper snaps to center
			return new LinearSnapHelper();
		}

		public void Dispose()
		{
			_recyclerView?.Dispose();
			_snapHelper?.Dispose();
		}
	}
}
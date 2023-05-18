#nullable disable
using System;
using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class SnapManager : IDisposable
	{
		readonly RecyclerView _recyclerView;
		SnapHelper _snapHelper;

		public SnapManager(RecyclerView recyclerView)
		{
			_recyclerView = recyclerView;
		}

		internal void UpdateSnapBehavior(IItemsLayout iitemsLayout)
		{
			if (iitemsLayout is not ItemsLayout itemsLayout)
				return;

			var snapPointsType = itemsLayout.SnapPointsType;

			// Clear our the existing snap helper (if any)
			DetachSnapHelper();

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
						return new CenterSnapHelper();
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
						return new StartSingleSnapHelper();
					case SnapPointsAlignment.Center:
						return new SingleSnapHelper();
					case SnapPointsAlignment.End:
						return new EndSingleSnapHelper();
					default:
						throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
				}
			}

			// Use center snapping as the default
			return new CenterSnapHelper();
		}

		internal SnapHelper GetCurrentSnapHelper()
		{
			return _snapHelper;
		}

		void DetachSnapHelper()
		{
			_snapHelper?.AttachToRecyclerView(null);
			_snapHelper?.Dispose();
			_snapHelper = null;
		}

		public void Dispose()
		{
			DetachSnapHelper();
		}
	}
}

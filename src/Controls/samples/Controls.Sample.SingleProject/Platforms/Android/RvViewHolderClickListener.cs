using Android.Views;
using System;

namespace Microsoft.Maui
{
	internal partial class RvAdapter
	{
		class RvViewHolderClickListener : Java.Lang.Object, View.IOnClickListener
		{
			public RvViewHolderClickListener(RvItemHolder viewHolder, Action<RvItemHolder> clickHandler)
			{
				ViewHolder = viewHolder;
				ClickHandler = clickHandler;
			}

			public RvItemHolder ViewHolder { get; }

			public Action<RvItemHolder> ClickHandler { get; }

			public void OnClick(View v)
			{
				if (ViewHolder?.PositionInfo != null && ViewHolder.PositionInfo.Kind == PositionKind.Item)
					ClickHandler?.Invoke(ViewHolder);
			}
		}
	}
}
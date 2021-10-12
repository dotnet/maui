using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using Android.Views.Accessibility;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui;

namespace Recipes.Platforms
{

	public class RecipesLinearLayoutManager : LinearLayoutManager
	{
		public RecipesLinearLayoutManager(Context context) : base(context)
		{
		}

		public override void OnInitializeAccessibilityNodeInfoForItem(RecyclerView.Recycler recycler, RecyclerView.State state, View host, AccessibilityNodeInfoCompat info)
		{
			base.OnInitializeAccessibilityNodeInfoForItem(recycler, state, host, info);
		}
	}

	public class RecipesVirtualListViewHandler : VirtualListViewHandler
	{

		protected override void ConnectHandler(RecyclerView nativeView)
		{
			base.ConnectHandler(nativeView);
			NativeView.SetAccessibilityDelegateCompat(new RecipesRecyclerViewDelegate(this));
		}



		class RecipesRecyclerViewDelegate : RecyclerViewAccessibilityDelegate
		{
			public RecipesRecyclerViewDelegate(RecipesVirtualListViewHandler recipesVirtualListViewHandler)
				: base(recipesVirtualListViewHandler.NativeView)
			{
				RecipesVirtualListViewHandler = recipesVirtualListViewHandler;
				RecyclerView = recipesVirtualListViewHandler.NativeView;
			}

			public RecyclerView RecyclerView { get; set; }
			public RecipesVirtualListViewHandler RecipesVirtualListViewHandler { get; }
			public int ItemCount => RecipesVirtualListViewHandler.VirtualView?.Adapter?.ItemsForSection(0) ?? 0;

			public override void OnInitializeAccessibilityNodeInfo(View host, AccessibilityNodeInfoCompat info)
			{
				base.OnInitializeAccessibilityNodeInfo(host, info);
				info.SetCollectionInfo(AccessibilityNodeInfoCompat.CollectionInfoCompat.Obtain(ItemCount, 1, false,
					(int)global::Android.Views.Accessibility.SelectionMode.None));
			}

			public override AccessibilityDelegateCompat GetItemDelegate()
			{
				return new RecipesRecyclerViewItemDelegate(this);
			}

			class RecipesRecyclerViewItemDelegate : RecipesRecyclerViewDelegate.ItemDelegate
			{
				readonly RecipesRecyclerViewDelegate _recyclerViewDelegate;

				public RecipesRecyclerViewItemDelegate(RecipesRecyclerViewDelegate recyclerViewDelegate) : base(recyclerViewDelegate)
				{
					_recyclerViewDelegate = recyclerViewDelegate;
				}

				public override void OnInitializeAccessibilityNodeInfo(View host, AccessibilityNodeInfoCompat info)
				{
					base.OnInitializeAccessibilityNodeInfo(host, info);
					
					var adapter = _recyclerViewDelegate.RecipesVirtualListViewHandler.VirtualView?.Adapter;

					if (host is RvViewContainer container && adapter != null &&
						container.VirtualView is Microsoft.Maui.Controls.BindableObject bo)
					{
						// TODO HACK RECIPES
						int i = 0;
						for(int j = 0; j < _recyclerViewDelegate.ItemCount; j++)
						{
							var data = adapter.DataFor(PositionKind.Item, 0, j);
							if (data == bo.BindingContext)
							{
								i = j;
								break;
							}
						}

						//TODO MAUI it seems like there's a bug with recycler view scrolling?
						//info.SetCollectionItemInfo(AccessibilityNodeInfoCompat.CollectionItemInfoCompat.Obtain(i, 1, 0, 1, false, false));
					}
				}
			}
		}
	}
}


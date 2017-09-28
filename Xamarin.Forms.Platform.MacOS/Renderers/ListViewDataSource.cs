using System;
using System.Collections;
using System.Collections.Generic;
using AppKit;
using Foundation;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.MacOS
{
	internal class ListViewDataSource : NSTableViewSource
	{
		IVisualElementRenderer _prototype;
		const int DefaultItemTemplateId = 1;
		static int s_dataTemplateIncrementer = 2; // lets start at not 0 because
		static int s_sectionCount;
		readonly nfloat _defaultSectionHeight;
		readonly Dictionary<DataTemplate, int> _templateToId = new Dictionary<DataTemplate, int>();
		readonly NSTableView _nsTableView;
		protected readonly ListView List;

		ITemplatedItemsView<Cell> TemplatedItemsView => List;

		bool _selectionFromNative;

		public virtual bool IsGroupingEnabled => List.IsGroupingEnabled;

		public Dictionary<int, int> Counts { get; set; }

		public ListViewDataSource(ListViewDataSource source)
		{
			List = source.List;
			_nsTableView = source._nsTableView;
			_defaultSectionHeight = source._defaultSectionHeight;
			_selectionFromNative = source._selectionFromNative;
			Counts = new Dictionary<int, int>();
		}

		public ListViewDataSource(ListView list, NSTableView tableView)
		{
			List = list;
			List.ItemSelected += OnItemSelected;
			_nsTableView = tableView;
			Counts = new Dictionary<int, int>();
		}

		public void Update()
		{
			_nsTableView.ReloadData();
		}

		public void OnRowClicked()
		{
			var selectedRow = _nsTableView.SelectedRow;
			if (selectedRow == -1)
				return;

			Cell cell = null;
			NSIndexPath indexPath = GetPathFromRow(selectedRow, ref cell);

			if (cell == null)
				return;

			_selectionFromNative = true;
			List.NotifyRowTapped((int)indexPath.Section, (int)indexPath.Item, cell);
		}


		public void OnItemSelected(object sender, SelectedItemChangedEventArgs eventArg)
		{
			if (_selectionFromNative)
			{
				_selectionFromNative = false;
				return;
			}

			var location = TemplatedItemsView.TemplatedItems.GetGroupAndIndexOfItem(eventArg.SelectedItem);
			if (location.Item1 == -1 || location.Item2 == -1)
			{
				var row = _nsTableView.SelectedRow;
				int groupIndex = 1;
				var selectedIndexPath = NSIndexPath.FromItemSection(row, groupIndex);
				if (selectedIndexPath != null)
					_nsTableView.DeselectRow(selectedIndexPath.Item);
				return;
			}

			var rowId = location.Item2;

			_nsTableView.SelectRow(rowId, false);
		}

		public override bool IsGroupRow(NSTableView tableView, nint row)
		{
			if (!IsGroupingEnabled)
				return false;

			int sectionIndex;
			bool isGroupHeader;
			int itemIndexInSection;

			GetComputedIndexes(row, out sectionIndex, out itemIndexInSection, out isGroupHeader);
			return isGroupHeader;
		}

		public override bool ShouldSelectRow(NSTableView tableView, nint row)
		{
			return !IsGroupRow(tableView, row);
		}

		public override nfloat GetRowHeight(NSTableView tableView, nint row)
		{
			if (!List.HasUnevenRows)
				return List.RowHeight == -1 ? ListViewRenderer.DefaultRowHeight : List.RowHeight;

			Cell cell = null;
			GetPathFromRow(row, ref cell);

			return CalculateHeightForCell(tableView, cell);
		}

		public override nint GetRowCount(NSTableView tableView)
		{
			var templatedItems = TemplatedItemsView.TemplatedItems;
			nint count = 0;

			if (!IsGroupingEnabled)
			{
				count = templatedItems.Count;
			}
			else
			{
				var sections = templatedItems.Count;
				for (int i = 0; i < sections; i++)
				{
					var group = (IList)((IList)templatedItems)[i];
					count += group.Count + 1;
				}
				s_sectionCount = sections;
			}
			return count;
		}

		public override NSView GetViewForItem(NSTableView tableView, NSTableColumn tableColumn, nint row)
		{
			var sectionIndex = 0;
			var itemIndexInSection = (int)row;
			Cell cell;

			var isHeader = false;

			if (IsGroupingEnabled)
				GetComputedIndexes(row, out sectionIndex, out itemIndexInSection, out isHeader);

			var indexPath = NSIndexPath.FromItemSection(itemIndexInSection, sectionIndex);
			var templateId = isHeader ? "headerCell" : TemplateIdForPath(indexPath).ToString();

			NSView nativeCell;

			var cachingStrategy = List.CachingStrategy;
			if (cachingStrategy == ListViewCachingStrategy.RetainElement)
			{
				cell = GetCellForPath(indexPath, isHeader);
				nativeCell = CellNSView.GetNativeCell(tableView, cell, templateId, isHeader);
			}
			else if ((cachingStrategy & ListViewCachingStrategy.RecycleElement) != 0)
			{
				nativeCell = tableView.MakeView(templateId, tableView);
				if (nativeCell == null)
				{
					cell = GetCellForPath(indexPath, isHeader);
					nativeCell = CellNSView.GetNativeCell(tableView, cell, templateId, isHeader, true);
				}
				else
				{
					var templatedList = TemplatedItemsView.TemplatedItems.GetGroup(sectionIndex);
					cell = (Cell)((INativeElementView)nativeCell).Element;

					cell.SendDisappearing();
					templatedList.UpdateContent(cell, itemIndexInSection);
					cell.SendAppearing();
				}
			}
			else
				throw new NotSupportedException();
			return nativeCell;
		}

		protected virtual Cell GetCellForPath(NSIndexPath indexPath, bool isGroupHeader)
		{
			var templatedItems = TemplatedItemsView.TemplatedItems;
			if (IsGroupingEnabled)
				templatedItems = (TemplatedItemsList<ItemsView<Cell>, Cell>)((IList)templatedItems)[(int)indexPath.Section];

			var cell = isGroupHeader ? templatedItems.HeaderContent : templatedItems[(int)indexPath.Item];
			return cell;
		}

		int TemplateIdForPath(NSIndexPath indexPath)
		{
			var itemTemplate = List.ItemTemplate;
			var selector = itemTemplate as DataTemplateSelector;
			if (selector == null)
				return DefaultItemTemplateId;

			var templatedList = TemplatedItemsView.TemplatedItems;
			if (List.IsGroupingEnabled)
				templatedList = (TemplatedItemsList<ItemsView<Cell>, Cell>)((IList)templatedList)[(int)indexPath.Section];

			var item = templatedList.ListProxy[(int)indexPath.Item];

			itemTemplate = selector.SelectTemplate(item, List);
			int key;
			if (!_templateToId.TryGetValue(itemTemplate, out key))
			{
				s_dataTemplateIncrementer++;
				key = s_dataTemplateIncrementer;
				_templateToId[itemTemplate] = key;
			}
			return key;
		}

		NSIndexPath GetPathFromRow(nint row, ref Cell cell)
		{
			var sectionIndex = 0;
			bool isGroupHeader = false;
			int itemIndexInSection;
			if (IsGroupingEnabled)
				GetComputedIndexes(row, out sectionIndex, out itemIndexInSection, out isGroupHeader);
			else
				itemIndexInSection = (int)row;
			NSIndexPath indexPath = NSIndexPath.FromItemSection(itemIndexInSection, sectionIndex);
			cell = GetCellForPath(indexPath, isGroupHeader);
			return indexPath;
		}

		nfloat CalculateHeightForCell(NSTableView tableView, Cell cell)
		{
			var viewCell = cell as ViewCell;
			double renderHeight;
			if (List.RowHeight == -1 && viewCell?.View != null)
			{
				var target = viewCell.View;
				if (_prototype == null)
				{
					_prototype = Platform.CreateRenderer(target);
					Platform.SetRenderer(target, _prototype);
				}
				else
				{
					_prototype.SetElement(target);
					Platform.SetRenderer(target, _prototype);
				}

				var req = target.Measure(tableView.Frame.Width, double.PositiveInfinity, MeasureFlags.IncludeMargins);

				target.ClearValue(Platform.RendererProperty);
				foreach (var descendant in target.Descendants())
					descendant.ClearValue(Platform.RendererProperty);

				renderHeight = req.Request.Height;
			}
			else
			{
				renderHeight = cell.RenderHeight;
			}

			return renderHeight > 0 ? (nfloat)renderHeight : ListViewRenderer.DefaultRowHeight;
		}

		void GetComputedIndexes(nint row, out int sectionIndex, out int itemIndexInSection, out bool isHeader)
		{
			var templatedItems = TemplatedItemsView.TemplatedItems;
			var totalItems = 0;
			isHeader = false;
			sectionIndex = 0;
			itemIndexInSection = 0;

			for (int i = 0; i < s_sectionCount; i++)
			{
				var group = (IList)((IList)templatedItems)[i];
				var itemsInSection = group.Count + 1;

				if (row < totalItems + itemsInSection)
				{
					sectionIndex = i;
					itemIndexInSection = (int)row - totalItems;
					isHeader = itemIndexInSection == 0;
					if (isHeader)
						itemIndexInSection = -1;
					else
						itemIndexInSection = itemIndexInSection - 1;
					break;
				}
				totalItems += itemsInSection;
			}
		}
	}
}
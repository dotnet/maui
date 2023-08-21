// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public interface ITableModel
	{
		Cell GetCell(int section, int row);
		Cell GetHeaderCell(int section);
		object GetItem(int section, int row);
		int GetRowCount(int section);
		int GetSectionCount();
		string[] GetSectionIndexTitles();
		string GetSectionTitle(int section);
		Color GetSectionTextColor(int section);
		void RowLongPressed(int section, int row);
		void RowSelected(object item);
		void RowSelected(int section, int row);
	}
}

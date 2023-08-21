// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.ListViewGalleries
{
	public class ListViewEntryCell : ContentPage
	{
		public ListViewEntryCell()
		{
			Content = new ListView()
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var cell = new EntryCell();
					cell.SetBinding(EntryCell.TextProperty, ".");
					return cell;
				}),
				ItemsSource = Enumerable.Range(0, 100).Select(i => $"Entry {i}").ToList()
			};
		}
	}
}

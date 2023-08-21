// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;

namespace Maui.Controls.Sample.Pages
{
	public partial class ListViewViewCell
	{
		public ListViewViewCell()
		{
			InitializeComponent();
			listView.ItemsSource = Enumerable.Range(0, 100).Select(i => $" Text {i}").ToList();
		}
	}
}
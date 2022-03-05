using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.ListViewGalleries
{
	public class ListViewTextCell : ContentPage
	{
		public ListViewTextCell()
		{
			Content = new ListView()
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var cell = new TextCell();
					cell.SetBinding(TextCell.TextProperty, ".");
					return cell;
				}),
				ItemsSource = Enumerable.Range(0, 100).Select(i => $"Text {i}").ToList()
			};
		}
	}
}

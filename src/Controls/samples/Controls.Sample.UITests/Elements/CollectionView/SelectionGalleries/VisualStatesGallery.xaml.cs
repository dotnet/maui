using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.CollectionViewGalleries.SelectionGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class VisualStatesGallery : ContentPage
	{
		public VisualStatesGallery()
		{
			InitializeComponent();
			BindingContext = this;
		}

		public LineItem[] SingleSelectionItems { get; } = new LineItem[]
		{
			new LineItem() { ItemName = "Item 1" },
			new LineItem() { ItemName = "Item 2" },
			new LineItem() { ItemName = "Item 3" },
		};

		public LineItem[] MultiSelectionItems { get; } = new LineItem[]
		{
			new LineItem() { ItemName = "Item 1" },
			new LineItem() { ItemName = "Item 2" },
			new LineItem() { ItemName = "Item 3" },
			new LineItem() { ItemName = "Item 4" },
		};
	}

	public class LineItem
	{
		public string ItemName { get; set; }

		public override string ToString() => ItemName;
	}
}
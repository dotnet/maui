using System;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class ListViewScenario1 : PerformanceScenario
	{
		public ListViewScenario1()
			: base("[ListView] Empty")
		{
			View = new ListView();
		}
	}

	[Preserve(AllMembers = true)]
	internal class ListViewScenario2 : PerformanceScenario
	{
		public ListViewScenario2()
			: base("[ListView] with 1k ViewCells")
		{
			View = new ListView
			{
				ItemsSource = Enumerable.Range(0, 1000),
				ItemTemplate = new DataTemplate(() => new ViewCell { View = new Label { Text = "Yay!" } })
			};
		}
	}

	[Preserve(AllMembers = true)]
	internal class ListViewScenario3 : PerformanceScenario
	{
		public ListViewScenario3()
			: base("[ListView] with 1k ViewCells & DTS")
		{
			View = new ListView
			{
				ItemsSource = Enumerable.Range(0, 1000),
				ItemTemplate = new MyDataTemplateSelector()
			};
		}

		class MyDataTemplateSelector : DataTemplateSelector
		{
			DataTemplate EvenTemplate;
			DataTemplate OddTemplate;

			public MyDataTemplateSelector()
			{
				EvenTemplate = new DataTemplate(() => new ViewCell { View = new Label { Text = "Even!" } });

				OddTemplate = new DataTemplate(() => new ViewCell { View = new Label { Text = "Odd!" } });
			}

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
			{
				int number = (int)item;

				if (number % 2 == 0)
					return EvenTemplate;
				else
					return OddTemplate;
			}
		}

	}
}

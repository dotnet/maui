using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

internal class ListViewCoreGalleryPage : CoreGalleryPage<ListView>
{
	protected override void InitializeElement(ListView element)
	{
		base.InitializeElement(element);

		var items = new List<string>();

		for (int n = 0; n < 1000; n++)
		{
			items.Add(DateTime.Now.AddDays(n).ToString("D"));
		}

		element.ItemsSource = items;

		element.HeightRequest = 250;
	}

	protected override void Build()
	{
		base.Build();
	}
}

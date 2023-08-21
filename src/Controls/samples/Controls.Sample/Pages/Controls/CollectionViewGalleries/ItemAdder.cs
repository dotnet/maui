// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries
{
	internal class ItemAdder : ObservableCollectionModifier
	{
		public ItemAdder(CollectionView cv) : base(cv, "Adder")
		{
		}

		protected override void ModifyObservableCollection(ObservableCollection<CollectionViewGalleryTestItem> observableCollection, params int[] indexes)
		{
			var item = new CollectionViewGalleryTestItem(DateTime.Now, "Added", "oasis.jpg", observableCollection.Count);
			observableCollection.Add(item);
		}
	}
}
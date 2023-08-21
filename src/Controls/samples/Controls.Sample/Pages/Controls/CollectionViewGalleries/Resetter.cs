// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries
{
	internal class Resetter : MultiTestObservableCollectionModifier
	{
		public Resetter(CollectionView cv) : base(cv, "Reset")
		{
		}

		protected override void ModifyObservableCollection(MultiTestObservableCollection<CollectionViewGalleryTestItem> observableCollection, params int[] indexes)
		{
			observableCollection.TestReset();
		}
	}
}
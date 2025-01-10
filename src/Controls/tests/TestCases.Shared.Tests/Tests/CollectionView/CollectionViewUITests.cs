﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public abstract class CollectionViewUITests : UITest
	{
		const string CollectionViewGallery = "CollectionView Gallery";

		public CollectionViewUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			BaseFixtureSetup(CollectionViewGallery);
		}

		[TearDown]
		public void CollectionViewUITestTearDown()
		{
			if (Device != TestDevice.Windows)
				this.Back();
		}

		internal void VisitInitialGallery(string collectionTestName)
		{
			var galleryName = $"{collectionTestName} Galleries";
			var regexGalleryName = System.Text.RegularExpressions.Regex.Replace(galleryName, " |\\(|\\)", string.Empty);

			App.WaitForElementTillPageNavigationSettled(regexGalleryName);
			App.Tap(regexGalleryName);
		}

		internal void VisitSubGallery(string galleryName)
		{
			App.WaitForElementTillPageNavigationSettled(galleryName);
			App.Tap(galleryName);
		}
	}
}
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public abstract class CollectionViewUITests : CoreGalleryBasePageTest
	{
		const string CollectionViewGallery = "CollectionView Gallery";

		public CollectionViewUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(CollectionViewGallery);
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
			var regexGalleryName = RegexHelper.AutomationIdRegex.Replace(galleryName, string.Empty);

			App.WaitForElementTillPageNavigationSettled(regexGalleryName);
			App.Tap(regexGalleryName);
		}

		internal void VisitSubGallery(string galleryName)
		{
			App.WaitForElementTillPageNavigationSettled(galleryName);
			App.Tap(galleryName);
		}
	}

	internal static partial class RegexHelper
	{
		#if NET7_0_OR_GREATER
		[GeneratedRegex (" |\\(|\\)", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
		internal static partial Regex AutomationIdRegex
		{
			get;
		}
		#else
		internal static readonly Regex AutomationIdRegex =
										new (
											" |\\(|\\)",
											RegexOptions.Compiled,		
											TimeSpan.FromMilliseconds(1000)							// against malicious input
											);
		#endif
	}
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh8799 : ContentPage
	{
		public Gh8799()
		{
			InitializeComponent();

			this.Gh8799Label.Style = Resources["Gh8799Text"] as Style;
		}

		[TestFixture]
		class Tests
		{
			[Test]
			public void CanAccessNamedStyleWhenLoadedIntoMergedDictionaryBySource([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh8799();
				Assert.That(layout, Is.Not.Null);
			}
		}
	}
}

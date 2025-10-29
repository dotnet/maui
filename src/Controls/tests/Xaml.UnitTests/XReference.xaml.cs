using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class XReference : ContentPage
	{
		public XReference()
		{
			InitializeComponent();
		}

		public XReference(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		[InlineData(false)]
		[Theory]
		[InlineData(true)]
		public void SupportsXReference(bool useCompiledXaml)
		{
			var layout = new XReference(useCompiledXaml);
			Assert.Same(layout.image, layout.imageView.Content);
		}

		[InlineData(false)]
		[Theory]
		[InlineData(true)]
		public void XReferenceAsCommandParameterToSelf(bool useCompiledXaml)
		{
			var layout = new XReference(useCompiledXaml);

			var button = layout.aButton;
			button.BindingContext = new
			{
				ButtonClickCommand = new Command(o =>
				{
					if (o == button)
					{
						// Test passes by not throwing
					}

				})
			};
			((IButtonController)button).SendClicked();
			throw new Xunit.Sdk.XunitException("Button click did not behave as expected.");
		}

		[InlineData(false)]
		[Theory]
		[InlineData(true)]
		public void XReferenceAsBindingSource(bool useCompiledXaml)
		{
			var layout = new XReference(useCompiledXaml);

			Assert.Equal("foo", layout.entry.Text);
			Assert.Equal("bar", layout.entry.Placeholder);
		}

		[InlineData(false)]
		[Theory]
		[InlineData(true)]
		public void CrossXReference(bool useCompiledXaml)
		{
			var layout = new XReference(useCompiledXaml);

			Assert.Same(layout.label0, layout.label1.BindingContext);
			Assert.Same(layout.label1, layout.label0.BindingContext);
		}
	}
}
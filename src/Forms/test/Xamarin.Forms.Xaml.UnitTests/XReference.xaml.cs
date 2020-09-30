using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
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

		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void SupportsXReference(bool useCompiledXaml)
			{
				var layout = new XReference(useCompiledXaml);
				Assert.AreSame(layout.image, layout.imageView.Content);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void XReferenceAsCommandParameterToSelf(bool useCompiledXaml)
			{
				var layout = new XReference(useCompiledXaml);

				var button = layout.aButton;
				button.BindingContext = new
				{
					ButtonClickCommand = new Command(o =>
					{
						if (o == button)
							Assert.Pass();
					})
				};
				((IButtonController)button).SendClicked();
				Assert.Fail();
			}

			[TestCase(false)]
			[TestCase(true)]
			public void XReferenceAsBindingSource(bool useCompiledXaml)
			{
				var layout = new XReference(useCompiledXaml);

				Assert.AreEqual("foo", layout.entry.Text);
				Assert.AreEqual("bar", layout.entry.Placeholder);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void CrossXReference(bool useCompiledXaml)
			{
				var layout = new XReference(useCompiledXaml);

				Assert.AreSame(layout.label0, layout.label1.BindingContext);
				Assert.AreSame(layout.label1, layout.label0.BindingContext);
			}
		}
	}
}
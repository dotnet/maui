using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Issue1250AspectRatioContainer : ContentView
	{
		protected override SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
		{
			return new SizeRequest(new Size(widthConstraint, widthConstraint * AspectRatio));
		}

		public double AspectRatio { get; set; }
	}

	public partial class Issue1250 : ContentPage
	{
		public Issue1250()
		{
			InitializeComponent();
		}

		public Issue1250(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void AddCustomElementInCollection(bool useCompiledXaml)
			{
				var page = new Issue1250(useCompiledXaml);
				var stack = page.stack;

				Assert.AreEqual(3, stack.Children.Count);
				Assert.That(stack.Children[0], Is.TypeOf<Label>());
				Assert.That(stack.Children[1], Is.TypeOf<Issue1250AspectRatioContainer>());
				Assert.That(stack.Children[2], Is.TypeOf<Label>());
			}
		}
	}
}
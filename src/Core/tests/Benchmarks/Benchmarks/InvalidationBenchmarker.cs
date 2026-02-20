using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class InvalidationBenchmarker
	{
		const int InvalidationCount = 1000;

		View _leafWithSubscriber = null!;
		View _leafUnderPage = null!;
		View _leafUnderLegacyLayout = null!;

		[GlobalSetup]
		public void Setup()
		{
			typeof(ContentPage).Assembly
				.GetType("Microsoft.Maui.Controls.Hosting.CompatibilityCheck")?
				.GetMethod("UseCompatibility", BindingFlags.Static | BindingFlags.NonPublic)?
				.Invoke(null, null);

			_leafWithSubscriber = new Border();
			_leafWithSubscriber.MeasureInvalidated += OnMeasureInvalidated;

			var page = new ContentPage();
			var pageContainer = new Grid();
			_leafUnderPage = new Border();
			pageContainer.Add(_leafUnderPage);
			page.Content = pageContainer;

#pragma warning disable CS0618 // Legacy layout benchmark coverage
			var legacyLayout = new Microsoft.Maui.Controls.Compatibility.StackLayout();
#pragma warning restore CS0618
			_leafUnderLegacyLayout = new Border();
			legacyLayout.Children.Add(_leafUnderLegacyLayout);
		}

		[Benchmark]
		public void RaiseMeasureInvalidatedWithSubscriber()
		{
#pragma warning disable CS0618
			for (int i = 0; i < InvalidationCount; i++)
			{
				_leafWithSubscriber.InvalidateMeasureNonVirtual(InvalidationTrigger.MeasureChanged);
			}
#pragma warning restore CS0618
		}

		[Benchmark]
		public void PropagateMeasureInvalidationThroughPage()
		{
#pragma warning disable CS0618
			for (int i = 0; i < InvalidationCount; i++)
			{
				_leafUnderPage.InvalidateMeasureNonVirtual(InvalidationTrigger.MeasureChanged);
			}
#pragma warning restore CS0618
		}

		[Benchmark]
		public void PropagateMeasureInvalidationThroughLegacyLayout()
		{
#pragma warning disable CS0618
			for (int i = 0; i < InvalidationCount; i++)
			{
				_leafUnderLegacyLayout.InvalidateMeasureNonVirtual(InvalidationTrigger.MeasureChanged);
			}
#pragma warning restore CS0618
		}

		static void OnMeasureInvalidated(object sender, EventArgs e)
		{
		}
	}
}

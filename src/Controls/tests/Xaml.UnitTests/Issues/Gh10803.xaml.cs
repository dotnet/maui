using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh10803 : ContentPage
	{
		public Gh10803() => InitializeComponent();
		public Gh10803(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			bool enableDiagnosticsInitialState;
			int failures = 0;

			[SetUp]
			public void Setup()
			{
				DispatcherProvider.SetCurrent(new DispatcherProviderStub());
				VisualDiagnostics.VisualTreeChanged += VTChanged;
				enableDiagnosticsInitialState = RuntimeFeature.EnableDiagnostics;
				RuntimeFeature.EnableDiagnostics = true;
			}

			[TearDown]
			public void TearDown()
			{
				RuntimeFeature.EnableDiagnostics = enableDiagnosticsInitialState;
				DispatcherProvider.SetCurrent(null);
				VisualDiagnostics.VisualTreeChanged -= VTChanged;
				failures = 0;
			}

			[Test]
			public void SourceInfoForElementsInDT([Values(false)] bool useCompiledXaml)
			{
				var layout = new Gh10803(useCompiledXaml);
				var listview = layout.listview;
				var cell = listview.TemplatedItems.GetOrCreateContent(0, null);
				Assert.That(failures, Is.EqualTo(0), "one or more element without source info, or with invalid ChildIndex");
			}

			void VTChanged(object sender, VisualTreeChangeEventArgs e)
			{
				var parentSourInfo = e.Parent == null ? null : VisualDiagnostics.GetSourceInfo(e.Parent);
				var childSourceInfo = VisualDiagnostics.GetSourceInfo(e.Child);
				if (childSourceInfo == null)
					failures++;
				if (e.Parent != null && parentSourInfo == null)
					failures++;
				if (e.Parent != null && e.ChildIndex == -1)
					failures++;
			}
		}
	}
}

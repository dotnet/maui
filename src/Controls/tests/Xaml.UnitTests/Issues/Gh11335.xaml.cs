using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh11335 : ContentPage
	{
		public Gh11335() => InitializeComponent();
		public Gh11335(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		void Remove(object sender, EventArgs e) => stack.Children.Remove(label);

		void Add(object sender, EventArgs e)
		{
			int index = stack.Children.IndexOf(label);
			Label newLabel = new Label { Text = "New Inserted Label" };
			stack.Children.Insert(index + 1, newLabel);
		}

		[TestFixture]
		class Tests
		{
			bool enableDiagnosticsInitialState;

			[SetUp]
			public void Setup()
			{
				enableDiagnosticsInitialState = RuntimeFeature.EnableDiagnostics;
				RuntimeFeature.EnableMauiDiagnostics = true;
			}

			[TearDown]
			public void TearDown()
			{
				RuntimeFeature.EnableMauiDiagnostics = enableDiagnosticsInitialState;
				VisualDiagnostics.VisualTreeChanged -= OnVTChanged;
			}

			void OnVTChanged(object sender, VisualTreeChangeEventArgs e)
			{
				Assert.That(e.ChangeType, Is.EqualTo(VisualTreeChangeType.Add));
				Assert.That(e.ChildIndex, Is.EqualTo(1));
				Assert.Pass();
			}

			[Test]
			public void ChildIndexOnAdd([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh11335(useCompiledXaml);
				VisualDiagnostics.VisualTreeChanged += OnVTChanged;
				layout.Add(null, EventArgs.Empty);
				Assert.Fail();
			}
		}
	}
}
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh11334 : ContentPage
{
	public Gh11334() => InitializeComponent();

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
			Assert.That(e.ChangeType, Is.EqualTo(VisualTreeChangeType.Remove));
			Assert.That(e.ChildIndex, Is.EqualTo(0));
			Assert.Pass();
		}

		[Test]
		public void ChildIndexOnRemove([Values] XamlInflator inflator)
		{
			var layout = new Gh11334(inflator);
			VisualDiagnostics.VisualTreeChanged += OnVTChanged;
			layout.Remove(null, EventArgs.Empty);
			Assert.Fail();
		}
	}
}

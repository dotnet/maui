using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
public partial class Gh11335 : ContentPage
{
	public Gh11335() => InitializeComponent();

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
		bool _debuggerinitialstate;

		[SetUp]
		public void Setup()
		{
			_debuggerinitialstate = DebuggerHelper._mockDebuggerIsAttached;
			DebuggerHelper._mockDebuggerIsAttached = true;
		}

		[TearDown]
		public void TearDown()
		{
			DebuggerHelper._mockDebuggerIsAttached = _debuggerinitialstate;
			VisualDiagnostics.VisualTreeChanged -= OnVTChanged;
		}

		void OnVTChanged(object sender, VisualTreeChangeEventArgs e)
		{
			Assert.That(e.ChangeType, Is.EqualTo(VisualTreeChangeType.Add));
			Assert.That(e.ChildIndex, Is.EqualTo(1));
			Assert.Pass();
		}

		[Test]
		public void ChildIndexOnAdd([Values] XamlInflator inflator)
		{
			var layout = new Gh11335(inflator);
			VisualDiagnostics.VisualTreeChanged += OnVTChanged;
			layout.Add(null, EventArgs.Empty);
			Assert.Fail();
		}
	}
}
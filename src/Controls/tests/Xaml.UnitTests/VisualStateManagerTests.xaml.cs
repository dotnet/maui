using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class VisualStateManagerTests : ContentPage
{
	public VisualStateManagerTests() => InitializeComponent();

	class Tests
	{
		[SetUp] public void SetUp() => Application.Current = new MockApplication();

		[TearDown] public void TearDown() => Application.Current = null;

		[Test]
		public void VisualStatesFromStyleXaml([Values] XamlInflator inflator)
		{
			var layout = new VisualStateManagerTests(inflator);

			var entry0 = layout.Entry0;

			// Verify that Entry0 has no VisualStateGroups
			Assert.False(entry0.HasVisualStateGroups());
			Assert.That(null, Is.EqualTo(entry0.TextColor));
			Assert.That(null, Is.EqualTo(entry0.PlaceholderColor));

			var entry1 = layout.Entry1;

			// Verify that the correct groups are set up for Entry1
			var groups = VisualStateManager.GetVisualStateGroups(entry1);
			Assert.AreEqual(3, groups.Count);
			Assert.That(groups[0].Name, Is.EqualTo("CommonStates"));
			Assert.Contains("Normal", groups[0].States.Select(state => state.Name).ToList());
			Assert.Contains("Disabled", groups[0].States.Select(state => state.Name).ToList());

			Assert.AreEqual(null, entry1.TextColor);
			Assert.AreEqual(null, entry1.PlaceholderColor);

			// Change the state of Entry1
			Assert.True(VisualStateManager.GoToState(entry1, "Disabled"));

			// And verify that the changes took
			Assert.AreEqual(Colors.Gray, entry1.TextColor);
			Assert.AreEqual(Colors.LightGray, entry1.PlaceholderColor);

			// Verify that Entry0 was unaffected
			Assert.AreEqual(null, entry0.TextColor);
			Assert.AreEqual(null, entry0.PlaceholderColor);
		}

		[Test]
		public void UnapplyVisualState([Values] XamlInflator inflator)
		{
			var layout = new VisualStateManagerTests(inflator);
			var entry1 = layout.Entry1;

			Assert.AreEqual(null, entry1.TextColor);
			Assert.AreEqual(null, entry1.PlaceholderColor);

			// Change the state of Entry1
			var groups = VisualStateManager.GetVisualStateGroups(entry1);
			Assert.True(VisualStateManager.GoToState(entry1, "Disabled"));

			// And verify that the changes took
			Assert.AreEqual(Colors.Gray, entry1.TextColor);
			Assert.AreEqual(Colors.LightGray, entry1.PlaceholderColor);

			// Now change it to Normal
			Assert.True(VisualStateManager.GoToState(entry1, "Normal"));

			// And verify that the changes reverted
			Assert.AreEqual(null, entry1.TextColor);
			Assert.AreEqual(null, entry1.PlaceholderColor);
		}

		[Test]
		public void VisualStateGroupsDirectlyOnElement([Values] XamlInflator inflator)
		{
			var layout = new VisualStateManagerTests(inflator);

			var entry = layout.Entry2;

			var groups = VisualStateManager.GetVisualStateGroups(entry);

			Assert.NotNull(groups);
			Assert.That(groups.Count, Is.EqualTo(2));
		}

		[Test]
		public void EmptyGroupDirectlyOnElement([Values] XamlInflator inflator)
		{
			var layout = new VisualStateManagerTests(inflator);

			var entry3 = layout.Entry3;

			var groups = VisualStateManager.GetVisualStateGroups(entry3);

			Assert.NotNull(groups);
			Assert.True(groups.Count == 1);
		}

		[Test]
		public void VisualStateGroupsFromStylesAreDistinct([Values] XamlInflator inflator)
		{
			var layout = new VisualStateManagerTests(inflator);

			var label1 = layout.ErrorLabel1;
			var label2 = layout.ErrorLabel2;

			var groups1 = VisualStateManager.GetVisualStateGroups(label1);
			var groups2 = VisualStateManager.GetVisualStateGroups(label2);

			Assert.AreNotSame(groups1, groups2);

			var currentState1 = groups1[0].CurrentState;
			var currentState2 = groups2[0].CurrentState;

			Assert.That(currentState1.Name, Is.EqualTo("Normal"));
			Assert.That(currentState2.Name, Is.EqualTo("Normal"));

			VisualStateManager.GoToState(label1, "Invalid");

			Assert.That(groups1[0].CurrentState.Name, Is.EqualTo("Invalid"));
			Assert.That(groups2[0].CurrentState.Name, Is.EqualTo("Normal"));
		}

		[Test]
		public void SettersAreAddedToCorrectState([Values] XamlInflator inflator)
		{
			var layout = new VisualStateManagerTests(inflator);

			var entry = layout.Entry4;

			var groups = VisualStateManager.GetVisualStateGroups(entry);

			Assert.That(groups.Count, Is.EqualTo(1));

			var common = groups[0];

			var normal = common.States.Single(state => state.Name == "Normal");
			var disabled = common.States.Single(state => state.Name == "Disabled");

			Assert.That(normal.Setters.Count, Is.EqualTo(0));
			Assert.That(disabled.Setters.Count, Is.EqualTo(2));
		}

		[Test]
		public void VisualElementGoesToCorrectStateWhenAvailable([Values] XamlInflator inflator)
		{
			var layout = new VisualStateManagerTests(inflator);

			var button = layout.Button1;

			Assert.That(button.BackgroundColor, Is.EqualTo(Colors.Lime));
		}

		[Test]
		public void TargetedVisualElementGoesToCorrectState([Values] XamlInflator inflator)
		{
			var layout = new VisualStateManagerTests(inflator);

			var label1 = layout.TargetLabel1;

			VisualStateManager.GoToState(layout, "Red");

			Assert.That(label1.Text, Is.EqualTo("Red"));

			VisualStateManager.GoToState(layout, "Blue");

			Assert.That(label1.Text, Is.EqualTo("Blue"));

		}
	}
}
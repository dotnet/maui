using System.Linq;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class VisualStateManagerTests : ContentPage
	{
		public VisualStateManagerTests()
		{
			InitializeComponent();
		}

		public VisualStateManagerTests(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void SetUp()
			{
				Device.PlatformServices = new MockPlatformServices();
				Application.Current = new MockApplication();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
				Application.Current = null;
			}

			[TestCase(false)]
			[TestCase(true)]
			public void VisualStatesFromStyleXaml(bool useCompiledXaml)
			{
				var layout = new VisualStateManagerTests(useCompiledXaml);

				var entry0 = layout.Entry0;

				// Verify that Entry0 has no VisualStateGroups
				Assert.False(entry0.HasVisualStateGroups());
				Assert.That(Color.Default, Is.EqualTo(entry0.TextColor));
				Assert.That(Color.Default, Is.EqualTo(entry0.PlaceholderColor));

				var entry1 = layout.Entry1;

				// Verify that the correct groups are set up for Entry1
				var groups = VisualStateManager.GetVisualStateGroups(entry1);
				Assert.AreEqual(3, groups.Count);
				Assert.That(groups[0].Name, Is.EqualTo("CommonStates"));
				Assert.Contains("Normal", groups[0].States.Select(state => state.Name).ToList());
				Assert.Contains("Disabled", groups[0].States.Select(state => state.Name).ToList());

				Assert.AreEqual(Color.Default, entry1.TextColor);
				Assert.AreEqual(Color.Default, entry1.PlaceholderColor);

				// Change the state of Entry1
				Assert.True(VisualStateManager.GoToState(entry1, "Disabled"));

				// And verify that the changes took
				Assert.AreEqual(Color.Gray, entry1.TextColor);
				Assert.AreEqual(Color.LightGray, entry1.PlaceholderColor);

				// Verify that Entry0 was unaffected
				Assert.AreEqual(Color.Default, entry0.TextColor);
				Assert.AreEqual(Color.Default, entry0.PlaceholderColor);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void UnapplyVisualState(bool useCompiledXaml)
			{
				var layout = new VisualStateManagerTests(useCompiledXaml);
				var entry1 = layout.Entry1;

				Assert.AreEqual(Color.Default, entry1.TextColor);
				Assert.AreEqual(Color.Default, entry1.PlaceholderColor);

				// Change the state of Entry1
				var groups = VisualStateManager.GetVisualStateGroups(entry1);
				Assert.True(VisualStateManager.GoToState(entry1, "Disabled"));

				// And verify that the changes took
				Assert.AreEqual(Color.Gray, entry1.TextColor);
				Assert.AreEqual(Color.LightGray, entry1.PlaceholderColor);

				// Now change it to Normal
				Assert.True(VisualStateManager.GoToState(entry1, "Normal"));

				// And verify that the changes reverted
				Assert.AreEqual(Color.Default, entry1.TextColor);
				Assert.AreEqual(Color.Default, entry1.PlaceholderColor);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void VisualStateGroupsDirectlyOnElement(bool useCompiledXaml)
			{
				var layout = new VisualStateManagerTests(useCompiledXaml);

				var entry = layout.Entry2;

				var groups = VisualStateManager.GetVisualStateGroups(entry);

				Assert.NotNull(groups);
				Assert.That(groups.Count, Is.EqualTo(2));
			}

			[TestCase(false)]
			[TestCase(true)]
			public void EmptyGroupDirectlyOnElement(bool useCompiledXaml)
			{
				var layout = new VisualStateManagerTests(useCompiledXaml);

				var entry3 = layout.Entry3;

				var groups = VisualStateManager.GetVisualStateGroups(entry3);

				Assert.NotNull(groups);
				Assert.True(groups.Count == 1);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void VisualStateGroupsFromStylesAreDistinct(bool useCompiledXaml)
			{
				var layout = new VisualStateManagerTests(useCompiledXaml);

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

			[TestCase(false)]
			[TestCase(true)]
			public void SettersAreAddedToCorrectState(bool useCompiledXaml)
			{
				var layout = new VisualStateManagerTests(useCompiledXaml);

				var entry = layout.Entry4;

				var groups = VisualStateManager.GetVisualStateGroups(entry);

				Assert.That(groups.Count, Is.EqualTo(1));

				var common = groups[0];

				var normal = common.States.Single(state => state.Name == "Normal");
				var disabled = common.States.Single(state => state.Name == "Disabled");

				Assert.That(normal.Setters.Count, Is.EqualTo(0));
				Assert.That(disabled.Setters.Count, Is.EqualTo(2));
			}

			[TestCase(false)]
			[TestCase(true)]
			public void VisualElementGoesToCorrectStateWhenAvailable(bool useCompiledXaml)
			{
				var layout = new VisualStateManagerTests(useCompiledXaml);

				var button = layout.Button1;

				Assert.That(button.BackgroundColor, Is.EqualTo(Color.Lime));
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TargetedVisualElementGoesToCorrectState(bool useCompiledXaml)
			{
				var layout = new VisualStateManagerTests(useCompiledXaml);

				var label1 = layout.TargetLabel1;

				VisualStateManager.GoToState(layout, "Red");

				Assert.That(label1.Text, Is.EqualTo("Red"));

				VisualStateManager.GoToState(layout, "Blue");

				Assert.That(label1.Text, Is.EqualTo("Blue"));

			}
		}
	}
}
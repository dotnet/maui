#if IOS // Device orientation changes are not supported on Windows or MacCatalyst.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29264 : _IssuesUITest
{
	public Issue29264(TestDevice device) : base(device) { }

	public override string Issue => "Rotating My Recipes changes the selected item";

	protected override bool ResetAfterEachTest => true;

	[TestCase(3, true)]
	[TestCase(4, true)]
	[TestCase(3, false)]
	[TestCase(4, false)]
	[ShardedTestCategory(UITestCategories.CarouselView)]
	public void RepeatedRotationsPreserveSelectedRecipe(int position, bool loop)
	{
		App.SetOrientationPortrait();
		App.WaitForElement("RecipeCarousel");
		Assert.That(App.WaitForTextToBePresentInElement("OrientationLabel", "Portrait"), Is.True);
		if (!loop)
		{
			App.Tap("RecipeLoopSwitch");
			Assert.That(App.WaitForTextToBePresentInElement("RecipeLoopLabel", "Loop: False"), Is.True);
		}
		Assert.That(App.FindElement("RecipePositionLabel").GetText(), Is.EqualTo("Position: 0"));

		try
		{
			for (int i = 0; i < position; i++)
			{
				App.SwipeRightToLeft("RecipeCarousel");
				Assert.That(App.WaitForTextToBePresentInElement("RecipePositionLabel", $"Position: {i + 1}"), Is.True);
			}

			var recipes = new[] { "First Recipe", "Second Recipe", "Third Recipe", "Fourth Recipe", "Fifth Recipe" };
			Assert.That(App.WaitForTextToBePresentInElement("RecipeCurrentItemLabel", recipes[position]), Is.True);
			var positionChanges = App.FindElement("RecipePositionChangedLabel").GetText();

			for (int rotation = 0; rotation < 3; rotation++)
			{
				App.SetOrientationLandscape();
				Assert.That(App.WaitForTextToBePresentInElement("OrientationLabel", "Landscape"), Is.True);
				AssertSelection(position, recipes[position], positionChanges);

				App.SetOrientationPortrait();
				Assert.That(App.WaitForTextToBePresentInElement("OrientationLabel", "Portrait"), Is.True);
				AssertSelection(position, recipes[position], positionChanges);
			}

			if (position < recipes.Length - 1)
			{
				App.SwipeRightToLeft("RecipeCarousel");
				Assert.That(App.WaitForTextToBePresentInElement("RecipePositionLabel", $"Position: {position + 1}"), Is.True);
				Assert.That(App.WaitForTextToBePresentInElement("RecipeCurrentItemLabel", recipes[position + 1]), Is.True);
			}
		}
		finally
		{
			App.SetOrientationPortrait();
		}
	}

	void AssertSelection(int position, string recipe, string? positionChanges)
	{
		Assert.That(App.FindElement("RecipePositionLabel").GetText(), Is.EqualTo($"Position: {position}"));
		Assert.That(App.FindElement("RecipeCurrentItemLabel").GetText(), Is.EqualTo(recipe));
		Assert.That(App.FindElement("RecipePositionChangedLabel").GetText(), Is.EqualTo(positionChanges),
			"Resizing must not emit a PositionChanged event, even if the original position is later restored.");

		var carousel = App.WaitForElement("RecipeCarousel").GetRect();
		var item = App.WaitForElement(recipe).GetRect();
		Assert.That(item.CenterX(), Is.InRange(carousel.X, carousel.X + carousel.Width),
			"The selected recipe must remain visible after resizing.");
	}
}
#endif
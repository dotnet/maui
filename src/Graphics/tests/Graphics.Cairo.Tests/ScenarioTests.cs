using System.Linq;
using GraphicsTester.Scenarios;
using Xunit;

namespace Microsoft.Maui.Graphics.Cairo.Tests
{
	/// <summary>
	/// Renders every GraphicsTester scenario (the drawables used to compare the canvas implementations) with the Cairo canvas.
	/// </summary>
	public class ScenarioTests
	{
		public static TheoryData<string> Scenarios =>
			new(ScenarioList.Scenarios.Select(s => s.ToString()));

		[Theory]
		[MemberData(nameof(Scenarios))]
		public void ScenarioRenders(string scenarioName)
		{
			var scenario = ScenarioList.Scenarios.Single(s => s.ToString() == scenarioName);

			using var s = new CairoTestSurface((int)scenario.Width, (int)scenario.Height);
			scenario.Draw(s.Canvas);
			s.Canvas.ResetState();

			Assert.True(s.PaintedBounds().HasValue, $"scenario '{scenarioName}' painted nothing");
		}
	}
}

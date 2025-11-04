using System.Collections.Generic;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class TemplatedPageUnitTests : BaseTestFixture
	{
		[Fact]
		public void TemplatedPage_should_have_the_InternalChildren_correctly_when_ControlTemplate_changed()
		{
			var sut = new TemplatedPage();
			var controlTemplated = (IControlTemplated)sut;
			var internalChildren = controlTemplated.InternalChildren;
			controlTemplated.AddLogicalChild(new VisualElement());
			controlTemplated.AddLogicalChild(new VisualElement());
			controlTemplated.AddLogicalChild(new VisualElement());

			sut.ControlTemplate = new ControlTemplate(typeof(ExpectedView));

			Assert.Single(internalChildren);
			Assert.IsType<ExpectedView>(internalChildren[0]);
		}

		private class ExpectedView : View
		{
			public ExpectedView()
			{
			}
		}
	}
}

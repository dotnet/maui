using System.Reflection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Foldable;
using Microsoft.Maui.Foldable;
using Xunit;

namespace Microsoft.Maui.Controls.Foldable.UnitTests
{
	public class WindowSpanModeStateTriggerTests
	{
		[Fact]
		public void UsesAttachedVisualElementInsteadOfGlobalCurrent()
		{
			var element = new Grid();
			var trigger = new WindowSpanModeStateTrigger
			{
				SpanMode = TwoPaneViewMode.Wide,
			};
			var state = new VisualState { Name = "Wide" };
			state.StateTriggers.Add(trigger);
			var group = new VisualStateGroup();
			group.States.Add(state);
			group.VisualElement = element;
			state.VisualStateGroup = group;
			trigger.VisualState = state;
			trigger.SendAttached();

			var infoField = typeof(WindowSpanModeStateTrigger).GetField("_info", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(infoField);
			var info = Assert.IsType<DualScreenInfo>(infoField.GetValue(trigger));
			Assert.Same(element, info.Element);
		}
	}
}

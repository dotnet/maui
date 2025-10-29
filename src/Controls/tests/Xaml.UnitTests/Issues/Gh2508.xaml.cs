using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Gh2508FlashingTriggerAction : TriggerAction<VisualElement>
	{
		public uint Duration { get; set; } = 1000;

		protected override void Invoke(VisualElement sender)
		{
			new Animation {
					{ 0.0, 0.5, new Animation(v => sender.Opacity = v)},
					{0.5, 1.0, new Animation(v => sender.Opacity = 1.0 - v) },
				}.Commit(sender, "FlashingAnimation", 16U, Duration, Easing.Linear, null, () => true);
		}
	}

	public partial class Gh2508 : ContentPage
	{
		public Gh2508()
		{
			InitializeComponent();
		}

		public Gh2508(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{

			[InlineData(true), InlineData(false)]
			public void UintProperties(bool useCompiledXaml)
			{
				var layout = new Gh2508(useCompiledXaml);
				Assert.Equal(2000, ((layout.entry.Triggers[0] as Trigger).EnterActions[0] as Gh2508FlashingTriggerAction).Duration);
			}
		}
	}
}

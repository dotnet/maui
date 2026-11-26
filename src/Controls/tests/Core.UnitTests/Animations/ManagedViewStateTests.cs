using System.Collections.Generic;
using Microsoft.Maui.Controls.Animations;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Animations
{
	public class ManagedViewStateTests : BaseTestFixture
	{
		sealed class RecordingHandler : ViewHandler<IView, object>
		{
			public RecordingHandler() : base(new PropertyMapper<IView, RecordingHandler>())
			{
			}

			public List<string> Updated { get; } = new();

			protected override object CreatePlatformView() => new object();

			public override void UpdateValue(string property)
			{
				Updated.Add(property);
				base.UpdateValue(property);
			}
		}

		[Fact]
		public void NothingChangedMapsNothingAgain()
		{
			var element = new BoxView { Opacity = 0.5, TranslationX = 3 };
			var handler = new RecordingHandler();
			element.Handler = handler;
			var state = ManagedViewState.Of(element);
			handler.Updated.Clear();

			state.ReapplyChanged();

			Assert.Empty(handler.Updated);
		}

		[Fact]
		public void ValuesChangedDuringATransitionAreMappedAgain()
		{
			// A transition puts back the native state it captured; whatever the element changed meanwhile must win.
			var element = new BoxView();
			var handler = new RecordingHandler();
			element.Handler = handler;
			var state = ManagedViewState.Of(element);
			element.Opacity = 0.25;
			element.Scale = 2;
			element.IsVisible = false;
			handler.Updated.Clear();

			state.ReapplyChanged();

			Assert.Equal(new[] { nameof(IView.Opacity), nameof(IView.Visibility), nameof(IView.Scale) }, handler.Updated);
			Assert.True(state.OpacityChanged);
			Assert.True(state.VisibilityChanged);
		}

		[Fact]
		public void NoElementReappliesNothing()
		{
			var state = ManagedViewState.Of(null);

			state.ReapplyChanged();

			Assert.Empty(state.ChangedProperties());
			Assert.False(state.OpacityChanged);
		}
	}
}

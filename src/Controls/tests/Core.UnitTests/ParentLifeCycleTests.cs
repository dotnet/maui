using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ParentLifeCycleTests : BaseTestFixture
	{
		[Fact]
		public void ChangingAndChangedBothFireForParentOverride()
		{
			LifeCycleButton button = new LifeCycleButton();
			bool changing = false;
			bool changed = false;

			button.Parent = new Button();

			button.ParentChanging += (_, __) => changing = true;
			button.ParentChanged += (_, __) =>
			{
				if (!changing)
					throw new XunitException("Attached fired before changing");

				changed = true;
			};

			Assert.False(changing);
			Assert.False(changed);

			button.ParentOverride = new Button();

			Assert.True(changing);
			Assert.True(changed);
		}

		[Fact]
		public void ChangingAndChangedBothFireInitially()
		{
			LifeCycleButton button = new LifeCycleButton();
			bool changing = false;
			bool changed = false;

			button.ParentChanging += (_, __) => changing = true;
			button.ParentChanged += (_, __) =>
			{
				if (!changing)
					throw new XunitException("Attached fired before changing");

				changed = true;
			};

			Assert.Equal(0, button.changing);
			Assert.Equal(0, button.changed);
			Assert.False(changing);
			Assert.False(changed);

			button.Parent = new Button();

			Assert.True(changing);
			Assert.True(changed);
			Assert.Equal(1, button.changing);
			Assert.Equal(1, button.changed);
		}

		[Fact]
		public void ParentOverrideChangingArgsSendCorrectly()
		{
			LifeCycleButton button = new LifeCycleButton();

			Assert.Null(button.Parent);
			var firstParent = new Button();
			button.Parent = firstParent;

			Assert.Equal(button.LastParentChangingEventArgs.NewParent, firstParent);
			Assert.Null(button.LastParentChangingEventArgs.OldParent);

			var secondParent = new Button();
			button.ParentOverride = secondParent;
			Assert.Equal(button.LastParentChangingEventArgs.OldParent, firstParent);
			Assert.Equal(button.LastParentChangingEventArgs.NewParent, secondParent);

			button.ParentOverride = null;
			Assert.Equal(button.LastParentChangingEventArgs.OldParent, secondParent);
			Assert.Equal(button.LastParentChangingEventArgs.NewParent, firstParent);
		}

		[Fact]
		public void ChangingArgsAreSetCorrectly()
		{
			LifeCycleButton button = new LifeCycleButton();

			Assert.Null(button.Parent);
			var firstParent = new Button();
			button.Parent = firstParent;

			Assert.Equal(button.LastParentChangingEventArgs.NewParent, firstParent);
			Assert.Null(button.LastParentChangingEventArgs.OldParent);

			var secondParent = new Button();
			button.Parent = secondParent;
			Assert.Equal(button.LastParentChangingEventArgs.OldParent, firstParent);
			Assert.Equal(button.LastParentChangingEventArgs.NewParent, secondParent);

			button.Parent = null;
			Assert.Equal(button.LastParentChangingEventArgs.OldParent, secondParent);
			Assert.Null(button.LastParentChangingEventArgs.NewParent);

			Assert.Equal(3, button.changing);
			Assert.Equal(3, button.changed);
		}

		public class LifeCycleButton : Button
		{
			public int changing = 0;
			public int changed = 0;
			public ParentChangingEventArgs LastParentChangingEventArgs { get; set; }

			protected override void OnParentChanging(ParentChangingEventArgs args)
			{
				LastParentChangingEventArgs = args;
				changing++;
				base.OnParentChanging(args);
			}

			protected override void OnParentChanged()
			{
				changed++;
				base.OnParentChanged();
			}
		}
	}
}

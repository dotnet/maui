using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class ParentLifeCycleTests : BaseTestFixture
	{
		[Test]
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
					Assert.Fail("Attached fired before changing");

				changed = true;
			};

			Assert.IsFalse(changing);
			Assert.IsFalse(changed);

			button.ParentOverride = new Button();

			Assert.IsTrue(changing);
			Assert.IsTrue(changed);
		}

		[Test]
		public void ChangingAndChangedBothFireInitially()
		{
			LifeCycleButton button = new LifeCycleButton();
			bool changing = false;
			bool changed = false;

			button.ParentChanging += (_, __) => changing = true;
			button.ParentChanged += (_, __) =>
			{
				if (!changing)
					Assert.Fail("Attached fired before changing");

				changed = true;
			};

			Assert.AreEqual(0, button.changing);
			Assert.AreEqual(0, button.changed);
			Assert.IsFalse(changing);
			Assert.IsFalse(changed);

			button.Parent = new Button();

			Assert.IsTrue(changing);
			Assert.IsTrue(changed);
			Assert.AreEqual(1, button.changing);
			Assert.AreEqual(1, button.changed);
		}

		[Test]
		public void ParentOverrideChangingArgsSendCorrectly()
		{
			LifeCycleButton button = new LifeCycleButton();

			Assert.IsNull(button.Parent);
			var firstParent = new Button();
			button.Parent = firstParent;

			Assert.AreEqual(button.LastParentChangingEventArgs.NewParent, firstParent);
			Assert.IsNull(button.LastParentChangingEventArgs.OldParent);

			var secondParent = new Button();
			button.ParentOverride = secondParent;
			Assert.AreEqual(button.LastParentChangingEventArgs.OldParent, firstParent);
			Assert.AreEqual(button.LastParentChangingEventArgs.NewParent, secondParent);

			button.ParentOverride = null;
			Assert.AreEqual(button.LastParentChangingEventArgs.OldParent, secondParent);
			Assert.AreEqual(button.LastParentChangingEventArgs.NewParent, firstParent);
		}

		[Test]
		public void ChangingArgsAreSetCorrectly()
		{
			LifeCycleButton button = new LifeCycleButton();

			Assert.IsNull(button.Parent);
			var firstParent = new Button();
			button.Parent = firstParent;

			Assert.AreEqual(button.LastParentChangingEventArgs.NewParent, firstParent);
			Assert.IsNull(button.LastParentChangingEventArgs.OldParent);

			var secondParent = new Button();
			button.Parent = secondParent;
			Assert.AreEqual(button.LastParentChangingEventArgs.OldParent, firstParent);
			Assert.AreEqual(button.LastParentChangingEventArgs.NewParent, secondParent);

			button.Parent = null;
			Assert.AreEqual(button.LastParentChangingEventArgs.OldParent, secondParent);
			Assert.AreEqual(button.LastParentChangingEventArgs.NewParent, null);

			Assert.AreEqual(3, button.changing);
			Assert.AreEqual(3, button.changed);
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

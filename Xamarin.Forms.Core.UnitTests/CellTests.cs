using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class CellTests : BaseTestFixture
	{
		internal class TestCell : Cell
		{
			public bool OnAppearingSent { get; set; }
			public bool OnDisappearingSent { get; set; }

			protected override void OnAppearing()
			{
				base.OnAppearing();
				OnAppearingSent = true;
			}

			protected override void OnDisappearing()
			{
				base.OnDisappearing();
				OnDisappearingSent = true;
			}
		}

		[Test]
		public void Selected()
		{
			var cell = new TestCell();

			bool tapped = false;
			cell.Tapped += (sender, args) => tapped = true;

			cell.OnTapped();
			Assert.IsTrue(tapped);
		}

		[Test]
		public void AppearingEvent()
		{
			var cell = new TestCell();

			bool emitted = false;
			cell.Appearing += (sender, args) => emitted = true;

			cell.SendAppearing();
			Assert.True(emitted);
			Assert.True(cell.OnAppearingSent);
			Assert.False(cell.OnDisappearingSent);
		}

		[Test]
		public void DisappearingEvent()
		{
			var cell = new TestCell();

			bool emitted = false;
			cell.Disappearing += (sender, args) => emitted = true;

			cell.SendDisappearing();
			Assert.True(emitted);
			Assert.False(cell.OnAppearingSent);
			Assert.True(cell.OnDisappearingSent);
		}

		[Test]
		public void TestBindingContextPropagationOnImageCell()
		{
			var context = new object();
			var cell = new ImageCell();
			cell.BindingContext = context;
			var source = new FileImageSource();
			cell.ImageSource = source;
			Assert.AreSame(context, source.BindingContext);

			cell = new ImageCell();
			source = new FileImageSource();
			cell.ImageSource = source;
			cell.BindingContext = context;
			Assert.AreSame(context, source.BindingContext);
		}

		[Test]
		public void HasContextActions()
		{
			bool changed = false;

			var cell = new TextCell();
			cell.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "HasContextActions")
					changed = true;
			};

			Assert.That(cell.HasContextActions, Is.False);
			Assert.That(changed, Is.False);

			var collection = cell.ContextActions;

			Assert.That(cell.HasContextActions, Is.False);
			Assert.That(changed, Is.False);

			collection.Add(new MenuItem());

			Assert.That(cell.HasContextActions, Is.True);
			Assert.That(changed, Is.True);
		}

		[Test]
		public void MenuItemsGetBindingContext()
		{
			var cell = new TextCell
			{
				ContextActions = {
					new MenuItem ()
				}
			};

			object bc = new object();

			cell.BindingContext = bc;
			Assert.That(cell.ContextActions[0].BindingContext, Is.SameAs(bc));

			cell = new TextCell { BindingContext = new object() };
			cell.ContextActions.Add(new MenuItem());

			Assert.That(cell.ContextActions[0].BindingContext, Is.SameAs(cell.BindingContext));
		}

		[Test]
		public void RenderHeightINPCFromParent()
		{
			var lv = new ListView();
			var cell = new TextCell();
			cell.Parent = lv;

			int changing = 0, changed = 0;
			cell.PropertyChanging += (sender, args) =>
			{
				if (args.PropertyName == "RenderHeight")
					changing++;
			};

			cell.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "RenderHeight")
					changed++;
			};

			lv.RowHeight = 5;

			Assume.That(cell.RenderHeight, Is.EqualTo(5));

			Assert.That(changing, Is.EqualTo(1));
			Assert.That(changed, Is.EqualTo(1));
		}

		[Test]
		public async Task ForceUpdateSizeCallsAreRateLimited()
		{
			var lv = new ListView { HasUnevenRows = true };
			var cell = new ViewCell { Parent = lv };

			int numberOfCalls = 0;
			cell.ForceUpdateSizeRequested += (object sender, System.EventArgs e) => { numberOfCalls++; };

			cell.ForceUpdateSize();
			cell.ForceUpdateSize();
			cell.ForceUpdateSize();
			cell.ForceUpdateSize();

			await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(150));

			Assert.AreEqual(1, numberOfCalls);
		}

		[Test]
		public async Task ForceUpdateSizeWillNotBeCalledIfParentIsNotAListViewWithUnevenRows()
		{
			var lv = new ListView { HasUnevenRows = false };
			var cell = new ViewCell { Parent = lv };

			int numberOfCalls = 0;
			cell.ForceUpdateSizeRequested += (object sender, System.EventArgs e) => { numberOfCalls++; };

			cell.ForceUpdateSize();

			await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(16));

			Assert.AreEqual(0, numberOfCalls);
		}
	}
}
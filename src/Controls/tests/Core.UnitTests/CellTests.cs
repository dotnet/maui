using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

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

		[Fact]
		public void Selected()
		{
			var cell = new TestCell();

			bool tapped = false;
			cell.Tapped += (sender, args) => tapped = true;

			cell.OnTapped();
			Assert.True(tapped);
		}

		[Fact]
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

		[Fact]
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

		[Fact]
		public void TestBindingContextPropagationOnImageCell()
		{
			var context = new object();
			var cell = new ImageCell();
			cell.BindingContext = context;
			var source = new FileImageSource();
			cell.ImageSource = source;
			Assert.Same(context, source.BindingContext);

			cell = new ImageCell();
			source = new FileImageSource();
			cell.ImageSource = source;
			cell.BindingContext = context;
			Assert.Same(context, source.BindingContext);
		}

		[Fact]
		public void HasContextActions()
		{
			bool changed = false;

			var cell = new TextCell();
			cell.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "HasContextActions")
					changed = true;
			};

			Assert.False(cell.HasContextActions);
			Assert.False(changed);

			var collection = cell.ContextActions;

			Assert.False(cell.HasContextActions);
			Assert.False(changed);

			collection.Add(new MenuItem());

			Assert.True(cell.HasContextActions);
			Assert.True(changed);
		}

		[Fact]
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
			Assert.Same(cell.ContextActions[0].BindingContext, bc);

			cell = new TextCell { BindingContext = new object() };
			cell.ContextActions.Add(new MenuItem());

			Assert.Same(cell.ContextActions[0].BindingContext, cell.BindingContext);
		}

		[Fact]
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

			Assert.Equal(5, cell.RenderHeight);

			Assert.Equal(1, changing);
			Assert.Equal(1, changed);
		}

		[Fact]
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

			Assert.Equal(1, numberOfCalls);
		}

		[Fact]
		public async Task ForceUpdateSizeWillNotBeCalledIfParentIsNotAListViewWithUnevenRows()
		{
			var lv = new ListView { HasUnevenRows = false };
			var cell = new ViewCell { Parent = lv };

			int numberOfCalls = 0;
			cell.ForceUpdateSizeRequested += (object sender, System.EventArgs e) => { numberOfCalls++; };

			cell.ForceUpdateSize();

			await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(16));

			Assert.Equal(0, numberOfCalls);
		}
	}
}

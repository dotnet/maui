using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Path)]
	public partial class PathTests
	{
		[Fact(DisplayName = "Update Path Data Test")]
		public async Task UpdatePathDataTest()
		{
			SetupBuilder();

			var layout = new StackLayout();

			var path = new Path()
			{
				HeightRequest = 300,
				WidthRequest = 300,
				Stroke = new SolidColorBrush(Colors.Red),
			};

			string pathData = "M 10,100 C 10,300 300,-200 300,100";
			var pathGeometry = new PathGeometry();
			var pathFigureCollectionConverter = new PathFigureCollectionConverter();
			PathFigureCollection figures = pathFigureCollectionConverter.ConvertFromInvariantString(pathData) as PathFigureCollection;
			pathGeometry.Figures = figures;
			path.Data = pathGeometry;

			var button = new Button()
			{
				Text = "Update Path Data"
			};

			layout.Add(path);
			layout.Add(button);

			var clicked = false;

			var pathGeometry2 = new PathGeometry();

			button.Clicked += delegate
			{
				string pathData = "M 10,100 C 10,400 400,-200 400,100";
				PathFigureCollection figures = pathFigureCollectionConverter.ConvertFromInvariantString(pathData) as PathFigureCollection;
				pathGeometry2.Figures = figures;
				path.Data = pathGeometry2;

				clicked = true;
			};

			await PerformClick(button);

			Assert.True(clicked);

			await InvokeOnMainThreadAsync(() =>
			path.ToPlatform(MauiContext).AttachAndRun(() =>
			{
				var platformView = (W2DGraphicsView)path.ToPlatform(MauiContext);
				Assert.NotNull(platformView);

				var shapeDrawable = (ShapeDrawable)platformView.Drawable;
				var shapeData = ((Path)shapeDrawable.ShapeView.Shape).Data;

				Assert.Equal(pathGeometry2, shapeData);
			}));
		}

		Task PerformClick(IButton button)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var platformButton = GetNativeButton(CreateHandler<ButtonHandler>(button));
				var ap = new ButtonAutomationPeer(platformButton);
				var ip = ap.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
				ip?.Invoke();
			});
		}

		UI.Xaml.Controls.Button GetNativeButton(ButtonHandler buttonHandler) =>
			buttonHandler.PlatformView;
	}
}
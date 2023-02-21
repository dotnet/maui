using System;
using GraphicsTester.Scenarios;
using Microsoft.Maui.Graphics.Skia.Views;
using Tizen.Applications;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.NUI.Components;

namespace GraphicsTester.Skia.Tizen
{
	class Program : NUIApplication
	{
		SimpleViewStack _stack;

		protected override void OnCreate()
		{
			base.OnCreate();
			FocusManager.Instance.EnableDefaultAlgorithm(true);
			Initialize();
			_stack.Push(CreateTesterView());
		}

		private void Initialize()
		{
			View.SetDefaultGrabTouchAfterLeave(true);
			Window.Instance.KeyEvent += OnKeyEvent;

			_stack = new SimpleViewStack
			{
				BackgroundColor = Color.White
			};
			Window.Instance.GetDefaultLayer().Add(_stack);

			Window.Instance.AddAvailableOrientation(Window.WindowOrientation.Landscape);
			Window.Instance.AddAvailableOrientation(Window.WindowOrientation.LandscapeInverse);
			Window.Instance.AddAvailableOrientation(Window.WindowOrientation.Portrait);
			Window.Instance.AddAvailableOrientation(Window.WindowOrientation.PortraitInverse);
		}

		private View CreateTesterView()
		{
			var layout = new View
			{
				HeightResizePolicy = ResizePolicyType.FillToParent,
				WidthResizePolicy = ResizePolicyType.FillToParent,
				Layout = new LinearLayout
				{
					LinearOrientation = LinearLayout.Orientation.Vertical,
				}
			};

			EventHandler<ClickedEventArgs> clicked = (object sender, ClickedEventArgs args) =>
			{
				var scenario = ((sender as View).BindingContext as AbstractScenario);
				var graphicsView = new SkiaGraphicsView
				{
					Drawable = scenario
				};
				_stack.Push(graphicsView);
			};


			var collectionview = new ScrollableBase
			{
				Layout = new LinearLayout
				{
					LinearOrientation = LinearLayout.Orientation.Vertical
				}
			};

			foreach (var scenario in ScenarioList.Scenarios)
			{
				var itemView = new DefaultLinearItem();

				itemView.Focusable = true;
				itemView.BindingContext = scenario;
				itemView.Clicked += clicked;

				var label = new TextLabel
				{
					VerticalAlignment = VerticalAlignment.Center,
					WidthSpecification = LayoutParamPolicies.MatchParent,
					HeightSpecification = LayoutParamPolicies.MatchParent,
				};

				label.PixelSize = 30;
				label.Text = scenario.ToString();

				itemView.Add(label);
				collectionview.Add(itemView);
			}
			layout.Add(collectionview);

			return layout;
		}

		private void OnKeyEvent(object sender, Window.KeyEventArgs e)
		{
			if (e.Key.State == Key.StateType.Down && (e.Key.KeyPressedName == "XF86Back" || e.Key.KeyPressedName == "Escape"))
			{
				if (_stack.Stack.Count > 1)
				{
					_stack.Pop();
				}
				else
				{
					Exit();
				}
			}
		}

		static void Main(string[] args)
		{
			var app = new Program();
			app.Run(args);
		}
	}
}

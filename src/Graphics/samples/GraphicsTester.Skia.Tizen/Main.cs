using System;
using ElmSharp;
using Tizen.Applications;
using GraphicsTester.Scenarios;
using Microsoft.Maui.Graphics.Skia.Views;

namespace GraphicsTester.Skia.Tizen
{
	class Program : CoreUIApplication
	{
		protected override void OnCreate()
		{
			base.OnCreate();
			CreateTesterView();
		}

		private void CreateTesterView()
		{
			Window window = new Window("GraphicsTester")
			{
				AvailableRotations = DisplayRotation.Degree_0 | DisplayRotation.Degree_180 | DisplayRotation.Degree_270 | DisplayRotation.Degree_90
			};
			window.Show();
			window.BackButtonPressed += (s, e) =>
			{
				EcoreMainloop.Quit();
			};

			Conformant conformant = new Conformant(window);
			conformant.Show();

			Naviframe navi = new Naviframe(window)
			{
				PreserveContentOnPop = true,
				DefaultBackButtonEnabled = true
			};
			navi.Show();

			GenList list = new GenList(window)
			{
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1
			};
			GenItemClass defaultClass = new GenItemClass("default")
			{
				GetTextHandler = (data, part) =>
				{
					var scenario = data as AbstractScenario;
					return scenario == null ? "" : scenario.GetType().Name;
				}
			};
			foreach (var scenario in ScenarioList.Scenarios)
			{
				list.Append(defaultClass, scenario);
			}

			SkiaGraphicsView graphicsView = new SkiaGraphicsView(window)
			{
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1,
				BackgroundColor = Color.White
			};
			graphicsView.Show();

			list.ItemSelected += (s, e) =>
			{
				var scenario = ScenarioList.Scenarios[e.Item.Index-1];
				graphicsView.Drawable = scenario;
				navi.Push(graphicsView, scenario.GetType().Name);
			};
			list.Show();
			navi.Push(list, "GraphicsTester.Skia.Tizen");
			conformant.SetContent(navi);
		}

		static void Main(string[] args)
		{
			var app = new Program();
			app.Run(args);
		}
	}
}

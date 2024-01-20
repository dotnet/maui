using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class ControlTemplateBenchmarker
	{
		private ContentView[] _contentViews;

		private View _content;
		private ControlTemplate _controlTemplate;

		private const int Iterations = 1000;

		[GlobalSetup]
		public void Setup()
		{			
			_content = new Frame();
			_controlTemplate = new ControlTemplate(typeof(Frame));

			_contentViews = [
								new Frame(),
								new SwipeView(),
								new ContentView(),
								new RefreshView(),
								new SwipeItemView(),
							];
		}

		[Benchmark]
		public void ChangeContent()
		{
			for (int i = 0; i < Iterations; i++)
			{
				foreach (var view in _contentViews)
				{
					view.Content = _content;
					view.Content = null;
				}
			}
		}

		[Benchmark]
		public void ChangeTemplate()
		{
			for (int i = 0; i < Iterations; i++)
			{
				foreach (var view in _contentViews)
				{
					view.ControlTemplate = _controlTemplate;
					view.ControlTemplate = null;
				}
			}
		}

		[Benchmark]
		public void ChangeTemplateWithContentPresenters()
		{
			var layout = new VerticalStackLayout
			{
				Children = {
					new ContentPresenter(),
					new ContentPresenter(),
					new ContentPresenter(),
					new ContentPresenter(),
					new ContentPresenter(),
					new ContentPresenter(),
					new ContentPresenter(),
					new ContentPresenter(),
					new ContentPresenter(),
					new ContentPresenter(),
					new ContentPresenter(),
				}
			};

			var contentView = new ContentView { Content = layout };

			for (int i = 0; i < Iterations; i++)
			{
				contentView.ControlTemplate = _controlTemplate;
				contentView.ControlTemplate = null;
			}
		}
	}
}

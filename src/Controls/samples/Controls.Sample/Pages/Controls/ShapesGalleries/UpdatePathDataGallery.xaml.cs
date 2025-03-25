using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Pages.ShapesGalleries
{
	public partial class UpdatePathDataGallery : ContentPage
	{
		int _counter;
		readonly PathFigureCollectionConverter _pathFigureCollectionConverter;

		public UpdatePathDataGallery()
		{
			InitializeComponent();

			_pathFigureCollectionConverter = new PathFigureCollectionConverter();
		}

		void OnUpdatePathDataButtonClicked(object sender, EventArgs args)
		{
			_counter += 10;
			string pathData = $"M 10,100 C 10,{300 + _counter} {300 + _counter},-200 {300 + _counter},100";

			var pathGeometry = new PathGeometry();
			var figures = _pathFigureCollectionConverter.ConvertFromInvariantString(pathData) as PathFigureCollection;
			pathGeometry.Figures = figures;

			Path.Data = pathGeometry;
		}
	}
}
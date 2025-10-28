using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Border : ContentPage
	{
		public Border() => InitializeComponent();

		public Border(bool useCompiledXaml)
		{
			// This stub will be replaced at compile time
		}		public class Tests
		{
			[InlineData(false)]
			[InlineData(true)]
			public void InitializeStrokeShape(bool useCompiledXaml)
			{
				var layout = new Border(useCompiledXaml);
				Assert.NotNull(layout.Border0.StrokeShape);
				Assert.NotNull(layout.Border1.StrokeShape);
				Assert.NotNull(layout.Border2.StrokeShape);
			}

			[InlineData(false)]
			[InlineData(true)]
			public void BindingToStrokeShapeWorks(bool useCompiledXaml)
			{
				DispatcherProvider.SetCurrent(new DispatcherProviderStub());
				var layout = new Border(useCompiledXaml);

				BorderViewModel viewModel = new();
				layout.BindingContext = viewModel;
				Assert.True(layout.BorderWithBinding.StrokeShape is RoundRectangle);
				Assert.Equal(4, ((RoundRectangle)layout.BorderWithBinding.StrokeShape).CornerRadius.TopLeft);

				viewModel.RoundedRect = new RoundRectangle() { CornerRadius = new CornerRadius(8) };
				Assert.Equal(8, ((RoundRectangle)layout.BorderWithBinding.StrokeShape).CornerRadius.TopLeft);
				DispatcherProvider.SetCurrent(null);
			}
		}
	}

	public class BorderViewModel : INotifyPropertyChanged
	{
		private RoundRectangle _roundedRect;
		public RoundRectangle RoundedRect
		{
			get => _roundedRect;
			set
			{
				_roundedRect = value;
				OnPropertyChanged();
			}
		}

		public BorderViewModel()
		{
			RoundedRect = new RoundRectangle() { CornerRadius = new CornerRadius(4) };
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string name = "") =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
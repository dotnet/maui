using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Border : ContentPage
{
	public Border() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void InitializeStrokeShape(XamlInflator inflator)
		{
			var layout = new Border(inflator);
			Assert.NotNull(layout.Border0.StrokeShape);
			Assert.NotNull(layout.Border1.StrokeShape);
			Assert.NotNull(layout.Border2.StrokeShape);
		}

		[Theory]
		[Values]
		public void BindingToStrokeShapeWorks(XamlInflator inflator)
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			var layout = new Border(inflator);

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
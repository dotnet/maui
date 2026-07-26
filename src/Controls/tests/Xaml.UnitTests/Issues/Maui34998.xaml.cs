using System;
using System.Windows.Input;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public interface Maui34998RunningCommand : ICommand
{
	bool IsRunning { get; }
}

public interface Maui34998AsyncCommand : Maui34998RunningCommand
{
}

public class Maui34998Command : Maui34998AsyncCommand
{
	public bool IsRunning { get; set; }

	public event EventHandler? CanExecuteChanged
	{
		add { }
		remove { }
	}

	public bool CanExecute(object? parameter) => true;

	public void Execute(object? parameter)
	{
	}
}

public class Maui34998ViewModel
{
	public Maui34998AsyncCommand FirstCommand { get; } = new Maui34998Command();
}

public partial class Maui34998 : ContentPage
{
	public Maui34998() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		const string SourceGenAdditionalSource = """
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace CommunityToolkit.Mvvm.Input
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class RelayCommandAttribute : Attribute
	{
	}

	public interface IRelayCommand : ICommand
	{
	}

	public interface IRunningCommand : IRelayCommand
	{
		bool IsRunning { get; }
	}

	public interface IAsyncRelayCommand : IRunningCommand
	{
	}
}

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlProcessing(XamlInflator.Runtime, true)]
	public partial class Maui34998 : ContentPage
	{
		public Maui34998() => InitializeComponent();
	}

	public partial class Maui34998ViewModel
	{
		[RelayCommand]
		Task FirstAsync() => Task.CompletedTask;
	}
}
""";

		[Theory]
		[XamlInflatorData]
		internal void SourceGenResolvesRelayCommandInterfaceBaseMembers(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(SourceGenAdditionalSource)
					.RunMauiSourceGenerator(typeof(Maui34998));

				Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIG2045");
				Assert.Contains("TypedBinding", result.GeneratedInitializeComponent(), StringComparison.Ordinal);
			}
			else
			{
				var page = new Maui34998(inflator);
				Assert.False(page.checkBox.IsEnabled);
			}
		}
	}
}

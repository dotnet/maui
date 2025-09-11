using PoolMath;
using PoolMath.Data;
using PoolMathApp.Xaml;

namespace Microsoft.Maui.ManualTests.Performance.CollectionViewPool;

public partial class PoolPage : ContentPage
{
	List<Log> Logs = new List<Log>();

	public PoolPage()
	{
		InitializeComponent();

		cv.ItemTemplate = new LogTemplateSelector();
	}

	public async Task LoadLogs()
	{

		using var stream = await FileSystem.OpenAppPackageFileAsync("logs.json");
		using var reader = new StreamReader(stream);


		var contents = reader.ReadToEnd();

		Logs = JsonUtil.Deserialize<List<Log>>(contents);



		cv.ItemsSource = Logs;
	}
}
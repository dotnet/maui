using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests
{

	[Category(TestCategory.TextInput)]
	public partial class TextInputTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<Editor, EditorHandler>();
					handlers.AddHandler<SearchBar, SearchBarHandler>();
				});
			});
		}
	}
}

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler
	{
		public static PropertyMapper<IEntry, EntryHandler> EntryMapper = new PropertyMapper<IEntry, EntryHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IEntry.Text)] = MapText,
			[nameof(IEntry.TextColor)] = MapTextColor,
			[nameof(IEntry.IsPassword)] = MapIsPassword
		};

		public EntryHandler() : base(EntryMapper)
		{

		}

		public EntryHandler(PropertyMapper mapper) : base(mapper ?? EntryMapper)
		{

		}
	}
}
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Controls
{
	public class BordelessEntry : Entry
	{
		public BordelessEntry()
		{
			BordelessEntryServiceBuilder.TryAddHandler<BordelessEntry, BordelessEntryHandler>();
		}
	}
}
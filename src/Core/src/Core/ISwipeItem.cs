namespace Microsoft.Maui
{
	public interface ISwipeItem : IElement
	{
		void OnInvoked();

		string AutomationId { get; }
	}
}

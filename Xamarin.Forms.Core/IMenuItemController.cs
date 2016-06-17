namespace Xamarin.Forms
{
	public interface IMenuItemController
	{
		bool IsEnabled { get; }
		string IsEnabledPropertyName { get; }

		void Activate();
	}
}

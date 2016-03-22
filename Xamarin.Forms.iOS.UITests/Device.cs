namespace Xamarin.Forms.UITests
{

	public enum DeviceType 
	{
		Phone,
		Tablet
	}

	public class Device 
	{
		public DeviceType Type { get; set; }
		public string IP { get; set; }

		public Device (DeviceType type, string ip)
		{
			Type = type;
			IP = ip;
		}
	}	
}

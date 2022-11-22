namespace Microsoft.Maui.Networking
{
	/// <summary>
	/// Describes the type of connection the device is using.
	/// </summary>
	public enum ConnectionProfile
	{
		/// <summary>Other unknown type of connection.</summary>
		Unknown = 0,

		/// <summary>The bluetooth data connection.</summary>
		Bluetooth = 1,

		/// <summary>The mobile/cellular data connection.</summary>
		Cellular = 2,

		/// <summary>The ethernet data connection.</summary>
		Ethernet = 3,

		/// <summary>The Wi-Fi data connection.</summary>
		WiFi = 4
	}

	/// <summary>
	/// Various states of the connection to the internet.
	/// </summary>
	public enum NetworkAccess
	{
		/// <summary>The state of the connectivity is not known.</summary>
		Unknown = 0,

		/// <summary>No connectivity.</summary>
		None = 1,

		/// <summary>Local network access only.</summary>
		Local = 2,

		/// <summary>Limited internet access.</summary>
		ConstrainedInternet = 3,

		/// <summary>Local and Internet access.</summary>
		Internet = 4
	}
}

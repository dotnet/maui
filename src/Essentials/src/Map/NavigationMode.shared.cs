namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// Represents various modes of navigation that can be passed to the operating system's Maps app.
	/// </summary>
	public enum NavigationMode
	{
		/// <summary>No navigation mode.</summary>
		None = 0,

		/// <summary>The default navigation mode on the platform.</summary>
		Default = 1,

		/// <summary>Bicycle route mode.</summary>
		Bicycling = 2,

		/// <summary>Car route mode.</summary>
		Driving = 3,

		/// <summary>Transit route mode.</summary>
		Transit = 4,

		/// <summary>Walking route mode.</summary>
		Walking = 5,
	}
}

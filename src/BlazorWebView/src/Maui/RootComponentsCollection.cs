using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components.Web;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// A collection of <see cref="RootComponent"/> items.
	/// </summary>
	public class RootComponentsCollection : ObservableCollection<RootComponent>, IJSComponentConfiguration
	{
		/// <summary>
		/// Initializes a new instance of <see cref="RootComponentsCollection"/>.
		/// </summary>
		/// <param name="jsComponents">Configuration to enable JS component support.</param>
		public RootComponentsCollection(JSComponentConfigurationStore jsComponents)
		{
			JSComponents = jsComponents;
		}

		/// <inheritdoc />
		public JSComponentConfigurationStore JSComponents { get; }
	}
}

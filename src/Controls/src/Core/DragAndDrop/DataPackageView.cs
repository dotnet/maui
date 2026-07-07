#nullable disable
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A read-only view of a <see cref="Controls.DataPackage"/>.
	/// </summary>
	public class DataPackageView
	{
		DataPackage DataPackage { get; }
		internal DataPackagePropertySetView PropertiesInternal { get; }

		/// <summary>
		/// Gets the read-only view of custom properties associated with this data package.
		/// </summary>
		public DataPackagePropertySetView Properties { get; }

		internal DataPackageView(DataPackage dataPackage)
		{
			_ = dataPackage ?? throw new global::System.ArgumentNullException(nameof(dataPackage));
			DataPackage = dataPackage;
			PropertiesInternal = new DataPackagePropertySetView(DataPackage.PropertiesInternal);
			Properties = new DataPackagePropertySetView(DataPackage.Properties);
		}

		/// <summary>
		/// Asynchronously gets the image data from this data package.
		/// </summary>
		/// <returns>A task that returns the <see cref="ImageSource"/> from the data package.</returns>
		public Task<ImageSource> GetImageAsync()
		{
			return Task.FromResult(DataPackage.Image);
		}

		/// <summary>
		/// Asynchronously gets the text data from this data package.
		/// </summary>
		/// <returns>A task that returns the text from the data package.</returns>
		public Task<string> GetTextAsync()
		{
			return Task.FromResult(DataPackage.Text);
		}
	}
}

#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Contains data being transferred during a drag and drop operation.
	/// </summary>
	public class DataPackage
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DataPackage"/> class.
		/// </summary>
		public DataPackage()
		{
			Properties = new DataPackagePropertySet();
			PropertiesInternal = new DataPackagePropertySet();
		}

		/// <summary>
		/// Gets the custom properties associated with this data package.
		/// </summary>
		public DataPackagePropertySet Properties { get; }
		internal DataPackagePropertySet PropertiesInternal { get; }

		/// <summary>
		/// Gets or sets the image data for this data package.
		/// </summary>
		public ImageSource Image { get; set; }

		/// <summary>
		/// Gets or sets the text data for this data package.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Gets a read-only view of this data package.
		/// </summary>
		public DataPackageView View => new DataPackageView(this.Clone());

		internal DataPackage Clone()
		{
			DataPackage dataPackage = new DataPackage()
			{
				Text = Text,
				Image = Image
			};

			foreach (var property in Properties)
				dataPackage.Properties.Add(property.Key, property.Value);

			foreach (var property in PropertiesInternal)
				dataPackage.PropertiesInternal.Add(property.Key, property.Value);

			return dataPackage;
		}
	}
}

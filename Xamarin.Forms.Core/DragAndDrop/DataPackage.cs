namespace Xamarin.Forms
{
	public class DataPackage
	{
		public DataPackage()
		{
			Properties = new DataPackagePropertySet();
			PropertiesInternal = new DataPackagePropertySet();
		}

		public DataPackagePropertySet Properties { get; }
		internal DataPackagePropertySet PropertiesInternal { get; }

		public ImageSource Image { get; set; }
		public string Text { get; set; }
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
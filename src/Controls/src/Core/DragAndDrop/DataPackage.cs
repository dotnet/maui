namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/DataPackage.xml" path="Type[@FullName='Microsoft.Maui.Controls.DataPackage']/Docs" />
	public class DataPackage
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/DataPackage.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public DataPackage()
		{
			Properties = new DataPackagePropertySet();
			PropertiesInternal = new DataPackagePropertySet();
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/DataPackage.xml" path="//Member[@MemberName='Properties']/Docs" />
		public DataPackagePropertySet Properties { get; }
		internal DataPackagePropertySet PropertiesInternal { get; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/DataPackage.xml" path="//Member[@MemberName='Image']/Docs" />
		public ImageSource Image { get; set; }
		/// <include file="../../../docs/Microsoft.Maui.Controls/DataPackage.xml" path="//Member[@MemberName='Text']/Docs" />
		public string Text { get; set; }
		/// <include file="../../../docs/Microsoft.Maui.Controls/DataPackage.xml" path="//Member[@MemberName='View']/Docs" />
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

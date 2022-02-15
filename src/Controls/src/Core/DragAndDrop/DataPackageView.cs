using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/DataPackageView.xml" path="Type[@FullName='Microsoft.Maui.Controls.DataPackageView']/Docs" />
	public class DataPackageView
	{
		DataPackage DataPackage { get; }
		internal DataPackagePropertySetView PropertiesInternal { get; }
		/// <include file="../../../docs/Microsoft.Maui.Controls/DataPackageView.xml" path="//Member[@MemberName='Properties']/Docs" />
		public DataPackagePropertySetView Properties { get; }

		internal DataPackageView(DataPackage dataPackage)
		{
			_ = dataPackage ?? throw new global::System.ArgumentNullException(nameof(dataPackage));
			DataPackage = dataPackage;
			PropertiesInternal = new DataPackagePropertySetView(DataPackage.PropertiesInternal);
			Properties = new DataPackagePropertySetView(DataPackage.Properties);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/DataPackageView.xml" path="//Member[@MemberName='GetImageAsync']/Docs" />
		public Task<ImageSource> GetImageAsync()
		{
			return Task.FromResult(DataPackage.Image);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/DataPackageView.xml" path="//Member[@MemberName='GetTextAsync']/Docs" />
		public Task<string> GetTextAsync()
		{
			return Task.FromResult(DataPackage.Text);
		}
	}
}

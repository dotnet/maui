using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;

namespace Xamarin.Forms.Build.Tasks
{
	public class FixedCreateCSharpManifestResourceName : CreateCSharpManifestResourceName
	{
		[Output]
		public ITaskItem[] ResourceFilesWithManifestResourceNames { get; set; }

		public override bool Execute()
		{
			var ret = base.Execute();

			ResourceFilesWithManifestResourceNames = new TaskItem[ResourceFiles.Length];

			for (var i = 0; i < ResourceFiles.Length; i++)
			{
				var copy = new TaskItem(ResourceFiles[i]);
				copy.SetMetadata("ManifestResourceName", ManifestResourceNames[i].ItemSpec);
				ResourceFilesWithManifestResourceNames[i] = copy;
			}
			return ret;
		}
	}
}
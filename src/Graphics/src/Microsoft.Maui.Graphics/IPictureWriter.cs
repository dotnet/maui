using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics
{
    public interface IPictureWriter
    {
        void Save(IPicture picture, Stream stream);
        Task SaveAsync(IPicture picture, Stream stream);
    }
}
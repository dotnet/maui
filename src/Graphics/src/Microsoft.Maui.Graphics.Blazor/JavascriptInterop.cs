using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Microsoft.Maui.Graphics.Blazor
{
    public static class JavascriptInterop
    {
        public static async Task<float> SetupCanvas(
            this IJSRuntime runtime,
            string id)
        {
            var t = await runtime.InvokeAsync<float>(
                "SystemDrawingInterop.SetupCanvas",
                id );

            return t;
        }
        
        public static Task<bool> PointIsInPath(
            this IJSRuntime runtime,
            PathF path,
            float x,
            float y)
        {
            var pathDefinition = path.ToDefinitionString();
            return PointIsInPath(runtime, pathDefinition, x, y);
        }
        
        public static async Task<bool> PointIsInPath(
            this IJSRuntime runtime,
            string path,
            float x,
            float y)
        {
            var b = await runtime.InvokeAsync<bool>(
                "SystemDrawingInterop.PointIsInPath",
                path,
                x,
                y);

            return b;
        }
    }
}
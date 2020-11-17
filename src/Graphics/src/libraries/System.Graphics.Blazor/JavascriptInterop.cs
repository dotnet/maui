using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace System.Graphics.Blazor
{
    public static class JavascriptInterop
    {
        public static Task<float> SetupCanvas(
            this IJSRuntime runtime,
            string id)
        {
            return runtime.InvokeAsync<float>(
                "EWGraphicsInterop.SetupCanvas",
                id );
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
        
        public static Task<bool> PointIsInPath(
            this IJSRuntime runtime,
            string path,
            float x,
            float y)
        {
            return runtime.InvokeAsync<bool>(
                "EWGraphicsInterop.PointIsInPath",
                path,
                x,
                y);
        }
    }
}
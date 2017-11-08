using System;

namespace Xamarin.Forms.Platform.GTK
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ExportImageSourceHandlerAttribute : HandlerAttribute
    {
        public ExportImageSourceHandlerAttribute(Type handler, Type target) : base(handler, target)
        {

        }
    }
}
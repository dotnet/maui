using System;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Microsoft.Maui.Graphics.Blazor
{
    public abstract class RenderingContext : IDisposable
    {
        private const string NamespacePrefix = "BlazorExtensions";
        private const string SetPropertyAction = "setProperty";
        private const string GetPropertyAction = "getProperty";
        private const string CallMethodAction = "call";
        private const string AddAction = "add";
        private const string RemoveAction = "remove";
        
        private readonly string _contextName;
        private readonly IJSInProcessRuntime _runtime;

        public ElementReference Canvas { get; }

        
        internal RenderingContext(CanvasComponentBase reference, string contextName, object parameters = null)
        {
            Canvas = reference.CanvasReference;
            _contextName = contextName;
            _runtime = (IJSInProcessRuntime)reference.JSRuntime;
            _runtime.Invoke<object>($"{NamespacePrefix}.{_contextName}.{AddAction}", Canvas, parameters);
        }

        protected void SetProperty(string property, object value)
        {
            _runtime.Invoke<object>($"{NamespacePrefix}.{_contextName}.{SetPropertyAction}", Canvas, property, value);
        }

        protected T GetProperty<T>(string property)
        {
            return _runtime.Invoke<T>($"{NamespacePrefix}.{_contextName}.{GetPropertyAction}", Canvas, property);
        }

        protected T CallMethod<T>(string method)
        {
            return _runtime.Invoke<T>($"{NamespacePrefix}.{_contextName}.{CallMethodAction}", Canvas, method);
        }

        protected T CallMethod<T>(string method, params object[] value)
        {
            return _runtime.Invoke<T>($"{NamespacePrefix}.{_contextName}.{CallMethodAction}", Canvas, method, value);
        }

        public void Dispose()
        {
            _runtime.Invoke<object>($"{NamespacePrefix}.{_contextName}.{RemoveAction}", Canvas);
        }
    }
}

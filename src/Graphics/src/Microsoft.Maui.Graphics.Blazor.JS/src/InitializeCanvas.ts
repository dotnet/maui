import { ContextManager } from './CanvasContextManager';

namespace Canvas {
  const blazorExtensions: string = 'BlazorExtensions';
  // define what this extension adds to the window object inside BlazorExtensions
  const extensionObject = {
    Canvas2d: new ContextManager("2d"),
    WebGL: new ContextManager("webgl")
  };

  export function initialize(): void {
    if (typeof window !== 'undefined' && !window[blazorExtensions]) {
      // when the library is loaded in a browser via a <script> element, make the
      // following APIs available in global scope for invocation from JS
      window[blazorExtensions] = {
        ...extensionObject
      };
    } else {
      window[blazorExtensions] = {
        ...window[blazorExtensions],
        ...extensionObject
      };
    }
  }
}

Canvas.initialize();

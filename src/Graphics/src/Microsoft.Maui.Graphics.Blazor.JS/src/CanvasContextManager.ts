export class ContextManager {
  private readonly contexts = new Map<string, any>();
  private readonly webGLObject = new Array<any>();
  private readonly contextName: string;
  private webGLContext = false;
  private readonly prototypes: any;
  private readonly webGLTypes = [
    WebGLBuffer, WebGLShader, WebGLProgram, WebGLFramebuffer, WebGLRenderbuffer, WebGLTexture, WebGLUniformLocation
  ];

  public constructor(contextName: string) {
    this.contextName = contextName;
    if (contextName === "2d")
      this.prototypes = CanvasRenderingContext2D.prototype;
    else if (contextName === "webgl" || contextName === "experimental-webgl") {
      this.prototypes = WebGLRenderingContext.prototype;
      this.webGLContext = true;
    } else
      throw new Error(`Invalid context name: ${contextName}`);
  }

  public add = (canvas: HTMLCanvasElement, parameters: any) => {
    if (!canvas) throw new Error('Invalid canvas.');
    if (this.contexts.get(canvas.id)) return;

    var context;
    if (parameters)
      context = canvas.getContext(this.contextName, parameters);
    else
      context = canvas.getContext(this.contextName);

    if (!context) throw new Error('Invalid context.');

    this.contexts.set(canvas.id, context);
  }

  public remove = (canvas: HTMLCanvasElement) => {
    this.contexts.delete(canvas.id);
  }

  public setProperty = (canvas: HTMLCanvasElement, property: string, value: any) => {
    const context = this.getContext(canvas);
    context[property] = this.deserialize(property, value);
  }

  public getProperty = (canvas: HTMLCanvasElement, property: string) => {
    const context = this.getContext(canvas);
    return this.serialize(context[property]);
  }

  public call = (canvas: HTMLCanvasElement, method: string, args: any) => {
    const context = this.getContext(canvas);
    return this.serialize(this.prototypes[method].apply(context, args != undefined ? args.map((value) => this.deserialize(method, value)) : []));
  }

  private getContext = (canvas: HTMLCanvasElement) => {
    if (!canvas) throw new Error('Invalid canvas.');

    const context = this.contexts.get(canvas.id);
    if (!context) throw new Error('Invalid context.');

    return context;
  }

  private deserialize = (method: string, object: any) => {
    if (!this.webGLContext) return object; //deserialization only needs to happen for webGL

    if (object.hasOwnProperty("webGLType") && object.hasOwnProperty("id")) {
      return (this.webGLObject[object["id"]]);
    } else if (Array.isArray(object) && !method.endsWith("v")) {
      return Int8Array.of(...(object as number[]));
    } else
      return object;
  }

  private serialize = (object: any) => {
    if (!this.webGLContext) return object; //serialization only needs to happen for webGL

    const type = this.webGLTypes.find((type) => object instanceof type);
    if (type != undefined) {
      const id = this.webGLObject.length;
      this.webGLObject.push(object);

      return {
        webGLType: type.name,
        id: id
      };
    } else
      return object;
  }
}

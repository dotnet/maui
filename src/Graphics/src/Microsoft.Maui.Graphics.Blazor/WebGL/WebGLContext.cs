using System;
using System.Runtime.InteropServices;

namespace Microsoft.Maui.Graphics.Blazor.WebGL
{
	public class WebGLContext : RenderingContext
	{
		#region Constants
		private const string CONTEXT_NAME = "WebGL";
		private const string CLEAR_COLOR = "clearColor";
		private const string CLEAR = "clear";
		private const string DRAWING_BUFFER_WIDTH = "drawingBufferWidth";
		private const string DRAWING_BUFFER_HEIGHT = "drawingBufferHeight";
		private const string GET_CONTEXT_ATTRIBUTES = "getContextAttributes";
		private const string IS_CONTEXT_LOST = "isContextLost";
		private const string SCISSOR = "scissor";
		private const string VIEWPORT = "viewport";
		private const string ACTIVE_TEXTURE = "activeTexture";
		private const string BLEND_COLOR = "blendColor";
		private const string BLEND_EQUATION = "blendEquation";
		private const string BLEND_EQUATION_SEPARATE = "blendEquationSeparate";
		private const string BLEND_FUNC = "blendFunc";
		private const string BLEND_FUNC_SEPARATE = "blendFuncSeparate";
		private const string CLEAR_DEPTH = "clearDepth";
		private const string CLEAR_STENCIL = "clearStencil";
		private const string COLOR_MASK = "colorMask";
		private const string CULL_FACE = "cullFace";
		private const string DEPTH_FUNC = "depthFunc";
		private const string DEPTH_MASK = "depthMask";
		private const string DEPTH_RANGE = "depthRange";
		private const string DISABLE = "disable";
		private const string ENABLE = "enable";
		private const string FRONT_FACE = "frontFace";
		private const string GET_PARAMETER = "getParameter";
		private const string GET_ERROR = "getError";
		private const string HINT = "hint";
		private const string IS_ENABLED = "isEnabled";
		private const string LINE_WIDTH = "lineWidth";
		private const string PIXEL_STORE_I = "pixelStorei";
		private const string POLYGON_OFFSET = "polygonOffset";
		private const string SAMPLE_COVERAGE = "sampleCoverage";
		private const string STENCIL_FUNC = "stencilFunc";
		private const string STENCIL_FUNC_SEPARATE = "stencilFuncSeparate";
		private const string STENCIL_MASK = "stencilMask";
		private const string STENCIL_MASK_SEPARATE = "stencilMaskSeparate";
		private const string STENCIL_OP = "stencilOp";
		private const string STENCIL_OP_SEPARATE = "stencilOpSeparate";
		private const string BIND_BUFFER = "bindBuffer";
		private const string BUFFER_DATA = "bufferData";
		private const string BUFFER_SUB_DATA = "bufferSubData";
		private const string CREATE_BUFFER = "createBuffer";
		private const string DELETE_BUFFER = "deleteBuffer";
		private const string GET_BUFFER_PARAMETER = "getBufferParameter";
		private const string IS_BUFFER = "isBuffer";
		private const string BIND_FRAMEBUFFER = "bindFramebuffer";
		private const string CHECK_FRAMEBUFFER_STATUS = "checkFramebufferStatus";
		private const string CREATE_FRAMEBUFFER = "createFramebuffer";
		private const string DELETE_FRAMEBUFFER = "deleteFramebuffer";
		private const string FRAMEBUFFER_RENDERBUFFER = "framebufferRenderbuffer";
		private const string FRAMEBUFFER_TEXTURE_2D = "framebufferTexture2D";
		private const string GET_FRAMEBUFFER_ATTACHMENT_PARAMETER = "getFramebufferAttachmentParameter";
		private const string IS_FRAMEBUFFER = "isFramebuffer";
		private const string READ_PIXELS = "readPixels";
		private const string BIND_RENDERBUFFER = "bindRenderbuffer";
		private const string CREATE_RENDERBUFFER = "createRenderbuffer";
		private const string DELETE_RENDERBUFFER = "deleteRenderbuffer";
		private const string GET_RENDERBUFFER_PARAMETER = "getRenderbufferParameter";
		private const string IS_RENDERBUFFER = "isRenderbuffer";
		private const string RENDERBUFFER_STORAGE = "renderbufferStorage";
		private const string BIND_TEXTURE = "bindTexture";
		private const string COPY_TEX_IMAGE_2D = "copyTexImage2D";
		private const string COPY_TEX_SUB_IMAGE_2D = "copyTexSubImage2D";
		private const string CREATE_TEXTURE = "createTexture";
		private const string DELETE_TEXTURE = "deleteTexture";
		private const string GENERATE_MIPMAP = "generateMipmap";
		private const string GET_TEX_PARAMETER = "getTexParameter";
		private const string IS_TEXTURE = "isTexture";
		private const string TEX_IMAGE_2D = "texImage2D";
		private const string TEX_SUB_IMAGE_2D = "texSubImage2D";
		private const string TEX_PARAMETER_F = "texParameterf";
		private const string TEX_PARAMETER_I = "texParameteri";
		private const string ATTACH_SHADER = "attachShader";
		private const string BIND_ATTRIB_LOCATION = "bindAttribLocation";
		private const string COMPILE_SHADER = "compileShader";
		private const string CREATE_PROGRAM = "createProgram";
		private const string CREATE_SHADER = "createShader";
		private const string DELETE_PROGRAM = "deleteProgram";
		private const string DELETE_SHADER = "deleteShader";
		private const string DETACH_SHADER = "detachShader";
		private const string GET_ATTACHED_SHADERS = "getAttachedShaders";
		private const string GET_PROGRAM_PARAMETER = "getProgramParameter";
		private const string GET_PROGRAM_INFO_LOG = "getProgramInfoLog";
		private const string GET_SHADER_PARAMETER = "getShaderParameter";
		private const string GET_SHADER_PRECISION_FORMAT = "getShaderPrecisionFormat";
		private const string GET_SHADER_INFO_LOG = "getShaderInfoLog";
		private const string GET_SHADER_SOURCE = "getShaderSource";
		private const string IS_PROGRAM = "isProgram";
		private const string IS_SHADER = "isShader";
		private const string LINK_PROGRAM = "linkProgram";
		private const string SHADER_SOURCE = "shaderSource";
		private const string USE_PROGRAM = "useProgram";
		private const string VALIDATE_PROGRAM = "validateProgram";
		private const string DISABLE_VERTEX_ATTRIB_ARRAY = "disableVertexAttribArray";
		private const string ENABLE_VERTEX_ATTRIB_ARRAY = "enableVertexAttribArray";
		private const string GET_ACTIVE_ATTRIB = "getActiveAttrib";
		private const string GET_ACTIVE_UNIFORM = "getActiveUniform";
		private const string GET_ATTRIB_LOCATION = "getAttribLocation";
		private const string GET_UNIFORM = "getUniform";
		private const string GET_UNIFORM_LOCATION = "getUniformLocation";
		private const string GET_VERTEX_ATTRIB = "getVertexAttrib";
		private const string GET_VERTEX_ATTRIB_OFFSET = "getVertexAttribOffset";
		private const string UNIFORM = "uniform";
		private const string UNIFORM_MATRIX = "uniformMatrix";
		private const string VERTEX_ATTRIB = "vertexAttrib";
		private const string VERTEX_ATTRIB_POINTER = "vertexAttribPointer";
		private const string DRAW_ARRAYS = "drawArrays";
		private const string DRAW_ELEMENTS = "drawElements";
		private const string FINISH = "finish";
		private const string FLUSH = "flush";
		#endregion

		#region Properties
		public int DrawingBufferWidth => this.GetProperty<int>(DRAWING_BUFFER_WIDTH);
		public int DrawingBufferHeight => this.GetProperty<int>(DRAWING_BUFFER_HEIGHT);
		#endregion

		internal WebGLContext(CanvasComponentBase reference, WebGLContextAttributes attributes = null) : base(reference, CONTEXT_NAME, attributes)
		{
		}

		#region Methods
		public void ClearColor(float red, float green, float blue, float alpha) => this.CallMethod<object>(CLEAR_COLOR, red, green, blue, alpha);
		public void Clear(BufferBits mask) => this.CallMethod<object>(CLEAR, mask);
		public WebGLContextAttributes GetContextAttributes() => this.CallMethod<WebGLContextAttributes>(GET_CONTEXT_ATTRIBUTES);
		public bool IsContextLost() => this.CallMethod<bool>(IS_CONTEXT_LOST);
		public void Scissor(int x, int y, int width, int height) => this.CallMethod<object>(SCISSOR, x, y, width, height);
		public void Viewport(int x, int y, int width, int height) => this.CallMethod<object>(VIEWPORT, x, y, width, height);
		public void ActiveTexture(Texture texture) => this.CallMethod<object>(ACTIVE_TEXTURE, texture);
		public void BlendColor(float red, float green, float blue, float alpha) => this.CallMethod<object>(BLEND_COLOR, red, green, blue, alpha);
		public void BlendEquation(BlendingEquation equation) => this.CallMethod<object>(BLEND_EQUATION, equation);
		public void BlendEquationSeparate(BlendingEquation modeRGB, BlendingEquation modeAlpha) => this.CallMethod<object>(BLEND_EQUATION_SEPARATE, modeRGB, modeAlpha);
		public void BlendFunc(BlendingMode sfactor, BlendingMode dfactor) => this.CallMethod<object>(BLEND_FUNC, sfactor, dfactor);
		public void BlendFuncSeparate(BlendingMode srcRGB, BlendingMode dstRGB, BlendingMode srcAlpha, BlendingMode dstAlpha) => this.CallMethod<object>(BLEND_FUNC_SEPARATE, srcRGB, dstRGB, srcAlpha, dstAlpha);
		public void ClearDepth(float depth) => this.CallMethod<object>(CLEAR_DEPTH, depth);
		public void ClearStencil(int stencil) => this.CallMethod<object>(CLEAR_STENCIL, stencil);
		public void ColorMask(bool red, bool green, bool blue, bool alpha) => this.CallMethod<object>(COLOR_MASK, red, green, blue, alpha);
		public void CullFace(Face mode) => this.CallMethod<object>(CULL_FACE, mode);
		public void DepthFunc(CompareFunction func) => this.CallMethod<object>(DEPTH_FUNC, func);
		public void DepthMask(bool flag) => this.CallMethod<object>(DEPTH_MASK, flag);
		public void DepthRange(float zNear, float zFar) => this.CallMethod<object>(DEPTH_RANGE, zNear, zFar);
		public void Disable(EnableCap cap) => this.CallMethod<object>(DISABLE, cap);
		public void Enable(EnableCap cap) => this.CallMethod<object>(ENABLE, cap);
		public void FrontFace(FrontFaceDirection mode) => this.CallMethod<object>(FRONT_FACE, mode);
		public T GetParameter<T>(Parameter parameter) => this.CallMethod<T>(GET_PARAMETER, parameter);
		public Error GetError() => this.CallMethod<Error>(GET_ERROR);
		public void Hint(HintTarget target, HintMode mode) => this.CallMethod<object>(HINT, target, mode);
		public bool IsEnabled(EnableCap cap) => this.CallMethod<bool>(IS_ENABLED, cap);
		public bool LineWidth(float width) => this.CallMethod<bool>(LINE_WIDTH, width);
		public bool PixelStoreI(PixelStorageMode pname, int param) => this.CallMethod<bool>(PIXEL_STORE_I, pname, param);
		public void PolygonOffset(float factor, float units) => this.CallMethod<object>(POLYGON_OFFSET, factor, units);
		public void SampleCoverage(float value, bool invert) => this.CallMethod<object>(SAMPLE_COVERAGE, value, invert);
		public void StencilFunc(CompareFunction func, int reference, uint mask) => this.CallMethod<object>(STENCIL_FUNC, func, reference, mask);
		public void StencilFuncSeparate(Face face, CompareFunction func, int reference, uint mask) => this.CallMethod<object>(STENCIL_FUNC_SEPARATE, face, func, reference, mask);
		public void StencilMask(uint mask) => this.CallMethod<object>(STENCIL_MASK, mask);
		public void StencilMaskSeparate(Face face, uint mask) => this.CallMethod<object>(STENCIL_MASK_SEPARATE, face, mask);
		public void StencilOp(StencilFunction fail, StencilFunction zfail, StencilFunction zpass) => this.CallMethod<object>(STENCIL_OP, fail, zfail, zpass);
		public void StencilOpSeparate(Face face, StencilFunction fail, StencilFunction zfail, StencilFunction zpass) => this.CallMethod<object>(STENCIL_OP_SEPARATE, face, fail, zfail, zpass);

		public void BindBuffer(BufferType target, WebGLBuffer buffer) => this.CallMethod<object>(BIND_BUFFER, target, buffer);
		public void BufferData(BufferType target, int size, BufferUsageHint usage) => this.CallMethod<object>(BUFFER_DATA, target, size, usage);
		public void BufferData<T>(BufferType target, T[] data, BufferUsageHint usage) => this.CallMethod<object>(BUFFER_DATA, target, this.ConvertToByteArray(data), usage);
		public void BufferSubData<T>(BufferType target, uint offset, T[] data) => this.CallMethod<object>(BUFFER_SUB_DATA, target, offset, this.ConvertToByteArray(data));
		public WebGLBuffer CreateBuffer() => this.CallMethod<WebGLBuffer>(CREATE_BUFFER);
		public void DeleteBuffer(WebGLBuffer buffer) => this.CallMethod<WebGLBuffer>(DELETE_BUFFER, buffer);
		public T GetBufferParameter<T>(BufferType target, BufferParameter pname) => this.CallMethod<T>(GET_BUFFER_PARAMETER, target, pname);
		public bool IsBuffer(WebGLBuffer buffer) => this.CallMethod<bool>(IS_BUFFER, buffer);

		public void BindFramebuffer(FramebufferType target, WebGLFramebuffer framebuffer) => this.CallMethod<object>(BIND_FRAMEBUFFER, target, framebuffer);
		public FramebufferStatus CheckFramebufferStatus(FramebufferType target) => this.CallMethod<FramebufferStatus>(CHECK_FRAMEBUFFER_STATUS, target);
		public WebGLFramebuffer CreateFramebuffer() => this.CallMethod<WebGLFramebuffer>(CREATE_FRAMEBUFFER);
		public void DeleteFramebuffer(WebGLFramebuffer buffer) => this.CallMethod<object>(DELETE_FRAMEBUFFER, buffer);
		public void FramebufferRenderbuffer(FramebufferType target, FramebufferAttachment attachment, RenderbufferType renderbuffertarget, WebGLRenderbuffer renderbuffer) => this.CallMethod<object>(FRAMEBUFFER_RENDERBUFFER, target, attachment, renderbuffertarget, renderbuffer);
		public void FramebufferTexture2D(FramebufferType target, FramebufferAttachment attachment, Texture2DType textarget, WebGLTexture texture, int level) => this.CallMethod<object>(FRAMEBUFFER_TEXTURE_2D, target, attachment, textarget, texture, level);
		public T GetFramebufferAttachmentParameter<T>(FramebufferType target, FramebufferAttachment attachment, FramebufferAttachmentParameter pname) => this.CallMethod<T>(GET_FRAMEBUFFER_ATTACHMENT_PARAMETER, target, attachment, pname);
		public bool IsFramebuffer(WebGLFramebuffer framebuffer) => this.CallMethod<bool>(IS_FRAMEBUFFER, framebuffer);
		public void ReadPixels(int x, int y, int width, int height, PixelFormat format, PixelType type, byte[] pixels) => this.CallMethod<object>(READ_PIXELS, x, y, width, height, format, type, pixels); //pixels should be an ArrayBufferView which the data gets read into

		public void BindRenderbuffer(RenderbufferType target, WebGLRenderbuffer renderbuffer) => this.CallMethod<object>(BIND_RENDERBUFFER, target, renderbuffer);
		public WebGLRenderbuffer CreateRenderbuffer() => this.CallMethod<WebGLRenderbuffer>(CREATE_RENDERBUFFER);
		public void DeleteRenderbuffer(WebGLRenderbuffer buffer) => this.CallMethod<object>(DELETE_RENDERBUFFER, buffer);
		public T GetRenderbufferParameter<T>(RenderbufferType target, RenderbufferParameter pname) => this.CallMethod<T>(GET_RENDERBUFFER_PARAMETER, target, pname);
		public bool IsRenderbuffer(WebGLRenderbuffer renderbuffer) => this.CallMethod<bool>(IS_RENDERBUFFER, renderbuffer);
		public void RenderbufferStorage(RenderbufferType type, RenderbufferFormat internalFormat, int width, int height) => this.CallMethod<object>(RENDERBUFFER_STORAGE, type, internalFormat, width, height);

		public void BindTexture(TextureType type, WebGLTexture texture) => this.CallMethod<object>(BIND_TEXTURE, type, texture);
		public void CopyTexImage2D(Texture2DType target, int level, PixelFormat format, int x, int y, int width, int height, int border) => this.CallMethod<object>(COPY_TEX_IMAGE_2D, target, level, format, x, y, width, height, border);
		public void CopyTexSubImage2D(Texture2DType target, int level, int xoffset, int yoffset, int x, int y, int width, int height) => this.CallMethod<object>(COPY_TEX_SUB_IMAGE_2D, target, level, xoffset, yoffset, x, y, width, height);
		public WebGLTexture CreateTexture() => this.CallMethod<WebGLTexture>(CREATE_TEXTURE);
		public void DeleteTexture(WebGLTexture texture) => this.CallMethod<object>(DELETE_TEXTURE, texture);
		public void GenerateMipmap(TextureType target) => this.CallMethod<object>(GENERATE_MIPMAP, target);
		public T GetTexParameter<T>(TextureType target, TextureParameter pname) => this.CallMethod<T>(GET_TEX_PARAMETER, target, pname);
		public bool IsTexture(WebGLTexture texture) => this.CallMethod<bool>(IS_TEXTURE, texture);
		public void TexImage2D<T>(Texture2DType target, int level, PixelFormat internalFormat, int width, int height, PixelFormat format, PixelType type, T[] pixels)
			where T : struct
			=> this.CallMethod<object>(TEX_IMAGE_2D, target, level, internalFormat, width, height, format, type, pixels);
		public void TexSubImage2D<T>(Texture2DType target, int level, int xoffset, int yoffset, int width, int height, PixelFormat format, PixelType type, T[] pixels)
			where T : struct
			=> this.CallMethod<object>(TEX_SUB_IMAGE_2D, target, level, xoffset, yoffset, width, height, format, type, pixels);
		public void TexParameter(TextureType target, TextureParameter pname, float param) => this.CallMethod<object>(TEX_PARAMETER_F, target, pname, param);
		public void TexParameter(TextureType target, TextureParameter pname, int param) => this.CallMethod<object>(TEX_PARAMETER_I, target, pname, param);

		public void AttachShader(WebGLProgram program, WebGLShader shader) => this.CallMethod<object>(ATTACH_SHADER, program, shader);
		public void BindAttribLocation(WebGLProgram program, uint index, string name) => this.CallMethod<object>(BIND_ATTRIB_LOCATION, program, index, name);
		public void CompileShader(WebGLShader shader) => this.CallMethod<object>(COMPILE_SHADER, shader);
		public WebGLProgram CreateProgram() => this.CallMethod<WebGLProgram>(CREATE_PROGRAM);
		public WebGLShader CreateShader(ShaderType type) => this.CallMethod<WebGLShader>(CREATE_SHADER, type);
		public void DeleteProgram(WebGLProgram program) => this.CallMethod<object>(DELETE_PROGRAM, program);
		public void DeleteShader(WebGLShader shader) => this.CallMethod<object>(DELETE_SHADER, shader);
		public void DetachShader(WebGLProgram program, WebGLShader shader) => this.CallMethod<object>(DETACH_SHADER, program, shader);
		public WebGLShader[] GetAttachedShaders(WebGLProgram program) => this.CallMethod<WebGLShader[]>(GET_ATTACHED_SHADERS, program);
		public T GetProgramParameter<T>(WebGLProgram program, ProgramParameter pname) => this.CallMethod<T>(GET_PROGRAM_PARAMETER, program, pname);
		public string GetProgramInfoLog(WebGLProgram program) => this.CallMethod<string>(GET_PROGRAM_INFO_LOG, program);
		public T GetShaderParameter<T>(WebGLShader shader, ShaderParameter pname) => this.CallMethod<T>(GET_SHADER_PARAMETER, shader, pname);
		public WebGLShaderPrecisionFormat GetShaderPrecisionFormat(ShaderType shaderType, ShaderPrecision precisionType) => this.CallMethod<WebGLShaderPrecisionFormat>(GET_SHADER_PRECISION_FORMAT, shaderType, precisionType);
		public string GetShaderInfoLog(WebGLShader shader) => this.CallMethod<string>(GET_SHADER_INFO_LOG, shader);
		public string GetShaderSource(WebGLShader shader) => this.CallMethod<string>(GET_SHADER_SOURCE, shader);
		public bool IsProgram(WebGLProgram program) => this.CallMethod<bool>(IS_PROGRAM, program);
		public bool IsShader(WebGLShader shader) => this.CallMethod<bool>(IS_SHADER, shader);
		public void LinkProgram(WebGLProgram program) => this.CallMethod<object>(LINK_PROGRAM, program);
		public void ShaderSource(WebGLShader shader, string source) => this.CallMethod<object>(SHADER_SOURCE, shader, source);
		public void UseProgram(WebGLProgram program) => this.CallMethod<object>(USE_PROGRAM, program);
		public void ValidateProgram(WebGLProgram program) => this.CallMethod<object>(VALIDATE_PROGRAM, program);

		public void DisableVertexAttribArray(uint index) => this.CallMethod<object>(DISABLE_VERTEX_ATTRIB_ARRAY, index);
		public void EnableVertexAttribArray(uint index) => this.CallMethod<object>(ENABLE_VERTEX_ATTRIB_ARRAY, index);
		public WebGLActiveInfo GetActiveAttrib(WebGLProgram program, uint index) => this.CallMethod<WebGLActiveInfo>(GET_ACTIVE_ATTRIB, program, index);
		public WebGLActiveInfo GetActiveUniform(WebGLProgram program, uint index) => this.CallMethod<WebGLActiveInfo>(GET_ACTIVE_UNIFORM, program, index);
		public int GetAttribLocation(WebGLProgram program, string name) => this.CallMethod<int>(GET_ATTRIB_LOCATION, program, name);
		public T GetUniform<T>(WebGLProgram program, WebGLUniformLocation location) => this.CallMethod<T>(GET_UNIFORM, program, location);
		public WebGLUniformLocation GetUniformLocation(WebGLProgram program, string name) => this.CallMethod<WebGLUniformLocation>(GET_UNIFORM_LOCATION, program, name);
		public T GetVertexAttrib<T>(uint index, VertexAttribute pname) => this.CallMethod<T>(GET_VERTEX_ATTRIB, index, pname);
		public long GetVertexAttribOffset(uint index, VertexAttributePointer pname) => this.CallMethod<long>(GET_VERTEX_ATTRIB_OFFSET, index, pname);
		public void VertexAttribPointer(uint index, int size, DataType type, bool normalized, int stride, long offset) => this.CallMethod<object>(VERTEX_ATTRIB_POINTER, index, size, type, normalized, stride, offset);

		public void Uniform(WebGLUniformLocation location, params float[] value)
		{
			switch (value.Length)
			{
				case 1:
					this.CallMethod<object>(UNIFORM + "1fv", location, value);
					break;
				case 2:
					this.CallMethod<object>(UNIFORM + "2fv", location, value);
					break;
				case 3:
					this.CallMethod<object>(UNIFORM + "3fv", location, value);
					break;
				case 4:
					this.CallMethod<object>(UNIFORM + "4fv", location, value);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(value), value.Length, "Value array is empty or too long");
			}
		}

		public void Uniform(WebGLUniformLocation location, params int[] value)
		{
			switch (value.Length)
			{
				case 1:
					this.CallMethod<object>(UNIFORM + "1iv", location, value);
					break;
				case 2:
					this.CallMethod<object>(UNIFORM + "2iv", location, value);
					break;
				case 3:
					this.CallMethod<object>(UNIFORM + "3iv", location, value);
					break;
				case 4:
					this.CallMethod<object>(UNIFORM + "4iv", location, value);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(value), value.Length, "Value array is empty or too long");
			}
		}

		public void UniformMatrix(WebGLUniformLocation location, bool transpose, float[] value)
		{
			switch (value.Length)
			{
				case 2 * 2:
					this.CallMethod<object>(UNIFORM_MATRIX + "2fv", location, transpose, value);
					break;
				case 3 * 3:
					this.CallMethod<object>(UNIFORM_MATRIX + "3fv", location, transpose, value);
					break;
				case 4 * 4:
					this.CallMethod<object>(UNIFORM_MATRIX + "4fv", location, transpose, value);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(value), value.Length, "Value array has incorrect size");
			}
		}

		public void VertexAttrib(uint index, params float[] value)
		{
			switch (value.Length)
			{
				case 1:
					this.CallMethod<object>(VERTEX_ATTRIB + "1fv", index, value);
					break;
				case 2:
					this.CallMethod<object>(VERTEX_ATTRIB + "2fv", index, value);
					break;
				case 3:
					this.CallMethod<object>(VERTEX_ATTRIB + "3fv", index, value);
					break;
				case 4:
					this.CallMethod<object>(VERTEX_ATTRIB + "4fv", index, value);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(value), value.Length, "Value array is empty or too long");
			}
		}

		public void DrawArrays(Primitive mode, int first, int count) => this.CallMethod<object>(DRAW_ARRAYS, mode, first, count);
		public void DrawElements(Primitive mode, int count, DataType type, long offset) => this.CallMethod<object>(DRAW_ELEMENTS, mode, count, type, offset);
		public void Finish() => this.CallMethod<object>(FINISH);
		public void Flush() => this.CallMethod<object>(FLUSH);

		private byte[] ConvertToByteArray<T>(T[] arr)
		{
			byte[] byteArr = new byte[arr.Length * Marshal.SizeOf<T>()];
			Buffer.BlockCopy(arr, 0, byteArr, 0, byteArr.Length);
			return byteArr;
		}
		#endregion
	}
}

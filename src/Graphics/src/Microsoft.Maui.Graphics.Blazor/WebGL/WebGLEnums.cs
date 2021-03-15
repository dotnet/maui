namespace Microsoft.Maui.Graphics.Blazor.WebGL
{
    public enum BufferBits
    {
        COLOR_BUFFER_BIT = 0x4000,
        DEPTH_BUFFER_BIT = 0x100,
        STENCIL_BUFFER_BIT = 0x400
    }

    public enum Primitive
    {
        POINTS = 0,
        LINES = 1,
        LINE_LOOP = 2,
        LINE_STRIP = 3,
        TRIANGLES = 4,
        TRINAGLE_STRIP = 5,
        TRIANGLE_FAN = 6
    }

    public enum BlendingMode
    {
        ZERO = 0,
        ONE = 1,
        SRC_COLOR = 0x300,
        ONE_MINUS_SRC_COLOR = 0x301,
        SRC_ALPHA = 0x302,
        ONE_MINUS_SRC_ALPHA = 0x303,
        DST_ALPHA = 0x304,
        ONE_MINUS_DST_ALPHA = 0x305,
        DST_COLOR = 0x306,
        ONE_MINUS_DST_COLOR = 0x307,
        SRC_ALPHA_SATURATE = 0x308,
        CONSTANT_COLOR = 0x8001,
        ONE_MINUS_CONSTANT_COLOR = 0x8002,
        CONSTANT_ALPHA = 0x8003,
        ONE_MINUS_CONSTANT_ALPHA = 0x8004
    }

    public enum BlendingEquation
    {
        FUNC_ADD = 0x8006,
        FUNC_SUBTRACT = 0x800A,
        FUNC_REVERSE_SUBTRACT = 0x800B
    }

    public enum Parameter
    {
        BLEND_EQUATION = 0x8009,
        BLEND_EQUATION_RGB = 0x8009,
        BLEND_EQUATION_ALPHA = 0x883D,
        BLEND_DST_RGB = 0x80C8,
        BLEND_SRC_RBG = 0x80C9,
        BLEND_DST_ALPHA = 0x80CA,
        BLEND_SRC_ALPHA = 0x80CB,
        BLEND_COLOR = 0x8005,
        ARRAY_BUFFER_BINDING = 0x8894,
        ELEMENT_ARRAY_BUFFER_BINDING = 0x8895,
        LINE_WIDTH = 0x0B21,
        ALIASED_POINT_SIZE_RANGE = 0x846D,
        ALIASED_LINE_WIDTH_RANGE = 0x846E,
        CULL_FACE_MODE = 0x0B45,
        FRONT_FACE = 0x0B46,
        DEPTH_RANGE = 0x0B70,
        DEPTH_WRITEMASK = 0x0B72,
        DEPTH_CLEAR_VALUE = 0x0B73,
        DEPTH_FUNC = 0x0B74,
        STENCIL_CLEAR_VALUE = 0x0B91,
        STENCIL_FUNC = 0x0B92,
        STENCIL_FAIL = 0x0B94,
        STENCIL_PASS_DEPTH_FAIL = 0x0B95,
        STENCIL_PASS_DEPTH_PASS = 0x0B96,
        STENCIL_REF = 0x0B97,
        STENCIL_VALUE_MASK = 0x0B93,
        STENCIL_WRITEMASK = 0x0B98,
        STENCIL_BACK_FUNC = 0x8800,
        STENCIL_BACK_FAIL = 0x8801,
        STENCIL_BACK_PASS_DEPTH_FAIL = 0x8802,
        STENCIL_BACK_PASS_DEPTH_PASS = 0x8803,
        STENCIL_BACK_REF = 0x8CA3,
        STENCIL_BACK_VALUE_MASK = 0x8CA4,
        STENCIL_BACK_WRITEMASK = 0x8CA5,
        VIEWPORT = 0x0BA2,
        SCISSOR_BOX = 0x0C10,
        COLOR_CLEAR_VALUE = 0x0C22,
        COLOR_WRITEMASK = 0x0C23,
        UNPACK_ALIGNMENT = 0x0CF5,
        PACK_ALIGNMENT = 0x0D05,
        MAX_TEXTURE_SIZE = 0x0D33,
        MAX_VIEWPORT_DIMS = 0x0D3A,
        SUBPIXEL_BITS = 0x0D50,
        RED_BITS = 0x0D52,
        GREEN_BITS = 0x0D53,
        BLUE_BITS = 0x0D54,
        ALPHA_BITS = 0x0D55,
        DEPTH_BITS = 0x0D56,
        STENCIL_BITS = 0x0D57,
        POLYGON_OFFSET_UNITS = 0x2A00,
        POLYGON_OFFSET_FACTOR = 0x8038,
        TEXTURE_BINDING_2D = 0x8069,
        SAMPLE_BUFFERS = 0x80A8,
        SAMPLES = 0x80A9,
        SAMPLE_COVERAGE_VALUE = 0x80AA,
        SAMPLE_COVERAGE_INVERT = 0x80AB,
        COMPRESSED_TEXTURE_FORMATS = 0x86A3,
        VENDOR = 0x1F00,
        RENDERER = 0x1F01,
        VERSION = 0x1F02,
        IMPLEMENTATION_COLOR_READ_TYPE = 0x8B9A,
        IMPLEMENTATION_COLOR_READ_FORMAT = 0x8B9B,
        BROWSER_DEFAULT_WEBGL = 0x9244,
        MAX_VERTEX_ATTRIBS = 0x8869,
        MAX_VERTEX_UNIFORM_VECTORS = 0x8DFB,
        MAX_VARYING_VECTORS = 0x8DFC,
        MAX_COMBINED_TEXTURE_IMAGE_UNITS = 0x8B4D,
        MAX_VERTEX_TEXTURE_IMAGE_UNITS = 0x8B4C,
        MAX_TEXTURE_IMAGE_UNITS = 0x8872,
        MAX_FRAGMENT_UNIFORM_VECTORS = 0x8DFD,
        SHADING_LANGUAGE_VERSION = 0x8B8C,
        CURRENT_PROGRAM = 0x8B8D,
        MAX_RENDERBUFFER_SIZE = 0x84E8,
        FRAMEBUFFER_BINDING = 0x8CA6,
        RENDERBUFFER_BINDING = 0x8CA7,
        ACTIVE_TEXTURE = 0x84E0
    }

    public enum BufferUsageHint
    {
        STATIC_DRAW = 0x88E4,
        STREAM_DRAW = 0x88E0,
        DYNAMIC_DRAW = 0x88E8
    }

    public enum BufferType
    {
        ARRAY_BUFFER = 0x8892,
        ELEMENT_ARRAY_BUFFER = 0x8893
    }

    public enum BufferParameter
    {
        BUFFER_SIZE = 0x8764,
        BUFFER_USAGE = 0x8765
    }

    public enum VertexAttribute
    {
        CURRENT_VERTEX_ATTRIB = 0x8626,
        VERTEX_ATTRIB_ARRAY_ENABLED = 0x8622,
        VERTEX_ATTRIB_ARRAY_SIZE = 0x8623,
        VERTEX_ATTRIB_ARRAY_STRIDE = 0x8624,
        VERTEX_ATTRIB_ARRAY_TYPE = 0x8625,
        VERTEX_ATTRIB_ARRAY_NORMALIZED = 0x886A,
        VERTEX_ATTRIB_ARRAY_BUFFER_BINDING = 0x889F
    }

    public enum VertexAttributePointer
    {
        VERTEX_ATTRIB_ARRAY_POINTER = 0x8645
    }

    public enum Face
    {
        FRONT = 0x0404,
        BACK = 0x0405,
        FRONT_AND_BACK = 0x0408
    }

    public enum EnableCap
    {
        CULL_FACE = 0x0B44,
        BLEND = 0x0BE2,
        DEPTH_TEST = 0x0B71,
        DITHER = 0x0BD0,
        POLYGON_OFFSET_FILL = 0x8037,
        SAMPLE_ALPHA_TO_COVERAGE = 0x809E,
        SAMPLE_COVERAGE = 0x80A0,
        SCISSOR_TEST = 0x0C11,
        STENCIL_TEST = 0x0B90
    }

    public enum Error
    {
        NO_ERROR = 0,
        INVALID_ENUM = 0x0500,
        INVALID_VALUE = 0x0501,
        INVALID_OPERATION = 0x0502,
        OUT_OF_MEMORY = 0x0505,
        CONTEXT_LOST_WEBGL = 0x9242
    }

    public enum FrontFaceDirection
    {
        CW = 0x0900,
        CCW = 0x0901
    }

    public enum HintMode
    {
        DONT_CARE = 0x1100,
        FASTEST = 0x1101,
        NICEST = 0x1102
    }

    public enum HintTarget
    {
        GENERATE_MIPMAP_HINT = 0x8192
    }

    public enum PixelFormat
    {
        ALPHA = 0x1906,
        RGB = 0x1907,
        RGBA = 0x1908,
        LUMINANCE = 0x1909,
        LUMINANCE_ALPHA = 0x190A
    }

    public enum PixelType
    {
        UNSIGNED_BYTE = 0x1401,
        UNSIGNED_SHORT_4_4_4_4 = 0x8033,
        UNSIGNED_SHORT_5_5_5_1 = 0x8034,
        UNSIGNED_SHORT_5_6_5 = 0x8363,
        FLOAT = 0x1406
    }

    public enum ShaderType
    {
        FRAGMENT_SHADER = 0x8B30,
        VERTEX_SHADER = 0x8B31
    }

    public enum ShaderParameter
    {
        COMPILE_STATUS = 0x8B81,
        DELETE_STATUS = 0x8B80,
        SHADER_TYPE = 0x8B4F
    }

    public enum ProgramParameter
    {
        DELETE_STATUS = 0x8B80,
        LINK_STATUS = 0x8B82,
        VALIDATE_STATUS = 0x8B83,
        ATTACHED_SHADERS = 0x8B85,
        ACTIVE_ATTRIBUTES = 0x8B89,
        ACTIVE_UNIFORMS = 0x8B86
    }

    public enum CompareFunction
    {
        NEVER = 0x0200,
        ALWAYS = 0x0207,
        LESS = 0x0201,
        EQUAL = 0x0202,
        LEQUAL = 0x0203,
        GREATER = 0x0204,
        GEQUAL = 0x0206,
        NOTEQUAL = 0x0205
    }

    public enum StencilFunction
    {
        KEEP = 0x1E00,
        REPLACE = 0x1E01,
        INCR = 0x1E02,
        DECR = 0x1E03,
        INVERT = 0x150A,
        INCR_WRAP = 0x8507,
        DECR_WRAP = 0x8508
    }

    public enum TextureType
    {
        TEXTURE_2D = 0x0DE1,
        TEXTURE_CUBE_MAP = 0x8513,
    }

    public enum Texture2DType
    {
        TEXTURE_2D = 0x0DE1,
        TEXTURE_CUBE_MAP_POSITIVE_X = 0x8515,
        TEXTURE_CUBE_MAP_NEGATIVE_X = 0x8516,
        TEXTURE_CUBE_MAP_POSITIVE_Y = 0x8517,
        TEXTURE_CUBE_MAP_NEGATIVE_Y = 0x8518,
        TEXTURE_CUBE_MAP_POSITIVE_Z = 0x8519,
        TEXTURE_CUBE_MAP_NEGATIVE_Z = 0x851A
    }

    public enum TextureParameter
    {
        TEXTURE_MAG_FILTER = 0x2800,
        TEXTURE_MIN_FILTER = 0x2801,
        TEXTURE_WRAP_S = 0x2802,
        TEXTURE_WRAP_T = 0x2803
    }

    public enum TextureParameterValue
    {
        NEAREST = 0x2600,
        LINEAR = 0x2601,
        NEAREST_MIPMAP_NEAREST = 0x2700,
        LINEAR_MIPMAP_NEAREST = 0x2701,
        NEAREST_MIPMAP_LINEAR = 0x2702,
        LINEAR_MIPMAP_LINEAR = 0x2703,
        REPEAT = 0x2901,
        CLAMP_TO_EDGE = 0x812F,
        MIRRORED_REPEAT = 0x8370
    }

    public enum UniformType
    {
        FLOAT_VEC2 = 0x8B50,
        FLOAT_VEC3 = 0x8B51,
        FLOAT_VEC4 = 0x8B52,
        INT_VEC2 = 0x8B53,
        INT_VEC3 = 0x8B54,
        INT_VEC4 = 0x8B55,
        BOOL = 0x8B56,
        BOOL_VEC2 = 0x8B57,
        BOOL_VEC3 = 0x8B58,
        BOOL_VEC4 = 0x8B59,
        FLOAT_MAT2 = 0x8B5A,
        FLOAT_MAT3 = 0x8B5B,
        FLOAT_MAT4 = 0x8B5C,
        SAMPLER_2D = 0x8B5E,
        SAMPLER_CUBE = 0x8B60
    }

    public enum ShaderPrecision
    {
        LOW_FLOAT = 0x8DF0,
        MEDIUM_FLOAT = 0x8DF1,
        HIGH_FLOAT = 0x8DF2,
        LOW_INT = 0x8DF3,
        MEDIUM_INT = 0x8DF4,
        HIGH_INT = 0x8DF5
    }

    public enum RenderbufferType
    {
        RENDERBUFFER = 0x8D41
    }

    public enum FramebufferType
    {
        FRAMEBUFFER = 0x8D40
    }

    public enum RenderbufferParameter
    {
        RENDERBUFFER_WIDTH = 0x8D42,
        RENDERBUFFER_HEIGHT = 0x8D43,
        RENDERBUFFER_INTERNAL_FORMAT = 0x8D44,
        RENDERBUFFER_RED_SIZE = 0x8D50,
        RENDERBUFFER_GREEN_SIZE = 0x8D51,
        RENDERBUFFER_BLUE_SIZE = 0x8D52,
        RENDERBUFFER_ALPHA_SIZE = 0x8D53,
        RENDERBUFFER_DEPTH_SIZE = 0x8D54,
        RENDERBUFFER_STENCIL_SIZE = 0x8D55
    }

    public enum FramebufferAttachment
    {
        COLOR_ATTACHMENT0 = 0x8CE0,
        DEPTH_ATTACHMENT = 0x8D00,
        STENCIL_ATTACHMENT = 0x8D20,
        DEPTH_STENCIL_ATTACHMENT = 0x821A
    }

    public enum FramebufferAttachmentParameter
    {
        FRAMEBUFFER_ATTACHMENT_OBJECT_TYPE = 0x8CD0,
        FRAMEBUFFER_ATTACHMENT_OBJECT_NAME = 0x8CD1,
        FRAMEBUFFER_ATTACHMENT_TEXTURE_LEVEL = 0x8CD2,
        FRAMEBUFFER_ATTACHMENT_TEXTURE_CUBE_MAP_FACE = 0x8CD3
    }

    public enum FramebufferStatus
    {
        NONE = 0,
        FRAMEBUFFER_COMPLETE = 0x8CD5,
        FRAMEBUFFER_INCOMPLETE_ATTACHMENT = 0x8CD6,
        FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT = 0x8CD7,
        FRAMEBUFFER_INCOMPLETE_DIMENSIONS = 0x8CD9,
        FRAMEBUFFER_UNSUPPORTED = 0x8CDD
    }

    public enum PixelStorageMode
    {
        UNPACK_FLIP_Y_WEBGL = 0x9240,
        UNPACK_PREMULTIPLY_ALPHA_WEBGL = 0x9241,
        UNPACK_COLORSPACE_CONVERSION_WEBGL = 0x9243
    }

    public enum RenderbufferFormat
    {
        RGBA4 = 0x8056,
        RGB5_A1 = 0x8057,
        RGB565 = 0x8D62,
        DEPTH_COMPONENT16 = 0x81A5,
        STENCIL_INDEX8 = 0x8D48,
        DEPTH_STENCIL = 0x84F9
    }

    public enum DataType
    {
        BYTE = 0x1400,
        UNSIGNED_BYTE = 0x1401,
        SHORT = 0x1402,
        UNSIGNED_SHORT = 0x1403,
        INT = 0x1404,
        UNSIGNED_INT = 0x1405,
        FLOAT = 0x1406
    }

    public enum Texture
    {
        TEXTURE0 = 0x84C0,
        TEXTURE1,
        TEXTURE2,
        TEXTURE3,
        TEXTURE4,
        TEXTURE5,
        TEXTURE6,
        TEXTURE7,
        TEXTURE8,
        TEXTURE9,
        TEXTURE10,
        TEXTURE11,
        TEXTURE12,
        TEXTURE13,
        TEXTURE14,
        TEXTURE15,
        TEXTURE16,
        TEXTURE17,
        TEXTURE18,
        TEXTURE19,
        TEXTURE20,
        TEXTURE21,
        TEXTURE22,
        TEXTURE23,
        TEXTURE24,
        TEXTURE25,
        TEXTURE26,
        TEXTURE27,
        TEXTURE28,
        TEXTURE29,
        TEXTURE30,
        TEXTURE31
    }
}
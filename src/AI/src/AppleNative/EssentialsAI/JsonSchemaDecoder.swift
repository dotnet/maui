import Foundation
import FoundationModels

class JsonSchemaDecoder {

    enum SchemaError: Error, LocalizedError {
        case unsupportedType(String)
        case missingObjectProperties
        case missingArrayItems

        var errorDescription: String? {
            switch self {
            case .unsupportedType(let type):
                return "Unsupported JSON schema type '\(type)'"
            case .missingObjectProperties:
                return "Object schema missing 'properties'"
            case .missingArrayItems:
                return "Array schema missing 'items'"
            }
        }
    }

    /// Simple JSON Schema representation
    private class JsonSchema: Codable {
        var type: String?
        var title: String?
        var description: String?
        var properties: [String: JsonSchema]?
        var required: [String]?
        var items: JsonSchema?
        var additionalProperties: Bool?
        var `enum`: [String]?
        var minItems: Int?
        var maxItems: Int?

        static func parse(jsonString: String) throws -> JsonSchema? {
            // Decode into an object
            guard let data = jsonString.data(using: .utf8) else {
                return nil
            }
            let jsonSchema = try JSONDecoder().decode(
                JsonSchema.self,
                from: data
            )

            // These seem to be required for tools to be called
            jsonSchema.type = jsonSchema.type ?? "object"
            if jsonSchema.type == "object" {
                jsonSchema.properties = jsonSchema.properties ?? [:]
            }

            return jsonSchema
        }
    }

    /// Some common JSON Schemas
    static let StringGenerationSchema: GenerationSchema =
        try! GenerationSchema(
            root: DynamicGenerationSchema(type: String.self),
            dependencies: []
        )

    /// Parse a string into a GenerationSchema
    static func parse(_ jsonString: String) throws -> GenerationSchema? {
        // Parse the JSON string
        guard let jsonSchema = try JsonSchema.parse(jsonString: jsonString)
        else { return nil }

        // Convert into a DynamicJsonSchema
        let dynamicSchema = try toDynamicSchema(jsonSchema)

        // Get the final GenerationSchema
        return try GenerationSchema(root: dynamicSchema, dependencies: [])
    }

    /// Convert the object representation of a JSON schema into a DynamicGenerationSchema
    private static func toDynamicSchema(_ schema: JsonSchema) throws
        -> DynamicGenerationSchema
    {
        switch schema.type {
        // Handle objects with properties
        case "object":
            guard let properties = schema.properties else {
                throw SchemaError.missingObjectProperties
            }
            let props = try properties.map { (name, value) in
                try parseJsonProperty(name, value, schema)
            }
            return DynamicGenerationSchema(
                name: schema.title ?? "Object",
                description: schema.description,
                properties: props
            )
        // Handle arrays with items
        case "array":
            guard let items = schema.items else {
                throw SchemaError.missingArrayItems
            }
            let itemSchema = try toDynamicSchema(items)
            return DynamicGenerationSchema(
                arrayOf: itemSchema,
                minimumElements: schema.minItems,
                maximumElements: schema.maxItems
            )
        // Handle primitive data types
        case "string":
            // Support enum values
            if let enumValues = schema.enum, !enumValues.isEmpty {
                return DynamicGenerationSchema(type: String.self, guides: [.anyOf(enumValues)])
            }
            // No enum values
            return DynamicGenerationSchema(type: String.self)
        case "integer": return DynamicGenerationSchema(type: Int.self)
        case "number": return DynamicGenerationSchema(type: Double.self)
        case "boolean": return DynamicGenerationSchema(type: Bool.self)
        default:
            throw SchemaError.unsupportedType(schema.type ?? "unknown")
        }
    }

    private static func parseJsonProperty(
        _ propertyName: String,
        _ value: JsonSchema,
        _ parentSchema: JsonSchema
    ) throws -> DynamicGenerationSchema.Property {
        let nestedSchema = try toDynamicSchema(value)
        let isRequired = parentSchema.required?.contains(propertyName) == true
        return DynamicGenerationSchema.Property(
            name: propertyName,
            description: value.description,
            schema: nestedSchema,
            isOptional: !isRequired
        )
    }

}

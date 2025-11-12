
import Foundation
import FoundationModels

class JsonSchemaDecoder {

    /// Simple JSON Schema representation
    private class JsonSchema: Codable {
        var type: String?
        var title: String?
        var description: String?
        var properties: [String: JsonSchema]?
        var required: [String]?
        var items: JsonSchema?
        var additionalProperties: Bool?

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
        guard let dynamicSchema = toDynamicSchema(jsonSchema)
        else { return nil }

        // Get the final GenerationSchema
        return try GenerationSchema(root: dynamicSchema, dependencies: [])
    }

    /// Convert the object representation of a JSON schema into a DynamicGenerationSchema
    private static func toDynamicSchema(_ schema: JsonSchema)
        -> DynamicGenerationSchema?
    {
        switch schema.type {
        // Handle objects with properties
        case "object":
            guard let properties = schema.properties else { return nil }
            let props = properties.compactMap { (name, value) in
                parseJsonProperty(name, value, schema)
            }
            return DynamicGenerationSchema(
                name: schema.title ?? "Object",
                description: schema.description,
                properties: props
            )
        // Handle arrays with items
        case "array":
            guard
                let items = schema.items,
                let itemSchema = toDynamicSchema(items)
            else { return nil }
            return DynamicGenerationSchema(arrayOf: itemSchema)
        // Handle primitive data types
        case "string": return DynamicGenerationSchema(type: String.self)
        case "integer": return DynamicGenerationSchema(type: Int.self)
        case "number": return DynamicGenerationSchema(type: Double.self)
        case "boolean": return DynamicGenerationSchema(type: Bool.self)
        default: return nil
        }
    }

    private static func parseJsonProperty(
        _ propertyName: String,
        _ value: JsonSchema,
        _ parentSchema: JsonSchema
    ) -> DynamicGenerationSchema.Property? {
        guard let nestedSchema = toDynamicSchema(value) else {
            return nil
        }
        let isRequired = parentSchema.required?.contains(propertyName) == true
        return DynamicGenerationSchema.Property(
            name: propertyName,
            description: value.description,
            schema: nestedSchema,
            isOptional: !isRequired
        )
    }

}

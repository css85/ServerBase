using System;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace SampleGame.Shared.Common
{
    public class BigIntegerConverter : JsonConverter<BigInteger>
    {
        /// <summary>
        /// Converts a JSON value to a <see cref="BigInteger"/>.
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
        /// <param name="typeToConvert">The type of the object to convert.</param>
        /// <param name="options">The serializer options to use.</param>
        /// <returns>The converted <see cref="BigInteger"/>.</returns>
        public override BigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Check if the input is a numeric value
            if (reader.TokenType == JsonTokenType.Number)
            {
                // Try to parse the input directly as a BigInteger using Utf8Parser
                ReadOnlySpan<byte> span = reader.ValueSpan;
                string stringValue = Encoding.UTF8.GetString(span);
                
                if (BigInteger.TryParse(stringValue, System.Globalization.NumberStyles.Number, null, out BigInteger result))
                {
                    return result;
                }
            }
            // Check if the input is a string value
            else if (reader.TokenType == JsonTokenType.String)
            {
                // Try to parse the input as a BigInteger using BigInteger.TryParse
                if (BigInteger.TryParse(reader.GetString(), out BigInteger result))
                {
                    return result;
                }
            }

            // If parsing fails, throw a JsonException
            throw new JsonException($"Could not convert \"{reader.GetString()}\" to BigInteger.");
        }

        /// <summary>
        /// Writes a <see cref="BigInteger"/> value as a JSON number.
        /// </summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The serializer options to use.</param>
        public override void Write(Utf8JsonWriter writer, BigInteger value, JsonSerializerOptions options)
        {
            // Convert the BigInteger value to a byte array using UTF8 encoding
            byte[] bytes = Encoding.UTF8.GetBytes(value.ToString());

            // Write the byte array as a raw JSON numeric value (without quotes)
            writer.WriteRawValue(Encoding.UTF8.GetString(bytes));
        }
    }
}
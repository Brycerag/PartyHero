using System;
using System.Collections.Generic;
using System.Text;

namespace MoonscraperChartEditor.Song
{
    /// <summary>
    /// Simple OSC (Open Sound Control) message parser for DAW sync.
    /// Supports basic OSC types: int32, float32, string, blob
    /// </summary>
    public class OscMessage
    {
        public string Address { get; private set; }
        public List<object> Arguments { get; private set; }

        public OscMessage(string address)
        {
            Address = address;
            Arguments = new List<object>();
        }

        /// <summary>
        /// Parse OSC message from raw UDP bytes
        /// </summary>
        public static OscMessage Parse(byte[] data, int length)
        {
            if (data == null || length < 8)
                return null;

            int index = 0;

            // Read address pattern (null-terminated, padded to 4-byte boundary)
            string address = ReadString(data, ref index);
            if (string.IsNullOrEmpty(address) || !address.StartsWith("/"))
                return null;

            OscMessage message = new OscMessage(address);

            // Read type tag string (starts with comma)
            if (index >= length)
                return message; // No arguments

            string typeTags = ReadString(data, ref index);
            if (string.IsNullOrEmpty(typeTags) || !typeTags.StartsWith(","))
                return message; // No arguments

            // Parse arguments based on type tags
            for (int i = 1; i < typeTags.Length; i++)
            {
                if (index >= length)
                    break;

                char typeTag = typeTags[i];
                switch (typeTag)
                {
                    case 'i': // int32
                        message.Arguments.Add(ReadInt32(data, ref index));
                        break;

                    case 'f': // float32
                        message.Arguments.Add(ReadFloat32(data, ref index));
                        break;

                    case 's': // string
                        message.Arguments.Add(ReadString(data, ref index));
                        break;

                    case 'b': // blob
                        message.Arguments.Add(ReadBlob(data, ref index));
                        break;

                    case 'T': // True
                        message.Arguments.Add(true);
                        break;

                    case 'F': // False
                        message.Arguments.Add(false);
                        break;

                    case 'N': // Nil/Null
                        message.Arguments.Add(null);
                        break;

                    default:
                        // Unknown type, skip
                        break;
                }
            }

            return message;
        }

        private static string ReadString(byte[] data, ref int index)
        {
            int start = index;
            while (index < data.Length && data[index] != 0)
                index++;

            if (index >= data.Length)
                return string.Empty;

            string result = Encoding.UTF8.GetString(data, start, index - start);

            // Skip null terminator and padding to 4-byte boundary
            index++;
            while (index % 4 != 0)
                index++;

            return result;
        }

        private static int ReadInt32(byte[] data, ref int index)
        {
            if (index + 4 > data.Length)
                return 0;

            // OSC uses big-endian (network byte order)
            int value = (data[index] << 24) | (data[index + 1] << 16) |
                       (data[index + 2] << 8) | data[index + 3];
            index += 4;
            return value;
        }

        private static float ReadFloat32(byte[] data, ref int index)
        {
            if (index + 4 > data.Length)
                return 0f;

            // OSC uses big-endian
            byte[] bytes = new byte[4];
            bytes[3] = data[index++];
            bytes[2] = data[index++];
            bytes[1] = data[index++];
            bytes[0] = data[index++];

            return BitConverter.ToSingle(bytes, 0);
        }

        private static byte[] ReadBlob(byte[] data, ref int index)
        {
            int size = ReadInt32(data, ref index);
            if (size < 0 || index + size > data.Length)
                return new byte[0];

            byte[] blob = new byte[size];
            Array.Copy(data, index, blob, 0, size);
            index += size;

            // Pad to 4-byte boundary
            while (index % 4 != 0)
                index++;

            return blob;
        }

        public int GetInt(int argIndex, int defaultValue = 0)
        {
            if (argIndex < 0 || argIndex >= Arguments.Count)
                return defaultValue;

            object arg = Arguments[argIndex];
            if (arg is int intVal)
                return intVal;
            if (arg is float floatVal)
                return (int)floatVal;

            return defaultValue;
        }

        public float GetFloat(int argIndex, float defaultValue = 0f)
        {
            if (argIndex < 0 || argIndex >= Arguments.Count)
                return defaultValue;

            object arg = Arguments[argIndex];
            if (arg is float floatVal)
                return floatVal;
            if (arg is int intVal)
                return (float)intVal;

            return defaultValue;
        }

        public string GetString(int argIndex, string defaultValue = "")
        {
            if (argIndex < 0 || argIndex >= Arguments.Count)
                return defaultValue;

            object arg = Arguments[argIndex];
            return arg?.ToString() ?? defaultValue;
        }

        public bool GetBool(int argIndex, bool defaultValue = false)
        {
            if (argIndex < 0 || argIndex >= Arguments.Count)
                return defaultValue;

            object arg = Arguments[argIndex];
            if (arg is bool boolVal)
                return boolVal;
            if (arg is int intVal)
                return intVal != 0;
            if (arg is float floatVal)
                return Math.Abs(floatVal) > 0.0001f;

            return defaultValue;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Address);
            if (Arguments.Count > 0)
            {
                sb.Append(" [");
                for (int i = 0; i < Arguments.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append(Arguments[i]?.ToString() ?? "null");
                }
                sb.Append("]");
            }
            return sb.ToString();
        }
    }
}

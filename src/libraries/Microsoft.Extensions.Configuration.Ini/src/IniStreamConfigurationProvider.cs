// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Extensions.Configuration.Ini
{
    /// <summary>
    /// Provides configuration key-value pairs that are obtained from an INI stream.
    /// </summary>
    public class IniStreamConfigurationProvider : StreamConfigurationProvider
    {
        /// <summary>
        /// Initializes a new instance of <see cref="IniStreamConfigurationProvider"/>.
        /// </summary>
        /// <param name="source">The <see cref="IniStreamConfigurationSource"/>.</param>
        public IniStreamConfigurationProvider(IniStreamConfigurationSource source) : base(source) { }

        /// <summary>
        /// Read a stream of INI values into a key/value dictionary.
        /// </summary>
        /// <param name="stream">The stream of INI data.</param>
        /// <returns>The <see cref="IDictionary{String, String}"/> which was read from the stream.</returns>
        public static IDictionary<string, string?> Read(Stream stream)
        {
            var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            using (var reader = new StreamReader(stream))
            {
                string sectionPrefix = string.Empty;

                while (reader.Peek() != -1)
                {
                    string rawLine = reader.ReadLine()!; // Since Peak didn't return -1, stream hasn't ended.
                    string line = rawLine.Trim();

                    // Ignore blank lines
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    // Ignore comments
                    if (line[0] is ';' or '#' or '/')
                    {
                        continue;
                    }
                    // [Section:header]
                    if (line[0] == '[' && line[line.Length - 1] == ']')
                    {
                        // remove the brackets
#if NET
                        sectionPrefix = string.Concat(line.AsSpan(1, line.Length - 2).Trim(), ConfigurationPath.KeyDelimiter);
#else
                        sectionPrefix = line.Substring(1, line.Length - 2).Trim() + ConfigurationPath.KeyDelimiter;
#endif
                        continue;
                    }

                    // key = value OR "value"
                    int separator = line.IndexOf('=');
                    if (separator < 0)
                    {
                        throw new FormatException(SR.Format(SR.Error_UnrecognizedLineFormat, rawLine));
                    }

                    string key = sectionPrefix + line.Substring(0, separator).Trim();
                    string value = line.Substring(separator + 1).Trim();

                    // Remove quotes
                    if (value.Length > 1 && value[0] == '"' && value[value.Length - 1] == '"')
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    if (data.ContainsKey(key))
                    {
                        throw new FormatException(SR.Format(SR.Error_KeyIsDuplicated, key));
                    }

                    data[key] = value;
                }
            }
            return data;
        }

        /// <summary>
        /// Loads INI configuration key-value pairs from a stream into a provider.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to load INI configuration data from.</param>
        public override void Load(Stream stream)
        {
            Data = Read(stream);
        }
    }
}

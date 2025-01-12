using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpenseTracker.Core.Transactions;

namespace ExpenseTracker.Integrations.Files
{
    // Base class for CSV parsing
    public abstract class BaseCsvParser
    {
        protected virtual char FieldDelimiter { get; } = ',';
        protected virtual char StringDelimiter { get; } = '"';

        private int _fieldCount;

        public List<Transaction> Parse(string data)
        {
            var results = new List<Transaction>();

            using (var sr = new StringReader(data))
            {
                var header = sr.ReadLine();
                var headerFields = ParseFields(header);
                ValidateHeader(header);
                _fieldCount = headerFields.Length;

                var line = sr.ReadLine();
                while (!string.IsNullOrWhiteSpace(line))
                {
                    var fields = ParseFields(line);

                    if (fields.Length != _fieldCount)
                    {
                        throw new Exception($"Invalid row: incorrect number of fields (expected {_fieldCount}, got {fields.Length}).");
                    }

                    var entity = MapRowToEntity(fields);
                    if (entity != null)
                    {
                        results.Add(entity);
                    }

                    line = sr.ReadLine();
                }
            }

            return results;
        }

        public IEnumerable<Transaction> ParseFromFile(string filePath)
        {
            return Parse(File.ReadAllText(filePath));
        }

        // Abstract methods for specific implementations
        protected abstract void ValidateHeader(string header);

        protected abstract Transaction MapRowToEntity(string[] fields);

        // Parses a line into fields considering field and string delimiters
        protected internal virtual string[] ParseFields(string line)
        {
            var fields = new List<string>();
            var currentField = string.Empty;
            var insideString = false;

            for (int i = 0; i < line.Length; i++)
            {
                var ch = line[i];

                if (ch == StringDelimiter)
                {
                    // Handle embedded quotes
                    if (insideString && i + 1 < line.Length && line[i + 1] == StringDelimiter)
                    {
                        currentField += ch; // Add the embedded quote
                        i++; // Skip the next character as it's part of the embedded quote
                    }
                    else
                    {
                        insideString = !insideString;
                    }
                }
                else if (ch == FieldDelimiter && !insideString)
                {
                    fields.Add(currentField);
                    currentField = string.Empty; // Reset for the next field
                }
                else
                {
                    currentField += ch;
                }
            }

            // Add the last field (including if it's empty)
            fields.Add(currentField);

            return fields.ToArray();
        }
    }
}

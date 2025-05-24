using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpenseTracker.Core.Transactions;

namespace ExpenseTracker.Integrations.Files.Base
{
    // Base class for CSV parsing
    public abstract class CsvParserBase<T>
    {
        private int _fieldCount;

        protected virtual char FieldDelimiter { get; } = ',';
        protected virtual char StringDelimiter { get; } = '"';
        protected string[] headerFields;
        private readonly IEnumerable<string> requiredFields = new string[0];

        protected virtual IEnumerable<string> RequiredFields => requiredFields;
        protected bool TryGetFieldValue(string[] fields, string fieldName, out string value)
        {
            var index = Array.IndexOf(this.headerFields, fieldName);
            if (index >= 0)
            {
                value = fields[index];
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public List<T> Parse(string data)
        {
            var results = new List<T>();

            using (var sr = new StringReader(data))
            {
                var header = sr.ReadLine();
                this.headerFields = ParseFields(header);
                ValidateHeader();
                _fieldCount = headerFields.Length;

                var line = sr.ReadLine();
                while (!string.IsNullOrWhiteSpace(line))
                {
                    var fields = ParseFields(line);

                    if (fields.Length != _fieldCount)
                    {
                        throw new Exception($"Invalid row: incorrect number of fields (expected {_fieldCount}, got {fields.Length}) for line {line}.");
                    }

                    T entity;
                    try
                    {
                        entity = MapRowToEntity(fields);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error parsing line: '{line}'. {ex.Message}", ex);
                    }
                    
                    if (entity != null)
                    {
                        results.Add(entity);
                    }

                    line = sr.ReadLine();
                }
            }

            return results;
        }

        public IEnumerable<T> ParseFromFile(string filePath)
        {
            return Parse(File.ReadAllText(filePath));
        }

        protected abstract T MapRowToEntity(string[] fields);

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

        private void ValidateHeader()
        {
            foreach (var field in RequiredFields)
            {
                if (!this.headerFields.Contains(field))
                {
                    throw new ArgumentException($"Missing required field: {field}");
                }
            }
        }

    }
}

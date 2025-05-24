using System;

namespace ExpenseTracker.Integrations
{
    /// <summary>
    /// Represents an error that occurred during the import process.
    /// </summary>
    public class ImportError
    {
        /// <summary>
        /// The error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The type of import operation that failed
        /// </summary>
        public ImportErrorType ErrorType { get; set; }

        /// <summary>
        /// The source of the import (e.g., file name, email, etc.)
        /// </summary>
        public string ImportSource { get; set; }
        
        /// <summary>
        /// Additional details about the error (e.g., inner exception message)
        /// </summary>
        public string Details { get; set; }

        public ImportError()
        {
            // Default constructor for serialization
        }
          public ImportError(string message, ImportErrorType errorType, string source = null)
        {
            Message = message;
            ErrorType = errorType;
            ImportSource = source;
        }
        
        public ImportError(string message, ImportErrorType errorType, Exception innerException, string source = null)
        {
            Message = message;
            ErrorType = errorType;
            ImportSource = source;
            
            // Store inner exception details as a string
            if (innerException != null)
            {
                Details = innerException.Message;
            }
        }
    }

    /// <summary>
    /// Categorizes the type of import error that occurred
    /// </summary>
    public enum ImportErrorType
    {
        /// <summary>
        /// Error accessing the data source (file not found, connection issues, etc.)
        /// </summary>
        AccessError,
        
        /// <summary>
        /// Error parsing the content (invalid format, unexpected data, etc.)
        /// </summary>
        ParseError,
        
        /// <summary>
        /// Error validating the transaction data (missing fields, invalid values, etc.)
        /// </summary>
        ValidationError,
        
        /// <summary>
        /// Error saving the transaction (duplicate, database error, etc.)
        /// </summary>
        SaveError,
        
        /// <summary>
        /// Unexpected or miscellaneous errors
        /// </summary>
        OtherError
    }
}

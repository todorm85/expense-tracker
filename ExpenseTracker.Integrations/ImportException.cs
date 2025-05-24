using System;

namespace ExpenseTracker.Integrations
{
    /// <summary>
    /// Exception type for errors that occur during the import process.
    /// </summary>
    public class ImportException : Exception
    {
        /// <summary>
        /// The type of import operation that failed
        /// </summary>
        public ImportErrorType ErrorType { get; }        /// <summary>
        /// The source of the import (e.g., file name, email, etc.)
        /// </summary>
        public string ImportSource { get; }
        
        /// <summary>
        /// Indicates whether this error is retryable
        /// </summary>
        public bool CanRetry { get; }
        
        /// <summary>
        /// Optional data that can be used for retry operations
        /// </summary>
        public object RetryData { get; }        public ImportException(string message, ImportErrorType errorType, string source = null, bool canRetry = false, object retryData = null) 
            : base(message)
        {
            ErrorType = errorType;
            ImportSource = source;
            CanRetry = canRetry;
            RetryData = retryData;
            
            // Also set the base Source property
            if (source != null)
            {
                base.Source = source;
            }
        }

        public ImportException(string message, ImportErrorType errorType, Exception innerException, string source = null, bool canRetry = false, object retryData = null) 
            : base(message, innerException)
        {
            ErrorType = errorType;
            ImportSource = source;
            CanRetry = canRetry;
            RetryData = retryData;
            
            // Also set the base Source property
            if (source != null)
            {
                base.Source = source;
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

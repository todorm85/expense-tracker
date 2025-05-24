using ExpenseTracker.Integrations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Shared
{
    public class ExceptionsViewModel
    {
        public ExceptionsViewModel()
        {
            ImportErrors = new List<ImportError>();
        }

        public IList<ImportError> ImportErrors { get; set; }
        public bool HasErrors => ImportErrors != null && ImportErrors.Any();

        /// <summary>
        /// Gets errors grouped by their error type for better display
        /// </summary>
        public IEnumerable<IGrouping<string, ImportError>> GroupedErrors
        {
            get
            {
                if (ImportErrors == null || !ImportErrors.Any())
                    return Enumerable.Empty<IGrouping<string, ImportError>>();

                return ImportErrors.GroupBy(err => err.ErrorType.ToString());
            }
        }

        /// <summary>
        /// Adds an exception to the list of import errors with proper categorization
        /// </summary>
        public void AddException(Exception exception, string? source = null)
        {
            if (exception == null)
                return;
                  // Create a new ImportError
            var errorType = DetermineErrorType(exception);
            var importError = new ImportError(
                exception.Message, 
                errorType, 
                exception, 
                source);
                
            ImportErrors.Add(importError);
        }

        /// <summary>
        /// Adds an import error directly
        /// </summary>
        public void AddError(ImportError error)
        {
            if (error == null)
                return;
                
            ImportErrors.Add(error);
        }

        /// <summary>
        /// Determine the most likely error type based on the exception
        /// </summary>
        private ImportErrorType DetermineErrorType(Exception ex)
        {
            var exType = ex.GetType().Name.ToLowerInvariant();

            if (exType.Contains("io") || exType.Contains("file") || exType.Contains("connection"))
                return ImportErrorType.AccessError;

            if (exType.Contains("format") || exType.Contains("parse"))
                return ImportErrorType.ParseError;

            if (exType.Contains("argument") || exType.Contains("invalid"))
                return ImportErrorType.ValidationError;

            if (exType.Contains("db") || exType.Contains("sql") || exType.Contains("database"))
                return ImportErrorType.SaveError;

            return ImportErrorType.OtherError;
        }

        /// <summary>
        /// Add multiple exceptions to the list
        /// </summary>
        public void AddExceptions(IEnumerable<Exception> exceptions, string? source = null)
        {
            if (exceptions == null)
                return;

            foreach (var exception in exceptions)
            {
                AddException(exception, source);
            }
        }
    }
}

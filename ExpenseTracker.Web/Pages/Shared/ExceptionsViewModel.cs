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
            Exceptions = new List<Exception>();
        }

        public IList<Exception> Exceptions { get; set; }
        public bool HasExceptions => Exceptions != null && Exceptions.Any();

        /// <summary>
        /// Gets exceptions grouped by their error type for better display
        /// </summary>
        public IEnumerable<IGrouping<string, Exception>> GroupedExceptions
        {
            get
            {
                if (Exceptions == null || !Exceptions.Any())
                    return Enumerable.Empty<IGrouping<string, Exception>>();

                return Exceptions.GroupBy(ex =>
                {                if (ex is ImportException importEx)
                    return importEx.ErrorType.ToString();
                return ex.GetType().Name;
                });
            }
        }

        /// <summary>
        /// Adds an exception to the list with proper categorization
        /// </summary>
        public void AddException(Exception exception, string? source = null)
        {
            if (exception == null)
                return;
                
            // If it's already an ImportException, just add it
            if (exception is ImportException)
            {
                Exceptions.Add(exception);
                return;
            }
            
            // Otherwise, wrap it in an ImportException with appropriate categorization
            var errorType = DetermineErrorType(exception);
            var importException = new ImportException(
                exception.Message, 
                errorType, 
                exception, 
                source,
                errorType != ImportErrorType.AccessError); // Only access errors are typically not retryable
                
            Exceptions.Add(importException);
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

using System;
using System.Collections.Generic;
using ExpenseTracker.Core.Transactions;

namespace ExpenseTracker.Core.Services.Models
{
    public class TransactionsFilterParams : ItemsFilterParams<Transaction>
    {
        public const string UncategorizedOptionValue = "-";
        
        public TransactionsFilterParams()
        {
            SelectedCategories = new List<string>();
            CategoryExpression = string.Empty;
            Search = string.Empty;
        }
        
        public List<string> SelectedCategories { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        
        public string Search { get; set; }
        public List<string> SelectedSources { get; set; } = new List<string>() { UncategorizedOptionValue };
        public string CategoryExpression { get; set; }
        public SortOptions SortOptions { get; set; }
    }
}
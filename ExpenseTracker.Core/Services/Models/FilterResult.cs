using System.Collections.Generic;
using System.Linq;
using ExpenseTracker.Core.Transactions;

namespace ExpenseTracker.Core.Services.Models;

public class TransactionsFilterResult : ItemsFilterResult<Transaction>
{
    public List<string> AvailableCategories { get; set; } = new List<string>();
    public List<string> AvailableSources { get; set; } = new List<string>();
    
}
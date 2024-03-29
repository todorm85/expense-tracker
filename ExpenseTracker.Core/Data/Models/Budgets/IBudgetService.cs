﻿using ExpenseTracker.Core.Data;
using System;

namespace ExpenseTracker.Core.Budget
{
    public interface IBudgetService : IRepository<Budget>
    {
        Budget GetCumulativeForMonth(DateTime month);
    }
}
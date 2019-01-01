using ExpenseTracker.Core;

namespace ExpenseTracker.UI
{
    public class BudgetMenu : DataItemMenuBase<Budget>
    {
        private readonly IBudgetService budgetService;

        public BudgetMenu(IBudgetService service, IOutputRenderer renderer) : base(renderer)
        {
            this.budgetService = service;
            this.Service = service;
        }

        public override IBaseDataItemService<Budget> Service { get; set; }
    }
}
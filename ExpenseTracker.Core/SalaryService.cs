using System.Linq;

namespace ExpenseTracker.Core
{
    public class SalaryService : ISalaryService
    {
        public SalaryService(IUnitOfWork uow)
        {
            this.salaryRepo = uow.GetDataItemsRepo<Salary>();
        }

        public decimal SalaryAmount
        {
            get
            {
                return this.GetSalary().Amount;
            }

            set
            {
                var salary = this.GetSalary();
                salary.Amount = value;
                this.salaryRepo.Update(new Salary[] { salary });
            }
        }

        private Salary GetSalary()
        {
            var salary = this.salaryRepo.GetAll().FirstOrDefault(x => x.Id == SalaryId);
            if (salary == null)
            {
                salary = new Salary() { Id = SalaryId, Amount = 0 };
                this.salaryRepo.Insert(new Salary[] { salary });
            }

            return salary;
        }

        private const int SalaryId = 1;
        private IGenericRepository<Salary> salaryRepo;
    }
}
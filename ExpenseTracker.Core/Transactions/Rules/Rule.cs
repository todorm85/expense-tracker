namespace ExpenseTracker.Core.Transactions.Rules
{
    public class Rule
    {
        public int Id { get; set; }
        public RuleAction Action { get; set; }
        public RuleCondition Condition { get; set; }
        public string Property { get; set; }
        public string PropertyToSet { get; set; }
        public string Value { get; set; }
        public string ValueToSet { get; set; }

        public bool Process(Transaction tr)
        {
            var p = tr.GetType().GetProperty(Property);
            var v = p.GetValue(tr).ToString();
            if (Condition == RuleCondition.Contains && v.Contains(Value))
            {
                if (Action == RuleAction.Skip)
                    return false;
                if (Action == RuleAction.SetProperty)
                {
                    var pt = tr.GetType().GetProperty(PropertyToSet);
                    if (pt.PropertyType == typeof(TransactionType))
                    {
                        pt.SetValue(tr, System.Enum.Parse<TransactionType>(ValueToSet));
                    }
                    else
                    {
                        pt.SetValue(tr, ValueToSet);
                    }
                }
            }

            return true;
        }
    }
}
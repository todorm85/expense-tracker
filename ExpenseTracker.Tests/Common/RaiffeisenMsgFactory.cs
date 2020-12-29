using ExpenseTracker.Allianz;
using System;

namespace ExpenseTracker.Tests.Common
{
    public class RaiffeisenMsgFactory : MessageFactoryBase
    {
        private string ReasonText;

        public ExpenseMessage GetExpenseMessage(string reason)
        {
            this.ReasonText = reason;

            return new ExpenseMessage()
            {
                Body = GetBody(),
                Subject = "Notification from RBBBG",
                EmailDate = DateTime.Now
            };
        }

        public override ExpenseMessage GetInValidMessage()
        {
            return GetExpenseMessage("neuspeshen opit za POKUPKA");
        }

        public override ExpenseMessage GetValidMessage()
        {
            return GetExpenseMessage("POKUPKA");
        }

        private string GetBody()
        {
            return @$"Uvazhaemi g-ne/g-zho,
Bihme iskali da Vi uvedomim za {this.ReasonText} za {this.Amount} BGN s Vashata null * ***3680 v BGR pri {this.Location} na {this.Date} 09:34:34.Razpolagaema nalichnost po kartata 379.59 BGN.
S uvazhenie,
                Raiffeisenbank(Bulgaria) EAD
Sofia 1407,
                blvd Nikola I.Vaptzarov  55
070010000(VIVACOM)  1721(A1 i Telenor)";
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Integrations;
using ExpenseTracker.Integrations.Files.Base;

public abstract class TransactionCsvParserBase : CsvParserBase<Transaction>
{
    protected abstract string FieldNameForDate { get; }
    protected abstract string FieldNameForAmount { get; }
    protected abstract string[] FieldNamesForDetails { get; }

    override protected IEnumerable<string> RequiredFields => base.RequiredFields.Concat(new[] { FieldNameForDate, FieldNameForAmount }).Concat(FieldNamesForDetails);

    protected override Transaction MapRowToEntity(string[] fields)
    {
        var date = ParseDate(fields);
        var details = ParseDetails(fields);
        var amount = ParseAmount(fields);
        var source = ParseSource(fields);

        var t = new Transaction
        {
            Date = date,
            Details = details,
            Amount = amount,
            Type = amount < 0 ? TransactionType.Expense : TransactionType.Income,
            Source = source
        };

        SetTransactionId(t, fields);

        return t;
    }

    protected virtual decimal ParseAmount(string[] fields)
    {
        return decimal.Parse(fields[Array.IndexOf(this.headerFields, FieldNameForAmount)]);
    }

    protected virtual string ParseDetails(string[] fields)
    {
        return string.Join(" - ", FieldNamesForDetails
            .Select(fieldName => fields[Array.IndexOf(this.headerFields, fieldName)])
            .Where(value => !string.IsNullOrEmpty(value)));
    }

    protected virtual void SetTransactionId(Transaction t, string[] fields)
    {
        string truncatedDetails = t.Details.Length > 128 ? t.Details.Substring(0, 128) : t.Details;
        t.TransactionId = $"{t.Source}-{t.Date:yyyyMMddHHmmssfff}-{truncatedDetails}-{t.Amount}";
    }

    protected virtual DateTime ParseDate(string[] fields)
    {
        var date = fields[Array.IndexOf(this.headerFields, FieldNameForDate)];
        var formats = new[]
        {
            "yyyy-MM-dd HH:mm:ss.fff",
            "yyyy-MM-dd HH:mm:ss",
            "dd/MM/yyyy HH:mm:ss",
            "MM/dd/yyyy HH:mm:ss",
            "M/d/yyyy H:mm",
            "d.M.yyyy H:mm"
        };

        if (!DateTime.TryParseExact(date, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        {
            throw new Exception("Invalid date format.");
        }

        return parsedDate.ConvertToUtcFromBgTimeUnknowKind();
    }

    protected abstract string ParseSource(string[] fields);
}
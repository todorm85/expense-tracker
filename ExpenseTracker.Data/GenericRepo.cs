using ExpenseTracker.Core;
using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpenseTracker.Data
{
    public class GenericRepo<T> : IRepository<T>
        where T : class
    {
        private LiteCollection<T> context;
        private LiteDatabase db;

        public GenericRepo(LiteDatabase db, string collection)
        {
            this.db = db;
            this.context = this.db.GetCollection<T>(collection);
            if (typeof(T) == typeof(Transaction))
            {
                var transactionsContext = this.context as LiteCollection<Transaction>;
                transactionsContext.EnsureIndex(x => x.Date);
                transactionsContext.EnsureIndex(x => x.Amount);
                transactionsContext.EnsureIndex(x => x.TransactionId);
                transactionsContext.EnsureIndex(x => x.Category);
            }
        }

        public virtual int Count(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate != null)
                return this.context.Count(predicate);
            else
                return this.context.Count();
        }

        // The only thing needed to optimize for local db is filtering in order to protect against serialization of all db entries, everything else like ordering and skip and take should be done in memmory
        public virtual IEnumerable<T> GetAll(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
            {
                    return this.context.FindAll();
            }

            var query = TryConvertToLiteDbQuery(predicate);
            return query != null ? this.context.Find(query) : this.context.Find(predicate);
        }

        public virtual T GetById(object id)
        {
            var all = this.context.FindById(new BsonValue(id));
            return all;
        }

        public virtual void Insert(IEnumerable<T> items)
        {
            this.context.Insert(items);
        }

        public virtual void Insert(T item)
        {
            this.context.Insert(item);
        }

        public virtual void RemoveById(object id)
        {
            this.context.Delete(new BsonValue(id));
        }

        public virtual void Update(IEnumerable<T> items)
        {
            this.context.Update(items);
        }

        public virtual void Update(T item)
        {
            this.context.Update(item);
        }

        private Query TryConvertToLiteDbQuery(Expression<Func<T, bool>> predicate)
        {
            return ProcessExpression(predicate.Body);
        }

        private Query ProcessExpression(Expression expression)
        {
            if (expression is BinaryExpression binaryExpr)
            {
                return ProcessBinaryExpression(binaryExpr);
            }
            else if (expression is UnaryExpression unaryExpr)
            {
                return ProcessUnaryExpression(unaryExpr);
            }
            return null; // Fallback if the expression type isn’t supported
        }

        private Query ProcessBinaryExpression(BinaryExpression expression)
        {
            // Handle logical AND (&&) and OR (||)
            if (expression.NodeType == ExpressionType.AndAlso)
            {
                var leftQuery = ProcessExpression(expression.Left);
                var rightQuery = ProcessExpression(expression.Right);
                if (leftQuery != null && rightQuery != null)
                {
                    return Query.And(leftQuery, rightQuery);
                }
            }
            else if (expression.NodeType == ExpressionType.OrElse)
            {
                var leftQuery = ProcessExpression(expression.Left);
                var rightQuery = ProcessExpression(expression.Right);
                if (leftQuery != null && rightQuery != null)
                {
                    return Query.Or(leftQuery, rightQuery);
                }
            }
            // Handle comparisons with member on the left and constant on the right
            else if (expression.Left is MemberExpression member && expression.Right is ConstantExpression constant)
            {
                string fieldName = member.Member.Name;
                BsonValue value = new BsonValue(constant.Value);
                switch (expression.NodeType)
                {
                    case ExpressionType.Equal:
                        return Query.EQ(fieldName, value);
                    case ExpressionType.NotEqual:
                        return Query.Not(Query.EQ(fieldName, value));
                    case ExpressionType.GreaterThanOrEqual:
                        return Query.GTE(fieldName, value);
                    case ExpressionType.LessThanOrEqual:
                        return Query.LTE(fieldName, value);
                    case ExpressionType.GreaterThan:
                        return Query.GT(fieldName, value);
                    case ExpressionType.LessThan:
                        return Query.LT(fieldName, value);
                }
            }
            // Optionally, handle the reverse (constant on the left, member on the right)
            else if (expression.Left is ConstantExpression constLeft && expression.Right is MemberExpression memberRight)
            {
                string fieldName = memberRight.Member.Name;
                BsonValue value = new BsonValue(constLeft.Value);
                switch (expression.NodeType)
                {
                    case ExpressionType.Equal:
                        return Query.EQ(fieldName, value);
                    case ExpressionType.NotEqual:
                        return Query.Not(Query.EQ(fieldName, value));
                    case ExpressionType.GreaterThanOrEqual:
                        // constant >= member  => member <= constant
                        return Query.LTE(fieldName, value);
                    case ExpressionType.LessThanOrEqual:
                        // constant <= member  => member >= constant
                        return Query.GTE(fieldName, value);
                    case ExpressionType.GreaterThan:
                        return Query.LT(fieldName, value);
                    case ExpressionType.LessThan:
                        return Query.GT(fieldName, value);
                }
            }
            return null; // Fallback to LINQ if no conversion applies
        }

        private Query ProcessUnaryExpression(UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Not)
            {
                // Support for expressions like: !string.IsNullOrEmpty(x.Category)
                if (expression.Operand is MethodCallExpression methodCall)
                {
                    if (methodCall.Method.Name == "IsNullOrEmpty" &&
                        methodCall.Method.DeclaringType == typeof(string) &&
                        methodCall.Arguments.Count == 1 &&
                        methodCall.Arguments[0] is MemberExpression member)
                    {
                        string fieldName = member.Member.Name;
                        // Convert: !string.IsNullOrEmpty(x.Category)
                        // To: field is NOT (null OR empty)
                        return Query.Not(Query.Or(
                            Query.EQ(fieldName, BsonValue.Null),
                            Query.EQ(fieldName, new BsonValue(string.Empty))
                        ));
                    }
                }
                // Handle simple negation of a boolean member: !x.Ignored  =>  x.Ignored == false
                else if (expression.Operand is MemberExpression member)
                {
                    return Query.EQ(member.Member.Name, new BsonValue(false));
                }
                // Handle negated binary expressions
                else if (expression.Operand is BinaryExpression binaryOperand)
                {
                    var innerQuery = ProcessBinaryExpression(binaryOperand);
                    if (innerQuery != null)
                    {
                        return Query.Not(innerQuery);
                    }
                }
            }
            return null;
        }


    }
}
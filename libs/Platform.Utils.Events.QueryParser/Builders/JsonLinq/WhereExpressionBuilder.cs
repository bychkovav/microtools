using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Platform.Utils.Events.QueryParser.Builders.JsonLinq.Extensions;
using Platform.Utils.Events.QueryParser.Domain.Enums;

namespace Platform.Utils.Events.QueryParser.Builders.JsonLinq
{
    public class WhereExpressionBuilder
    {
        private static readonly Dictionary<CriteriaComparator, Func<dynamic, dynamic, bool>> comparatorTable =
            new Dictionary<CriteriaComparator, Func<dynamic, dynamic, bool>>
            {
                {CriteriaComparator.Eq, Eq},
                {CriteriaComparator.NotEq, NotEq},
                {CriteriaComparator.Gt, Gt},
                {CriteriaComparator.Ge, Ge},
                {CriteriaComparator.Lt, Lt},
                {CriteriaComparator.Le, Le},
                {CriteriaComparator.In, In},
                {CriteriaComparator.Between, Between},
                {CriteriaComparator.Like, Like},

            };

        public static Expression<Func<T, bool>> GetPredicate<T>(CriteriaComparator comparator, bool not, Func<T, dynamic> accessor, dynamic value)
        {
            var comparatorFunc = comparatorTable[comparator];

            var expr = new Func<T, bool>(x =>
            {
                var res = comparatorFunc(accessor(x), value);


                return res;
            });

            // Inverse WHERE predicate if needed (eg. IN / NOT IN)
            if (not)
                expr = PredicateExtensions.Not(expr);

            Expression<Func<T, bool>> result = x => expr(x);

            return result;
        }

        private static bool Eq(dynamic accessor, dynamic value)
        {
            var res = accessor == value;

            return res;
        }

        private static bool NotEq(dynamic accessor, dynamic value)
        {
            var res = accessor != value;

            return res;
        }

        private static bool Gt(dynamic accessor, dynamic value)
        {
            var res = accessor > value;

            return res;
        }

        private static bool Ge(dynamic accessor, dynamic value)
        {
            var res = accessor >= value;

            return res;
        }

        private static bool Lt(dynamic accessor, dynamic value)
        {
            var res = accessor < value;

            return res;
        }

        private static bool Le(dynamic accessor, dynamic value)
        {
            var res = accessor <= value;

            return res;
        }

        private static bool In(dynamic accessor, dynamic value)
        {
            var res = Enumerable.Contains((IEnumerable<dynamic>)value, (dynamic)accessor.Value);

            return res;
        }

        private static bool Like(dynamic accessor, dynamic value)
        {
            var res = accessor.Contains(value.ToString());

            return res;
        }

        private static bool Between(dynamic accessor, dynamic value)
        {
            var res = accessor >= value[0] && accessor <= value[1];

            return res;
        }
    }
}
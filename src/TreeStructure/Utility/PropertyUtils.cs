using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.Utility {
    public static class PropertyUtils {
        #region static methods
        public static IEnumerable<string> GetPropertyPath<T, TValue>(Expression<Func<T, TValue>> expression) {
            var memberExpression = GetMemberExpression(expression.Body);

            if (memberExpression == null) {
                return Enumerable.Empty<string>();//new string[1] { string.Empty };
            }
            var propNames = new Stack<string>();
            //var propertyNames = new List<string>();

            while (memberExpression != null) {
                //propertyNames.Insert(0, memberExpression.Member.Name);
                propNames.Push(memberExpression.Member.Name);
                memberExpression = GetMemberExpression(memberExpression.Expression);
            }

            return propNames;
        }

        static MemberExpression? GetMemberExpression([AllowNull] Expression expression) {
            if (expression is UnaryExpression unaryExpression) {
                return unaryExpression.Operand as MemberExpression;
            }
            return expression as MemberExpression;
        }

        public static object? GetValueFromPropertyName(object? obj, string propertyName) {
            // obj が null の場合 null を返す
            if (obj == null) return null;
            // obj の型から指定されたプロパティを取得
            PropertyInfo? propertyInfo = obj.GetType().GetProperty(propertyName);

            // プロパティが存在するか確認
            if (propertyInfo != null) {
                // プロパティの値を取得
                return propertyInfo.GetValue(obj);
            } else {
                // プロパティが存在しない場合
                throw new ArgumentException($"Property '{propertyName}' not found in type {obj.GetType().Name}");
            }
        }
        #endregion

    }
}

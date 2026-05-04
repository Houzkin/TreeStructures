using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace TreeStructures.Collections {
	public class ExpressionList<T> : IEnumerable<Expression<Func<T, object>>> {
		List<Expression<Func<T, object>>>? _list;// = new List<Expression<Func<T, object>>>();
		internal ExpressionList() { }
		public ExpressionList(Expression<Func<T, object>> property, params Expression<Func<T, object>>[] properties)
			: this(properties.AddHead(property)) {
		}
		public ExpressionList(IEnumerable<Expression<Func<T,object>>> properties) {
			_list = new List<Expression<Func<T, object>>>(properties);
		}

		public virtual IEnumerator<Expression<Func<T, object>>> GetEnumerator() {
			return _list?.GetEnumerator() ?? Enumerable.Empty<Expression<Func<T, object>>>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
		public static ExpressionList<T> Create(Expression<Func<T,object>> property, params Expression<Func<T, object>>[] properties) {
			return new ExpressionList<T>(property, properties);
		}
	}


}

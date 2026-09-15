using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using TreeStructures.Collections;

namespace TreeStructures.Tree.Dev {
	/**
	 * インデクサの対応、INotifyCollectionChangedを実装したプロパティが指定された時の対応を思考中
	 * ex)
	 * var listner = tree.Subscribe(x=>x.Parent.Children.LastOrdefault().Number);
	 * var listner = tree.Observe(x=>x.Parent.Members[0].Number)
	 *				.Subscribe(a=>...);
	 * var listner = tree.Observe(x=>x.Children, (y,e)=>y.LastOrDefault())
	 *				.Observe(z=>z.Number)
	 *				.Subscribe(a=>...);
	 * var listner = tree.Observe(x=>x.Children, itm=> itm.Score)
	 *				.Subscribe((collection,itm)=>...); 
	 * **/

	public class ObservableMemberTree<TSrc> : IDisposable {
		public ChainedRootNode Root { get; }
		public void Dispose() {
			throw new NotImplementedException();
		}
		public ChainedNodeBase ObserveProperty<TValue>(Expression<Func<TSrc,TValue>> expression){ 
			throw new NotImplementedException();
		}

		public abstract class ChainedNodeBase : TreeNodeBase<ChainedNodeBase> {

		
			protected override bool CanAddChildNode(ChainedNodeBase child) {
				if (child is not ChainedNode) return false;
				return base.CanAddChildNode(child);
			}
			protected override IEnumerable<ChainedNodeBase> SetupInnerChildCollection() {
				return new HashSet<ChainedNode>(Equality<ChainedNode>.ComparerBy(a => a.SourceName));
			}
			public object Source { get; internal set; }
			protected void AddChildProcess(ChainedNodeBase child)
				=> base.InsertChildProcess(0, child, (c, i, n) => ((ICollection<ChainedNode>)c).Add((ChainedNode)n));

			public virtual ChainedNodeBase ObserveProperty<TValue>(Expression<Func<TSrc,TValue>> expression) { throw new NotImplementedException(); }
			public virtual ChainedNodeBase ObserveElements<TCollection,TItm>(Expression<Func<TSrc,TCollection>> expression)
				where TCollection :IEnumerable<TItm>, INotifyCollectionChanged {
				throw new NotImplementedException();
			}
			public virtual ChainedNodeBase ObserveElements<TCollection,TItm,TValue>(Expression<Func<TSrc,TCollection>> collectionExp, Expression<Func<TItm,TValue>> propExp)
				where TCollection : IEnumerable<TItm>, INotifyCollectionChanged {  throw new NotImplementedException(); }
		}
		public class ChainedRootNode : ChainedNodeBase {
			public TSrc Source => base.Source is TSrc src ? src : default(TSrc);
			public override ChainedNodeBase ObserveProperty<TValue>(Expression<Func<TSrc, TValue>> expression) {
				return base.ObserveProperty(expression);
			}
		}
		public class ChainedNode : ChainedNodeBase {
			public int observingCount { get; set; }
			public string SourceName { get; private set; }

		}
		public class ChainedPropertyNode<TValue> : ChainedNode {
			public TValue Source => base.Source is TValue val ? val : default;
		}
		public class ChainedCollectionNode<TItm> : ChainedNode {
			public IEnumerable<TItm> Source => base.Source is IEnumerable<TItm> src ? src : default;
		}
		public class ChainedIndexNode<TValue> : ChainedNode {

		}
	}
	public record ExpressionElement {
		public string Type { get; }
		public string Name { get; }
		public string Detail { get; }
		public ExpressionElement(string type, string name, string detail) => (Type, Name, Detail) = (type, name, detail);
	}

	public class ExpressionAnalyzer : ExpressionVisitor {
		private readonly List<ExpressionElement> _elements = new();
		public IEnumerable<ExpressionElement> Elements => _elements;

		public static IEnumerable<ExpressionElement> Analyze<T, TResult>(Expression<Func<T, TResult>> expression) {
			var analyzer = new ExpressionAnalyzer();
			analyzer.Visit(expression.Body);

			// 式木は後ろ（右）から解析されるため、直感的に左から右の順になるよう反転させる
			return analyzer._elements.AsEnumerable().Reverse();
		}

		protected override Expression VisitMember(MemberExpression node) {
			string type = node.Member switch {
				PropertyInfo p when p.GetMethod?.IsStatic == true => "Static Property",
				PropertyInfo => "Property",
				FieldInfo f when f.IsStatic => "Static Field",
				FieldInfo => "Field",
				_ => "Member"
			};
			_elements.Add(new ExpressionElement(type, node.Member.Name, node.Type.Name));
			return base.VisitMember(node);
		}

		protected override Expression VisitMethodCall(MethodCallExpression node) {
			// インデクサ (List[0] など) の実体は get_Item メソッド
			if (node.Method.Name == "get_Item") {
				var arg = string.Join(", ", node.Arguments.Select(a => a.ToString()));
				_elements.Add(new ExpressionElement("Indexer", "[]", $"Index: {arg}"));
				return Visit(node.Object);
			}

			bool isExtension = node.Method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false);
			_elements.Add(new ExpressionElement(
				isExtension ? "Extension Method" : "Method",
				node.Method.Name,
				node.Method.ToString()
			));

			if (isExtension && node.Arguments.Count > 0) {
				// 拡張メソッドの第1引数が、呼び出し元のオブジェクト
				Visit(node.Arguments[0]);
				// 第2引数以降（Skip(1)の1など）をパースしたい場合はここでVisitするが、チェーンを辿るだけなら不要
				return node;
			}

			return base.VisitMethodCall(node);
		}
		protected override Expression VisitBinary(BinaryExpression node) {
			if (node.NodeType == ExpressionType.ArrayIndex) {
				_elements.Add(new ExpressionElement("Array Index", "[]", $"Index: {node.Right}"));
				return Visit(node.Left);
			}
			return base.VisitBinary(node);
		}

		protected override Expression VisitParameter(ParameterExpression node) {
			_elements.Add(new ExpressionElement("Parameter", node.Name ?? "param", node.Type.Name));
			return base.VisitParameter(node);
		}

	}


}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Collections;
using TreeStructures.Linq;
using Reactive;
using Reactive.Bindings;
using Reactive.Bindings.Helpers;
using Reactive.Bindings.Extensions;

namespace SampleConsoleApp;
public class BinaryWrpr : TreeNodeWrapper<NamedBinaryNode, BinaryWrpr> {
	public BinaryWrpr(NamedBinaryNode namedBinaryNode) : base(namedBinaryNode) { }
	protected BinaryWrpr() : base(null) { }
	protected override BinaryWrpr GenerateChild(NamedBinaryNode sourceChildNode) {
		if(sourceChildNode != null) return new BinaryWrpr(sourceChildNode);
		return new InfiniteLoopEmptyBinaryWrpr();
	}
	public virtual string Name => Source.Name;
}
public class InfiniteLoopEmptyBinaryWrpr : BinaryWrpr{
	public InfiniteLoopEmptyBinaryWrpr() { }
	protected override IEnumerable<BinaryWrpr> SetupPublicChildCollection(CombinableChildrenProxyCollection<BinaryWrpr> children) {
		var appendlist = new ObservableCollection<BinaryWrpr>();
		appendlist.Add(new InfiniteLoopEmptyBinaryWrpr());
		children.AppendCollection(appendlist);
		return children;
	}
	public override string Name => "Empty";
}

public static partial class UseageSample{
	public static void MethodF3() {

		var hrcyA = new OtherHierarchy("A");
		var hrcyB = new OtherHierarchy("B");
		var hrcyC = new OtherHierarchy("C");
		var hrcyD = new OtherHierarchy("D");

		hrcyA.Nests.Add(hrcyB);
		hrcyA.Nests.Add(hrcyC);
		hrcyA.Nests.Add(hrcyD);

		hrcyC.Nests.Add(hrcyA);

		var wrprRoot = hrcyA.AsValuedTreeNode(x => x.Nests, x => x.Name);
		Console.WriteLine(wrprRoot.ToTreeDiagram(x => x.Value));
		Console.WriteLine($"Preorder : {string.Join(',', wrprRoot.PreOrder().Select(x => x.Value))}");
		Console.WriteLine($"Levelorder : {string.Join(',', wrprRoot.PreOrder().Select(x => x.Value))}");
		Console.WriteLine($"Postorder : {string.Join(',', wrprRoot.PostOrder().Select(x => x.Value))}");
		Console.WriteLine($"Inorder : {string.Join(',', wrprRoot.InOrder().Select(x => x.Value))}");
		Console.WriteLine($"Leafs : {string.Join(',', wrprRoot.Leafs().Select(x => x.Value))}\n");

		var wrprC = hrcyC.AsValuedTreeNode(x => x.Nests, x => x.Name);
		Console.WriteLine(wrprC.ToTreeDiagram(x => x.Value));
		Console.WriteLine($"Preorder : {string.Join(',', wrprC.PreOrder().Select(x => x.Value))}");
		Console.WriteLine($"Levelorder : {string.Join(',', wrprC.PreOrder().Select(x => x.Value))}");
		Console.WriteLine($"Postorder : {string.Join(',', wrprC.PostOrder().Select(x => x.Value))}");
		Console.WriteLine($"Inorder : {string.Join(',', wrprC.InOrder().Select(x => x.Value))}");
		Console.WriteLine($"Leafs : {string.Join(',', wrprC.Leafs().Select(x => x.Value))}\n");

		Console.WriteLine("Enumeration is not performed, but access is allowed.");
		Console.WriteLine($"wrprC.Children.First().Children.ElementAt(1).Value : {wrprC.Children.First().Children.ElementAt(1).Value}");

		var conRt = wrprRoot.Convert(x => new NamedNode() { Name = x.Value });
		Console.WriteLine(conRt.ToTreeDiagram(x => x.Name));
		Console.WriteLine($"Preorder : {string.Join(',', conRt.PreOrder().Select(x => x.Name))}");
		Console.WriteLine($"Levelorder : {string.Join(',', conRt.PreOrder().Select(x => x.Name))}");
		Console.WriteLine($"Postorder : {string.Join(',', conRt.PostOrder().Select(x => x.Name))}");
		Console.WriteLine($"Inorder : {string.Join(',', conRt.InOrder().Select(x => x.Name))}");
		Console.WriteLine($"Leafs : {string.Join(',', conRt.Leafs().Select(x => x.Name))}\n");

		var conC = wrprC.ToNodeMap(x => x.Value).AssembleTree(x => new NamedNode() { Name = x });
		Console.WriteLine(conC.ToTreeDiagram(x=>x.Name));
		Console.WriteLine($"Preorder : {string.Join(',', conC.PreOrder().Select(x => x.Name))}");
		Console.WriteLine($"Levelorder : {string.Join(',', conC.PreOrder().Select(x => x.Name))}");
		Console.WriteLine($"Postorder : {string.Join(',', conC.PostOrder().Select(x => x.Name))}");
		Console.WriteLine($"Inorder : {string.Join(',', conC.InOrder().Select(x => x.Name))}");
		Console.WriteLine($"Leafs : {string.Join(',', conC.Leafs().Select(x => x.Name))}\n");
	}
	public static void MethodF2() {
		var ndA = new NamedBinaryNode() { Name = "A" };
		ndA.Right = new NamedBinaryNode() { Name = "B" };

		var wpr = new BinaryWrpr(ndA);

		//Console.WriteLine(wpr.ToTreeDiagram(x => x.Name));//infinite loop
	}
	public static void MethodF(){
		var nodeA = new MemberNode("A");
		var wrprA = new MemberWrapper<MemberNode>(nodeA);
		var wrprAA = new MemberWrapper<MemberNode>(nodeA);

		var NullWrpr1 = new MemberWrapper<MemberNode>(null);
		var NullWrpr2 = new MemberWrapper<MemberNode>(null);
		var NullWrpr3 = NullWrpr2;

		var result1 = wrprA == wrprAA;
		Console.WriteLine(result1);//true

		var result2 = NullWrpr1 == NullWrpr2;
		Console.WriteLine(result2);//false

		var result3 = NullWrpr2 == NullWrpr3;
		Console.WriteLine(result3);//true

		var result4 = wrprA == new MemberWrapper<MemberNode>(new MemberNode("A"));
		Console.WriteLine(result4);//false
		
	}
}

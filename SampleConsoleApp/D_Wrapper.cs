using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Linq;

namespace SampleConsoleApp;
/// <summary>
/// An object forming a composite pattern without implementing ITreeNode.
/// </summary>
public class OtherHierarchy{
	public OtherHierarchy(string name){ 
		Name = name;
	}
	public string Name { get; }
	public virtual IList<OtherHierarchy> Nests { get; } = new List<OtherHierarchy>();
}
/// <summary><inheritdoc/></summary>
/// <remarks>Managing nested objects with an Observable Collection.</remarks>
public class ObservableOtherHierarchy : OtherHierarchy{
	public ObservableOtherHierarchy(string name):base(name){ }
	public override IList<OtherHierarchy> Nests { get; } = new ObservableCollection<OtherHierarchy>();
}
public class RootOtherHierarchy : OtherHierarchy {
	public RootOtherHierarchy(string name) : base(name) { }
}

/// <summary>
/// Wrap objects forming the hierarchy object.
/// <summary>
public class OtherHierarchyWrapper : HierarchyWrapper<OtherHierarchy, OtherHierarchyWrapper> {
	public OtherHierarchyWrapper(OtherHierarchy composite) : base(composite) { }
	protected override IEnumerable<OtherHierarchy>? SourceChildren => Source.Nests;

	protected override OtherHierarchyWrapper GenerateChild(OtherHierarchy sourceChildNode) {
		return new OtherHierarchyWrapper(sourceChildNode);
	}
	public string SourceName => Source.Name;
}
public class BindableOtherHierarchyWrapper : BindableHierarchyWrapper<OtherHierarchy, BindableOtherHierarchyWrapper> {
	public BindableOtherHierarchyWrapper(OtherHierarchy source) : base(source) { }

	protected override IEnumerable<OtherHierarchy>? SourceChildren => Source.Nests;

	protected override BindableOtherHierarchyWrapper GenerateChild(OtherHierarchy sourceChildNode) {
		return new BindableOtherHierarchyWrapper(sourceChildNode);
	}
	public string SourceName => Source.Name;
}
public static partial class UseageSample {
	public static void MethodD(){

		var dic = new Dictionary<int[], string>() {
			[new int[] { }] = "A",
			[new int[] { 0 }] = "B",
			[new int[] { 0, 0 }] = "C",
			[new int[] { 0, 1 }] = "D",
			[new int[] { 1 }] = "E",
			[new int[] { 2 }] = "F",
			[new int[] { 2, 0 }]= "G",
		};
		//Assemble hierarchy object
		var root = dic.AssembleTree(x=>new OtherHierarchy(x),(p,c)=>p.Nests.Add(c));

		//Wrapping
		var wrapRt = new OtherHierarchyWrapper(root);
		Console.WriteLine(wrapRt.ToTreeDiagram(x => x.SourceName));

		//Assemble Observable Hierarchy object
		var obvableRt = wrapRt.Convert(x => new ObservableOtherHierarchy(x.SourceName), (p, c) => p.Nests.Add(c));
		// OR
		//var obvableRt =  dic.AssembleTree(x => new ObservableOtherHierarchy(x), (p, c) => p.Nests.Add(c));

		//Wrapping
		var wrappingObvableRt = new OtherHierarchyWrapper(obvableRt);
		Console.WriteLine("Before");
		Console.WriteLine(wrappingObvableRt.ToTreeDiagram(x=>x.SourceName));

		obvableRt.Nests.First().Nests.Clear();
		Console.WriteLine("After");
		Console.WriteLine(wrappingObvableRt.ToTreeDiagram(x => x.SourceName));

		//For simple data like in this example, you can also use the AsValuedTreeNode method.
		Console.WriteLine("used AsValuedTreeNode method");
		var valuedNode = (obvableRt as OtherHierarchy).AsValuedTreeNode(x => x.Nests, x => x.Name);
		Console.WriteLine(valuedNode.ToTreeDiagram(x => x.Value));

	}
	public static void MethodDD(){
		var root = "ABCDEFG".ToCharArray().Select(x=>x.ToString()).AssembleAsNAryTree(2,x=>new NamedNode(){ Name = x });
		var wrpRt = root.AsValuedTreeNode(x=>x.Name);
		root.PreOrder().First(x => x.Name == "B").TryRemoveOwn();
		Console.WriteLine(root.ToTreeDiagram(x => x.Name));

		var nodeA = new RootOtherHierarchy("A");
		var nodeB = new OtherHierarchy("B");
		var nodeC = new OtherHierarchy("C");
		var nodeD = new ObservableOtherHierarchy("D");

		nodeA.Nests.Add(nodeB);
		nodeA.Nests.Add(nodeD);
		nodeD.Nests.Add(nodeC);

		var node_a = nodeA.AsValuedTreeNode<OtherHierarchy, string>(p => p.Nests, x => x.Name);
	}

	public static void MethodDDD() {
		var tree = "ABCDEFG".ToCharArray().Select(x => x.ToString()).AssembleAsNAryTree(2,
			x => new ObservableOtherHierarchy(x) as OtherHierarchy,
			x => x.Nests,
			(p, c) => p.Nests.Add(c));

		var Wrpr = new OtherHierarchyWrapper(tree);
		var BWrpr = new BindableOtherHierarchyWrapper(tree);
		((INotifyCollectionChanged)Wrpr.Children).CollectionChanged += (s, e) => Console.WriteLine("Wrpr.Children collection changed"); 
		((INotifyCollectionChanged)BWrpr.Children).CollectionChanged += (s, e) =>  Console.WriteLine("BWrpr.Children collection changed");

		tree.Nests.Add(new OtherHierarchy("H"));
		//BWrpr.Children collection changed

		Console.WriteLine(BWrpr.ToTreeDiagram(x => x.SourceName));
		/*
		A
		├ B
		│ ├ D
		│ └ E
		├ C
		│ ├ F
		│ └ G
		└ H
		 * */

		Console.WriteLine(Wrpr.ToTreeDiagram(x => x.SourceName));
		/*
		//Wrpr.Children collection changed
		A
		├ B
		│ ├ D
		│ └ E
		├ C
		│ ├ F
		│ └ G
		└ H 
		 * */

	}
}


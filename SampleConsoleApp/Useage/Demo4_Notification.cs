using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Linq;

namespace SampleConsoleApp;

public static partial class Demo {

	public static void Notification_1() {

		var nodesDic = "ABCDEFGHI".ToCharArray().Select(x => x.ToString()).ToDictionary(x => x, x => new ObservableNamedNode() { Name = x });
		var root = nodesDic.Values.AssembleAsNAryTree(2);
		Console.WriteLine(root.ToTreeDiagram(x => x.Name));

		EventHandler<StructureChangedEventArgs<ObservableNamedNode>> structreChangedHdlr = (s, e) => {
			Console.WriteLine($"sender:{s} \nTarget:{e.Target} TreeAction:{e.TreeAction} PreviousParentOfTarget:{e.PreviousParentOfTarget} OldIndex:{e.OldIndex} IsAncestorChanged:{e.IsAncestorChanged} IsDescendantChanged:{e.IsDescendantChanged}");
			if (e.IsAncestorChanged) {
				var info = e.AncestorInfo!;
				Console.WriteLine($"MovedTarget:{info.MovedTarget} OldIndex:{info.OldIndex} PreviousParentOfTarget:{info.PreviousParentOfTarget} IsRootChanged:{info.IsRootChanged}");
			}
			if (e.IsDescendantChanged) {
				var info = e.DescendantInfo!;
				Console.WriteLine($"Target:{info.Target} SubtreeAction:{info.SubTreeAction} OldIndex:{info.OldIndex} PreviousParentOfTarget:{info.PreviousParentOfTarget}");
			}
			Console.Write("\n");
		};
		PropertyChangedEventHandler propertyChangedHdlr = (s, e) => {
			if(e.PropertyName == "Parent")
				Console.WriteLine($"sender:{s} Parent Changed.\n");
		};

		EventHandler disposedHdlr = (s, e) => { Console.WriteLine($"sender:{s} Disposed.\n"); };

		foreach (var node in root.PreOrder()) {
			node.StructureChanged += structreChangedHdlr;
			node.PropertyChanged += propertyChangedHdlr;
			//node.Disposed += disposedHdlr;
		}
		Console.WriteLine("Remove node D\n");
		nodesDic["B"].RemoveChild(nodesDic["D"]);
		Console.WriteLine(root.ToTreeDiagram(x => x.Name));

		Console.WriteLine("Add node D as a child node to node E.");
		nodesDic["E"].AddChild(nodesDic["D"]);
		Console.WriteLine(root.ToTreeDiagram(x => x.Name));

		Console.WriteLine("Move node E to be a child node of node C.\n");
		nodesDic["C"].AddChild(nodesDic["E"]);
		Console.WriteLine(root.ToTreeDiagram(x => x.Name));

		//Console.WriteLine("Dispose node E \n");
		//nodesDic["E"].Dispose();
		//Console.WriteLine(root.ToTreeDiagram(x => x.Name));

	}
	public static void Disp_2() {
		var dic = new List<ObservableNamedNode>() {
			new DspObsNamedNode(){ Name = "A" },
			new DspObsNamedNode(){ Name = "B" },
			new ObservableNamedNode(){ Name = "C" },
			new ObservableNamedNode(){ Name = "D" },
			new DspObsNamedNode(){ Name = "E" },
		}.ToDictionary(x => x.Name, x => x);

		var root = dic.Values.AssembleAsNAryTree(1);

		EventHandler<StructureChangedEventArgs<ObservableNamedNode>> structreChangedHdlr = (s, e) => {
			if (s == e.Target && e.IsAncestorChanged) {
				Console.WriteLine($"sender:{e.Target} ,TreeAction:{e.TreeAction}");
			}
		};


		foreach (var node in root.PreOrder()) {
			node.StructureChanged += structreChangedHdlr;
		}
		Console.WriteLine(root.ToTreeDiagram(x => x.Name));


		(dic["B"] as DspObsNamedNode)?.Dispose();

		Console.WriteLine(root.ToTreeDiagram(x => x.Name));

		Console.ReadKey();
	}
	public static void Disp_1() {

		var nodesDic = "ABCD".ToCharArray().Select(x => x.ToString()).ToDictionary(x => x, x => new DisposableNode() { Name = x });
		var root = nodesDic.Values.AssembleAsNAryTree(1);

		EventHandler<StructureChangedEventArgs<DisposableNode>> structreChangedHdlr = (s, e) => {
			if (s == e.Target && e.IsAncestorChanged) {
				Console.WriteLine($"sender:{e.Target} ,TreeAction:{e.TreeAction}");
			}
		};

		foreach (var node in root.PreOrder()) {
			node.StructureChanged += structreChangedHdlr;
		}
		Console.WriteLine(root.ToTreeDiagram(x => x.Name));


		nodesDic["B"].Dispose();

		Console.WriteLine(root.ToTreeDiagram(x => x.Name));

		Console.ReadKey();
	}
	public static void NAry_1() {
		var lst = "ABCDEFGH".ToCharArray().Select(x => new CustomNaryTreeNode(2) { Name = x.ToString()});
		var root = lst.AssembleAsNAryTree(3);
		Console.WriteLine(root.ToTreeDiagram(x => $"{x.Name}, BranchIndex: {x.BranchIndex()}"));
		/*
		A, BranchIndex: -1
		├ B, BranchIndex: 0
		│ ├ E, BranchIndex: 0
		│ └ F, BranchIndex: 1
		└ C, BranchIndex: 1
		  └ H, BranchIndex: 0
		 */

		root.RemoveAllChild(x=>x.Name == "B");
		Console.WriteLine(root.ToTreeDiagram(x => $"{x.Name}, BranchIndex: {x.BranchIndex()}"));
		Console.WriteLine("In-Order:" + string.Join(",", root.InOrder().Select(x=>x.Name)));
		/*
		A, BranchIndex: -1
		└ C, BranchIndex: 1
		  └ H, BranchIndex: 0

		In-Order:A,H,C
		 */

		var dic = root.ToNodeMap(x=>x.Name);
		foreach(var pair in dic) 
			Console.WriteLine($"key:{pair.Key}, value:{pair.Value}");
		

		var root1 = dic.AssembleTree(x => new CustomNaryTreeNode(2) { Name = x.ToString() }, (p, c, i) => p.SetChild(i, c));
		Console.WriteLine(root1.ToTreeDiagram(x => $"{x.Name}, BranchIndex: {x.BranchIndex()}"));
		Console.WriteLine("In-Order:" + string.Join(",", root1.InOrder().Select(x=>x.Name)));
		/*
		A, BranchIndex: -1
		└ C, BranchIndex: 1
		  └ H, BranchIndex: 0

		In-Order:A,H,C
		 */

		var root2 = dic.AssembleTree(x=>new CustomNaryTreeNode(2) { Name = x});
		Console.WriteLine(root2.ToTreeDiagram(x => $"{x.Name}, BranchIndex: {x.BranchIndex()}"));
		Console.WriteLine("In-Order:" + string.Join(",", root2.InOrder().Select(x=>x.Name)));
		/*
		A, BranchIndex: -1
		└ C, BranchIndex: 0
		  └ H, BranchIndex: 0

		In-Order:H,C,A
		 */

		var root3 = root.Convert(x => new NamedNode() { Name = x.Name });
		Console.WriteLine(root3.ToTreeDiagram(x => $"{x.Name}, BranchIndex: {x.BranchIndex()}"));
		Console.WriteLine("In-Order:" + string.Join(",", root3.InOrder().Select(x=>x.Name)));
		/*
		A, BranchIndex: -1
		└ C, BranchIndex: 0
		  └ H, BranchIndex: 0

		In-Order:H,C,A
		 */
	}
}

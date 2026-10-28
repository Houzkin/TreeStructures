using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Linq;

namespace SampleConsoleApp;

public static partial class Demo {


	public static void Wrap_1() {
		var layers = "ABCDEF".ToCharArray()
			.Select(x => x.ToString())
			.ToDictionary(x => x, x => new NamedLayer() { Name = x });

		var root = layers.Values.AssembleAsNAryTree(1, x => x, x => x.ChildLayers, (p, c) => p.ChildLayers.Add(c));

		PrintHierarchy(root);
		/*
		└ A
		 　└ B
		 　 　└ C
		 　 　 　└ D
		 　 　 　 　└ E
		 　 　 　 　 　└ F
		*/
		var wRoot = new NamedLayerWrapper(root);
		// Create new (A)

		Console.WriteLine("--- Print wrapper. ---");
		Console.WriteLine(wRoot.ToTreeDiagram(x => x.Name));
		/*
		--- Print wrapper. ---
		Create new (B)
		Create new (C)
		Create new (D)
		Create new (E)
		Create new (F)
		(A)
		└ (B)
		  └ (C)
			└ (D)
			  └ (E)
				└ (F)
		*/
		Console.WriteLine("--- Remove layer E. ---");
		layers["D"].ChildLayers.Remove(layers["E"]);

		Console.WriteLine("--- Add layer E to layer A. ---");
		layers["A"].ChildLayers.Add(layers["E"]);

		Console.WriteLine("--- Print wrapper. ---");
		Console.WriteLine(wRoot.ToTreeDiagram(x => x.Name));
		/*
		--- Remove layer E. ---
		--- Add layer E to layer A. ---
		--- Print wrapper. ---
		Create new (E)
		Removed (E)
		Create new (F)
		(A)
		├ (B)
		│ └ (C)
		│   └ (D)
		└ (E)
		  └ (F)
		*/

		Console.WriteLine("--- Remove layer E. ---");
		layers["A"].ChildLayers.Remove(layers["E"]);

		Console.WriteLine("--- Insert layer E to layer A at index 0. ---");
		layers["A"].ChildLayers.Insert(0, layers["E"]);

		Console.WriteLine("--- Print wrapper. ---");
		Console.WriteLine(wRoot.ToTreeDiagram(x => x.Name));
		/*
		--- Remove layer E. ---
		--- Insert layer E to layer A at index 0. ---
		--- Print wrapper. ---
		(A)
		├ (E)
		│ └ (F)
		└ (B)
		  └ (C)
			└ (D)
		 */

		var wRoot2 = root.AsValuedTreeNode(x => x.ChildLayers, x => "[" + x.Name + "]");
		Console.WriteLine(wRoot2.ToTreeDiagram(x => x.Value));
		/*
		[A]
		├ [E]
		│ └ [F]
		└ [B]
		  └ [C]
			└ [D]
		 */
	}
	public static void Wrap_2() {
		var nodes = "ABCDEF".ToCharArray()
			.Select(x => x.ToString())
			.ToDictionary(x => x, x => new ObservableNamedNode() { Name = x });
		var root = nodes.Values.AssembleAsNAryTree(1);

		Console.WriteLine(root.ToTreeDiagram(x => x.Name));

		var wRoot = new ReadOnlyLinkedNamedNode(root);

		Console.WriteLine("--- Print wrapper. ---");
		Console.WriteLine(wRoot.ToTreeDiagram(x => x.Name));

		Console.WriteLine("--- Remove node E. ---");
		nodes["D"].RemoveChild(nodes["E"]);

		Console.WriteLine("--- Add node E to node A. ---");
		nodes["A"].AddChild(nodes["E"]);

		Console.WriteLine("--- Print wrapper. ---");
		Console.WriteLine(wRoot.ToTreeDiagram(x => x.Name));


		Console.WriteLine("--- Remove node E. ---");
		nodes["A"].RemoveChild(nodes["E"]);

		Console.WriteLine("--- Insert node E to node A at index 0. ---");
		nodes["A"].InsertChild(0, nodes["E"]);

		Console.WriteLine("--- Print wrapper. ---");
		Console.WriteLine(wRoot.ToTreeDiagram(x => x.Name));


		Console.ReadKey();
	}
	public static void Wrap_3() {
		var nodes = "ABCD".ToCharArray()
			.Select(x => x.ToString())
			.ToDictionary(x=>x,x=>new ObservableNamedNode() { Name = x });
		var root = nodes.Values.AssembleAsNAryTree(1);


		var wRoot = new ReadOnlyLinkedNamedNode(root);
		//Console.WriteLine(wRoot.ToTreeDiagram(x => x.Name));
		foreach (var wrp in wRoot.LevelOrder()) {
			if (wrp.Children is INotifyCollectionChanged clds) {
				clds.CollectionChanged += (s, e) => {
					if (e.OldItems != null && e.OldItems[0] is ReadOnlyLinkedNamedNode node) {
						Console.WriteLine($"sender:{node.Name}, action:{e.Action}, IsDisposed:{node.IsDisposed}");
					}
				};
			}
		}

		wRoot.DescendArrivals(x => x.Name, new string[] {"{A}","{B}"}).First().Dispose();

		Console.WriteLine(wRoot.ToTreeDiagram(x=> $"{x.Name}, IsDisposed = {x.IsDisposed}"));

		Console.ReadKey();
	}
	public static void Wrap_4() {
		var nodes = "ABCD".ToCharArray()
			.Select(x => x.ToString())
			.ToDictionary(x=>x,x=>new ObservableNamedNode() { Name = x });
		var root = nodes.Values.AssembleAsNAryTree(1);
		Console.WriteLine(root.ToTreeDiagram(x => x.Name));

		var wRoot = new ReadOnlyLinkedNamedNode(root);

		wRoot.Children.First().Dispose();
		Console.WriteLine(wRoot.ToTreeDiagram(x=> $"{x.Name}, IsDisposed = {x.IsDisposed}"));

		Console.ReadKey();
	}
}
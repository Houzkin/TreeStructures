using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Linq;

namespace SampleConsoleApp;

public class LoopNode : NamedNode {
	protected override bool CanAddChildNode(NamedNode child) {
		if (child is null) return false;
		else return true;
	}
}
public static partial class Demo {

	public static void Caveat_1() {
		var layerA = new NamedLayer() { Name = "A" };
		var layerB = new NamedLayer() { Name = "B" };
		var layerC = new NamedLayer() { Name = "C" };
		var layerD = new NamedLayer() { Name = "D" };

		layerA.ChildLayers.Add(layerB);
		layerA.ChildLayers.Add(layerC);
		layerA.ChildLayers.Add(layerD);
		layerC.ChildLayers.Add(layerA);//cycle

		var wrpr1 = layerA.AsValuedTreeNode(x => x.ChildLayers, x => x);
		Console.WriteLine(wrpr1.ToTreeDiagram(x => x.Value.Name));
		Console.WriteLine($"PreOrder: {string.Join(',', wrpr1.PreOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"PostOrder: {string.Join(',', wrpr1.PostOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"LevelOrder: {string.Join(',', wrpr1.LevelOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"InOrder: {string.Join(',', wrpr1.InOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"Leafs: {string.Join(',', wrpr1.Leafs().Select(x => x.Value.Name))}");
		Console.WriteLine($"Upstream: {string.Join(',', wrpr1.Upstream().Select(x => x.Value.Name))}");


		var wrpr2C = layerC.AsValuedTreeNode(x => x.ChildLayers, x => x);
		Console.WriteLine(wrpr2C.ToTreeDiagram(x => x.Value.Name));
		Console.WriteLine($"PreOrder: {string.Join(',', wrpr2C.PreOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"PostOrder: {string.Join(',', wrpr2C.PostOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"LevelOrder: {string.Join(',', wrpr2C.LevelOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"InOrder: {string.Join(',', wrpr2C.InOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"Leafs: {string.Join(',', wrpr2C.Leafs().Select(x => x.Value.Name))}");
		Console.WriteLine($"Upstream: {string.Join(',', wrpr2C.Upstream().Select(x => x.Value.Name))}");

		Console.WriteLine(string.Join(",",
			wrpr2C.LevelOrder().First(x => x.Value.Name == "A")
			.Children.Select(x => x.Value.Name)));

		var wrpr2A = wrpr2C.LevelOrder().First(x=>x.Value.Name == "A");
		Console.WriteLine(wrpr2A.ToTreeDiagram(x => x.Value.Name));
		Console.WriteLine($"PreOrder: {string.Join(',', wrpr2A.PreOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"PostOrder: {string.Join(',', wrpr2A.PostOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"LevelOrder: {string.Join(',', wrpr2A.LevelOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"InOrder: {string.Join(',', wrpr2A.InOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"Leafs: {string.Join(',', wrpr2A.Leafs().Select(x => x.Value.Name))}");
		Console.WriteLine($"Upstream: {string.Join(',', wrpr2A.Upstream().Select(x => x.Value.Name))}");

		var newWrpr1 = wrpr1.Convert(x => new NamedLayer() { Name = x.Value.Name }, (p, c) => p.ChildLayers.Add(c))
			.AsValuedTreeNode(x => x.ChildLayers, x => x);
		Console.WriteLine(newWrpr1.ToTreeDiagram(x => x.Value.Name));
		Console.WriteLine($"PreOrder: {string.Join(',', newWrpr1.PreOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"PostOrder: {string.Join(',', newWrpr1.PostOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"LevelOrder: {string.Join(',', newWrpr1.LevelOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"InOrder: {string.Join(',', newWrpr1.InOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"Leafs: {string.Join(',', newWrpr1.Leafs().Select(x => x.Value.Name))}");
		Console.WriteLine($"Upstream: {string.Join(',', newWrpr1.Upstream().Select(x => x.Value.Name))}");


		var newWrpr2C = wrpr2C.ToNodeMap(x => x.Value.Name).AssembleTree(x => new NamedLayer() { Name = x }, (p, c) => p.ChildLayers.Add(c))
			.AsValuedTreeNode(x => x.ChildLayers, x => x);
		Console.WriteLine(newWrpr2C.ToTreeDiagram(x => x.Value.Name));
		Console.WriteLine($"PreOrder: {string.Join(',', newWrpr2C.PreOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"PostOrder: {string.Join(',', newWrpr2C.PostOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"LevelOrder: {string.Join(',', newWrpr2C.LevelOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"InOrder: {string.Join(',', newWrpr2C.InOrder().Select(x => x.Value.Name))}");
		Console.WriteLine($"Leafs: {string.Join(',', newWrpr2C.Leafs().Select(x => x.Value.Name))}");
		Console.WriteLine($"Upstream: {string.Join(',', newWrpr2C.Upstream().Select(x => x.Value.Name))}");

		Console.ReadKey();
	}

	public static void Caveat_2() {
		var nodeA = new LoopNode() { Name = "A" };
		var nodeB = new LoopNode() { Name = "B" };
		var nodeC = new LoopNode() { Name = "C" };
		var nodeD = new LoopNode() { Name = "D" };

		nodeA.AddChild(nodeB)
			.AddChild(nodeC)
			.AddChild(nodeD);

		nodeC.AddChild(nodeA);

		Console.WriteLine(nodeA.ToTreeDiagram(x => x.Name));
		Console.WriteLine(nodeC.ToTreeDiagram(x => x.Name));


		Console.ReadKey();
	}
}

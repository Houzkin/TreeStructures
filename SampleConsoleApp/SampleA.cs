using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Linq;

namespace SampleConsoleApp;

[Serializable]
public class NamedNode : GeneralTreeNode<NamedNode> {
    public NamedNode() { }
    public string Name { get; set; }
    public override string ToString() {
        return this.Name;
    }

}
public static　partial class UseageSample {
    public static void MethodA() {
        var A = new NamedNode { Name = "A" };
        var B = new NamedNode { Name = "B"};
        var C = new NamedNode { Name = "C"};
        var D = new NamedNode { Name = "D"};
        var E = new NamedNode { Name = "E"};
        var F = new NamedNode { Name = "F"};
        var G = new NamedNode { Name = "G" };
        var H = new NamedNode { Name = "H"};
        var I = new NamedNode { Name = "I"};
        var J = new NamedNode { Name = "J" };
        var K = new NamedNode { Name = "K" };
        var L = new NamedNode { Name = "L"};
        var M = new NamedNode { Name = "M"};
        var N = new NamedNode { Name = "N"};
        var O = new NamedNode { Name = "O"};
        var P = new NamedNode { Name = "P"};

        Console.WriteLine("Assemble using the AddChild method.");
        A.AddChild(B).AddChild(C).AddChild(D);
        B.AddChild(E).AddChild(F);
        F.AddChild(G).AddChild(H);
        C.AddChild(I);
        I.AddChild(J).AddChild(K).AddChild(L);
        D.AddChild(M);
        M.AddChild(N);
        N.AddChild(O).AddChild(P);

        Console.WriteLine(A.ToTreeDiagram(x => x.Name));
        Console.WriteLine("Displaying height, level, and node index as additional information.");
        Console.WriteLine(A.ToTreeDiagram(x => $"Name:{x.Name},Height:{x.Height()},Depth:{x.Depth()},NodeIndex:{x.NodeIndex()}"));


        Console.WriteLine("Move nodeN to be a child node of nodeE");
        E.AddChild(N);
        Console.WriteLine(A.ToTreeDiagram(x => x.Name));

		Console.WriteLine("Move nodeB to be a child node of nodeF (Failure: Cannot add due to cyclic relationship)");
		F.AddChild(B);
		Console.WriteLine(A.ToTreeDiagram(x => x.Name));

        Console.WriteLine("Add nodeD to nodeA (Failure: Cannot add duplicate child nodes)");
        A.AddChild(D);
        Console.WriteLine(A.ToTreeDiagram(x => x.Name));


		A.Disassemble();
        //Console.ReadLine();

        Console.WriteLine("Assembling from a Dictionary with specified indices.");
        var dic = new Dictionary<int[], NamedNode>() {
            [new int[] { }] = A,
            [new int[] { 0 }] = I,
            [new int[] { 0, 0 }] = C,
            [new int[] { 0, 1 }] = L,
            [new int[] { 0, 0, 0, }] = D,
            [new int[] { 0, 0, 0, 0 }] = E,
            [new int[] { 0, 0, 0, 1 }]=J,
            [new int[] { 0, 0, 0, 0, 0 }]=F,
            [new int[] { 0, 0, 0, 0, 1 }]=K,
            [new int[] { 1 }]=G,
            [new int[] { 1, 0 }]=H,
            [new int[] { 1, 1 }]=B,
            [new int[] { 1, 2 }]=M,
            [new int[] { 1, 3 }]=N,
            [new int[] { 1, 0, 0 }]=O,
            [new int[] { 1, 3, 0 }]=P,
        };
        Console.WriteLine(dic.AssembleTree().ToTreeDiagram(x => x.Name));
        Console.WriteLine("Displaying additional information such as indices assigned from parent nodes and paths.");
        Console.WriteLine(A.ToTreeDiagram(x => $"Name:{x.Name},BranchIndex:{x.BranchIndex()},NodePath:{x.NodePath(y=>y.Name)}"));

        A.Disassemble(); 
        //Console.ReadLine();


        Console.WriteLine("Building a collection as an N-ary tree.");
        var root = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Select(x => x.ToString())
            .AssembleAsNAryTree(3, x => new NamedNode() { Name = x });
        
        Console.WriteLine(root.ToTreeDiagram(x => x.Name));
        Console.WriteLine($"Levelorder:{string.Join(",", root.Levelorder().Select(x=>x.Name))}");
        Console.WriteLine($"Preorder:{string.Join(",", root.Preorder().Select(x => x.Name))}");
        Console.WriteLine($"Postorder:{string.Join(",", root.Postorder().Select(x => x.Name))}");
        Console.WriteLine($"Inorder:{string.Join(",", root.Inorder().Select(x => x.Name))}");

        Console.WriteLine("\n ----- Pickup C node -----");
        var nodeC = root.Levelorder().First(x => x.Name == "C");
        Console.WriteLine($"Upstream:{string.Join(",", nodeC.Upstream().Select(x=>x.Name))}");
        Console.WriteLine($"Ancestors:{string.Join(",", nodeC.Ancestors().Select(x => x.Name))}");
        Console.WriteLine($"Leafs:{string.Join(", ", nodeC.Leafs().Select(x=>x.Name))}");
        Console.WriteLine($"Genarations:{string.Join(",", nodeC.Generations().Select(x => x.Name))}");
        Console.WriteLine($"Siblings:{string.Join(",", nodeC.Siblings().Select(x=>x.Name))}");
        Console.WriteLine($"Width:{nodeC.Width()}");

        Console.WriteLine("\n ----- Pickup R node -----");
        var nodeR = root.Levelorder().First(x => x.Name == "R");
        Console.WriteLine($"Upstream:{string.Join(",", nodeR.Upstream().Select(x => x.Name))}");
        Console.WriteLine($"Ancestors:{string.Join(",", nodeR.Ancestors().Select(x => x.Name))}");
        Console.WriteLine($"Leafs:{string.Join(", ", nodeR.Leafs().Select(x => x.Name))}");
        Console.WriteLine($"Genarations:{string.Join(",", nodeR.Generations().Select(x => x.Name))}");
        Console.WriteLine($"Siblings:{string.Join(",", nodeR.Siblings().Select(x => x.Name))}");
        Console.WriteLine($"Width:{nodeR.Width()}");
        Console.WriteLine($"Root:{nodeR.Root().Name}");

        //Console.ReadLine();
    }
}
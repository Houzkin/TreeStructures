using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Linq;

namespace SampleConsoleApp;
public static　partial class SampleA {
    public static void Method() {
        var A = new ExampleNode { Name = "A" };
        var B = new ExampleNode { Name = "B"};
        var C = new ExampleNode { Name = "C"};
        var D = new ExampleNode { Name = "D"};
        var E = new ExampleNode { Name = "E"};
        var F = new ExampleNode { Name = "F"};
        var G = new ExampleNode { Name = "G" };
        var H = new ExampleNode { Name = "H"};
        var I = new ExampleNode { Name = "I"};
        var J = new ExampleNode { Name = "J" };
        var K = new ExampleNode { Name = "K" };
        var L = new ExampleNode { Name = "L"};
        var M = new ExampleNode { Name = "M"};
        var N = new ExampleNode { Name = "N"};
        var O = new ExampleNode { Name = "O"};
        var P = new ExampleNode { Name = "P"};

        Console.WriteLine("AddChildメソッドで組み立てる");
        A.AddChild(B).AddChild(C).AddChild(D);
        B.AddChild(E).AddChild(F);
        F.AddChild(G).AddChild(H);
        C.AddChild(I);
        I.AddChild(J).AddChild(K).AddChild(L);
        D.AddChild(M);
        M.AddChild(N);
        N.AddChild(O).AddChild(P);

        Console.WriteLine(A.ToTreeDiagram(x => x.Name));
        Console.WriteLine("高さ、レベル、ノードインデックスも追加で表示");
        Console.WriteLine(A.ToTreeDiagram(x => $"Name:{x.Name},Height:{x.Height()},Depth:{x.Depth()},NodeIndex:{x.NodeIndex()}"));
        A.Disassemble();//全て分解
        Console.ReadLine();

        Console.WriteLine("インデックスを指定したDictionaryから組み立てる");
        var dic = new Dictionary<int[], ExampleNode>() {
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
        Console.WriteLine("親ノードから振り当てられているインデックスと、パスを追加で表示");
        Console.WriteLine(A.ToTreeDiagram(x => $"Name:{x.Name},BranchIndex:{x.BranchIndex()},NodePath:{x.NodePath(y=>y.Name)}"));

        A.Disassemble();//全て分解
        Console.ReadLine();

        Console.WriteLine("コレクションをN分木として組み立てる");
        var root = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Select(x => x.ToString())
            .AssembleAsNAryTree(3, x => new ExampleNode() { Name = x });
        
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

        Console.ReadLine();
    }
}

public class ExampleNode : TreeNode<ExampleNode> {
    public ExampleNode() { }
    public string Name { get; set; }
    public override string ToString() {
        return this.Name;
    }
}
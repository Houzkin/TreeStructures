using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Linq;
using TreeStructures.Tree;

namespace SampleConsoleApp;
public class NamedBinaryNode : BinaryTreeNode<NamedBinaryNode> {
    public string Name { get; set; }
}
public static partial class UseageSample {
    public static void MethodC() {

        Console.WriteLine("Create a binary tree.");
        var A = new NamedBinaryNode() { Name = "A" };
        var B = new NamedBinaryNode() { Name = "B" };
        var C = new NamedBinaryNode() { Name = "C" };
        var D = new NamedBinaryNode() { Name = "D" };
        var E = new NamedBinaryNode() { Name = "E" };
        var F = new NamedBinaryNode() { Name = "F" };
        var G = new NamedBinaryNode() { Name = "G" };

        A.Left = B;
        A.Right = C;
        B.Right = D;
        D.Left = E;
        C.Right = F;
        F.Left = G;

        Console.WriteLine(A.ToTreeDiagram(x => x.Name));
        Console.WriteLine($"Inorder:{string.Join(",", A.Inorder().Select(x => x.Name))}");

        Console.WriteLine("\nTransform each node into a different node and assemble them.");
        var convertedRoot = A.Convert(
            x => new NamedBinaryNode() { Name = x.Name.ToLower() },
            (i, p, c) => p.SetChild(i, c));

        Console.WriteLine(convertedRoot.ToTreeDiagram(x => $"({x.Name})"));
        Console.WriteLine($"Inorder:{string.Join(",", convertedRoot.Inorder().Select(x => x.Name))}");

        Console.WriteLine("\nConvert the value of each node into a node map represented by a Dictionary and assemble them.");
		//Convert to a Dictionary with NodeIndex as the key.
		var dic = A.ToNodeMap(x => x.Name);
		//Assemble using NodeIndex.
		var assembledRoot = dic.AssembleTree(
            x => new NamedBinaryNode() { Name = x },
            (i, p, c) => p.SetChild(i, c));

        Console.WriteLine(assembledRoot.ToTreeDiagram(x => $"[{x.Name}]"));
        Console.WriteLine($"Inorder:{string.Join(",", assembledRoot.Inorder().Select(x => x.Name))}");

        Console.WriteLine("\nReassemble into nodes without fixed branches (GeneralTreeNode) for comparison.");
        var exroot = A.Convert(x => new NamedNode() { Name = x.Name });
        Console.WriteLine(exroot.ToTreeDiagram(x => x.Name));

        Console.WriteLine($"Inorder:{string.Join(",", exroot.Inorder().Select(x => x.Name))}");

        //Console.ReadLine();

    }
}


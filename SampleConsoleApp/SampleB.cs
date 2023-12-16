using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Tree;

namespace SampleConsoleApp;
public class BSampleBinary : BinaryTreeNode<BSampleBinary> {
    protected override ObservableCollection<BSampleBinary> ChildNodes { get; } = new();

    ReadOnlyObservableCollection<BSampleBinary>? _children;
    public override IEnumerable<BSampleBinary> Children => _children ??= new ReadOnlyObservableCollection<BSampleBinary>(ChildNodes);
    public string Value { get; set; }
    
}
public class BSampleImitator : TreeNodeImitator<BSampleBinary, BSampleImitator> {
    public BSampleImitator(BSampleBinary sourceNode) : base(sourceNode) { }
    public string Value => "(" + (this.SourceNode.Value ?? string.Empty) + ")";
    protected override BSampleImitator GenerateChild(BSampleBinary sourceChildNode) {
        if (sourceChildNode == null) return null; 
        return new BSampleImitator(sourceChildNode);
    }
       
}
internal class SampleB {
    public static void Method1() {
        
        Console.WriteLine("バイナリーツリーを作成");
        var root = "ABCDEF00IJ0LMNOPQ0S00VWX0Z".ToCharArray()
            .CreateAsNAryTree(2, x => new BSampleBinary() { Value = x.ToString() }, (p, c) => p.AddChild(c));

        Console.WriteLine(root.ToTreeDiagram(x=>x.Value));
        Console.WriteLine($"Preorder:{string.Join(",", root.Preorder().Select(x=>x.Value))}");
        Console.WriteLine($"Inorder:{string.Join(',', root.Inorder().Select(x => x.Value))}");

        Console.WriteLine("\nValueが０のノードを全て削除する");
        root.RemoveDescendant(x => x.Value == "0", (p, c) => p.RemoveChild(c));

        Console.WriteLine(root.ToTreeDiagram(x => x.Value));
        Console.WriteLine($"Preorder:{string.Join(",", root.Preorder().Select(x=>x.Value))}");
        Console.WriteLine($"Inorder:{string.Join(',', root.Inorder().Select(x=>x.Value))}");

        Console.WriteLine("\n各ノードを別のノードに変換して組み立てる");
        var convertedRoot = root.Convert(x => new BSampleBinary() { Value = x.Value },(p,c)=>p.AddChild(c));
        Console.WriteLine(convertedRoot.ToTreeDiagram(x => $"({x.Value})"));
        Console.WriteLine($"Preorder:{string.Join(",", convertedRoot.Preorder().Select(x => x.Value))}");
        Console.WriteLine($"Inorder:{string.Join(',', convertedRoot.Inorder().Select(x => x.Value))}");

        Console.WriteLine("\n各ノードの値をDictionaryで表すノードマップに変換して、組み立てる");
        //キーにNodeIndexをとるDictionaryに変換
        var dic = root.ToNodeMap(x => x.Value);
        //NodeIndexを使って組み立てる
        var assembledRoot = dic.AssembleTree(x => new BSampleBinary() { Value = x }, (p, c) => p.AddChild(c));

        Console.WriteLine(assembledRoot.ToTreeDiagram(x=> $"[{x.Value}]"));
        Console.WriteLine($"Preorder:{string.Join(",", assembledRoot.Preorder().Select(x => x.Value))}");
        Console.WriteLine($"Inorder:{string.Join(',', assembledRoot.Inorder().Select(x => x.Value))}");

        Console.ReadLine();
    }
    public static void Method2() {

        var root = "ABCDEF00IJ0LMNOPQ0S00VWX0Z".ToCharArray()
            .CreateAsNAryTree(2, x => new BSampleBinary() { Value = x.ToString() }, (p, c) => p.AddChild(c));

        var wrapperRoot = new BSampleImitator(root);

        Console.WriteLine(root.ToTreeDiagram(x => x.Value));

        Console.WriteLine(wrapperRoot.ToTreeDiagram(x => x.Value));

        root.RemoveDescendant(x => x.Value == "0", (p, c) => p.RemoveChild(c));

        Console.WriteLine(root.ToTreeDiagram(x => x.Value));
        Console.WriteLine(wrapperRoot.ToTreeDiagram(x => x.Value));

        Console.ReadLine();
    }
}


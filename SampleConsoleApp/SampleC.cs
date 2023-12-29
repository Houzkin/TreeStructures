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
public class SampleBinary : BinaryTreeNode<SampleBinary> {
    public string Value { get; set; }
    protected override IEnumerable<SampleBinary> SetupInnerChildCollection()
        => new ObservableCollection<SampleBinary>();
    protected override IEnumerable<SampleBinary> SetupPublicChildCollection(IEnumerable<SampleBinary> innerCollection) {
        return new ReadOnlyObservableCollection<SampleBinary>(innerCollection as ObservableCollection<SampleBinary>);
    }
}
public class InheritNary : NAryTreeNode<InheritNary> {
    public InheritNary(int nary) : base(nary) { }

    protected override IEnumerable<InheritNary> SetupInnerChildCollection()
        => new ObservableCollection<InheritNary>();
    protected override IEnumerable<InheritNary> SetupPublicChildCollection(IEnumerable<InheritNary> innerCollection) {
        return new ReadOnlyObservableCollection<InheritNary>(innerCollection as ObservableCollection<InheritNary>);
    }
}
internal class SampleC {
    public static void Method1() {

        Console.WriteLine("バイナリーツリーを作成");
        var A = new SampleBinary() { Value = "A" };
        var B = new SampleBinary() { Value = "B" };
        var C = new SampleBinary() { Value = "C" };
        var D = new SampleBinary() { Value = "D" };
        var E = new SampleBinary() { Value = "E" };
        var F = new SampleBinary() { Value = "F" };
        var G = new SampleBinary() { Value = "G" };

        A.Left = B;
        A.Right = C;
        B.Right = D;
        D.Left = E;
        C.Right = F;
        F.Left = G;

        Console.WriteLine(A.ToTreeDiagram(x => x.Value));
        Console.WriteLine($"Inorder:{string.Join(",", A.Inorder().Select(x => x.Value))}");

        Console.WriteLine("\n各ノードを別のノードに変換して組み立てる");
        var convertedRoot = A.Convert(
            x => new SampleBinary() { Value = x.Value },
            (i, p, c) => p.SetChild(i, c));

        Console.WriteLine(convertedRoot.ToTreeDiagram(x => $"({x.Value})"));
        Console.WriteLine($"Inorder:{string.Join(",", convertedRoot.Inorder().Select(x => x.Value))}");

        Console.WriteLine("\n各ノードの値をDictionaryで表すノードマップに変換して、組み立てる");
        //キーにNodeIndexをとるDictionaryに変換
        var dic = A.ToNodeMap(x => x.Value);
        //NodeIndexを使って組み立てる
        var assembledRoot = dic.AssembleTree(
            x => new SampleBinary() { Value = x },
            (i, p, c) => p.SetChild(i, c));

        Console.WriteLine(assembledRoot.ToTreeDiagram(x => $"[{x.Value}]"));
        Console.WriteLine($"Inorder:{string.Join(",", assembledRoot.Inorder().Select(x => x.Value))}");

        Console.WriteLine("\n比較のため、Branchを固定しないノードに組み替える");
        var exroot = A.Convert(x => new ExampleNode() { Name = x.Value });
        Console.WriteLine(exroot.ToTreeDiagram(x => x.Name));

        Console.WriteLine($"Inorder:{string.Join(",", exroot.Inorder().Select(x => x.Name))}");

        Console.ReadLine();

        ////Step4) Imitatorを初期化
        //var imitatorRoot = new SampleImitator(A);

        //Console.WriteLine("\nSampleImitatorのツリーを表示");
        //Console.WriteLine(imitatorRoot.ToTreeDiagram(x => $"「{x.Value}」"));
        //Console.WriteLine($"Inorder:{string.Join(",", imitatorRoot.Inorder().Select(x => x.Value))}");


        //Console.WriteLine("\nSampleImitatorをバイナリーツリーに変換し、Valueが0のノードを削除");
        ////Step5) Imitatorから変換し、EmptyNodeに該当するノードを削除
        //var A2 = imitatorRoot.Convert(x => new SampleBinary() { Value = x.Value }, (i, p, c) => p.AddChild(c));
        //A2.RemoveDescendant(x => x.Value == "0", (p, c) => p.RemoveChild(c));

        //Console.WriteLine(A2.ToTreeDiagram(x => x.Value));
        //Console.WriteLine($"Inorder:{string.Join(",", A2.Inorder().Select(x => x.Value))}");

        //Console.ReadLine();
    }
}


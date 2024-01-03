using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Linq;

namespace SampleConsoleApp;
/// <summary>
/// An object forming a composite pattern without implementing ITreeNode.
/// </summary>
public class OtherComposite{
	public OtherComposite(string name){ 
		Name = name;
	}
	public string Name { get; }
	public virtual IList<OtherComposite> Nests { get; } = new List<OtherComposite>();
}
/// <summary><inheritdoc/></summary>
/// <remarks>Managing nested objects with an Observable Collection.</remarks>
public class ObservableOtherComposite : OtherComposite{
	public ObservableOtherComposite(string name):base(name){ }
	public override IList<OtherComposite> Nests { get; } = new ObservableCollection<OtherComposite>();
}

/// <summary>
/// Wrap objects forming the Composite Pattern.
/// </summary>
public class OtherCompositeWrapper : CompositeWrapper<OtherComposite, OtherCompositeWrapper> {
	public OtherCompositeWrapper(OtherComposite composite) : base(composite) { }
	protected override IEnumerable<OtherComposite>? SourceChildren => Source.Nests;

	protected override OtherCompositeWrapper GenerateChild(OtherComposite sourceChildNode) {
		return new OtherCompositeWrapper(sourceChildNode);
	}
	public string SourceName =>"[" + Source.Name + "]";
}
internal class SampleD {
	public static void Method(){

		var dic = new Dictionary<int[], string>() {
			[new int[] { }] = "A",
			[new int[] { 0 }] = "B",
			[new int[] { 0, 0 }] = "C",
			[new int[] { 0, 1 }] = "D",
			[new int[] { 1 }] = "E",
			[new int[] { 2 }] = "F",
			[new int[] { 2, 0 }]= "G",
		};
		//Assemble Composite object
		var cmpRoot = dic.AssembleTree(x=>new OtherComposite(x),(p,c)=>p.Nests.Add(c));

		//Wrapping
		var wrpRoot = new OtherCompositeWrapper(cmpRoot);
		Console.WriteLine(wrpRoot.ToTreeDiagram(x => x.SourceName));

		//Assemble Observable Composite object
		var obvableCmpRoot = wrpRoot.Convert(x => new ObservableOtherComposite(x.SourceName), (p, c) => p.Nests.Add(c));
		// OR
		//var observableCmpRoot =  dic.AssembleTree(x => new ObservableOtherComposite(x), (p, c) => p.Nests.Add(c));

		//Wrapping
		var wrappingObvableCmpRoot = new OtherCompositeWrapper(obvableCmpRoot);
		Console.WriteLine("Before");
		Console.WriteLine(wrappingObvableCmpRoot.ToTreeDiagram(x=>x.SourceName));

		obvableCmpRoot.Nests.First().Nests.Clear();
		Console.WriteLine("After");
		Console.WriteLine(wrappingObvableCmpRoot.ToTreeDiagram(x => x.SourceName));

		//For simple data like in this example, you can also use the AsValuedTreeNode method.
		Console.WriteLine("used AsValuedTreeNode method");
		var valuedNode = (obvableCmpRoot as OtherComposite).AsValuedTreeNode(x => x.Nests, x => x.Name);
		Console.WriteLine(valuedNode.ToTreeDiagram(x => x.Value));

	}
}


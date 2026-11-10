using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Linq;

namespace SampleConsoleApp;

public class Example1TreeNode : TreeNodeBase<Example1TreeNode> {

	protected override IEnumerable<Example1TreeNode> SetupInnerChildCollection()
		=> new ObservableCollection<Example1TreeNode>();

	protected override IEnumerable<Example1TreeNode> SetupPublicChildCollection(IEnumerable<Example1TreeNode> innerCollection)
		=> new ReadOnlyObservableCollection<Example1TreeNode>((innerCollection as ObservableCollection<Example1TreeNode>)!);

	public new ReadOnlyObservableCollection<Example1TreeNode> Children => (base.Children as ReadOnlyObservableCollection<Example1TreeNode>)!;
}
public class Example2TreeNode : TreeNodeBase<Example2TreeNode> {
	protected override IEnumerable<Example2TreeNode> SetupInnerChildCollection()
		=> new HashSet<Example2TreeNode>();
	protected override void InsertChildCore(int index, Example2TreeNode child, Action<IEnumerable<Example2TreeNode>, int, Example2TreeNode>? action = null) {
		action ??= (collection, idx, node) => ((ICollection<Example2TreeNode>)collection).Add(node); 
		base.InsertChildCore(index, child, action);
	}
	public Example2TreeNode AddChild(Example2TreeNode child) {
		this.InsertChildCore(0, child);
		return this;
	}
}
public static partial class Demo {

}
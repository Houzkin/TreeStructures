using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Linq;

namespace SampleConsoleApp;

public interface IMemberNode {
	string MemberName { get; }
	int Followers { get; }
}
public class MemberNode : GeneralTreeNode<MemberNode>,IMemberNode{
	public MemberNode(string name) { MemberName = name; }

	public string MemberName { get; }
	public int Followers => this.Preorder().Count() - 1;
}
public class ObservableMemberNode : ObservableGeneralTreeNode<ObservableMemberNode>, IMemberNode {
	public ObservableMemberNode(string name){
		MemberName = name;
		this.StructureChanged += Member_StructureChanged;
	}

	private void Member_StructureChanged(object? sender, StructureChangedEventArgs<ObservableMemberNode> e) {
		if(e.IsDescendantChanged && e.DescendantInfo!.SubTreeAction != TreeNodeChangedAction.Move){
			this.RaisePropertyChanged(nameof(Followers));
		}
	}

	public string MemberName { get; }
	public int Followers => this.Preorder().Count() - 1;
}
public class MemberWrapper<TSrc> : TreeNodeWrapper<TSrc,MemberWrapper<TSrc>> where TSrc:class,ITreeNode<TSrc>,IMemberNode{
	public MemberWrapper(TSrc member) : base(member) { }

	protected override MemberWrapper<TSrc> GenerateChild(TSrc sourceChildNode) {
		return new MemberWrapper<TSrc>(sourceChildNode);
	}
	public string MemberName=>Source.MemberName;
	public int Followers => Source.Followers;
	public int WrappingFollowers => this.Preorder().Count() - 1;
}
public class DisposableMemberWrapper<TSrc>:BindableTreeNodeWrapper<TSrc,DisposableMemberWrapper<TSrc>>
where TSrc:class, ITreeNode<TSrc>,IMemberNode{ 
	public DisposableMemberWrapper(TSrc member):base(member){ 
	}
	protected override DisposableMemberWrapper<TSrc> GenerateChild(TSrc sourceChildNode) {
		return new DisposableMemberWrapper<TSrc>(sourceChildNode);
	}

	public string MemberName => Source.MemberName;
	public int Followers => Source.Followers;
	public int WrappingFollowers => this.Preorder().Count() - 1;
}
public static partial class UseageSample {
	public static void MethodE(){
		var members = "ABCDEFGHIJ".ToCharArray().Select(x=>x.ToString()).ToDictionary(x => x, x => new ObservableMemberNode(x));

		var memberRoot = members.Values.AssembleAsNAryTree(2);

		var wrpMemberRt = new MemberWrapper<ObservableMemberNode>(memberRoot);

		var dispoWrpMemberRt = new DisposableMemberWrapper<ObservableMemberNode>(memberRoot);

		Console.WriteLine("initial state of Wrapper");
		Console.WriteLine(wrpMemberRt.ToTreeDiagram(x => $"name:{x.MemberName}, Followers:{x.Followers}"));

		Console.WriteLine("initial state of DisposableWrapper.");
		Console.WriteLine(dispoWrpMemberRt.ToTreeDiagram(x => $"name:{x.MemberName}, Followers:{x.Followers}, IsDisposed:{x.IsDisposed}"));

		var wrpB = wrpMemberRt.Preorder().First(x => x.MemberName == "B");
		var dspWrpB = dispoWrpMemberRt.Preorder().First(x=> x.MemberName == "B");

		Console.WriteLine("remove node B\n");
		memberRoot.Levelorder().First(x => x.MemberName == "B").TryRemoveOwn();

		Console.WriteLine("each root wrappers");
		Console.WriteLine(wrpMemberRt.ToTreeDiagram(x => $"name:{x.MemberName}, Followers:{x.Followers}"));
		Console.WriteLine(dispoWrpMemberRt.ToTreeDiagram(x => $"name:{x.MemberName}, Followers:{x.Followers}, IsDisposed:{x.IsDisposed}"));


		Console.WriteLine("each nodeB wrappers");
		Console.WriteLine(wrpB.ToTreeDiagram(x => $"name:{x.MemberName}, Followers:{x.Followers}"));
		Console.WriteLine(dspWrpB.ToTreeDiagram(x => $"name:{x.MemberName}, Followers:{x.Followers}, IsDisposed:{x.IsDisposed}"));


		Console.WriteLine("Dispose node C wrapper");
		dispoWrpMemberRt.Preorder().First(x => x.MemberName == "C").Dispose();
		Console.WriteLine(dispoWrpMemberRt.ToTreeDiagram(x => $"name:{x.MemberName}, Followers:{x.Followers}, IsDisposed:{x.IsDisposed}"));
		//wrpMemberRoot.Levelorder().First(x => x.MemberName == "A").PauseImitation();
		//wrpMemberRoot.Levelorder().First(x => x.MemberName == "A").ImitateSourceSubTree();
		//memberRoot.AddChild(memberB);
		//wrpMemberRoot.RefreshHierarchy();
		//Console.WriteLine(dispoWrpMemberRt.ToTreeDiagram(x => $"name:{x.MemberName}, Followers:{x.Followers}"));

	}
}


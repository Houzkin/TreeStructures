using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Linq;

namespace SampleConsoleApp {

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
				//if(e.Target.TotalScore >0) this.Upstream().ForEach(x=>x.RaisePropertyChanged(nameof(TotalScore)));
			}
		}

		public string MemberName { get; }
		public int Followers => this.Preorder().Count() - 1;
		//int _score;
		//public int Score {
		//	get { return this._score; }
		//	set {
		//		if(this.SetProperty(ref _score, value)){
		//			this.Ancestors().ForEach(x => x.RaisePropertyChanged(nameof(TotalScore)));
		//		} 
		//	}
		//}
		//public void AddScore(int point){
		//	Score += point;
		//}
		//public int TotalScore => this.Preorder().Sum(x => x.Score);

	}
	public class MemberImitator<TSrc>:DisposableTreeNodeWrapper<TSrc,MemberImitator<TSrc>>
	where TSrc:class, ITreeNode<TSrc>,IMemberNode{ 
		public MemberImitator(TSrc member):base(member){ 
		}
		//protected override MemberImitator GenerateChild(ObservableMemberNode sourceChildNode) {
		//	return new MemberImitator(sourceChildNode);
		//}

		protected override MemberImitator<TSrc> GenerateChild(TSrc sourceChildNode) {
			return new MemberImitator<TSrc>(sourceChildNode);
		}

		public string MemberName => Source.MemberName;
		public int Followers => Source.Followers;
		public int ImitatingFollowers => this.Preorder().Count() - 1;
		public int Point { get; set; }
	}
	internal class SampleE {
		public static void Method(){
			var members = "ABCDEFGHIJ".ToCharArray().Select(x=>x.ToString()).ToDictionary(x => x, x => new MemberNode(x));

			var memberRoot = members.AssembleAsNAryTree(2, x =>x.Value);

			var wrpMemberRoot = new MemberImitator<MemberNode>(memberRoot);
			wrpMemberRoot.Preorder().ForEach(x => x.Point = x.Height());

			//Console.WriteLine(memberRoot.ToTreeDiagram(x => $"name:{x.MemberName}, Followers:{x.Followers}" ));
			Console.WriteLine("initial state.");
			Console.WriteLine(wrpMemberRoot.ToTreeDiagram(x => $"name:{x.MemberName}, Followers:{x.Followers}, Point:{x.Point}"));

			Console.WriteLine("remove node B");
			var memberB = memberRoot.Levelorder().First(x => x.MemberName == "B").TryRemoveOwn().Value;
			Console.WriteLine(wrpMemberRoot.ToTreeDiagram(x => $"name:{x.MemberName}, Followers:{x.Followers}, Point:{x.Point}"));


			//wrpMemberRoot.Levelorder().First(x => x.MemberName == "A").PauseImitation();
			//wrpMemberRoot.Levelorder().First(x => x.MemberName == "A").ImitateSourceSubTree();
			memberRoot.AddChild(memberB);
			//wrpMemberRoot.RefreshHierarchy();
			Console.WriteLine(wrpMemberRoot.ToTreeDiagram(x => $"name:{x.MemberName}, Followers:{x.Followers}, Point:{x.Point}"));

		}
	}
}

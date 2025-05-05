using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TreeStructures;
using TreeStructures.Collections;
using TreeStructures.Linq;
using TreeStructures.Tree;
using TreeStructures.Utilities;

namespace SampleConsoleApp;
public static partial class UseageSample {

	public static void MethodG() {
		var uniExecutor = new UniqueOperationExecutor();

		var sw = new Stopwatch();
		Action displayElapsedTime = () => {
			Console.WriteLine($"Time  {sw.Elapsed.Seconds}.{sw.Elapsed.Milliseconds}(sec)");
		};
		Action resetTime = () => sw.Restart();
		uniExecutor.Register("keyA", displayElapsedTime);
		uniExecutor.Register("keyB", resetTime);

		sw.Start();
		var t1 = Task.Run(() => {
			var exe = uniExecutor.ExecuteUnique("keyA");
			Thread.Sleep(1000);
			exe.Dispose();
		});
		var t2 = Task.Run(() => {
			Thread.Sleep(800);
			var exe = uniExecutor.ExecuteUnique("keyA");
			Thread.Sleep(800);
			exe.Dispose();
		});
		var t3 = Task.Run(() => {
			var exe = uniExecutor.ExecuteUnique("keyA");
			Thread.Sleep(500);
			exe .Dispose();
		});
		var t4 = Task.Run(() => {
			Thread.Sleep(2000);
			var exe = uniExecutor.ExecuteUnique("keyA");
			Thread .Sleep(500);
			exe .Dispose();
		});
		//var t5 = Task.Run(() => {
		//	var exe = uniExecutor.ExecuteUnique("keyB");
		//	Thread.Sleep(500);
		//	exe.Dispose();
		//});

		Task.WaitAll(t1, t2, t3, t4);
	}

}
public class NoticeLevelChangedNode : ObservableGeneralTreeNode<NoticeLevelChangedNode> {
	public NoticeLevelChangedNode() {
		evaluateLevelPropValue = whetherLevelChanged;
		this.StructureChanged += (s, e) => {
			if (e.IsAncestorChanged) {
				evaluateLevelPropValue.Dispose();
				evaluateLevelPropValue = whetherLevelChanged;
			}
		};
	}
	UniqueOperationExecutor? _uniExecutor;
	UniqueOperationExecutor uniExecutor {
		get {
			if (_uniExecutor == null) {
				_uniExecutor = new UniqueOperationExecutor();
				_uniExecutor.Register(nameof(Level), () => this.OnPropertyChanged(nameof(this.Level)));
			}
			return _uniExecutor;
		}
	}
	IDisposable evaluateLevelPropValue;
	IDisposable whetherLevelChanged => uniExecutor.LateEvaluate(nameof(Level), () => Level);
	public int Level => this.Depth();
}
public class NoticeDepthChangedNode : ObservableGeneralTreeNode<NoticeDepthChangedNode> {
	public NoticeDepthChangedNode() {
		//initialize instance
		var uniExectr = new UniqueOperationExecutor();
		//register action with key.
		uniExectr.Register(nameof(Level), () => OnPropertyChanged(nameof(this.Level)));

		//set initial status
		evaluateLevel = uniExectr.LateEvaluate(nameof(Level), () => Level);
		this.StructureChanged += (s, e) => {
			if (e.IsAncestorChanged) { 
				//evaluate whether level changed
				evaluateLevel.Dispose();
				//reset status
				evaluateLevel = uniExectr.LateEvaluate(nameof(Level), () => Level);
			}
		};
	}
	IDisposable evaluateLevel;
	public int Level => this.Depth();
}
class RandomDateTime {
	DateTime start;
	Random gen;
	int range;
	public RandomDateTime(DateTime start, DateTime until) {
		this.start = start;
		this.gen = new Random();
		range = (until - start).Days;
	}
	public DateTime Next() {
		return start.AddDays(gen.Next(range)).AddHours(gen.Next(0, 24)).AddMinutes(gen.Next(0, 60)).AddSeconds(gen.Next(0, 60));
	}
}
public class Anniversary {
	public DateTime Date { get; set; }
	public string Name { get; set; }

	public override string ToString() {
		return $"{Name} : "+ Date.ToString("yyyy/M/d(ddd)",CultureInfo.CreateSpecificCulture("en-US"));
	}
	public bool IsHoliday { get; set; }
}
//public class AnnivWrapper : TreeNodeWrapper<DateTimeTree<Anniversary>.Node, AnnivWrapper> {
//	public AnnivWrapper(DateTimeTree<Anniversary>.Node node) : base(node) { }
//	protected override AnnivWrapper GenerateChild(DateTimeTree<Anniversary>.Node sourceChildNode) {
//		return new AnnivWrapper(sourceChildNode);
//	}
//	public string HeaderString {
//		get{
//			if(this.Source.HasItemAndValue){
//				return this.Source.Item.ToString();
//			}
//			var str = this.Source.Depth() switch {
//				0 => string.Empty,
//				1 => $"FY{Source.NodeClass}",
//				2 => toMonth(Source.NodeClass > 12 ? Source.NodeClass - 12 : Source.NodeClass),
//				3 => Source.NodeClass.ToString()+"th",
//				_ => this.Source.ToString()
//			}; ;
//			return str;
//		}
//	}
//	private string toMonth(int month){
//		string str = month switch {
//			1 => "Jan.",
//			2 => "Feb.",
//			3 => "Mar.",
//			4 => "Apr.",
//			5 => "May ",
//			6 => "Jun.",
//			7 => "Jul.",
//			8 => "Aug.",
//			9 => "Sep.",
//			10 => "Oct.",
//			11 => "Nov.",
//			12 => "Dec.",
//			_ => string.Empty,
//		};
//		return str;
//	}
//}

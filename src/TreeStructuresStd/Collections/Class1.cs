using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace TreeStructures.Collections {
	//internal class SDPair<TSrc,TDst> {
	//	public SDPair(TSrc src, TDst dst) {
	//		Src = src;
	//		Dst = dst;
	//	}
	//	public TSrc Src { get; private set; }
	//	public TDst Dst { get; private set; }
	//}
	//public abstract class ImitableList<T> : ReadOnlyObservableCollection<T>,IDisposable {
	//	private bool isDisposed;

	//	public ImitableList() : base(new ObservableCollection<T>()) {
			
	//	}

	//	public abstract bool IsImitating { get; }
	//	public abstract void Imitate();
	//	public abstract void ClearAndPause();

	//	#region Dispose
	//	protected void ThrowExceptionIfDisposed() {
	//		if (isDisposed) throw new ObjectDisposedException(GetType().FullName,"The instance has already been disposed and cannot be operated on.");
	//	}
	//	protected virtual void Dispose(bool disposing) {
	//		if (!isDisposed) {
	//			if (disposing) {
	//				// TODO: マネージド状態を破棄します (マネージド オブジェクト)
	//			}

	//			// TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
	//			// TODO: 大きなフィールドを null に設定します
	//			isDisposed = true;
	//		}
	//	}

	//	// // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
	//	// ~ImitableList()
	//	// {
	//	//     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
	//	//     Dispose(disposing: false);
	//	// }

	//	public void Dispose() {
	//		// このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
	//		Dispose(disposing: true);
	//		GC.SuppressFinalize(this);
	//	}
	//	#endregion
	//}
	//public class ImitableList<TSrc, TDst> : ImitableList<TDst> {
	//	IEnumerable<TSrc> _srcs;
	//	IList<SDPair<TSrc, TDst>> _pairs;

	//	public override bool IsImitating => throw new NotImplementedException();

	//	public override void ClearAndPause() {
	//		throw new NotImplementedException();
	//	}

	//	public override void Imitate() {
	//		throw new NotImplementedException();
	//	}
	//}
}

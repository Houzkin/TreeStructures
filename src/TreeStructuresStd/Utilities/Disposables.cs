using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.Utilities;

//public static class DisposeExtensions {
//    public static void Using(this IDisposable self,Action action) {
//        action();
//        self.Dispose();
//    }
//}

public class DisposableObject : IDisposable {
    Action? _disp;
    public DisposableObject() { }
    public DisposableObject(Action? disp) { _disp = disp; }
    public void Dispose() { 
        if( _disp != null) {
            _disp.Invoke();
            _disp = null;
        }
    }
}
public class LumpedDisopsables : IEnumerable<IDisposable>, IDisposable {
    List<IDisposable> _disposings = new();
    public LumpedDisopsables(IEnumerable<IDisposable> disposables) {
        foreach(var dsp in disposables) this.Add(dsp);
    }
    public LumpedDisopsables() { }
    public void Dispose() {
        foreach (var disposable in _disposings) { disposable.Dispose(); }
        _disposings.Clear();
    }
    public void Add(IDisposable disposable) {
        if(disposable is LumpedDisopsables dispcollection)
            foreach(var dsp in dispcollection)_disposings.Add(dsp);
        else if (disposable != null)
            _disposings.Add(disposable);
    }
    public void Remove(IEnumerable<IDisposable> disp){
        foreach(var dis in disp){
            _disposings.Remove(dis);
        }
    }
    //public IEnumerator<IDisposable> GetEnumerator() {
    //    return _disposings.GetEnumerator();
    //}
    IEnumerator IEnumerable.GetEnumerator() {
        return _disposings.GetEnumerator();
    }

	IEnumerator<IDisposable> IEnumerable<IDisposable>.GetEnumerator() {
        return _disposings.GetEnumerator();
	}
}


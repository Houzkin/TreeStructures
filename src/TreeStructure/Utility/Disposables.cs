using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructure.Internals;

internal class DisposableObject : IDisposable {
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
internal class LumpedDisopsables : IEnumerable<IDisposable>, IDisposable {
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
    public IEnumerator<IDisposable> GetEnumerator() {
        return _disposings.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator() {
        return _disposings.GetEnumerator();
    }
}


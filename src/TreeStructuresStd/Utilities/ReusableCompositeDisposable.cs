using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeStructures.Utilities;

public sealed class ReusableCompositeDisposable : IEnumerable<IDisposable>, IDisposable {
    List<IDisposable> _disposings = new();
    public ReusableCompositeDisposable(IEnumerable<IDisposable> disposables) {
        foreach(var dsp in disposables) this.Add(dsp);
    }
    public ReusableCompositeDisposable(params IDisposable[] disposables) : this(disposables.AsEnumerable()) { }
    public ReusableCompositeDisposable() { }
    public void ClearAndDispose() {
        foreach (var disposable in _disposings) { disposable.Dispose(); }
        _disposings.Clear();
    }
	void IDisposable.Dispose() {
        this.ClearAndDispose();
	}
    public void Add(IDisposable disposable) {
        if (disposable is ReusableCompositeDisposable dispcollection)
            foreach (var dsp in dispcollection) _disposings.Add(dsp);
        else if (disposable != null)
            _disposings.Add(disposable);
    }
    public void Remove(IEnumerable<IDisposable> disp){
        foreach(var dis in disp){
            _disposings.Remove(dis);
        }
    }
    IEnumerator IEnumerable.GetEnumerator() {
        return _disposings.GetEnumerator();
    }

	IEnumerator<IDisposable> IEnumerable<IDisposable>.GetEnumerator() {
        return _disposings.GetEnumerator();
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.Utilities;

public class DisposableObject : IDisposable {
    Action? _disp;
	private bool disposedValue;

	public DisposableObject() { }
    public DisposableObject(Action? disp) { _disp = disp; }

	protected virtual void Dispose(bool disposing) {
		if (!disposedValue) {
			if (disposing) {
				if (_disp != null) {
					_disp.Invoke();
					_disp = null;
				}
			}
			disposedValue = true;
		}
	}
	public void Dispose() {
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}


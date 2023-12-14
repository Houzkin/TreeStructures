using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.EventManagement {

    //public class NotificationObject : INotifyPropertyChanged {
    //    public NotificationObject() {
    //        PropChangeProxy = new PropertyChangeProxy(this);
    //    }
    //    readonly PropertyChangeProxy PropChangeProxy;

    //    public event PropertyChangedEventHandler? PropertyChanged {
    //        add { this.PropChangeProxy.Changed += value; }
    //        remove { this.PropChangeProxy.Changed -= value; }
    //    }
    //    protected virtual bool SetProperty<T>(ref T strage, T value, [CallerMemberName] string? propertyName = null) =>
    //        PropChangeProxy.SetWithNotify(ref strage, value, propertyName);

    //    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null) =>
    //        PropChangeProxy.Notify(propertyName);
    //}
    /// <summary><see cref="INotifyPropertyChanged"/>の実装をサポートする</summary>
    public class PropertyChangeProxy :INotifyPropertyChanged {
        object? sender;
        /// <summary>新規インスタンスを初期化する</summary>
        /// <param name="sender">イベント発行時、発行元として指定するインスタンス</param>
        public PropertyChangeProxy(object? sender) {
            this.sender = sender;
        }
        /// <summary>値の変更と変更通知の発行を行う</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strage"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool SetWithNotify<T>(ref T strage, T value, [CallerMemberName] string? propertyName = null) {
            if (Equals(strage, value)) return false;
            strage = value;
            Notify(propertyName);
            return true;
        }
        /// <summary>プロパティ変更通知を発行する</summary>
        /// <param name="propertyName">プロパティ名</param>
        public void Notify([CallerMemberName] string? propertyName = null) {
            if (string.IsNullOrEmpty(propertyName)) return;
            Changed?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>登録されている全てのハンドラーを削除する</summary>
        public void ClearHandler() { 
            Changed = null;
        }
        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged {
            add { Changed += value; }
            remove { Changed -= value; }
        }
        /// <summary>プロパティ変更時、処理される</summary>
        public event PropertyChangedEventHandler? Changed;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.Utility {
    /// <summary>
    /// 
    /// </summary>
    public static class MulticastDelegateExtensions {
        /// <summary>登録されているハンドラーの数を取得する</summary>
        public static int GetLength(this MulticastDelegate? self) {
            if (self == null || self.GetInvocationList() == null) {
                return 0;
            }
            return self.GetInvocationList().Length;
        }
    }
}

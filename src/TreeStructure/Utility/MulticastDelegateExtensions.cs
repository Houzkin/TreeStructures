using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructure.Utility {
    public static class MulticastDelegateExtensions {
        public static int GetLength(this MulticastDelegate? self) {
            if (self == null || self.GetInvocationList() == null) {
                return 0;
            }
            return self.GetInvocationList().Length;
        }
    }
}

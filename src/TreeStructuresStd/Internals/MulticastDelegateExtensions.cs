﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Utilities;

namespace TreeStructures.Internals {
    /// <summary>
    /// 
    /// </summary>
    public static class MulticastDelegateExtensions {
        /// <summary>
        /// Gets the count of registered handlers.
        /// </summary>
        /// <returns>The count of registered handlers.</returns>
        public static int GetLength(this MulticastDelegate? self) {
            if (self == null || self.GetInvocationList() == null) {
                return 0;
            }
            return self.GetInvocationList().Length;
        }
    }
}

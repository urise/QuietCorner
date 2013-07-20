using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonClasses.Helpers
{
    public static class ExceptionHelper
    {
        public static string SmartMessage(this Exception ex)
        {
            if (ex == null) return string.Empty;
            int n = ex.Message.IndexOf("See the inner exception for details.");
            if (n == -1) return ex.Message;
            return ex.Message.Substring(0, n) + ex.InnerException.Message;
        }
    }
}

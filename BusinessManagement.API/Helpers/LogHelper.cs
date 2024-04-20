using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace App.Helpers
{
    /// <summary>
    /// Functions for quickly adding useful information to logs
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// Generate a log trace to quickly identify the method and line number of a logged error. 
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="sourceLineNumber"></param>
        /// <returns>The calling method name and the line number</returns>
        public static string TraceLog([CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            StringBuilder sb = new();
            sb.Append(Path.GetFileName(sourceFilePath));
            sb.Append(' ');
            sb.Append(memberName);
            sb.Append(' ');
            sb.Append("Line:");
            sb.Append(sourceLineNumber);

            return sb.ToString();
        }
    }
}

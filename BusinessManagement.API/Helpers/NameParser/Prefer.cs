/*
 * Parser.cs, Prefer.cs, and NameParserConfig.cs are provided here by aeshirey from the NameParserSharp library under the LGPL license. 
 * Please see "License.txt" for me details.
 * https://github.com/aeshirey/NameParserSharp
 */

namespace NameParser
{
    [Flags]
    public enum Prefer
    {
        Default = 0,

        /// <summary>
        ///  For Issue #20, when the parser detects a Title and a Last with prefixes (eg, "Mr. Del Richards"), 
        ///  convert the prefix to a first name.
        ///  
        /// This can cause incorrect flipping of prefix to first (eg, "Mr. Van Rossum"), so you should use
        /// this flag only when you know your data has a first name.
        /// </summary>
        FirstOverPrefix = 1,
    }
}
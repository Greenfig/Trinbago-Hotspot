using System.Text.RegularExpressions;

namespace Trinbago_MVC5
{
    public static class RecordsPerPage
    {
        public static int Records => 10;
    }
    
    public static class GlobalRegex
    {
        /// <summary>
        /// country location
        /// </summary>
        public static Regex CountryReg = new Regex(@"(lc)\d+", RegexOptions.None);
        /// <summary>
        /// region location
        /// </summary>
        public static Regex RegionReg = new Regex(@"(lr)\d+", RegexOptions.None);
        /// <summary>
        /// category
        /// </summary>
        public static Regex CategoryReg = new Regex(@"(c)\d+", RegexOptions.None);
        /// <summary>
        /// subcategory
        /// </summary>
        public static Regex SubcategoryReg = new Regex(@"(sc)\d+", RegexOptions.None);
    }

    public static class TextEditor
    {
        /// <summary>
        /// More than one space [ ]{2,}
        /// </summary>
        private static Regex reg = new Regex("[ ]{2,}", RegexOptions.None);
        /// <summary>
        /// Contact name [^a-zA-Z0-9'.-]
        /// </summary>
        private static Regex reg2 = new Regex("[^a-zA-Z'.-]", RegexOptions.None);
        /// <summary>
        /// Find -_. characters [_.-]
        /// </summary>
        private static Regex reg3 = new Regex("[_.-]", RegexOptions.None);
        /// <summary>
        /// Poster name [^0-9a-zA-Z'.()&-]
        /// </summary>
        private static Regex reg5 = new Regex("[^0-9a-zA-Z'.()&-]", RegexOptions.None);
        /// <summary>
        /// Duplicate special chars [.'()-]{2,}
        /// </summary>
        private static Regex reg6 = new Regex("[.'()-]{2,}", RegexOptions.None);

        public static string RemoveDoubleSpace(string str)
        {
            return reg.Replace(str.Trim(), " ");
        }

        public static string GetUserContactFromEmail(string str)
        {
            var s = str.Split('@')[0];
            s = reg2.Replace(s, " ");
            s = reg6.Replace(s, " ");
            s = reg.Replace(s, " ").Trim();
            return s;
        }

        public static string CleanAdContactName(string str)
        {
            var s = reg2.Replace(str, " ");
            s = reg6.Replace(s, " ");
            s = reg.Replace(s, " ").Trim();
            return s;
        }

        /// <summary>
        /// Rules: No double space, special chars, 2 or more of the same char
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string CleanPosterName(string str)
        {
            var s = reg5.Replace(str, " ");
            s = reg6.Replace(s, " ");
            s = reg.Replace(s, " ").Trim();
            return s;
        }
    }    
}
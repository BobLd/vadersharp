using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("VaderSharp2.Tests")]
namespace VaderSharp2
{
    internal static class Extensions
    {
        /// <summary>
        /// Determine if word is ALL CAPS
        /// </summary>
        /// <param name="word"></param>
        public static bool IsUpper(this string word)
        {
            bool hasLetter = false;
            foreach (char c in word)
            {
                if (char.IsLetter(c))
                {
                    hasLetter = true;
                    if (!char.IsUpper(c))
                    {
                        return false;
                    }
                }
            }
            return hasLetter;
        }

        /// <summary>
        /// Removes punctuation from word
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string RemovePunctuation(this string word)
        {
            return new string(word.Where(c => !char.IsPunctuation(c)).ToArray());
        }
    }
}

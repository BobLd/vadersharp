using System.Collections.Generic;
using System.Linq;
//using System.Runtime.CompilerServices;

//[assembly: InternalsVisibleTo("VaderSharp2.Tests")]
namespace VaderSharp2
{
    /// <summary>
    /// Identify sentiment-relevant string-level properties of input text.
    /// </summary>
    /// <remarks>Checked as of 01/02/2022</remarks>
    internal class SentiText
    {
        private string Text { get; }

        public IList<string> WordsAndEmoticons { get; }

        public bool IsCapDifferential { get; }

        /// <summary>
        /// Identify sentiment-relevant string-level properties of input text.
        /// </summary>
        /// <remarks>Checked as of 01/02/2022</remarks>
        public SentiText(string text)
        {
            Text = text;
            WordsAndEmoticons = GetWordsAndEmoticons();
            // doesn't separate words from
            // adjacent punctuation (keeps emoticons & contractions)
            IsCapDifferential = SentimentUtils.AllCapDifferential(WordsAndEmoticons);
        }

        /// <summary>
        /// Removes leading and trailing punctuation.
        /// <para>Leaves contractions and most emoticons.</para>
        /// Does not preserve punc-plus-letter emoticons (e.g. :D)
        /// </summary>
        /// <remarks>Checked as of 01/02/2022</remarks>
        private IList<string> GetWordsAndEmoticons()
        {
            var wes = Text.Split().Where(x => x.Length > 1).ToList();
            var stripped = wes.ConvertAll(x => StripPuncIfWord(x));
            return stripped;
        }

        private static readonly char[] punctuation = "!\"#$%&\'()*+,-./:;<=>?@[\\]^_`{|}~".ToCharArray();

        /// <summary>
        /// Removes all trailing and leading punctuation
        /// If the resulting string has two or fewer characters,
        /// then it was likely an emoticon, so return original string
        /// (ie ":)" stripped would be "", so just return ":)"
        /// </summary>
        /// <remarks>Checked as of 01/02/2022</remarks>
        private static string StripPuncIfWord(string token)
        {
            var stripped = token.Trim(punctuation);
            if (stripped.Length <= 2)
            {
                return token;
            }

            return stripped;
        }
    }
}

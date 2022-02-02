using System;
using System.Collections.Generic;
using System.Linq;

namespace VaderSharp2
{
    /// <summary>
    /// Sentiment tools.
    /// </summary>
    internal static class SentimentUtils
    {
        #region Constants
        /// <summary>
        /// Empirically derived mean sentiment intensity rating increase for booster words.
        /// </summary>
        public const double BIncr = 0.293;

        /// <summary>
        /// Empirically derived mean sentiment intensity rating increase for booster words.
        /// </summary>
        public const double BDecr = -0.293;

        /// <summary>
        /// Empirically derived mean sentiment intensity rating increase for using ALLCAPs to emphasize a word.
        /// </summary>
        public const double CIncr = 0.733;

        /// <summary>
        /// Empirically derived mean sentiment intensity rating increase for using ALLCAPs to emphasize a word.
        /// </summary>
        public const double NScalar = -0.74;

        /// <summary>
        /// Negations
        /// </summary>
        public static readonly string[] Negate =
        {
            "aint", "arent", "cannot", "cant", "couldnt", "darent", "didnt", "doesnt",
            "ain't", "aren't", "can't", "couldn't", "daren't", "didn't", "doesn't",
            "dont", "hadnt", "hasnt", "havent", "isnt", "mightnt", "mustnt", "neither",
            "don't", "hadn't", "hasn't", "haven't", "isn't", "mightn't", "mustn't",
            "neednt", "needn't", "never", "none", "nope", "nor", "not", "nothing", "nowhere",
            "oughtnt", "shant", "shouldnt", "uhuh", "wasnt", "werent",
            "oughtn't", "shan't", "shouldn't", "uh-uh", "wasn't", "weren't",
            "without", "wont", "wouldnt", "won't", "wouldn't", "rarely", "seldom", "despite"
        };

        /// <summary>
        /// Booster/dampener 'intensifiers' or 'degree adverbs'.
        /// <see cref="http://en.wiktionary.org/wiki/Category:English_degree_adverbs"/>
        /// </summary>
        public static readonly Dictionary<string, double> BoosterDict = new Dictionary<string, double>
        {
            // Incr
            { "absolutely", BIncr }, { "amazingly", BIncr }, { "awfully", BIncr },
            { "completely", BIncr }, { "considerable", BIncr }, { "considerably", BIncr },
            { "decidedly", BIncr }, { "deeply", BIncr }, { "effing", BIncr }, { "enormous", BIncr }, { "enormously", BIncr },
            { "entirely", BIncr },{ "especially", BIncr }, { "exceptional", BIncr }, { "exceptionally", BIncr },
            { "extreme", BIncr }, { "extremely", BIncr },
            { "fabulously", BIncr }, { "flipping", BIncr }, { "flippin", BIncr}, { "frackin", BIncr }, { "fracking", BIncr },
            { "fricking", BIncr }, { "frickin", BIncr }, { "frigging", BIncr }, { "friggin", BIncr }, { "fully", BIncr },
            { "fuckin", BIncr }, { "fucking", BIncr }, { "fuggin", BIncr }, { "fugging", BIncr },
            { "greatly", BIncr }, { "hella", BIncr }, { "highly", BIncr }, { "hugely", BIncr },
            { "incredible", BIncr }, { "incredibly", BIncr }, { "intensely", BIncr },
            { "major", BIncr }, { "majorly", BIncr }, { "more", BIncr }, { "most", BIncr }, { "particularly", BIncr },
            { "purely", BIncr }, { "quite", BIncr }, { "really", BIncr }, { "remarkably", BIncr },
            { "so", BIncr }, { "substantially", BIncr },
            { "thoroughly", BIncr }, { "total", BIncr }, { "totally", BIncr }, { "tremendous", BIncr }, { "tremendously", BIncr },
            { "uber", BIncr }, { "unbelievably", BIncr }, { "unusually", BIncr }, { "utter", BIncr }, { "utterly", BIncr },
            { "very", BIncr },

            // Decr
            { "almost", BDecr }, { "barely", BDecr }, { "hardly", BDecr }, { "just enough", BDecr },
            { "kind of", BDecr }, { "kinda", BDecr }, { "kindof", BDecr }, { "kind-of", BDecr },
            { "less", BDecr }, { "little", BDecr }, { "marginal", BDecr }, { "marginally", BDecr },
            { "occasional", BDecr }, { "occasionally", BDecr }, { "partly", BDecr },
            { "scarce", BDecr }, { "scarcely", BDecr }, { "slight", BDecr }, { "slightly", BDecr }, { "somewhat", BDecr },
            { "sort of", BDecr }, { "sorta", BDecr }, { "sortof", BDecr }, { "sort-of", BDecr }
        };

        /// <summary>
        /// Check for special case idioms and phrases containing lexicon words.
        /// </summary>
        public static readonly Dictionary<string, double> SpecialCases = new Dictionary<string, double>
        {
            { "the shit", 3 },
            { "the bomb", 3 },
            { "bad ass", 1.5 },
            { "badass", 1.5 },
            { "bus stop", 0.0 },
            { "yeah right", -2 },
            { "kiss of death", -1.5 },
            { "to die for", 3 },
            { "beating heart", 3.5 },   // set to 3.1 with 0150f59077ad3b8d899eff5d4c9670747c2d54c2 on 22 May 2020
            //{ "broken heart", -2.9 }, // added with 0150f59077ad3b8d899eff5d4c9670747c2d54c2 on 22 May 2020
        };
        #endregion

        #region Util static methods
        /// <summary>
        /// Determine if input contains negation words.
        /// </summary>
        public static bool Negated(IList<string> inputWords, bool includeNt = true)
        {
            inputWords = inputWords.Select(w => w.ToLower()).ToList();
            foreach (var word in Negate)
            {
                if (inputWords.Contains(word))
                {
                    return true;
                }
            }

            if (includeNt)
            {
                foreach (var word in inputWords)
                {
                    if (word.Contains("n't"))
                    {
                        return true;
                    }
                }
            }

            /*
            if (inputWords.Contains("least"))
            {
                int i = inputWords.IndexOf("least");
                if (i > 0 && inputWords[i - 1] != "at")
                {
                    return true;
                }
            }
            */

            return false;
        }

        /// <summary>
        /// Normalize the score to be between -1 and 1 using an alpha that
        /// approximates the max expected value.
        /// </summary>
        public static double Normalize(double score, double alpha = 15)
        {
            double normScore = score / Math.Sqrt((score * score) + alpha);

            if (normScore < -1.0)
            {
                return -1.0;
            }
            else if (normScore > 1.0)
            {
                return 1.0;
            }

            return normScore;
        }

        /// <summary>
        /// Checks whether some but not all of words in input are ALL CAPS.
        /// </summary>
        /// <param name="words">The words to inspect.</param>
        /// <returns>`True` if some but not all items in `words` are ALL CAPS.</returns>
        public static bool AllCapDifferential(IList<string> words)
        {
            int allCapWords = 0;

            foreach (var word in words)
            {
                if (word.IsUpper())
                {
                    allCapWords++;
                }
            }

            int capDifferential = words.Count - allCapWords;
            return capDifferential > 0 && capDifferential < words.Count;
        }

        /// <summary>
        /// Check if preceding words increase, decrease or negate the valence.
        /// </summary>
        public static double ScalarIncDec(string word, double valence, bool isCapDiff)
        {
            string wordLower = word.ToLower();
            if (!BoosterDict.ContainsKey(wordLower))
            {
                return 0.0;
            }

            double scalar = BoosterDict[wordLower];
            if (valence < 0)
            {
                scalar *= -1;
            }

            // Check if booster/dampener word is in ALLCAPS (while others aren't)
            if (word.IsUpper() && isCapDiff)
            {
                if (valence > 0)
                {
                    scalar += CIncr;
                }
                else
                {
                    scalar -= CIncr;
                }
            }

            return scalar;
        }
        #endregion
    }
}

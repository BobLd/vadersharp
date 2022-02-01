using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Globalization;

namespace VaderSharp2
{
    /// <summary>
    /// An abstraction to represent the sentiment intensity analyzer.
    /// </summary>
    public class SentimentIntensityAnalyzer
    {
        private const double ExclIncr = 0.292;
        private const double QuesIncrSmall = 0.18;
        private const double QuesIncrLarge = 0.96;

        private readonly Dictionary<string, double> Lexicon;
        private readonly string[] LexiconFullFile;

        private readonly Dictionary<string, string> Emojis;
        private readonly string[] EmojiFullFile;

        public SentimentIntensityAnalyzer()
        {
            Assembly assembly = typeof(SentimentIntensityAnalyzer).GetTypeInfo().Assembly;

            using (var stream = assembly.GetManifestResourceStream("VaderSharp2.vader_lexicon.txt"))
            using (var reader = new StreamReader(stream))
            {
                LexiconFullFile = reader.ReadToEnd().Split('\n');
                Lexicon = MakeLexDic();
            }

            using (var stream = assembly.GetManifestResourceStream("VaderSharp2.emoji_utf8_lexicon.txt"))
            using (var reader = new StreamReader(stream))
            {
                EmojiFullFile = reader.ReadToEnd().Split('\n');
                Emojis = MakeEmojiDic();
            }
        }

        public SentimentIntensityAnalyzer(string lexiconFile, string emojiLexicon)
        {
            if (Lexicon == null)
            {
                if (!File.Exists(lexiconFile))
                {
                    throw new Exception("Lexicon file not found");
                }

                using (var stream = new FileStream(lexiconFile, FileMode.Open))
                using (var reader = new StreamReader(stream))
                {
                    LexiconFullFile = reader.ReadToEnd().Split('\n');
                    Lexicon = MakeLexDic();
                }
            }

            throw new NotImplementedException("emojiLexicon");
        }

        /// <summary>
        /// Convert lexicon file to a dictionary.
        /// </summary>
        /// <remarks>Checked as of 01/02/2022</remarks>
        private Dictionary<string, double> MakeLexDic()
        {
            var lexDict = new Dictionary<string, double>();
            foreach (var line in LexiconFullFile)
            {
                var lineArray = line.Trim().Split('\t');
                lexDict.Add(lineArray[0], double.Parse(lineArray[1], CultureInfo.InvariantCulture));
            }
            return lexDict;
        }

        /// <summary>
        /// Convert emoji lexicon file to a dictionary.
        /// </summary>
        /// <remarks>Checked as of 01/02/2022</remarks>
        private Dictionary<string, string> MakeEmojiDic()
        {
            var emoji_dict = new Dictionary<string, string>();
            foreach (var line in EmojiFullFile)
            {
                var lineArray = line.Trim().Split('\t');
                string emoji = lineArray[0]; // emoji should be a char
                emoji_dict.Add(emoji, lineArray[1]);
            }
            return emoji_dict;
        }

        /// <summary>
        /// <para>Return a float for sentiment strength based on the input text.</para>
        /// <para>Positive values are positive valence, negative value are negative valence.</para>
        /// </summary>
        public SentimentAnalysisResults PolarityScores(string text)
        {
            // convert emojis to their textual descriptions
            foreach (var em in Emojis.Where(kvp => text.Contains(kvp.Key)))
            {
                text = text.Replace(em.Key, em.Value);
            }

            text = text.Trim();

            var sentiText = new SentiText(text);
            IList<double> sentiments = new List<double>();
            IList<string> wordsAndEmoticons = sentiText.WordsAndEmoticons;

            for (int i = 0; i < wordsAndEmoticons.Count; i++)
            {
                string item = wordsAndEmoticons[i];
                double valence = 0;
                if (i < wordsAndEmoticons.Count - 1 && item.ToLower() == "kind" && wordsAndEmoticons[i + 1] == "of"
                    || SentimentUtils.BoosterDict.ContainsKey(item.ToLower()))
                {
                    sentiments.Add(valence);
                    continue;
                }
                sentiments = SentimentValence(valence, sentiText, item, i, sentiments);
            }

            sentiments = ButCheck(wordsAndEmoticons, sentiments);

            return ScoreValence(sentiments, text);
        }

        private IList<double> SentimentValence(double valence, SentiText sentiText, string item, int i, IList<double> sentiments)
        {
            string itemLowerCase = item.ToLower();
            if (!Lexicon.ContainsKey(itemLowerCase)) // not in lexicon
            {
                sentiments.Add(valence);
                return sentiments;
            }

            bool isCapDiff = sentiText.IsCapDifferential;
            IList<string> wordsAndEmoticons = sentiText.WordsAndEmoticons;

            // Get the sentiment valence 
            valence = Lexicon[itemLowerCase];

            // check for "no" as negation for an adjacent lexicon item vs "no" as its own stand-alone lexicon item
            if (itemLowerCase == "no" && i != wordsAndEmoticons.Count - 1 && Lexicon.ContainsKey(wordsAndEmoticons[i + 1].ToLower()))
            {
                // don't use valence of "no" as a lexicon item. Instead set it's valence to 0.0 and negate the next item
                valence = 0;
            }

            if ((i > 0 && wordsAndEmoticons[i - 1].ToLower() == "no")
             || (i > 1 && wordsAndEmoticons[i - 2].ToLower() == "no")
             || (i > 2 && wordsAndEmoticons[i - 3].ToLower() == "no" && new[] { "or", "nor" }.Contains(wordsAndEmoticons[i - 1].ToLower())))
            {
                valence = Lexicon[itemLowerCase] * SentimentUtils.NScalar;
            }

            if (isCapDiff && item.IsUpper())
            {
                if (valence > 0)
                {
                    valence += SentimentUtils.CIncr;
                }
                else
                {
                    valence -= SentimentUtils.CIncr;
                }
            }

            for (int startI = 0; startI < 3; startI++)
            {
                if (i > startI && !Lexicon.ContainsKey(wordsAndEmoticons[i - (startI + 1)].ToLower()))
                {
                    double s = SentimentUtils.ScalarIncDec(wordsAndEmoticons[i - (startI + 1)], valence, isCapDiff);
                    if (startI == 1 && s != 0)
                    {
                        s *= 0.95;
                    }
                    else if (startI == 2 && s != 0)
                    {
                        s *= 0.9;
                    }

                    valence += s;

                    valence = NegationCheck(valence, wordsAndEmoticons, startI, i);

                    if (startI == 2)
                    {
                        valence = SpecialIdiomsCheck(valence, wordsAndEmoticons, i);
                    }
                }
            }

            valence = LeastCheck(valence, wordsAndEmoticons, i);
            sentiments.Add(valence);
            return sentiments;
        }

        /// <summary>
        /// check for modification in sentiment due to contrastive conjunction 'but'
        /// </summary>
        private static IList<double> ButCheck(IList<string> wordsAndEmoticons, IList<double> sentiments)
        {
            var wordsAndEmoticonsLower = wordsAndEmoticons.Select(w => w.ToLower()).ToList();
            if (!wordsAndEmoticonsLower.Contains("but"))
            {
                return sentiments;
            }

            int bi = wordsAndEmoticonsLower.IndexOf("but");

            for (int si = 0; si < sentiments.Count; si++)
            {
                double sentiment = sentiments[si];
                if (si < bi)
                {
                    sentiments.RemoveAt(si);
                    sentiments.Insert(si, sentiment * 0.5);
                }
                else if (si > bi)
                {
                    sentiments.RemoveAt(si);
                    sentiments.Insert(si, sentiment * 1.5);
                }
            }
            return sentiments;
        }

        private double LeastCheck(double valence, IList<string> wordsAndEmoticons, int i)
        {
            if (i > 1 && !Lexicon.ContainsKey(wordsAndEmoticons[i - 1].ToLower()) &&
                wordsAndEmoticons[i - 1].ToLower() == "least")
            {
                if (wordsAndEmoticons[i - 2].ToLower() != "at" && wordsAndEmoticons[i - 2].ToLower() != "very")
                {
                    valence *= SentimentUtils.NScalar;
                }
            }
            else if (i > 0 && !Lexicon.ContainsKey(wordsAndEmoticons[i - 1].ToLower())
                     && wordsAndEmoticons[i - 1].ToLower() == "least")
            {
                valence *= SentimentUtils.NScalar;
            }

            return valence;
        }

        private static double NegationCheck(double valence, IList<string> wordsAndEmoticons, int startI, int i)
        {
            var wordsAndEmoticonsLower = wordsAndEmoticons.Select(x => x.ToLower()).ToList();
            if (startI == 0)
            {
                if (SentimentUtils.Negated(new List<string> { wordsAndEmoticonsLower[i - 1] }))
                {
                    // 1 word preceding lexicon word (w/o stopwords)
                    valence *= SentimentUtils.NScalar;
                }
            }
            else if (startI == 1)
            {
                if (wordsAndEmoticonsLower[i - 2] == "never" &&
                    (wordsAndEmoticonsLower[i - 1] == "so" || wordsAndEmoticonsLower[i - 1] == "this"))
                {
                    valence *= 1.25;
                }
                else if (SentimentUtils.Negated(new List<string> { wordsAndEmoticonsLower[i - (startI + 1)] }))
                {
                    // 2 words preceding the lexicon word position
                    valence *= SentimentUtils.NScalar;
                }
            }
            else if (startI == 2)
            {
                if (wordsAndEmoticonsLower[i - 3] == "never"
                && (wordsAndEmoticonsLower[i - 2] == "so" || wordsAndEmoticonsLower[i - 2] == "this")
                || (wordsAndEmoticonsLower[i - 1] == "so" || wordsAndEmoticonsLower[i - 1] == "this"))
                {
                    valence *= 1.25;
                }
                else if (SentimentUtils.Negated(new List<string> { wordsAndEmoticonsLower[i - (startI + 1)] }))
                {
                    // 3 words preceding the lexicon word position
                    valence *= SentimentUtils.NScalar;
                }
            }

            return valence;
        }

        private static double SpecialIdiomsCheck(double valence, IList<string> wordsAndEmoticons, int i)
        {
            var wordsAndEmoticonsLower = wordsAndEmoticons.Select(x => x.ToLower()).ToList();

            var oneZero = string.Concat(wordsAndEmoticonsLower[i - 1], " ", wordsAndEmoticonsLower[i]);
            var twoOneZero = string.Concat(wordsAndEmoticonsLower[i - 2], " ", wordsAndEmoticonsLower[i - 1], " ", wordsAndEmoticonsLower[i]);
            var twoOne = string.Concat(wordsAndEmoticonsLower[i - 2], " ", wordsAndEmoticonsLower[i - 1]);
            var threeTwoOne = string.Concat(wordsAndEmoticonsLower[i - 3], " ", wordsAndEmoticonsLower[i - 2], " ", wordsAndEmoticonsLower[i - 1]);
            var threeTwo = string.Concat(wordsAndEmoticonsLower[i - 3], " ", wordsAndEmoticonsLower[i - 2]);

            string[] sequences = { oneZero, twoOneZero, twoOne, threeTwoOne, threeTwo };

            foreach (var seq in sequences)
            {
                if (SentimentUtils.SpecialCases.ContainsKey(seq))
                {
                    valence = SentimentUtils.SpecialCases[seq];
                    break;
                }
            }

            if (wordsAndEmoticonsLower.Count - 1 > i)
            {
                string zeroOne = string.Concat(wordsAndEmoticonsLower[i], " ", wordsAndEmoticonsLower[i + 1]);
                if (SentimentUtils.SpecialCases.ContainsKey(zeroOne))
                {
                    valence = SentimentUtils.SpecialCases[zeroOne];
                }
            }

            if (wordsAndEmoticonsLower.Count - 1 > i + 1)
            {
                string zeroOneTwo = string.Concat(wordsAndEmoticonsLower[i], " ", wordsAndEmoticonsLower[i + 1], " ", wordsAndEmoticonsLower[i + 2]);
                if (SentimentUtils.SpecialCases.ContainsKey(zeroOneTwo))
                {
                    valence = SentimentUtils.SpecialCases[zeroOneTwo];
                }
            }

            // check for booster/dampener bi-grams such as 'sort of' or 'kind of'
            var nGrams = new[] { threeTwoOne, threeTwo, twoOne };
            foreach (var nGram in nGrams)
            {
                if (SentimentUtils.BoosterDict.ContainsKey(nGram))
                {
                    valence += SentimentUtils.BoosterDict[nGram];
                }
            }
            return valence;
        }

        private static double PunctuationEmphasis(string text)
        {
            return AmplifyExclamation(text) + AmplifyQuestion(text);
        }

        /// <summary>
        /// Check for added emphasis resulting from exclamation points (up to 4 of them).
        /// </summary>
        private static double AmplifyExclamation(string text)
        {
            int epCount = text.Count(x => x == '!');

            if (epCount > 4)
            {
                epCount = 4;
            }

            // (empirically derived mean sentiment intensity rating increase for exclamation points)
            return epCount * ExclIncr;
        }

        /// <summary>
        /// Check for added emphasis resulting from question marks (2 or 3+).
        /// </summary>
        private static double AmplifyQuestion(string text)
        {
            int qmCount = text.Count(x => x == '?');

            if (qmCount < 1)
            {
                return 0;
            }

            // (empirically derived mean sentiment intensity rating increase for question marks)
            return qmCount <= 3 ? qmCount * QuesIncrSmall : QuesIncrLarge;
        }

        /// <summary>
        /// Want separate positive versus negative sentiment scores.
        /// </summary>
        private static SiftSentiments SiftSentimentScores(IList<double> sentiments)
        {
            var siftSentiments = new SiftSentiments();

            foreach (var sentiment in sentiments)
            {
                if (sentiment > 0)
                {
                    siftSentiments.PosSum += (sentiment + 1); // compensates for neutral words that are counted as 1
                }
                else if (sentiment < 0)
                {
                    siftSentiments.NegSum += (sentiment - 1); // when used with math.fabs(), compensates for neutrals
                }
                else if (sentiment == 0)
                {
                    siftSentiments.NeuCount++;
                }
            }
            return siftSentiments;
        }

        private static SentimentAnalysisResults ScoreValence(IList<double> sentiments, string text)
        {
            if (sentiments.Count == 0)
            {
                return new SentimentAnalysisResults(); //will return with all 0
            }

            double sum = sentiments.Sum();
            double puncAmplifier = PunctuationEmphasis(text);

            sum += Math.Sign(sum) * puncAmplifier;

            double compound = SentimentUtils.Normalize(sum);
            SiftSentiments sifted = SiftSentimentScores(sentiments);

            if (sifted.PosSum > Math.Abs(sifted.NegSum))
            {
                sifted.PosSum += puncAmplifier;
            }
            else if (sifted.PosSum < Math.Abs(sifted.NegSum))
            {
                sifted.NegSum -= puncAmplifier;
            }

            double total = sifted.PosSum + Math.Abs(sifted.NegSum) + sifted.NeuCount;
            return new SentimentAnalysisResults
            {
                Compound = Math.Round(compound, 4),
                Positive = Math.Round(Math.Abs(sifted.PosSum / total), 3),
                Negative = Math.Round(Math.Abs(sifted.NegSum / total), 3),
                Neutral = Math.Round(Math.Abs(sifted.NeuCount / total), 3)
            };
        }

        private class SiftSentiments
        {
            public double PosSum { get; set; }
            public double NegSum { get; set; }
            public int NeuCount { get; set; }
        }
    }
}

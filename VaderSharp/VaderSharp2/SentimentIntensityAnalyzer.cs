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

        private readonly Dictionary<string, double> Lexicon = null;
        private readonly string[] LexiconFullFile = null;

        public SentimentIntensityAnalyzer()
        {
            if (Lexicon == null)
            {
                Assembly assembly = typeof(SentimentIntensityAnalyzer).GetTypeInfo().Assembly;

                using (var stream = assembly.GetManifestResourceStream("VaderSharp2.vader_lexicon.txt"))
                using (var reader = new StreamReader(stream))
                {
                    LexiconFullFile = reader.ReadToEnd().Split('\n');
                    Lexicon = MakeLexDic();
                }
            }
        }

        public SentimentIntensityAnalyzer(string fileName)
        {
            if (Lexicon == null)
            {
                if (!File.Exists(fileName))
                {
                    throw new Exception("Lexicon file not found");
                }

                using (var stream = new FileStream(fileName, FileMode.Open))
                using (var reader = new StreamReader(stream))
                {
                    LexiconFullFile = reader.ReadToEnd().Split('\n');
                    Lexicon = MakeLexDic();
                }
            }
        }

        private Dictionary<string, double> MakeLexDic()
        {
            var dic = new Dictionary<string, double>();
            foreach (var line in LexiconFullFile)
            {
                var lineArray = line.Trim().Split('\t');
                dic.Add(lineArray[0], double.Parse(lineArray[1], CultureInfo.InvariantCulture));
            }
            return dic;
        }

        /// <summary>
        /// Return metrics for positive, negative and neutral sentiment based on the input text.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public SentimentAnalysisResults PolarityScores(string input)
        {
            var sentiText = new SentiText(input);
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

            return ScoreValence(sentiments, input);
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

                    valence = NeverCheck(valence, wordsAndEmoticons, startI, i);

                    if (startI == 2)
                    {
                        valence = IdiomsCheck(valence, wordsAndEmoticons, i);
                    }
                }
            }

            valence = LeastCheck(valence, wordsAndEmoticons, i);
            sentiments.Add(valence);
            return sentiments;
        }

        private static IList<double> ButCheck(IList<string> wordsAndEmoticons, IList<double> sentiments)
        {
            bool containsBUT = wordsAndEmoticons.Contains("BUT");
            bool containsbut = wordsAndEmoticons.Contains("but");
            if (!containsBUT && !containsbut)
                return sentiments;

            int butIndex = (containsBUT)
                ? wordsAndEmoticons.IndexOf("BUT")
                : wordsAndEmoticons.IndexOf("but");

            for (int i = 0; i < sentiments.Count; i++)
            {
                double sentiment = sentiments[i];
                if (i < butIndex)
                {
                    sentiments.RemoveAt(i);
                    sentiments.Insert(i, sentiment * 0.5);
                }
                else if (i > butIndex)
                {
                    sentiments.RemoveAt(i);
                    sentiments.Insert(i, sentiment * 1.5);
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

        private static double NeverCheck(double valence, IList<string> wordsAndEmoticons, int startI, int i)
        {
            if (startI == 0)
            {
                if (SentimentUtils.Negated(new List<string> { wordsAndEmoticons[i - 1].ToLower() }))
                {
                    valence *= SentimentUtils.NScalar;
                }
            }
            else if (startI == 1)
            {
                if (wordsAndEmoticons[i - 2] == "never" &&
                    (wordsAndEmoticons[i - 1] == "so" || wordsAndEmoticons[i - 1] == "this"))
                {
                    valence *= 1.5;
                }
                else if (SentimentUtils.Negated(new List<string> { wordsAndEmoticons[i - (startI + 1)].ToLower() }))
                {
                    valence *= SentimentUtils.NScalar;
                }
            }
            else if (startI == 2)
            {
                if (wordsAndEmoticons[i - 3] == "never"
                    && (wordsAndEmoticons[i - 2] == "so" || wordsAndEmoticons[i - 2] == "this")
                    || (wordsAndEmoticons[i - 1] == "so" || wordsAndEmoticons[i - 1] == "this"))
                {
                    valence *= 1.25;
                }
                else if (SentimentUtils.Negated(new List<string> { wordsAndEmoticons[i - (startI + 1)].ToLower() }))
                {
                    valence *= SentimentUtils.NScalar;
                }
            }

            return valence;
        }

        private static double IdiomsCheck(double valence, IList<string> wordsAndEmoticons, int i)
        {
            var oneZero = string.Concat(wordsAndEmoticons[i - 1], " ", wordsAndEmoticons[i]);
            var twoOneZero = string.Concat(wordsAndEmoticons[i - 2], " ", wordsAndEmoticons[i - 1], " ", wordsAndEmoticons[i]);
            var twoOne = string.Concat(wordsAndEmoticons[i - 2], " ", wordsAndEmoticons[i - 1]);
            var threeTwoOne = string.Concat(wordsAndEmoticons[i - 3], " ", wordsAndEmoticons[i - 2], " ", wordsAndEmoticons[i - 1]);
            var threeTwo = string.Concat(wordsAndEmoticons[i - 3], " ", wordsAndEmoticons[i - 2]);

            string[] sequences = { oneZero, twoOneZero, twoOne, threeTwoOne, threeTwo };

            foreach (var seq in sequences)
            {
                if (SentimentUtils.SpecialCase.ContainsKey(seq))
                {
                    valence = SentimentUtils.SpecialCase[seq];
                    break;
                }
            }

            if (wordsAndEmoticons.Count - 1 > i)
            {
                string zeroOne = string.Concat(wordsAndEmoticons[i], " ", wordsAndEmoticons[i + 1]);
                if (SentimentUtils.SpecialCase.ContainsKey(zeroOne))
                {
                    valence = SentimentUtils.SpecialCase[zeroOne];
                }
            }

            if (wordsAndEmoticons.Count - 1 > i + 1)
            {
                string zeroOneTwo = string.Concat(wordsAndEmoticons[i], " ", wordsAndEmoticons[i + 1], " ", wordsAndEmoticons[i + 2]);
                if (SentimentUtils.SpecialCase.ContainsKey(zeroOneTwo))
                {
                    valence = SentimentUtils.SpecialCase[zeroOneTwo];
                }
            }

            if (SentimentUtils.BoosterDict.ContainsKey(threeTwo) || SentimentUtils.BoosterDict.ContainsKey(twoOne))
            {
                valence += SentimentUtils.BDecr;
            }
            return valence;
        }

        private static double PunctuationEmphasis(string text)
        {
            return AmplifyExclamation(text) + AmplifyQuestion(text);
        }

        private static double AmplifyExclamation(string text)
        {
            int epCount = text.Count(x => x == '!');

            if (epCount > 4)
            {
                epCount = 4;
            }

            return epCount * ExclIncr;
        }

        private static double AmplifyQuestion(string text)
        {
            int qmCount = text.Count(x => x == '?');

            if (qmCount < 1)
            {
                return 0;
            }

            return qmCount <= 3 ? qmCount * QuesIncrSmall : QuesIncrLarge;
        }

        private static SiftSentiments SiftSentimentScores(IList<double> sentiments)
        {
            var siftSentiments = new SiftSentiments();

            foreach (var sentiment in sentiments)
            {
                if (sentiment > 0)
                {
                    siftSentiments.PosSum += (sentiment + 1); // 1 compensates for neutrals
                }
                else if (sentiment < 0)
                {
                    siftSentiments.NegSum += (sentiment - 1);
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

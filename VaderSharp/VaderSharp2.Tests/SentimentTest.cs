using System;
using Xunit;

namespace VaderSharp2.Tests
{
    public class SentimentTest
    {
        [Fact]
        public void MatchPythonTest()
        {
            var analyzer = new SentimentIntensityAnalyzer();

            var standardGoodTest = analyzer.PolarityScores("VADER is smart, handsome, and funny.");
            Assert.Equal(0, standardGoodTest.Negative);
            Assert.Equal(0.254, standardGoodTest.Neutral);
            Assert.Equal(0.746, standardGoodTest.Positive);
            Assert.Equal(0.8316, standardGoodTest.Compound);

            var kindOfTest = analyzer.PolarityScores("The book was kind of good.");
            Assert.Equal(0, kindOfTest.Negative);
            Assert.Equal(0.657, kindOfTest.Neutral);
            Assert.Equal(0.343, kindOfTest.Positive);
            Assert.Equal(0.3832, kindOfTest.Compound);

            var complexTest = analyzer.PolarityScores("The plot was good, but the characters are uncompelling and the dialog is not great.");
            Assert.Equal(0.327, complexTest.Negative);
            Assert.Equal(0.579, complexTest.Neutral);
            Assert.Equal(0.094, complexTest.Positive);
            Assert.Equal(-0.7042, complexTest.Compound);
        }

        [Fact]
        public void TestConfigStore()
        {
            ConfigStore cfg = ConfigStore.CreateConfig("en-gb");
            var negations = cfg.Negations;
            string[] Negate =
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
            bool isExisting;
            Assert.Equal(negations.Length, Negate.Length);

            foreach (var a in negations)
            {
                isExisting = false;
                foreach (var b in Negate)
                {
                    if (a.Equals(b))
                    {
                        isExisting = true;
                        break;
                    }
                }
                Assert.True(isExisting);
            }
        }
    }
}

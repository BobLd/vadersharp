using System.Collections.Generic;
using System.IO;
using Xunit;

namespace VaderSharp2.Tests
{
    /// <summary>
    /// Tests the port to C# by running the Ground Truth data sets from the python implementation,
    /// which contains expected results, against the results from the C# implementation
    /// </summary>
    public class PortTests
    {
        private static SentimentIntensityAnalyzer analyzer = new SentimentIntensityAnalyzer();

        // Constants
        private const char DELIMITER = '\t';

        // Variables

        public static IEnumerable<object[]> AmazonData;
        public static IEnumerable<object[]> MovieData;
        public static IEnumerable<object[]> NytData;
        public static IEnumerable<object[]> TweetsData;

        static PortTests()
        {
            // These TSVs have been taken from the Java implementation's tests: https://github.com/apanimesh061/VaderSentimentJava/tree/master/src/test/resources
            // If that isn't maintained in future, they could also be generated from the original Python implementation: https://github.com/cjhutto/vaderSentiment

            AmazonData = LoadSentenses("amazonReviewSnippets_GroundTruth_vader-3.3.2.tsv");
            MovieData = LoadSentenses("movieReviewSnippets_GroundTruth_vader-3.3.2.tsv");
            NytData = LoadSentenses("nytEditorialSnippets_GroundTruth_vader-3.3.2.tsv");
            TweetsData = LoadSentenses("tweets_GroundTruth_vader-3.3.2.tsv");
        }

        private static IEnumerable<object[]> LoadSentenses(string fileName)
        {
            string path = Path.Combine("../../../Resources/GroundTruth", fileName);

            using (var stream = File.Open(path, FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Split the line around the delimiter
                    string[] parts = line.Split(DELIMITER);

                    // Get the input & scores
                    string text = parts[5];

                    var expected = new SentimentAnalysisResults()
                    {
                        Negative = double.Parse(parts[1]),
                        Neutral = double.Parse(parts[2]),
                        Positive = double.Parse(parts[3]),
                        Compound = double.Parse(parts[4])
                    };
                    yield return new object[] { text, expected };
                }
            }
        }

        [Theory]
        [MemberData(nameof(AmazonData))]
        public void AmazonTest(string text, SentimentAnalysisResults expected)
        {
            Assert.Equal(expected, analyzer.PolarityScores(text));
        }

        [Theory]
        [MemberData(nameof(MovieData))]
        public void MovieTest(string text, SentimentAnalysisResults expected)
        {
            Assert.Equal(expected, analyzer.PolarityScores(text));
        }

        [Theory]
        [MemberData(nameof(NytData))]
        public void NytTest(string text, SentimentAnalysisResults expected)
        {
            Assert.Equal(expected, analyzer.PolarityScores(text));
        }

        [Theory]
        [MemberData(nameof(TweetsData))]
        public void TweetsTest(string text, SentimentAnalysisResults expected)
        {
            Assert.Equal(expected, analyzer.PolarityScores(text));
        }

        [Fact]
        public void EmojiTest()
        {
            // {'pos': 0.279, 'compound': 0.7003, 'neu': 0.721, 'neg': 0.0}
            string text = "Catch utf-8 emoji such as 💘 and 💋 and 😁";
            var expected = new SentimentAnalysisResults()
            {
                Positive = 0.279,
                Compound = 0.7003,
                Neutral = 0.721,
                Negative = 0.0
            };
            Assert.Equal(expected, analyzer.PolarityScores(text));
        }
    }
}

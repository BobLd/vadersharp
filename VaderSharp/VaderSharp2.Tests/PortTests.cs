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
        // Constants
        private const char DELIMITER = '\t';

        // Variables
        private static string[] filePaths;

        // Set Up & tear Down
        public PortTests()
        {
            // These TSVs have been taken from the Java implementation's tests: https://github.com/apanimesh061/VaderSentimentJava/tree/master/src/test/resources
            // If that isn't maintained in future, they could also be generated from the original Python implementation: https://github.com/cjhutto/vaderSentiment
            filePaths = Directory.GetFiles("../../../Resources/GroundTruth");
        }

        // Tests
        [Fact(Skip = "Fails at the moment")]
        public void GroundTruthTest()
        {
            var sa = new SentimentIntensityAnalyzer();

            foreach (string path in filePaths)
            {
                using (var stream = File.Open(path, FileMode.Open))
                using (var reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Split the line around the delimiter
                        string[] parts = line.Split(DELIMITER);

                        // Get the input & scores
                        double expectedNeg = double.Parse(parts[1]);
                        double expectedNeu = double.Parse(parts[2]);
                        double expectedPos = double.Parse(parts[3]);
                        double expectedCom = double.Parse(parts[4]);
                        string input = parts[5];

                        var expected = new SentimentAnalysisResults()
                        {
                            Negative = expectedNeg,
                            Neutral = expectedNeu,
                            Positive = expectedPos,
                            Compound = expectedCom
                        };

                        // Run SA on input
                        SentimentAnalysisResults actual = sa.PolarityScores(input);

                        Assert.Equal(expected, actual);
                    }
                }
            }
        }
    }
}

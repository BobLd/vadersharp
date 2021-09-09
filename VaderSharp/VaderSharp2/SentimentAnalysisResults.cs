namespace VaderSharp2
{
    /// <summary>
    /// A model to represent the result of analysis.
    /// </summary>
    public class SentimentAnalysisResults
    {
        /// <summary>
        /// The proportion of words in the sentence with negative valence.
        /// </summary>
        public double Negative { get; init; }

        /// <summary>
        /// The proportion of words in the sentence with no valence.
        /// </summary>
        public double Neutral { get; init; }

        /// <summary>
        /// The proportion of words in the sentence with positive valence.
        /// </summary>
        public double Positive { get; init; }

        /// <summary>
        /// Normalized sentiment score between -1 and 1.
        /// </summary>
        public double Compound { get; init; }

        public override bool Equals(object obj)
        {
            if (obj is SentimentAnalysisResults sar)
            {
                return Negative.Equals(sar.Negative) &&
                       Neutral.Equals(sar.Neutral) &&
                       Positive.Equals(sar.Positive) &&
                       Compound.Equals(sar.Compound);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Negative, Neutral, Positive, Compound);
        }
    }
}

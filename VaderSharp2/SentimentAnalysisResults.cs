namespace VaderSharp2
{
    /// <summary>
    /// A model to represent the result of analysis.
    /// </summary>
    public struct SentimentAnalysisResults
    {
        /// <summary>
        /// The proportion of words in the sentence with negative valence.
        /// </summary>
        public double Negative { get; internal set; }

        /// <summary>
        /// The proportion of words in the sentence with no valence.
        /// </summary>
        public double Neutral { get; internal set; }

        /// <summary>
        /// The proportion of words in the sentence with positive valence.
        /// </summary>
        public double Positive { get; internal set; }

        /// <summary>
        /// Normalized sentiment score between -1 and 1.
        /// </summary>
        public double Compound { get; internal set; }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashcode = 1430287;
            hashcode = hashcode * 7302013 ^ Negative.GetHashCode();
            hashcode = hashcode * 7302013 ^ Neutral.GetHashCode();
            hashcode = hashcode * 7302013 ^ Positive.GetHashCode();
            hashcode = hashcode * 7302013 ^ Compound.GetHashCode();
            return hashcode;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Neg: {Negative}, Neu: {Neutral}, Pos: {Positive}, Compound: {Compound}";
        }

        /// <inheritdoc/>
        public static bool operator ==(SentimentAnalysisResults left, SentimentAnalysisResults right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(SentimentAnalysisResults left, SentimentAnalysisResults right)
        {
            return !(left == right);
        }
    }
}

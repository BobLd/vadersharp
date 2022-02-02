# VaderSharp2 - The best sentiment analysis tool. In C#
"VADER (Valence Aware Dictionary and sEntiment Reasoner) is a lexicon and rule-based sentiment analysis tool that is specifically attuned to sentiments expressed in social media."

Previously VADER was only available in python (https://github.com/cjhutto/vaderSentiment), and was then ported to C# in https://github.com/codingupastorm/vadersharp (this is a fork from this repos).

VaderSharp2 is an updated version of the original VaderSharp.

[![Linux](https://github.com/BobLd/vadersharp/actions/workflows/dotnet-linux.yml/badge.svg)](https://github.com/BobLd/vadersharp/actions/workflows/dotnet-linux.yml)
[![Mac OS](https://github.com/BobLd/vadersharp/actions/workflows/dotnet-macos.yml/badge.svg)](https://github.com/BobLd/vadersharp/actions/workflows/dotnet-macos.yml)
[![Windows](https://github.com/BobLd/vadersharp/actions/workflows/dotnet-windows.yml/badge.svg)](https://github.com/BobLd/vadersharp/actions/workflows/dotnet-windows.yml)

## Citation Information ([source](https://github.com/cjhutto/vaderSentiment#citation-information))
If you use either the dataset or any of the VADER sentiment analysis tools (VADER sentiment lexicon or Python code for rule-based sentiment analysis engine) in your research, please cite the above paper. For example:  

>  **Hutto, C.J. & Gilbert, E.E. (2014). VADER: A Parsimonious Rule-based Model for Sentiment Analysis of Social Media Text. Eighth International Conference on Weblogs and Social Media (ICWSM-14). Ann Arbor, MI, June 2014.** 

## Changes since [original port](https://github.com/cjhutto/vaderSentiment) to C#
- Implement emojis
- Updated to the `3.3.2` Python version
- C# version now tracks Python versioning
- Targets `netcoreapp3.1`, `netcoreapp2.1`, `net452`, `net46`, `net461`, `net462`, `net47`, `net5.0`, `net6.0`
- In-depth testing

**NOTE:** There is a problem with how the [`_but_check`](https://github.com/cjhutto/vaderSentiment/blob/d8da3e21374a57201b557a4c91ac4dc411a08fed/vaderSentiment/vaderSentiment.py#L333-L346) function works in the python version - it uses `sentiments.index(sentiment)` on the double value... This results in unexpected results. This version has the correct behaviour.

## Getting Started
To install VaderSharp, run the following command in the Package Manager Console:

```
Install-Package VaderSharp2
```

## Usage
Import the package at the top of the page:
```csharp
using VaderSharp2;
```

Then just initialize an instance of SentimentIntensityAnalyzer and call it's PolarityScores method:
```csharp
var analyzer = new SentimentIntensityAnalyzer();

var results = analyzer.PolarityScores("Wow, this package is amazingly easy to use");

Console.WriteLine("Positive score: " + results.Positive);
Console.WriteLine("Negative score: " + results.Negative);
Console.WriteLine("Neutral score: " + results.Neutral);
Console.WriteLine("Compound score: " + results.Compound);
```

## About the Scoring ([source](https://github.com/cjhutto/vaderSentiment#about-the-scoring))
* The ``compound`` score is computed by summing the valence scores of each word in the lexicon, adjusted according to the rules, and then normalized to be between -1 (most extreme negative) and +1 (most extreme positive). This is the most useful metric if you want a single unidimensional measure of sentiment for a given sentence. Calling it a 'normalized, weighted composite score' is accurate. 
 
  It is also useful for researchers who would like to set standardized thresholds for classifying sentences as either positive, neutral, or negative.  
  Typical threshold values (used in the literature cited on this page) are:

> 1. **positive sentiment**: ``compound`` score >=  0.05
> 2. **neutral  sentiment**: (``compound`` score > -0.05) and (``compound`` score < 0.05)
> 3. **negative sentiment**: ``compound`` score <= -0.05
>
> **NOTE:** The ``compound`` score is the one most commonly used for sentiment analysis by most researchers, including the authors.

* The ``pos``, ``neu``, and ``neg`` scores are *ratios for proportions of text that fall in each category* (so these should all add up to be 1... or close to it with float operation).  These are the most useful metrics if you want to analyze the context & presentation of how sentiment is conveyed or embedded in rhetoric for a given sentence. For example, different writing styles may embed strongly positive or negative sentiment within varying proportions of neutral text -- i.e., some writing styles may reflect a penchant for strongly flavored rhetoric, whereas other styles may use a great deal of neutral text while still conveying a similar overall (compound) sentiment. As another example: researchers analyzing information presentation in journalistic or editorical news might desire to establish whether the proportions of text (associated with a topic or named entity, for example) are balanced with similar amounts of positively and negatively framed text versus being "biased" towards one polarity or the other for the topic/entity.

  * **IMPORTANTLY:** these proportions represent the "raw categorization" of each lexical item (e.g., words, emoticons/emojis, or initialisms) into positve, negative, or neutral classes; they **do not** account for the VADER rule-based enhancements such as word-order sensitivity for sentiment-laden multi-word phrases, degree modifiers, word-shape amplifiers, punctuation amplifiers, negation polarity switches, or contrastive conjunction sensitivity.

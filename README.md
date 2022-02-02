# VaderSharp 2
[![Linux](https://github.com/BobLd/vadersharp/actions/workflows/dotnet-linux.yml/badge.svg)](https://github.com/BobLd/vadersharp/actions/workflows/dotnet-linux.yml)
## Changes since original port
- Implement emojis
- Updated to the 3.3.2 Python version
- C# version now tracks Python version
- Targets netcoreapp3.1, netcoreapp2.1, net452, net46, net461, net462, net47, net5.0, net6.0
- **NOTE:** There is a problem with how the `_but_check` function works in the python version - it uses `sentiments.index(sentiment)` on the double value... This results in unexpected results. This version has the correct behaviour.

# ORIGINAL README VaderSharp. The best sentiment analysis tool. In C#.

"VADER (Valence Aware Dictionary and sEntiment Reasoner) is a lexicon and rule-based sentiment analysis tool that is specifically attuned to sentiments expressed in social media."

Previously VADER was only available in python (https://github.com/cjhutto/vaderSentiment). I wanted to use it in C# so ported it over.

# Getting Started

VaderSharp supports:

- .NET Core
- .NET Framework 3.5 and above
- Mono & Xamarin
- UWP

To install VaderSharp, run the following command in the Package Manager Console:

```
Install-Package CodingUpAStorm.VaderSharp
```

# Usage

Import the package at the top of the page:

```c#
using VaderSharp;
```

Then just initialize an instance of SentimentIntensityAnalyzer and call it's PolarityScores method:

```c#
SentimentIntensityAnalyzer analyzer = new SentimentIntensityAnalyzer();

var results = analyzer.PolarityScores("Wow, this package is amazingly easy to use");

Console.WriteLine("Positive score: " + results.Positive);
Console.WriteLine("Negative score: " + results.Negative);
Console.WriteLine("Neutral score: " + results.Neutral);
Console.WriteLine("Compound score: " + results.Compound);
```

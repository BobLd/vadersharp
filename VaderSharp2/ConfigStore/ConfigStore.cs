﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System;

namespace VaderSharp2
{
    /// <summary>
    /// <para>Proof of concept for loading the words to be used as boosters, negations etc.</para>
    /// <para>Currently not used.</para>
    /// </summary>
    [Obsolete("Proof of concept. Currently not used.")]
    internal class ConfigStore
    {
        private static ConfigStore config;

        public Dictionary<string, double> BoosterDict { get; private set; }

        public string[] Negations { get; private set; }

        public Dictionary<string, double> SpecialCaseIdioms { get; private set; }

        private ConfigStore(string languageCode)
        {
            LoadConfig(languageCode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="languageCode">Language code in writing style "language-country". Default is British English.</param>
        /// <returns>ConfigStore object.</returns>
        public static ConfigStore CreateConfig(string languageCode = "en-gb")
        {
            if (config == null)
            {
                config = new ConfigStore(languageCode);
            }
            return config;
        }

        /// <summary>
        /// Initializes the ConfigStore and loads the config file.
        /// </summary>
        /// <param name="languageCode">Language code in writing style "language-country".</param>
        private void LoadConfig(string languageCode)
        {
            string path = Path.GetFullPath($"../../../../VaderSharp2/ConfigStore/strings/{languageCode}.xml");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Language file was not found. Please check language code.");
            }
            XElement root = XDocument.Load(path).Document.Root;
            LoadNegations(root);
            LoadIdioms(root);
            LoadBooster(root);
        }

        /// <summary>
        /// Loads negations from config file.
        /// </summary>
        /// <param name="root">Root element of XML document</param>
        private void LoadNegations(XElement root)
        {
            var nodes = root.Descendants(XName.Get("negation"));
            int length = nodes.Count();
            Negations = new string[length];
            for (int i = 0; i < length; i++)
            {
                Negations[i] = nodes.ElementAt(i).Value;
            }
        }

        /// <summary>
        /// Loads idioms from config file.
        /// </summary>
        /// <param name="root">Root element of XML document</param>
        private void LoadIdioms(XElement root)
        {
            SpecialCaseIdioms = new Dictionary<string, double>();
            var nodes = root.Descendants(XName.Get("idiom"));
            double value;
            foreach (var n in nodes)
            {
                value = double.Parse(n.Attribute(XName.Get("value")).Value);
                SpecialCaseIdioms.Add(n.Value, value);
            }
        }

        /// <summary>
        /// Loads booster words from config file.
        /// </summary>
        /// <param name="root">Root element of XML document</param>
        private void LoadBooster(XElement root)
        {
            BoosterDict = new Dictionary<string, double>();
            var nodes = root.Descendants(XName.Get("booster"));
            double sign;
            foreach (var n in nodes)
            {
                sign = n.Attribute(XName.Get("sign")).Value == "BIncr" ? 0.293 : -0.293;
                BoosterDict.Add(n.Value, sign);
            }
        }
    }
}

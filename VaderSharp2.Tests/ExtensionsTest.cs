using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace VaderSharp2.Tests
{
    public class ExtensionsTest
    {
        public static readonly IEnumerable<object[]> IsUpperData = new List<object[]>
        {
            new object[] { ":D", true },
            new object[] { ":d", false },
            new object[] { ":)", false },
            new object[] { "Hello", false },
        };

        [Theory]
        [MemberData(nameof(IsUpperData))]
        public void IsUpperTest(string text, bool expected)
        {
            Assert.Equal(expected, text.IsUpper());
        }
    }
}

﻿/*
 * SonarLint for Visual Studio
 * Copyright (C) 2015-2016 SonarSource SA
 * mailto:contact@sonarsource.com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarLint.Common;
using SonarLint.Helpers;
using SonarLint.Runner;
using System.IO;
using System.Linq;

namespace SonarLint.UnitTest
{
    [TestClass]
    public class ProgramTest
    {
        [TestMethod]
        public void End_To_End()
        {
            Program.Main(new [] {
                $@"TestResources\{ParameterLoader.ParameterConfigurationFileName}",
                "Output.xml",
                AnalyzerLanguage.CSharp.ToString()});

            var textActual = new string(File.ReadAllText("Output.xml")
                .ToCharArray()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());

            CheckExpected(textActual);
            CheckNotExpected(textActual);
        }

        private static void CheckExpected(string textActual)
        {
            var expectedContent = new[]
            {
                @"<AnalysisOutput><Files><File><Path>TestResources\TestInput.cs</Path>",
                @"<Metrics><Lines>16</Lines>",
                @"<Issue><Id>S1134</Id><Line>3</Line>",
                @"<Issue><Id>S1135</Id><Line>5</Line>",
                @"<Id>S101</Id><Line>1</Line><Message>Renamethisclass""TestClass""tomatchtheregularexpression:^(?:[A-HJ-Z][a-zA-Z0-9])$</Message>",
                @"<Id>S103</Id><Line>10</Line><Message>Splitthis21characterslongline(whichisgreaterthan10authorized).</Message>",
                @"<Id>S103</Id><Line>13</Line><Message>Splitthis17characterslongline(whichisgreaterthan10authorized).</Message>",
                @"<Id>S104</Id><Line>1</Line><Message>Thisfilehas16lines,whichisgreaterthan10authorized.Splititintosmallerfiles.</Message>"
            };

            foreach (var expected in expectedContent)
            {
                if (!textActual.Contains(expected))
                {
                    Assert.Fail("Generated output file doesn't contain expected string '{0}'", expected);
                }
            }
        }
        private static void CheckNotExpected(string textActual)
        {
            var notExpectedContent = new[]
            {
                @"<Id>S1116</Id><Line>14</Line>"
            };

            foreach (var notExpected in notExpectedContent)
            {
                if (textActual.Contains(notExpected))
                {
                    Assert.Fail("Generated output file contains not expected string '{0}'", notExpected);
                }
            }
        }
    }
}

/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using NUnit.Framework;
using QuantConnect.Brokerages.Tradier;

namespace QuantConnect.Tests.Brokerages.Tradier
{
    [TestFixture]
    public class TradierSymbolMapperTests
    {
        private TradierSymbolMapper _symbolMapper;
        private static TestCaseData[] TestParameters
        {
            get
            {
                return new[]
                {
                    new TestCaseData(Symbols.AAPL, "AAPL", null),
                    new TestCaseData(Symbols.SPX, "SPX", null),
                    new TestCaseData(Symbol.Create("VIX", SecurityType.Index, Market.USA), "XSP", null),
                    new TestCaseData(Symbol.CreateOption("QQQ", Market.USA, SecurityType.Option.DefaultOptionStyle(), OptionRight.Put, 350m, new DateTime(2025, 7, 25)), "QQQ250725P00350000", null),
                    new TestCaseData(Symbol.CreateOption("SPY", Market.USA, SecurityType.Option.DefaultOptionStyle(), OptionRight.Call, 410m, new DateTime(2021, 3, 19)), "SPY210319C00410000", null),
                    new TestCaseData(Symbol.CreateOption(Symbols.SPX, "SPXW", Market.USA, SecurityType.IndexOption.DefaultOptionStyle(), OptionRight.Call, 5900m, new DateTime(2025, 7, 25)), "SPXW250725C05900000", "SPX"),
                    // BRK.B equity and option contract test cases
                    // Note: Brokerage symbols use slashes for dot tickers (BRK.B -> BRK/B)
                    new TestCaseData(Symbol.Create("BRK.B", SecurityType.Equity, Market.USA), "BRK/B", null),
                    new TestCaseData(Symbol.CreateOption(Symbol.Create("BRK.B", SecurityType.Equity, Market.USA), Market.USA, SecurityType.Option.DefaultOptionStyle(), OptionRight.Call, 190m, new DateTime(2025, 6, 20)), "BRKB250620C00190000", "BRK/B"),
                };
            }
        }

        [OneTimeSetUp]
        public void Setup()
        {
            _symbolMapper = new TradierSymbolMapper();
        }

        [Test, TestCaseSource(nameof(TestParameters))]
        public void ReturnsCorrectLeanSymbol(Symbol expectedLeanSymbol, string brokerSymbol, string underlyingBrokerageSymbol)
        {
            Assert.AreEqual(expectedLeanSymbol, _symbolMapper.GetLeanSymbol(brokerSymbol, underlyingBrokerageSymbol));
        }

        [Test, TestCaseSource(nameof(TestParameters))]
        public void ReturnsCorrectBrokerageSymbol(Symbol leanSymbol, string expectedBrokerSymbol, string underlyingBrokerageSymbol)
        {
            Assert.AreEqual(expectedBrokerSymbol, _symbolMapper.GetBrokerageSymbol(leanSymbol));
        }
    }
}
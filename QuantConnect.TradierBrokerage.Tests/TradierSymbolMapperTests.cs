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
        [Test]
        public void ReturnsCorrectLeanSymbol()
        {
            var mapper = new TradierSymbolMapper();

            var leanSymbol = mapper.GetLeanSymbol("AAPL");
            var expected = Symbols.AAPL;
            Assert.AreEqual(expected, leanSymbol);

            leanSymbol = mapper.GetLeanSymbol("SPY210319C00410000");
            expected = Symbol.CreateOption("SPY", Market.USA, OptionStyle.American, OptionRight.Call, 410m, new DateTime(2021, 3, 19));
            Assert.AreEqual(expected, leanSymbol);

            leanSymbol = mapper.GetLeanSymbol("QQQ250725P00350000");
            expected = Symbol.CreateOption("QQQ", Market.USA, SecurityType.Option.DefaultOptionStyle(), OptionRight.Put, 350m, new DateTime(2025, 7, 25));
            Assert.AreEqual(expected, leanSymbol);

            leanSymbol = mapper.GetLeanSymbol("SPX");
            Assert.AreEqual(Symbols.SPX, leanSymbol);

            leanSymbol = mapper.GetLeanSymbol("SPXW250725C05900000");
            expected = Symbol.CreateOption(Symbols.SPX, "SPXW", Market.USA, SecurityType.IndexOption.DefaultOptionStyle(), OptionRight.Call, 5900m, new DateTime(2025, 7, 25));
            Assert.AreEqual(expected, leanSymbol);

            leanSymbol = mapper.GetLeanSymbol("XSP");
            expected = Symbol.Create("XSP", SecurityType.Index, Market.USA);
            Assert.AreEqual(expected, leanSymbol);

            leanSymbol = mapper.GetLeanSymbol("XSP250725C00480000");
            var idxSymbol = Symbol.Create("XSP", SecurityType.Index, Market.USA);
            expected = Symbol.CreateOption(idxSymbol, "XSP", Market.USA, SecurityType.IndexOption.DefaultOptionStyle(), OptionRight.Call, 480m, new DateTime(2025, 7, 25));
            Assert.AreEqual(expected, leanSymbol);
        }

        [Test]
        public void ReturnsCorrectBrokerageSymbol()
        {
            var mapper = new TradierSymbolMapper();

            var equitySymbol = Symbols.AAPL;
            var brokerageSymbol = mapper.GetBrokerageSymbol(equitySymbol);
            Assert.AreEqual("AAPL", brokerageSymbol);

            var optionSymbol = Symbol.CreateOption("SPY", Market.USA, OptionStyle.American, OptionRight.Call, 410m, new DateTime(2021, 3, 19));
            brokerageSymbol = mapper.GetBrokerageSymbol(optionSymbol);
            Assert.AreEqual("SPY210319C00410000", brokerageSymbol);

            optionSymbol = Symbol.CreateOption("QQQ", Market.USA, SecurityType.Option.DefaultOptionStyle(), OptionRight.Put, 350m, new DateTime(2025, 7, 25));
            brokerageSymbol = mapper.GetBrokerageSymbol(optionSymbol);
            Assert.AreEqual("QQQ250725P00350000", brokerageSymbol);

            brokerageSymbol = mapper.GetBrokerageSymbol(Symbols.SPX);
            Assert.AreEqual("SPX", brokerageSymbol);

            optionSymbol = Symbol.CreateOption(Symbols.SPX, "SPXW", Market.USA, SecurityType.IndexOption.DefaultOptionStyle(), OptionRight.Call, 5900m, new DateTime(2025, 7, 25));
            brokerageSymbol = mapper.GetBrokerageSymbol(optionSymbol);
            Assert.AreEqual("SPXW250725C05900000", brokerageSymbol);

            var idxSymbol = Symbol.Create("XSP", SecurityType.Index, Market.USA);
            brokerageSymbol = mapper.GetBrokerageSymbol(idxSymbol);
            Assert.AreEqual("XSP", brokerageSymbol);

            optionSymbol = Symbol.CreateOption(idxSymbol, "XSP", Market.USA, SecurityType.IndexOption.DefaultOptionStyle(), OptionRight.Call, 480m, new DateTime(2025, 7, 25));
            brokerageSymbol = mapper.GetBrokerageSymbol(optionSymbol);
            Assert.AreEqual("XSP250725C00480000", brokerageSymbol);
        }
    }
}

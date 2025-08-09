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

using NUnit.Framework;
using QuantConnect.Brokerages.Tradier;
using QuantConnect.Configuration;
using QuantConnect.Tests.Engine.DataFeeds;
using System;
using System.Linq;

namespace QuantConnect.Tests.Brokerages.Tradier
{
    [TestFixture]
    public class TradierDataQueueUniverseProviderTests
    {
        private TradierBrokerage _brokerage;
        private bool _useSandbox = Config.Get("tradier-environment") == "paper";
        private readonly string _accountId = Config.Get("tradier-account-id");
        private readonly string _accessToken = Config.Get("tradier-access-token");


        private static TestCaseData[] TestParameters
        {
            get
            {
                return new[]
                {
                    new TestCaseData(Symbols.AAPL, 0).SetDescription("Cannot get lookup symbols for Equity symbols"),
                    new TestCaseData(Symbol.Create("QQQ", SecurityType.Option, Market.USA), 0).SetDescription("To fetch contracts we need OptionChainProvider, that is not possible with AlgorithmStub"),
                    new TestCaseData(Symbols.SPX, 0).SetDescription("To fetch contracts we need OptionChainProvider, that is not possible with AlgorithmStub"),
                    new TestCaseData(Symbol.CreateCanonicalOption(Symbols.SPX, "SPXW", Market.USA, "?SPXW"), 0).SetDescription("To fetch contracts we need OptionChainProvider, that is not possible with AlgorithmStub"),
                    new TestCaseData(Symbol.Create("XSP", SecurityType.IndexOption, Market.USA), 0).SetDescription("To fetch contracts we need OptionChainProvider, that is not possible with AlgorithmStub")
                };
            }
        }

        [OneTimeSetUp]
        public void Setup()
        {
            _brokerage = new TradierBrokerage(new AlgorithmStub(), null, null, null, _useSandbox, _accountId, _accessToken);
        }

        [Test, TestCaseSource(nameof(TestParameters))]
        public void LookUpSymbolsTest(Symbol symbol, int compareResponseCount)
        {
            //GetsFullDataOptionChain(symbol, DateTime.Now);
            var contracts = _brokerage.LookupSymbols(symbol, false, null);

            Assert.AreEqual(contracts.Count(), compareResponseCount);
        }
    }
}

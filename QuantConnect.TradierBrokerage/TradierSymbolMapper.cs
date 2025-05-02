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
using System.Collections.Generic;

namespace QuantConnect.Brokerages.Tradier
{
    /// <summary>
    /// Provides the mapping between Lean symbols and Tradier symbols.
    /// </summary>
    public class TradierSymbolMapper : ISymbolMapper
    {
        /// <summary>
        /// Converts a Lean symbol instance to a Tradier symbol
        /// </summary>
        /// <param name="symbol">A Lean symbol instance</param>
        /// <returns>The Tradier symbol</returns>
        public string GetBrokerageSymbol(Symbol symbol)
        {
            return symbol.SecurityType.IsOption()
                //? symbol.Value.Replace(" ", "")
                ? ConvertOptionSymbol(symbol.Value, symbol)
                : symbol.Value;
        }

        /// <summary>
        /// Converts a Tradier symbol to a Lean symbol instance
        /// </summary>
        /// <param name="brokerageSymbol">The Tradier symbol</param>
        /// <param name="securityType">The security type</param>
        /// <param name="market">The market</param>
        /// <param name="expirationDate">Expiration date of the security(if applicable)</param>
        /// <param name="strike">The strike of the security (if applicable)</param>
        /// <param name="optionRight">The option right of the security (if applicable)</param>
        /// <returns>A new Lean Symbol instance</returns>
        public Symbol GetLeanSymbol(
            string brokerageSymbol,
            SecurityType securityType,
            string market,
            DateTime expirationDate = default(DateTime),
            decimal strike = 0,
            OptionRight optionRight = OptionRight.Call
            )
        {
            // unused
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts a Tradier symbol to a Lean symbol instance
        /// </summary>
        /// <param name="brokerageSymbol">The Tradier symbol</param>
        /// <returns>A new Lean Symbol instance</returns>
        public Symbol GetLeanSymbol(string brokerageSymbol)
        {
            Symbol symbol;
            if (brokerageSymbol.Length > 15)
            {
                // convert the Tradier option symbol to OSI format
                var underlying = brokerageSymbol.Substring(0, brokerageSymbol.Length - 15);
                var ticker = underlying.PadRight(6, ' ') + brokerageSymbol.Substring(underlying.Length);
                symbol = SymbolRepresentation.ParseOptionTickerOSI(ticker, IndexOptionMapping.ContainsKey(underlying) ? SecurityType.IndexOption : SecurityType.Option);

                if (symbol.SecurityType == SecurityType.Option)
                {
                    symbol = QuantConnect.Symbol.CreateOption(symbol.Underlying, symbol.ID.Market, symbol.ID.OptionStyle,
                        symbol.ID.OptionRight, symbol.ID.StrikePrice, symbol.ID.Date, OptionAlias(symbol.Value, symbol));
                }
                else if (symbol.SecurityType == SecurityType.IndexOption)
                {
                    string symbolAlias = OptionAlias(symbol.Value, symbol);

                    var indSym = QuantConnect.Symbol.Create(symbol.Underlying.Value, SecurityType.Index, symbol.ID.Market);

                    var optSymTicker = symbol.Canonical != null ? symbol.Canonical.Value.Replace("?", "").Replace("/", "") : symbol.Underlying.Value;

                    symbol = QuantConnect.Symbol.CreateOption(indSym, optSymTicker, symbol.ID.Market, symbol.ID.OptionStyle,
                        symbol.ID.OptionRight, symbol.ID.StrikePrice, symbol.ID.Date, symbolAlias);
                }
            }
            else
            {
                // Check for Index symbol
                if (IndexOptionMapping.ContainsKey(brokerageSymbol))
                {
                    // This is the rare case, but possible
                    // Create Index
                    symbol = Symbol.Create(IndexOptionMapping[brokerageSymbol], SecurityType.Index, Market.USA);
                }
                else
                {
                    symbol = Symbol.Create(brokerageSymbol, SecurityType.Equity, Market.USA);
                }
            }

            return symbol;
        }

        public static string OptionAlias(string inputSymbol, Symbol symbol)
        {
            // Remove extra spaces and split into parts
            inputSymbol = inputSymbol.Trim();
            string underlying = inputSymbol.Substring(0, inputSymbol.IndexOf(' ')).Trim();
            string details = inputSymbol.Substring(inputSymbol.IndexOf(' ')).Trim(); // Get the rest (date, type, strike)

            // Extract date and convert to readable format
            string strikePrice = details.Substring(7); // Strike price part

            // Parse the date
            string formattedDate = symbol.ID.Date.ToString("ddMMMyy", System.Globalization.CultureInfo.InvariantCulture).ToUpper(); // e.g., 20SEP24 or 04OCT26

            // Specially format strike price to include a decimal (always with three decimal places)
            string formattedStrikePrice = (Convert.ToDecimal(strikePrice) / 1000).ToString("F3");

            // Determine the option type suffix
            string optionSuffix = symbol.ID.OptionRight == OptionRight.Call ? "CE" : "PE";

            // Build the final formatted string
            string result = $"{underlying}{formattedDate}{formattedStrikePrice}{optionSuffix}";

            return result;
        }

        public static string ConvertOptionSymbol(string inputSymbol, Symbol symbol)
        {
            if (inputSymbol.Length < 15)
            {
                // These are the UL IndexOptions
                return symbol.Value;
            }

            // Extract the underlying symbol (e.g., "AAPL", "MSFT")
            //string underlying = inputSymbol.Substring(0, inputSymbol.IndexOfAny("0123456789".ToCharArray()));
            string underlying = symbol.Canonical != null ? symbol.Canonical.Value.Replace("?", "").Replace("/", "") : symbol.Underlying.Value;

            // Extract the date part (e.g., "27SEP24")
            string datePart = inputSymbol.Substring(underlying.Length, 7);

            // Parse the date (e.g., "27SEP24" → "240927")
            string formattedDate = symbol.ID.Date.ToString("yyMMdd"); // Convert to YYMMDD format

            // Extract the strike price and option type (e.g., "305.500CE", "135.000PE")
            string rest = inputSymbol.Substring(underlying.Length + 7); // Extract strike price and type

            // Split the rest into strike price and option type (e.g., "305.500", "CE")
            string strikePricePart = rest.Substring(0, rest.Length - 2); // Strike price is the rest except the last 2 chars

            // Convert strike price to 6 digits without decimal (e.g., "305.500" → "0030550")
            string formattedStrikePrice = (Convert.ToDecimal(strikePricePart) * 1000).ToString("00000000");

            // Convert option type (e.g., "CE" → "C" or "PE" → "P")
            string optionSuffix = symbol.ID.OptionRight == OptionRight.Call ? "C" : "P";

            // Build the final formatted string
            string result = $"{underlying}{formattedDate}{optionSuffix}{formattedStrikePrice}";

            return result;
        }

        /// <summary>
        /// Dictionary containing Index Option Symbol mapping
        /// </summary>
        public Dictionary<string, string> IndexOptionMapping = new()
        {
            { "SPX", "SPX" },
            { "SPXW", "SPX" },
            { "XSP", "XSP" }
        };
    }
}

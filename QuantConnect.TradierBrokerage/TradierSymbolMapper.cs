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
using System.Linq;

namespace QuantConnect.Brokerages.Tradier
{
    /// <summary>
    /// Provides the mapping between Lean symbols and Tradier symbols.
    /// </summary>
    public class TradierSymbolMapper : ISymbolMapper
    {
        public static List<SecurityType> SupportedOptionTypes =[ SecurityType.Option, SecurityType.IndexOption];

        public static List<SecurityType> SupportedSecurityTypes = 
        [            
            SecurityType.Equity,
            SecurityType.Option,
            SecurityType.Index,
            SecurityType.IndexOption
        ];

        /// <summary>
        /// Converts a Lean symbol instance to a Tradier symbol
        /// </summary>
        /// <param name="symbol">A Lean symbol instance</param>
        /// <returns>The Tradier symbol</returns>
        public string GetBrokerageSymbol(Symbol symbol)
        {
            var normalized = symbol.Value.Replace('/', '.');
            var symbolValue = SupportedOptionTypes.Contains(symbol.SecurityType) ? normalized.Replace(" ", "") : normalized;
            
            return SupportedOptionTypes.Contains(symbol.SecurityType) 
                ? symbolValue.Replace(".", "")  // OCC format for options
                : ToBrokerageTickerFormat(symbolValue);  // Slash format for equities
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
        /// Normalizes a brokerage-formatted equity/index ticker to Lean format by replacing slashes ('/') with periods ('.').
        /// Example: "BRK/B" -> "BRK.B". Use when converting symbols received from Tradier back into Lean.
        /// </summary>
        /// <param name="brokerageTicker">Ticker received from Tradier (e.g., "BRK/B").</param>
        /// <returns>Lean-compatible ticker (e.g., "BRK.B").</returns>
        private static string FromBrokerageTickerFormat(string brokerageTicker) => brokerageTicker.Replace('/', '.');


        /// <summary>
        /// Converts a Lean equity/index ticker to Tradier brokerage format by replacing periods ('.') with slashes ('/').
        /// Example: "BRK.B" -> "BRK/B". Use when preparing symbols for Tradier REST requests.
        /// </summary>
        /// <param name="leanTicker">Lean ticker (e.g., "BRK.B").</param>
        /// <returns>Brokerage-compatible ticker (e.g., "BRK/B").</returns>
        private static string ToBrokerageTickerFormat(string leanTicker) => leanTicker.Replace('.', '/');

        /// <summary>
        /// Converts a Tradier symbol to a Lean symbol instance
        /// </summary>
        /// <param name="brokerageSymbol">The Tradier symbol</param>
        /// <param name="underlyingBrokerageSymbol">Optional underlying brokerage symbol for perfect bidirectional mapping</param>
        /// <returns>A new Lean Symbol instance</returns>
        public Symbol GetLeanSymbol(string brokerageSymbol, string underlyingBrokerageSymbol = null)
        {
            if (brokerageSymbol.Length > 15)
            {
                // Determine security type first
                var underlying = brokerageSymbol.Substring(0, brokerageSymbol.Length - 15);
                var securityType = IsIndexOptionSymbol(underlying) ? SecurityType.IndexOption : SecurityType.Option;

                switch (securityType)
                {
                    case SecurityType.IndexOption:
                        // Use OSI parsing for IndexOptions (works perfectly)
                        var ticker = underlying.PadRight(6, ' ') + brokerageSymbol.Substring(underlying.Length);
                        return SymbolRepresentation.ParseOptionTickerOSI(ticker, securityType, securityType.DefaultOptionStyle(), Market.USA);
                    
                    case SecurityType.Option:
                        // Manual creation for Options (like BRK.B options)
                        if (!SymbolRepresentation.TryDecomposeOptionTickerOSI(brokerageSymbol, out var optionTicker, out var expiration, out var right, out var strike))
                            throw new NotSupportedException($"Unsupported option symbol '{brokerageSymbol}': Could not parse as OSI format");

                        var originalUnderlying = !string.IsNullOrEmpty(underlyingBrokerageSymbol) 
                            ? FromBrokerageTickerFormat(underlyingBrokerageSymbol) 
                            : optionTicker;
                        
                        var underlyingSymbol = Symbol.Create(originalUnderlying, SecurityType.Equity, Market.USA);
                        
                        return Symbol.CreateOption(underlyingSymbol, originalUnderlying, Market.USA, 
                            securityType.DefaultOptionStyle(), right, strike, expiration);
                    
                    default:
                        throw new NotSupportedException($"Unsupported security type for option symbol '{brokerageSymbol}'");
                }
            }
            
            if (IsIndexOptionSymbol(brokerageSymbol))
                return Symbol.Create(brokerageSymbol, SecurityType.Index, Market.USA);
            
            return Symbol.Create(FromBrokerageTickerFormat(brokerageSymbol), SecurityType.Equity, Market.USA);
        }


        /// <summary>
        /// Checks if a symbol is an index option using Lean's method first, then falling back to AvailableIndexList
        /// Need to add this because IsIndexOption() method doesn't recognize all Tradier index symbols
        /// </summary>
        private static bool IsIndexOptionSymbol(string symbol)
        {
            // First try Lean's built-in method
            if (Securities.IndexOption.IndexOptionSymbol.IsIndexOption(symbol))
                return true;
            
            // Fallback using Tradier-specific index symbols that might not be recognized by Lean
            return AvailableIndexList.Contains(symbol);
        }
        
        private static string[] AvailableIndexList = ["SPX", "NDX", "VIX", "SPXW", "NQX", "VIXW", "RUT", "BKX", "BXD", "BXM", "BXN", "BXR", "CLL", "COR1M", "COR1Y", "COR30D", "COR3M", "COR6M", "COR9M", "DJX", "DUX", "DVS", "DXL", "EVZ", "FVX", "GVZ", "HGX", "MID", "MIDG", "MIDV", "MRUT", "NYA", "NYFANG", "NYXBT", "OEX", "OSX", "OVX", "XDA", "XDB", "XEO", "XMI", "XNDX", "XSP", "BRR", "BRTI", "CEX", "COMP", "DJCIAGC", "DJCICC", "DJCIGC", "DJCIGR", "DJCIIK", "DJCIKC", "DJCISB", "DJCISI", "DJR", "DRG", "PUT", "RUA", "RUI", "RVX", "SET", "SGX", "SKEW", "SPSIBI", "SVX", "TNX", "TYX", "UKX", "UTY", "VIF", "VIN", "VIX1D", "VIX1Y", "VIX3M", "VIX6M", "VIX9D", "VOLI", "VPD", "VPN", "VVIX", "VWA", "VWB", "VXD", "VXN", "VXO", "VXSLV", "VXTH", "VXTLT", "XAU", "DJI", "DWCPF", "UTIL", "DAX", "DXY", "RLS", "SMLG", "SPGSCI", "VAF", "VRO", "AEX", "DJINET", "DTX", "SP600", "SPSV", "FTW5000", "DWCF", "HSI", "N225", "SX5E", "DAX", "RUTW", "NDXP"];
    }
}

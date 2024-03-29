﻿using Microsoft.Extensions.Logging;

namespace PriceProducer;

public class Pricer : IPricerSetup, IPriceProducer
{
    internal const int MARGIN = 10;
    internal static Dictionary<double, double> DEFAULT_SPREAD = new Dictionary<double, double>()
    {
        { 100000, 100}, { 10000, 10}, {1000, 1 }, {100, 0.1 }, {10, 0.01}, {1, 0.001}, {0.1, 0.001}, {0.01, 0.001}, {0.001, 0.001}, {0.0001, 0.001}, {0, 0.001}
    };
    private readonly double _pctMargin;

    private ILogger<Pricer> _logger = null;

    internal static double GetSpread(double bid, double ask)
    {
        double diff = Math.Abs(bid - ask);
        double key = DEFAULT_SPREAD.Keys.First(i => i <= diff);
        return DEFAULT_SPREAD[key];
    }

    internal static double GetSpread(double bid)
    {
        double diff = Math.Abs(bid);
        double key = DEFAULT_SPREAD.Keys.First(i => i <= diff);
        return DEFAULT_SPREAD[key];
    }

    internal Pricer()
    {
        _pctMargin = MARGIN / 100d;
    }

    public Pricer(ILoggerFactory loggerFactory) : this()
    {
        _logger = loggerFactory.CreateLogger<Pricer>();
    }

    public PriceLimit SetPriceLimitForBid(double bid)
    {
        double spread = GetSpread(bid);
        return SetPriceLimitForBidAskSpread(bid, bid + spread, spread);
    }

    public PriceLimit SetPriceLimitForBidAsk(double bid, double ask)
    {

        return SetPriceLimitForBidAskSpread(bid, ask, GetSpread(bid, ask));
    }

    public PriceLimit SetPriceLimitForBidAskSpread(double bid, double ask, double spread)
    {
        double min = Math.Min(bid, ask);
        double max = Math.Max(bid, ask);
        double minInclusive = min - (min * _pctMargin);
        double maxInclusive = max + (max * _pctMargin);
        return SetPriceLimitForBidAskSpreadRange(bid, ask, spread, minInclusive, maxInclusive);
    }

    public PriceLimit SetPriceLimitForBidAskSpreadRange(double bid, double ask, double spread, double minInclusive, double maxInclusive)
    {
        if (!ValidateLimits(bid, ask, spread, minInclusive, maxInclusive))
            return null;

        return new PriceLimit(bid, ask, spread, minInclusive, maxInclusive);
    }

    internal bool ValidateLimits(double bid, double ask, double spread, double minInclusive, double maxInclusive)
    {
        if (spread <= 0)
        {
            _logger?.LogError($"Spread must be greater than 0. Spread: {spread}");
            return false;
        }
        if (minInclusive <= 0 || maxInclusive <= 0)
        {
            _logger?.LogError($"MinInclusive and MaxInclusive must be greater than 0. MinInclusive: {minInclusive}, MaxInclusive: {maxInclusive}");
            return false;
        }
        if (minInclusive >= maxInclusive)
        {
            _logger?.LogError($"MinInclusive must be less than MaxInclusive. MinInclusive: {minInclusive}, MaxInclusive: {maxInclusive}");
            return false;
        }
        if (bid <= 0 || ask <= 0)
        {
            _logger?.LogError($"Bid and Ask must be greater than 0. Bid: {bid}, Ask: {ask}");
            return false;
        }
        if (bid < minInclusive || bid > maxInclusive || ask < minInclusive || ask > maxInclusive)
        {
            _logger?.LogError($"Bid and Ask must be within the range. Bid: {bid}, Ask: {ask}, MinInclusive: {minInclusive}, MaxInclusive: {maxInclusive}");
            return false;
        }
        return
            true;
    }

    /// <summary>
    /// Returns the next price based on the current price and the random number and the price limit.
    /// If PriceLimit is null, then all checks are ignored.
    /// There would be scenarios where the Bid or the Ask will be outside the PriceLimit. 
    /// </summary>
    /// <param name="rates">double array with 0th element as bid and 1st element as ask</param>
    /// <param name="random">a positive or a negative number passed </param>
    /// <param name="addToBid">bool to decide if random is added to Bid or Ask</param>
    /// <param name="priceLimit">PriceLimit having boundaries for range and spread for next tick</param>
    public void NextPrice(double[] rates, double random, bool addToBid, PriceLimit priceLimit)
    {
        if (priceLimit == null)
        {
            if (addToBid)
                rates[0] += random;
            else
                rates[1] += random;
            return;
        }

        double tempBid = rates[0];
        double tempAsk = rates[1];

        if (addToBid)
        {
            tempBid += random;
            if (tempBid < priceLimit.MinInclusive)
                tempBid = priceLimit.MinInclusive;
            else if (tempBid > priceLimit.MaxInclusive)
                tempBid = priceLimit.MaxInclusive - priceLimit.Spread;

            //case 1: bid is greater than ask, then ask = bid + spread
            if (tempBid >= tempAsk)
                tempAsk = tempBid + priceLimit.Spread;
            //case 2: bid is less than ask more than spread, then ask = bid + spread
            else if (tempBid < tempAsk && tempAsk - tempBid > priceLimit.Spread)
                tempAsk = tempBid + priceLimit.Spread;
            //case 2: bid is less than ask and ask - bid is within or equal to spread, do nothing.
        }
        else
        {
            tempAsk += random;
            if (tempAsk < priceLimit.MinInclusive)
                tempAsk = priceLimit.MinInclusive + priceLimit.Spread;
            else if (tempAsk > priceLimit.MaxInclusive)
                tempAsk = priceLimit.MaxInclusive;

            //case 1: bid is greater than ask, then ask = bid + spread
            if (tempBid >= tempAsk)
                tempBid = tempAsk - priceLimit.Spread;
            //case 2: bid is less than ask more than spread, then bid = ask + spread
            else if (tempBid < tempAsk && tempAsk - tempBid > priceLimit.Spread)
                tempBid = tempAsk - priceLimit.Spread;
            //case 2: bid is less than ask and ask - bid is within or equal to spread, do nothing.
        }
        rates[0] = tempBid;
        rates[1] = tempAsk;
    }
}
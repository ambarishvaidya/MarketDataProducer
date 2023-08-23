using Microsoft.Extensions.Logging;

namespace PriceProducer;

public class Pricer : IPricerSetup, IPriceProducer
{
    internal const int BASIS_POINT = 5;
    internal const int TOLERANCE = 10;

    private readonly double _basisPoint;
    private readonly double _tolerance;

    private ILogger<Pricer> _logger = null;

    internal static double GetBasisPointSpread(double bid, double ask)
    {
        return (Math.Abs(bid - ask))/0.0001;
    }
    internal static double GetBasisPointSpread(int bps)
    {
        return bps / 100d / 100d ;
    }

    internal Pricer()
    {
        _basisPoint = GetBasisPointSpread(BASIS_POINT);
        _tolerance = TOLERANCE / 100d;              
    }

    public Pricer(ILoggerFactory loggerFactory) : this()
    {
        _logger = loggerFactory.CreateLogger<Pricer>();
    }

    public PriceLimit SetPriceLimitForBid(double bid) 
    {
        return SetPriceLimitForBidAskSpread(bid, bid + _basisPoint, _basisPoint);
    }

    public PriceLimit SetPriceLimitForBidAsk(double bid, double ask)
    {

        return SetPriceLimitForBidAskSpread(bid, ask, GetBasisPointSpread(bid, ask));
    }

    public PriceLimit SetPriceLimitForBidAskSpread(double bid, double ask, int basisPoint)
    {
        return SetPriceLimitForBidAskSpread(bid, ask, GetBasisPointSpread(basisPoint));
    }

    public PriceLimit SetPriceLimitForBidAskSpread(double bid, double ask, double basisPoint)
    {
        double minInclusive = Math.Min(bid, ask) - _tolerance;
        double maxInclusive = Math.Max(bid, ask) + _tolerance;
        return SetPriceLimitForBidAskSpreadRange(bid, ask, basisPoint, minInclusive, maxInclusive);
    }

    public PriceLimit SetPriceLimitForBidAskSpreadRange(double bid, double ask, int basisPoint, double minInclusive, double maxInclusive)
    {
        return SetPriceLimitForBidAskSpreadRange(bid, ask, GetBasisPointSpread(basisPoint), minInclusive, maxInclusive);
    }
    public PriceLimit SetPriceLimitForBidAskSpreadRange(double bid, double ask, double basisPoint, double minInclusive, double maxInclusive)
    {
        if (!ValidateLimits(bid, ask, basisPoint, minInclusive, maxInclusive))            
            return null;            

        return new PriceLimit(bid, ask, basisPoint, minInclusive, maxInclusive);
    }

    internal bool ValidateLimits(double bid, double ask, double basisPoint, double minInclusive, double maxInclusive)
    {
        int bpsAsInt = (int)(basisPoint * 10000);//loosing the decimal part of the bps
        if (bpsAsInt <= 0)
        {
            _logger?.LogError($"BasisPoint must be greater than 0. BasisPoint: {basisPoint}");
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
        if (bid >= ask)
        {
            _logger?.LogError($"Bid must be less than Ask. Bid: {bid}, Ask: {ask}");
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
        if(priceLimit == null)
        {
            if(addToBid)
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
                tempBid = priceLimit.MaxInclusive - priceLimit.BasisSpread;                
            
            //case 1: bid is greater than ask, then ask = bid + basis point
            if(tempBid >= tempAsk)
                tempAsk = tempBid + priceLimit.BasisSpread;
            //case 2: bid is less than ask more than basis point, then ask = bid + basis point
            else if (tempBid < tempAsk && tempAsk - tempBid > priceLimit.BasisSpread)
                tempAsk = tempBid + priceLimit.BasisSpread;
            //case 2: bid is less than ask and ask - bid is within or equal to basis point, do nothing.
        }
        else
        {
            tempAsk += random;
            if (tempAsk < priceLimit.MinInclusive)
                tempAsk = priceLimit.MinInclusive + priceLimit.BasisSpread;
            else if (tempAsk > priceLimit.MaxInclusive)
                tempAsk = priceLimit.MaxInclusive;

            //case 1: bid is greater than ask, then ask = bid + basis point
            if (tempBid >= tempAsk)
                tempBid = tempAsk - priceLimit.BasisSpread;
            //case 2: bid is less than ask more than basis point, then bid = ask + basis point
            else if (tempBid < tempAsk && tempAsk - tempBid > priceLimit.BasisSpread)
                tempBid = tempAsk - priceLimit.BasisSpread;
            //case 2: bid is less than ask and ask - bid is within or equal to basis point, do nothing.
        }
        rates[0] = tempBid;
        rates[1] = tempAsk;
    }
}
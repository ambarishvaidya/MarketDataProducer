namespace PriceProducer;

public interface IPricerSetup
{
    PriceLimit SetPriceLimitForBid(double bid);
    PriceLimit SetPriceLimitForBidAsk(double bid, double ask);
    PriceLimit SetPriceLimitForBidAskSpread(double bid, double ask, double bps);
    PriceLimit SetPriceLimitForBidAskSpreadRange(double bid, double ask, double bps, double minInclusive, double maxInclusive);
}

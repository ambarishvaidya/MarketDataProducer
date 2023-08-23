﻿namespace PriceProducer;

/// <summary>
/// PriceLimit is a record that holds the price limits for a given bid and ask.
/// </summary>
/// <param name="Bid">Initial Bid</param>
/// <param name="Ask">Initial Ask, which is greated than Bid</param>
/// <param name="BasisSpread">The max spread between Bid and Ask</param>
/// <param name="MinInclusive">The lowest value that Bid can reach</param>
/// <param name="MaxInclusive">The maximun value that Ask can reach</param>
public record PriceLimit
{
    public double Bid { get; }
    public double Ask { get; }
    public double BasisSpread { get; }
    public double MinInclusive { get; }
    public double MaxInclusive { get; }

    internal PriceLimit(double bid, double ask, double basisSpread, double minInclusive, double maxInclusive)
    {
        Bid = bid;
        Ask = ask;
        BasisSpread = basisSpread;
        MinInclusive = minInclusive;
        MaxInclusive = maxInclusive;
    }

    private PriceLimit()
    { }
}

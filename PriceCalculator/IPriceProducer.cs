namespace PriceProducer;

public interface IPriceProducer
{
    void NextPrice(double[] rates, double random, bool addToBid, PriceLimit priceLimit);    
}

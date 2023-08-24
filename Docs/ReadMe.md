# PriceProducer Library

The PriceProducer library provides functionality to dynamically calculate the value for a new tick based on limit conditions and ticker information. This library is particularly useful for financial and trading applications where tick value adjustments are necessary.

## Features

- **Price Limit**: Each ticker is configured with a limit within which the next tick is created. The limits include the following
  - **MinInclusive and MaxInclusive**: These values set the boundary within which the tick can vary. In case if the next value of tick for Bid or Ask goes out of the boundary, the value is adjusted accordingly. If the value is not provided, then 10% of the initial value is added to Bid for MinInclusive and Ask for MaxInclusive.
  - **Spread**: Bid and Ask can have a spread no more than what is specified. If spread is not provided, default spread is from dictionary is used. Lowest spread is 0.001. Default spread is read of a constant dictionary of pre-configured values.
  - **Ask**: Initial Ask value. 
  - **Bid**: This is a mandatory field. All above fields are calculated referencing the Bid.
    -  Spread is obtained from the pre-configured dictionary.
    -  Ask is Bid + Spread.
    -  MinInclusive and MaxInclusive are the evaluated based adding 10% to Bid and Ask.
  
- **Validation**: Validation is done on the fields. All values are double > 0. If validation fails, Null is returned.

- **Logging**: Library uses Microsoft Extensions.

## Working

  NextPrice method of IPriceProducer creates a new value, applies check if PriceLimit is not null, adjusted and returned.  
- **Tick Calculation**: The NextPrice method takes as an input a double array of Bid at 0th index and Ask at 1st Index, a random number, a boolean to add random to bid or ask and the PriceLimit.
  - If PriceLimit is null, no boundaries are checked.
  - If boolean passed is true, the random (which can be a positive or a negative number) is added to Bid.
  - Bid is then checked for boundaries and adjusted if required.
  - Ask id not updated if the current Ask is well within Spread difference to the new Bid.
  
## Usage

The main class of Library is **Pricer** which implements **IPricerSetup** for PriceLimit and **IPriceProducer** with method for NextTick.
Create a new Pricer instance passing **ILoggerFactory**.
```c#
Pricer pricer = new Pricer(_loggerFactory);
```

Use any of the below methods of IPricerSeteup to create a PriceLimit
```c#
public interface IPricerSetup
{
    PriceLimit SetPriceLimitForBid(double bid);
    PriceLimit SetPriceLimitForBidAsk(double bid, double ask);
    PriceLimit SetPriceLimitForBidAskSpread(double bid, double ask, double spread);
    PriceLimit SetPriceLimitForBidAskSpreadRange(double bid, double ask, double spread, double minInclusive, double maxInclusive);
}
```

With PriceLimit now available, call NextPrice IPriceProducer
```c#
public interface IPriceProducer
{
    void NextPrice(double[] rates, double random, bool addToBid, PriceLimit priceLimit);
}
```
Here
- double[] rates: is the Bid and Ask at 0th and 1st index.
- random: is a variation that suggests move in Bid or Ask passed in double. The variation can be positive or negative.
- addToBid: is a boolean that dictates if random is added to Bid or Ask.
- priceLimit: if not null performs checks and adjustments on the new data post adding the random number.   

using Microsoft.Extensions.Logging;
using PriceProducer;

namespace ConsolePriceProducer;

internal class Program
{
    static void Main(string[] args)
    {
        new Program();
        Console.ReadLine();
    }

    public Program()
    {
        //Create Pricer  
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole());
        Pricer pricer = new Pricer(loggerFactory);

        //Get PriceLimit
        PriceLimit pricerLimit = pricer.SetPriceLimitForBidAskSpreadRange(120.1234, 120.1238, 0.01, 119.8863, 121.4125);

        //Get Next Tick
        double[] rates = new double[] { 120.1234, 120.1238 };

        Random random = new Random();
        double variation = random.NextDouble() / 100 / 2; //Just a create a fraction.
        pricer.NextPrice(rates, variation, true, pricerLimit);
        Console.WriteLine($"Next Tick - {rates[0]}, {rates[1]}");
        
        //Passing a hard coded fraction
        pricer.NextPrice(rates, -0.002356, true, pricerLimit);
        Console.WriteLine($"Next Tick - {rates[0]}, {rates[1]}");
    }
}
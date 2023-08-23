using Microsoft.Extensions.Logging;
using PriceProducer;

namespace TestPriceCalculator
{
    public class TestPriceCalculator
    {
        private const double EPSILON = 1e-5;

        ILoggerFactory _loggerFactory;

        [SetUp]
        public void Setup()
        {
            _loggerFactory = new LoggerFactory();
        }

        [Test]
        public void Constructor_NoParameters_ReturnsIPricerSetup()
        {
            IPricerSetup pricer = new Pricer(_loggerFactory);
            Assert.IsNotNull(pricer);
        }

        [Test]
        public void SetPriceLimit_WithBid_ReturnsInstanceOfPricerSetup()
        {
            IPricerSetup pricer = new Pricer(_loggerFactory);
            var setup = pricer.SetPriceLimitForBid(10.2);
            Assert.IsNotNull(setup);
        }

        [Test]
        public void SetPriceLimit_WithBidAsk_ReturnsInstanceOfPricerSetup()
        {
            IPricerSetup pricer = new Pricer(_loggerFactory);
            var setup = pricer.SetPriceLimitForBidAsk(10.2, 10.4);
            Assert.IsNotNull(setup);
        }

        [Test]
        public void SetPriceLimit_WithBidAskSpreadPip_ReturnsInstanceOfPricerSetup()
        {
            IPricerSetup pricer = new Pricer(_loggerFactory);
            var setup = pricer.SetPriceLimitForBidAskSpread(10.2, 10.2004, 5);
            Assert.IsNotNull(setup);
        }

        [Test]
        public void SetPriceLimit_WithBidAskDblSpreadPip_ReturnsInstanceOfPricerSetup()
        {
            IPricerSetup pricer = new Pricer(_loggerFactory);
            var setup = pricer.SetPriceLimitForBidAskSpread(10.2, 10.2004, 0.0005);
            Assert.IsNotNull(setup);
        }

        [Test]
        public void SetPriceLimit_WithBidAskSpreadPipMinMax_ReturnsInstanceOfPricerSetup()
        {
            IPricerSetup pricer = new Pricer(_loggerFactory);
            var setup = pricer.SetPriceLimitForBidAskSpreadRange(10.2, 10.2004, 5, 9.8, 10.7);
            Assert.IsNotNull(setup);
        }

        [Test]
        public void SetPriceLimit_WithBidAskDblSpreadPipMinMax_ReturnsInstanceOfPricerSetup()
        {
            IPricerSetup pricer = new Pricer(_loggerFactory);
            var setup = pricer.SetPriceLimitForBidAskSpreadRange(10.2, 10.2004, 0.0005, 9.8, 10.7);
            Assert.IsNotNull(setup);
        }

        [TestCase(100, 100.1, 0.1, 99, 102)]
        [TestCase(1.1234, 1.1236, 0.0003, 1.1, 1.3)]
        [TestCase(102.56, 102.62, 0.8, 100, 106)]
        [TestCase(102.56, 103.32, 0.8, 100, 106)]
        [TestCase(1001, 1003, 10, 990, 1100)]
        public void ValidatePriceLimit_ForValidData_ReturnsInstanceOfPricerSetup(double bid, double ask, double bps, double min, double max)
        {
            Pricer pricer = new Pricer();
            var resp = pricer.ValidateLimits(bid, ask, bps, min, max);
            Assert.IsTrue(resp);
        }

        [TestCase(1.1234, 1.1236, 0.0003, 1.1235, 1.3)]
        [TestCase(0, 100.1, 0.1, 99, 102)]
        [TestCase(100, 0, 0.1, 99, 102)]
        [TestCase(100, 100.1, 0, 99, 102)]
        [TestCase(100, 100.1, 0.1, 0, 102)]
        [TestCase(100, 100.1, 0.1, 99, 0)]
        [TestCase(-1, 100.1, 0.1, 99, 102)]
        [TestCase(100, -1, 0.1, 99, 102)]
        [TestCase(100, 100.1, -1, 99, 102)]
        [TestCase(100, 100.1, 0.1, -1, 102)]
        [TestCase(100, 100.1, 0.1, 99, -1)]
        public void ValidatePriceLimit_ForValidData_ReturnsNull(double bid, double ask, double bps, double min, double max)
        {
            Pricer pricer = new Pricer();
            var resp = pricer.ValidateLimits(bid, ask, bps, min, max);
            Assert.IsFalse(resp);
        }

        [Test]
        public void Constructor_Parameterless_ReturnsIPricerCalculate()
        {
            IPriceProducer pricer = new Pricer();
            Assert.IsNotNull(pricer);
        }

        [TestCase(100, 101, 5, 99, 102, 0.235456, true, 100.235456, 101)]
        [TestCase(100, 100.02, 0.1, 99, 102, 0.235456, true, 100.235456, 100.335456)]
        [TestCase(0.8564, 0.9004, 0.07, 0.8, 1.0, 0.235456, true, 0.93, 1.0)]

        [TestCase(100, 101, 5, 99, 102, 0.235456, false, 100, 101.235456)]
        [TestCase(100, 101, 5, 99, 102, 6, false, 100, 102)]
        [TestCase(100, 100.02, 0.1, 99, 102, 0.235456, false, 100.155456, 100.255456)]
        [TestCase(0.8564, 0.9004, 0.07, 0.8, 1.0, 0.235456, false, 0.93, 1.0)]

        [TestCase(100, 101, 5, 99, 102, -0.235456, true, 99.764544, 101)]
        [TestCase(100, 100.02, 0.1, 99, 102, -0.235456, true, 99.76455, 99.86455)]
        [TestCase(0.8564, 0.9004, 0.07, 0.8, 1.0, -0.235456, true, 0.8, 0.87)]

        [TestCase(100, 101, 5, 99, 102, -0.235456, false, 100, 100.764544)]
        [TestCase(100, 101, 5, 99, 102, -6, false, 100, 104)]
        [TestCase(100, 100.02, 0.1, 99, 102, -0.235456, false, 99.684544, 99.784544)]
        [TestCase(0.8564, 0.9004, 0.07, 0.8, 1.0, -0.235456, false, 0.8564, 0.87)]

        public void NextPrice_WithValidInput_ReturnsExpectedResult(double bid, double ask, double bps, double min, double max, double random, bool addToBid, double newBid, double newAsk)
        {
            Pricer pricer = new Pricer(_loggerFactory);
            PriceLimit pricerLimit = pricer.SetPriceLimitForBidAskSpreadRange(bid, ask, bps, min, max);
            double[] rates = new double[] { bid, ask };
            IPriceProducer calculate = pricer;
            calculate.NextPrice(rates, random, addToBid, pricerLimit);
            Assert.That(newBid, Is.EqualTo(rates[0]).Within(EPSILON));
            Assert.That(rates[1], Is.EqualTo(newAsk).Within(EPSILON));
        }


        [TestCase(new double[] { 100, 101 }, 0.4, true, 100.4, 101)]
        [TestCase(new double[] { 100, 101 }, -0.4, true, 99.6, 101)]
        [TestCase(new double[] { 100, 101 }, 0.4, false, 100, 101.4)]
        [TestCase(new double[] { 100, 101 }, -0.4, false, 100, 100.6)]
        public void NextPrice_WhenPriceLimitIsNull_ReturnsExpexctedResult(double[] rates, double random, bool addToBid, double newBid, double newAsk)
        {
            Pricer pricer = new Pricer();            
            pricer.NextPrice(rates, random, addToBid, null);
            Assert.That(newBid, Is.EqualTo(rates[0]).Within(EPSILON));
            Assert.That(rates[1], Is.EqualTo(newAsk).Within(EPSILON));
        }

        [TestCase(100, 102, 2, 0, 105)]
        [TestCase(100, 102, 2, 95, 0)]
        [TestCase(100, 102, 2, 107, 105)]
        [TestCase(0.89, 0.98, 0.9, 0.8, 0)]
        [TestCase(0.89, 0.98, 0.9, 0, 1)]
        public void ValidatePriceLimit_ForInValidMinMaxData_ReturnsFalse(double bid, double ask, double spread, double min, double max)
        {
            Pricer pricer = new Pricer();
            var resp = pricer.ValidateLimits(bid, ask, spread, min, max);
            Assert.IsFalse(resp);
        }   
    
        //{ 100000, 100}, { 10000, 10}, {1000, 1 },
        //{100, 0.1 }, {10, 0.01}, {1, 0.001},
        //{0.1, 0.0001}, {0.01, 0.00001},
        //{0.001, 0.000001}, {0.0001, 0.0000001}, {0, 0}
        [TestCase(101, 102, 0.001)]
        [TestCase(101.23, 101.24, 0.00001)]
        [TestCase(-101.23, 102.24, 0.1)]
        public void GetSpread_BidAsk_ReturnsExpectedResult(double bid, double ask, double expected)
        {            
            var resp = Pricer.GetSpread(bid, ask);
            Assert.That(resp, Is.EqualTo(expected).Within(EPSILON));
        }

        [TestCase(101, 0.1)]
        [TestCase(101.23, 0.1)]
        [TestCase(-101.23, 0.1)]
        public void GetSpread_Bid_ReturnsExpectedResult(double bid, double expected)
        {
            var resp = Pricer.GetSpread(bid);
            Assert.That(resp, Is.EqualTo(expected).Within(EPSILON));
        }
    }
}
namespace strategy
{
    public class PaymentService
    {
        private IPaymentStrategy _strategy;
        public PaymentService(IPaymentStrategy strategy)
        {
            this._strategy = strategy;
        }

        public double pay(double amount)
        {
            return _strategy.pay(amount);
        }
    }
}
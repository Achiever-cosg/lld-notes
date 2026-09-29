namespace strategy
{
    public class CreditCardPayment : IPaymentStrategy
    {
        public double pay(double amount)
        {
            Console.Write("Credit card payment started: ");
            amount = amount + amount * 0.10;
            Console.WriteLine(amount);

            return amount;
        }
    }
}
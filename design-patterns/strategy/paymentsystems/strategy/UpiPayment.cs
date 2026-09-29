namespace strategy
{
    public class UpiPayment : IPaymentStrategy
    {
        public double pay(double amount)
        {
            Console.Write("UPI Payment started: ");

            Console.WriteLine(amount);
            return amount;
        }
    }
}
using System.Runtime.InteropServices;

namespace strategy
{
    public class DebitCardPayment : IPaymentStrategy
    {
        public double pay(double amount)
        {
            Console.Write("Debit Card payment started: ");
            amount = amount + amount * 0.05;
            Console.WriteLine(amount);

            return amount;
        }
    }
}
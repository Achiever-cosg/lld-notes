using System;

namespace strategy
{
    class Program
    {
        public static void Main(string[] args)
        {
            double amount = 200.0;
            var upiService = new PaymentService(new UpiPayment());
            var creditCardService = new PaymentService(new CreditCardPayment());
            var debitCardService = new PaymentService(new DebitCardPayment());

            double upiAmount = upiService.pay(amount);
            double creditCardAmount = creditCardService.pay(amount);
            double debitCardAmount = debitCardService.pay(amount);
        }
    }
}
namespace ECommAPI.Services
{
    public class OrderHelper
    {

        public static decimal ShippingFee { get; } = 5;

        public static Dictionary<string, string> PaymentMethods { get; } = new()
        {
            {"Cash", "Cash on Delivery"},
            {"PayPal", "PayPal"},
            {"Card", "Credit Card"}

        };

        public static List<string> PaymentStatuses { get; } = new()
        {
            "Pending", "Accepted", "Cancelled"

        };

        public static List<string> OrderStatuses { get; } = new()
        {
        "Created", "Accepted", "Cancelled,", "Shipped", "Delivered", "Returned"
        };



        /*
        takes a string of my product identifiers then separates by "-"
        returns a list of pairs(dictionary)

        pair name == product Id
        pair value == product quantity

        basically turns 9-9-7-9-6 to

         9:3
         7:1
         6:1

        */

        public static Dictionary<int, int> GetProductDictionary(string productIdentifiers)
        {
            var productDictionary = new Dictionary<int, int>();

            if (productIdentifiers.Length > 0)
            {
                string[] productIdArray = productIdentifiers.Split('-');
                foreach (var productId in productIdArray)
                {
                    try
                    {
                        int id = int.Parse(productId);

                        if (productDictionary.ContainsKey(id))
                        {
                            productDictionary[id] += 1;
                        }
                        else
                        {
                            productDictionary.Add(id, 1);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            return productDictionary;
        }
    }
}

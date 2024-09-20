using Microsoft.EntityFrameworkCore;

namespace ECommAPI.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        [Precision(16, 21)]
        public decimal UnitPrice { get; set; }


        //navigation properties

        public Order Order { get; set; }

        public Product Product { get; set; } = null!;




    }
}

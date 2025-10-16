using Consumer.Models;

namespace Consumer.Repositories
{
    public class OrderRepository
    {
        private readonly DBContext _dbContext;

        public OrderRepository(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void SaveOrder(Order order)
        {
            try
            {
                _dbContext.Orders.Add(order);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при сохранении заказа: {ex.Message}", ex);
            }
        }
        public Order CreateOrder(string iphoneModel, string color, string storage, string simType,string price, DateTime orderDate, int customerID)
        {
            return new Order
            {
                iPhoneModel = iphoneModel,
                Color = color,
                Storage = storage,
                SimType = simType,
                Price = price,
                OrderDate = orderDate,
                CustomerId = customerID
            };
        }
    }
}

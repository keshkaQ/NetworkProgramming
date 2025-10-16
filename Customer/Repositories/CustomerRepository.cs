using Consumer.Models;

namespace Consumer.Repositories
{
    public class CustomerRepository
    {
        private readonly DBContext _dbContext;
        public CustomerRepository(DBContext dBContext)
        {
            _dbContext = dBContext;
        }
        public Customer GetOrCreateCustomer(string customerName, string email)
        {
            var customer = _dbContext.Customers.FirstOrDefault(c => c.Email == email);
            if (customer == null)
            {
                var newCustomer = new Customer { Name = customerName, Email = email };
                _dbContext.Customers.Add(newCustomer);
                _dbContext.SaveChanges();
                return newCustomer;
            }
            return customer;
        }
    }
}

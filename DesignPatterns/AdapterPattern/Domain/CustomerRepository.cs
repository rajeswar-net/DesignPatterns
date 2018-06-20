namespace AdapterPattern.Domain
{
    using System.Collections.Generic;
    using Model;
    public class CustomerRepository : ICustomerRepository
    {
        public IList<Customer> GetCustomers()
        {
            return new List<Customer>();
        }
    }
}

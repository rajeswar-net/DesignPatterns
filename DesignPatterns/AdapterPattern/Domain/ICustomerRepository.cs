namespace AdapterPattern.Domain
{
    using Model;
    using System.Collections.Generic;

    public interface ICustomerRepository
    {
        IList<Customer> GetCustomers();
    }
}

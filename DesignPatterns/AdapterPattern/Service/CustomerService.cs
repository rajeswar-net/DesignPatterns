namespace AdapterPattern.Service
{
    using Domain;
    using Model;
    using System.Collections.Generic;
    using System.Web;

    /*
        This bit of code should be straightforward: we want to return a list of customers using the abstract dependency 
        ICustomerRepository. We check if the list is available in the HttpContext cache. 
        If that’s the case then convert the cached value and return the customers. 
        Otherwise fetch the list from the repository and put the value to the cache.
        So what’s wrong with the GetAllCustomers method?

        Testability
            The method is difficult to test because of the dependency on the HttpContext class. 
            If you want to get any reliable result from the test that tests the behaviour of this method you’ll need to somehow 
            provide a valid HttpContext object. Otherwise if the test fails, then why did it fail? Was it a genuine failure, 
            meaning that the customer list was not retrieved? Or was it because there was no HttpContext available? 
            It’s the wrong approach making the test outcome dependent on such a volatile object.
        Flexibility
            With this implementation we’re stuck with the HttpContext as our caching solution. 
            What if we want to change over to a different one, such as Memcached or System.Runtime? In that case we’d need to go 
            in and manually replace the HttpContext solution to a new one. Even worse, let’s say all your service classes use 
            HttpContext for caching and you want to make the transition to another caching solution for all of them. 
            You probably see how tedious, time consuming and error-prone this could be.

        On a different note: the method also violates the Single Responsibility Principle as it performs caching in its body. 
        Strictly speaking it should not be doing this as it then introduces a hidden side effect. 
        The solution to that problem is provided by the Decorator pattern, which we’ll look at in the next blog post.

        Solution
            It’s clear that we have to factor out the HttpContext.Current.Cache object and let the consumer of CustomerService inject it instead – a simple design principle known as Dependency Injection. 
            As usual, the most optimal option is to write an abstraction that encapsulates the functions of a cache solution. Insert the following interface to the Service layer:
 */

    public class CustomerService
    {
        /*OLD CODE
        private readonly ICustomerRepository _customerRepository;
        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public IList<Customer> GetAllCustomers()
        {
            IList<Customer> customers;
            string storageKey = "GetAllCustomers";
            customers = (List<Customer>)HttpContext.Current.Cache.Get(storageKey);
            if(customers==null)
            {
                customers = _customerRepository.GetCustomers();
                HttpContext.Current.Cache.Insert(storageKey, customers);
            }
            return customers;
        }
        */

        private readonly ICustomerRepository _customerRepository;
        private readonly ICacheStorage _cacheStorage;
        public CustomerService(ICustomerRepository customerRepository,ICacheStorage cacheStorage)
        {
            _customerRepository = customerRepository;
            _cacheStorage = cacheStorage;
        }
        public IList<Customer> GetAllCustomers()
        {
            IList<Customer> customers;
            string storageKey = "GetAllCustomers";
            customers = _cacheStorage.Retrieve<List<Customer>>(storageKey);
            if (customers == null)
            {
                customers = _customerRepository.GetCustomers();
                _cacheStorage.Store(storageKey, customers);
            }
            return customers;
        }
    }

    public interface ICacheStorage
    {
        void Remove(string key);
        void Store(string key, object data);
        T Retrieve<T>(string key);
    }

    public class HttpContextCacheStorage:ICacheStorage
    {
        public void Remove(string key)
        {
            HttpContext.Current.Cache.Remove(key);
        }

        public void Store(string key, object data)
        {
            HttpContext.Current.Cache.Insert(key, data);
        }

        public T Retrieve<T>(string key)
        {
            T itemsStored = (T)HttpContext.Current.Cache.Get(key);
            if (itemsStored == null)
            {
                itemsStored = default(T);
            }
            return itemsStored;
        }
    }
}

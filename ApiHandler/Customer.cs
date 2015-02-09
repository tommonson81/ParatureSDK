using System;
using System.Collections;
using System.Threading;
using System.Xml;
using ParatureSDK.ModuleQuery;
using ParatureSDK.ParaObjects;
using ParatureSDK.XmlToObjectParser;

namespace ParatureSDK.ApiHandler
{
    /// <summary>
    /// Contains all the methods that allow you to interact with the Parature Customer module.
    /// </summary>
    public class Customer
    {

        /// <summary>
        /// Provides the capability to delete a Customer.
        /// </summary>
        /// <param name="Customerid">
        /// The id of the Customer to delete
        /// </param>
        /// <param name="purge">
        /// If purge is set to true, the Customer will be permanently deleted. Otherwise, it will just be 
        /// moved to the trash bin, so it will still be able to be restored from the service desk.
        ///</param>
        public static ApiCallResponse Delete(Int64 Customerid, ParaCredentials ParaCredentials, bool purge)
        {
            return ApiCallFactory.ObjectDelete(ParaCredentials, ParaEnums.ParatureModule.Customer, Customerid, purge);
        }


        /// <summary>
        /// Creates a Parature Customer. Requires an Object and a credentials object. Will return the Newly Created Customerid. Returns 0 if the Customer creation fails.
        /// </summary>
        public static ApiCallResponse Insert(ParaObjects.Customer customer, ParaCredentials ParaCredentials)
        {
            return Insert(customer, ParaCredentials, false, false);
        }


        /// <summary>
        /// Creates a Parature Customer. Requires an Object and a credentials object. Need to specific whether to include a customer notification for the insert, as well as if the password should be included in the Notificaiton. 
        /// Will return the Newly Created Customerid. Returns 0 if the Customer creation fails.
        /// </summary>
        public static ApiCallResponse Insert(ParaObjects.Customer customer, ParaCredentials ParaCredentials, bool NotifyCustomer, bool IncludePasswordInNotification)
        {
            // Extra arguments for the customer module
            ArrayList arguments = new ArrayList();
            arguments.Add("_notify_=" + NotifyCustomer.ToString().ToLower());

            // Only if the user selects to notify the customer that we include the password.
            if (NotifyCustomer == true)
            {
                // This is currently a bug. Needs to be uncommented when fixed.
                arguments.Add("_includePassword_=" + IncludePasswordInNotification.ToString().ToLower());
            }

            ApiCallResponse ar = new ApiCallResponse();
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc = XmlGenerator.CustomerGenerateXml(customer);
            ar = ApiCallFactory.ObjectCreateUpdate(ParaCredentials, ParaEnums.ParatureModule.Customer, doc, 0, arguments);
            customer.Id = ar.Objectid;
            customer.uniqueIdentifier = ar.Objectid;
            return ar;
        }


        /// <summary>
        /// Updates a Parature Customer. Requires an Object and a credentials object.  Will return the updated Customerid. Returns 0 if the Customer update operation fails.
        /// </summary>
        public static ApiCallResponse Update(ParaObjects.Customer Customer, ParaCredentials ParaCredentials)
        {
            return Update(Customer, ParaCredentials, false, false);
        }

        /// <summary>
        /// Updates a Parature Customer. Requires an Object and a credentials object. Need to specific whether to include a customer notification for the insert, as well as if the password should be included in the Notificaiton. Will return the updated Customerid. Returns 0 if the Customer update operation fails.
        /// </summary>
        public static ApiCallResponse Update(ParaObjects.Customer Customer, ParaCredentials ParaCredentials, bool NotifyCustomer, bool IncludePasswordInNotification)
        {
            // Extra arguments for the customer module
            ArrayList arguments = new ArrayList();
            arguments.Add("_notify_=" + NotifyCustomer.ToString().ToLower());
            // Only if the user selects to notify the customer that we include the password.
            if (NotifyCustomer == true)
            {
                //arguments.Add("_include_password_=" + IncludePasswordInNotification.ToString().ToLower());
            }
            ApiCallResponse ar = new ApiCallResponse();
            ar = ApiCallFactory.ObjectCreateUpdate(ParaCredentials, ParaEnums.ParatureModule.Customer, XmlGenerator.CustomerGenerateXml(Customer), Customer.Id, arguments);
            return ar;
        }

        /// <summary>
        /// Returns a customer object with all the properties of a customer.
        /// </summary>
        /// <param name="customerid">
        ///The customer number that you would like to get the details of. 
        ///Value Type: <see cref="Int64" />   (System.Int64)
        ///</param>
        /// <param name="ParaCredentials">
        /// The Parature Credentials class is used to hold the standard login information. It is very useful to have it instantiated only once, with the proper information, and then pass this class to the different methods that need it.
        /// </param>
        /// <param name="RequestDepth">
        /// For a simple customer request, please put 0. <br/>When Requesting a Customer, there might be related objects linked to that Customer: such as Account, etc. <br/>With a regular Customer detail call, generally only the ID and names of the second level objects are loaded. 
        /// </param>
        public static ParaObjects.Customer GetDetails(Int64 customerid, ParaCredentials ParaCredentials, ParaEnums.RequestDepth RequestDepth)
        {
            ParaObjects.Customer Customer = new ParaObjects.Customer();
            Customer = FillDetails(customerid, ParaCredentials, RequestDepth, true);

            return Customer;

        }

        /// <summary>
        /// Returns a Customer object with all of its properties filled.
        /// </summary>
        /// <param name="Customerid">
        ///The Customer number that you would like to get the details of. 
        ///Value Type: <see cref="Int64" />   (System.Int64)
        ///</param>
        /// <param name="ParaCredentials">
        /// The Parature Credentials class is used to hold the standard login information. It is very useful to have it instantiated only once, with the proper information, and then pass this class to the different methods that need it.
        /// </param>
        public static ParaObjects.Customer GetDetails(Int64 Customerid, ParaCredentials ParaCredentials)
        {

            ParaObjects.Customer Customer = new ParaObjects.Customer();
            Customer = FillDetails(Customerid, ParaCredentials, ParaEnums.RequestDepth.Standard, true);

            return Customer;

        }

        /// <summary>
        /// Returns an customer object from a XML Document. No calls to the APIs are made when calling this method.
        /// </summary>
        /// <param name="CustomerXML">
        /// The Customer XML, is should follow the exact template of the XML returned by the Parature APIs.
        /// </param>
        public static ParaObjects.Customer GetDetails(XmlDocument CustomerXML)
        {
            var customer = new ParaObjects.Customer();
            customer = ParaEntityParser.EntityFill<ParaObjects.Customer>(CustomerXML);
            customer.FullyLoaded = true;

            customer.ApiCallResponse.xmlReceived = CustomerXML;
            customer.ApiCallResponse.Objectid = customer.Id;

            customer.IsDirty = false;
            return customer;
        }

        /// <summary>
        /// Returns an customer list object from a XML Document. No calls to the APIs are made when calling this method.
        /// </summary>
        /// <param name="CustomerListXML">
        /// The Customer List XML, is should follow the exact template of the XML returned by the Parature APIs.
        /// </param>
        public static ParaEntityList<ParaObjects.Customer> GetList(XmlDocument CustomerListXML)
        {
            var customersList = new ParaEntityList<ParaObjects.Customer>();
            customersList = ParaEntityParser.FillList<ParaObjects.Customer>(CustomerListXML);

            customersList.ApiCallResponse.xmlReceived = CustomerListXML;

            return customersList;
        }

        /// <summary>
        /// Provides you with the capability to list Customers, following criteria you would set
        /// by instantiating a ModuleQuery.CustomerQuery object
        /// </summary>
        public static ParaEntityList<ParaObjects.Customer> GetList(ParaCredentials ParaCredentials, ParaEntityQuery Query)
        {
            return FillList(ParaCredentials, Query, ParaEnums.RequestDepth.Standard);
        }

        /// <summary>
        /// Provides you with the capability to list Customers, following criteria you would set
        /// by instantiating a ModuleQuery.CustomerQuery object.
        /// Provides with the capability to pull extra data, by setting the proper request depth. Please be aware that 
        /// this might considerably slow your request, due to the high volume of API calls needed, in case you require more than 
        /// the standard field depth.
        /// </summary>
        public static ParaEntityList<ParaObjects.Customer> GetList(ParaCredentials ParaCredentials, ParaEntityQuery Query, ParaEnums.RequestDepth RequestDepth)
        {
            return FillList(ParaCredentials, Query, RequestDepth);
        }

        /// <summary>
        /// Returns the list of the first 25 customers returned by the API.
        /// Provides with the capability to pull extra data, by setting the proper request depth. Please be aware that 
        /// this might considerably slow your request, due to the high volume of API calls needed, in case you require more than 
        /// the standard field depth.
        /// </summary>            
        public static ParaEntityList<ParaObjects.Customer> GetList(ParaCredentials ParaCredentials, ParaEnums.RequestDepth RequestDepth)
        {
            return FillList(ParaCredentials, null, RequestDepth);
        }

        /// <summary>
        /// Returns the list of the first 25 customers returned by the API.
        /// </summary>
        public static ParaEntityList<ParaObjects.Customer> GetList(ParaCredentials ParaCredentials)
        {
            return FillList(ParaCredentials, null, ParaEnums.RequestDepth.Standard);
        }

        /// <summary>
        /// Fills an Customer list object.
        /// </summary>
        private static ParaEntityList<ParaObjects.Customer> FillList(ParaCredentials ParaCredentials, ParaEntityQuery Query, ParaEnums.RequestDepth RequestDepth)
        {
            int requestdepth = (int)RequestDepth;
            if (Query == null)
            {
                Query = new CustomerQuery();
            }
            // Making a schema call and returning all custom fields to be included in the call.
            if (Query.IncludeAllCustomFields)
            {
                ParaObjects.Customer objschem = new ParaObjects.Customer();
                objschem = Schema(ParaCredentials);
                Query.IncludeCustomField(objschem.CustomFields);
            }
            ApiCallResponse ar = new ApiCallResponse();
            var CustomersList = new ParaEntityList<ParaObjects.Customer>();

            if (Query.RetrieveAllRecords && Query.OptimizePageSize)
            {
                OptimizationResult rslt = ApiUtils.OptimizeObjectPageSize(CustomersList, Query, ParaCredentials, requestdepth, ParaEnums.ParatureModule.Customer);
                ar = rslt.apiResponse;
                Query = (CustomerQuery)rslt.Query;
                CustomersList = ((ParaEntityList<ParaObjects.Customer>)rslt.objectList);
                rslt = null;
            }
            else
            {
                ar = ApiCallFactory.ObjectGetList(ParaCredentials, ParaEnums.ParatureModule.Customer, Query.BuildQueryArguments());
                if (ar.HasException == false)
                {
                    CustomersList = ParaEntityParser.FillList<ParaObjects.Customer>(ar.xmlReceived);
                }
                CustomersList.ApiCallResponse = ar;
            }

            // Checking if the system needs to recursively call all of the data returned.
            if (Query.RetrieveAllRecords && !ar.HasException)
            {
                // A flag variable to check if we need to make more calls
                if (Query.OptimizeCalls)
                {
                    System.Threading.Thread t;
                    ThreadPool.ObjectList instance = null;
                    int callsRequired = (int)Math.Ceiling((double)(CustomersList.TotalItems / (double)CustomersList.PageSize));
                    for (int i = 2; i <= callsRequired; i++)
                    {
                        //ApiCallFactory.waitCheck(ParaCredentials.Accountid);
                        Query.PageNumber = i;
                        //implement semaphore right here (in the thread pool instance to control the generation of threads
                        instance = new ThreadPool.ObjectList(ParaCredentials, ParaEnums.ParatureModule.Customer, Query.BuildQueryArguments(), requestdepth);
                        t = new System.Threading.Thread(delegate() { instance.Go(CustomersList); });
                        t.Start();
                    }

                    while (CustomersList.TotalItems > CustomersList.Data.Count)
                    {
                        Thread.Sleep(500);
                    }

                    CustomersList.ResultsReturned = CustomersList.Data.Count;
                    CustomersList.PageNumber = callsRequired;
                }
                else
                {
                    bool continueCalling = true;
                    while (continueCalling)
                    {

                        if (CustomersList.TotalItems > CustomersList.Data.Count)
                        {
                            // We still need to pull data

                            // Getting next page's data
                            Query.PageNumber = Query.PageNumber + 1;

                            ar = ApiCallFactory.ObjectGetList(ParaCredentials, ParaEnums.ParatureModule.Customer, Query.BuildQueryArguments());
                            if (ar.HasException == false)
                            {
                                CustomersList.Data.AddRange(ParaEntityParser.FillList<ParaObjects.Customer>(ar.xmlReceived).Data);
                                CustomersList.ResultsReturned = CustomersList.Data.Count;
                                CustomersList.PageNumber = Query.PageNumber;
                            }
                            else
                            {
                                continueCalling = false;
                                CustomersList.ApiCallResponse = ar;
                                break;
                            }
                        }
                        else
                        {
                            // That is it, pulled all the items.
                            continueCalling = false;
                            CustomersList.ApiCallResponse = ar;
                        }
                    }
                }
            }

            return CustomersList;
        }

        static ParaObjects.Customer FillDetails(Int64 Customerid, ParaCredentials ParaCredentials, ParaEnums.RequestDepth RequestDepth, bool MinimalisticLoad)
        {
            int requestdepth = (int)RequestDepth;
            ParaObjects.Customer Customer = new ParaObjects.Customer();
            //Customer = null;
            ApiCallResponse ar = new ApiCallResponse();
            ar = ApiCallFactory.ObjectGetDetail(ParaCredentials, ParaEnums.ParatureModule.Customer, Customerid);
            if (ar.HasException == false)
            {
                Customer = ParaEntityParser.EntityFill<ParaObjects.Customer>(ar.xmlReceived);
                Customer.FullyLoaded = true;
            }
            else
            {
                Customer.FullyLoaded = false;
                Customer.Id = 0;
            }
            Customer.ApiCallResponse = ar;
            Customer.IsDirty = false;
            return Customer;
        }


        public static ParaObjects.Customer Schema(ParaCredentials ParaCredentials)
        {
            ParaObjects.Customer Customer = new ParaObjects.Customer();
            ApiCallResponse ar = new ApiCallResponse();
            ar = ApiCallFactory.ObjectGetSchema(ParaCredentials, ParaEnums.ParatureModule.Customer);

            if (ar.HasException == false)
            {
                Customer = ParaEntityParser.EntityFill<ParaObjects.Customer>(ar.xmlReceived);
            }
            Customer.ApiCallResponse = ar;
            return Customer;
        }

        /// <summary>
        /// Gets an empty object with the scheam (custom fields, if any).  This call will also try to create a dummy
        /// record in order to determine if any of the custom fields have special validation rules (e.g. email, phone, url)
        /// and set the "dataType" of the custom field accordingly.
        /// </summary> 
        static public ParaObjects.Customer SchemaWithCustomFieldTypes(ParaCredentials ParaCredentials)
        {
            ParaObjects.Customer Customer = Schema(ParaCredentials);

            Customer = (ParaObjects.Customer)ApiCallFactory.ObjectCheckCustomFieldTypes(ParaCredentials, ParaEnums.ParatureModule.Customer, Customer);

            return Customer;
        }

    }
}
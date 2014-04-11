using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.Web;

//using System.Web;

namespace ParaConnect
{

    public abstract class ParaQuery
    {
        protected class QueryElement
        {
            public string QueryName = "";
            public string QueryFilter = "";
            public string QueryValue = "";
        }
        private int _pagenumber = 1;
        private int _pagesize = 25;
        private int _seed = 50;
        private int _testCalls = 3;
        private bool _totalonly = false;
        private bool _optimizeCalls = false;
        private bool _optimizePageSize = false;
        private Paraenums.OutputFormat _outputFormat = Paraenums.OutputFormat.native;
        protected ArrayList _IncludedFields = new ArrayList();
        protected ArrayList _SortByFields = new ArrayList();
        protected ArrayList _QueryFilters = new ArrayList();
        protected ArrayList _CustomFilters = new ArrayList();
        protected List<QueryElement> QElements = new List<QueryElement>();

        protected string ProcessEncoding(string value)
        {
            string encodedValue = "";
            if (string.IsNullOrEmpty(value) == false)
            {
                value = Regex.Replace(value, ",", "\\,");
                encodedValue = HttpUtility.UrlEncode(value);
            }
            return encodedValue;
        }

        /// <summary>
        /// Number of test calls to be used for the optimization calculation.
        /// Suggested range, 3 to 10.  In practice 3 has been sufficient to 
        /// active approximately 75% optimization. Default value: 3.
        /// </summary>
        public int OptimizePageSizeTestCalls
        {
            get { return _testCalls; }
            set
            {
                _testCalls = value;
            }
        }

        /// <summary>
        /// The initial page size to be used when optimizing.  All subsequent calls are calculations.
        /// No custom fields, 150 is suggested.
        /// More than 5 custom fields, 50 is suggested.
        /// Default value: 50
        /// </summary>
        public int OptimizePageSizeSeed
        {
            get { return _seed; }
            set
            {
                _seed = value;
            }
        }

        /// <summary>
        /// Page Size Optimization will make a series of test calls and attempt to calculate the optimum page size.
        /// </summary>
        public bool OptimizePageSize
        {
            get { return _optimizePageSize; }
            set
            {
                _optimizePageSize = value;
            }
        }

        /// <summary>
        /// Enable Parallel Calls
        /// Parature's API can't handle parallel calls at this time
        /// </summary>
        public bool OptimizeCalls
        {
            get { return _optimizeCalls; }
            //set
            //{
            //    _optimizeCalls = value;
            //}
        }

        /// <summary>
        /// If you set this property to "True", only the total number of items meeting your query is returned. There will be no objects returned.
        /// </summary>        
        public bool TotalOnly
        {
            get { return _totalonly; }
            set
            {
                _totalonly = value;
            }
        }

        /// <summary>
        /// The number of the page you would like to request, first page should have the number 1 (which is the default value).
        /// </summary>       
        public int PageNumber
        {
            get { return _pagenumber; }
            set
            {
                _pagenumber = value;
            }
        }

        /// <summary>
        /// The number of records to return per page. Default is 25 (maximum is 500)
        /// </summary>
        public int PageSize
        {
            get { return _pagesize; }
            set
            {
                _pagesize = value;
            }
        }


        /// <summary>
        /// If you set this property to "True", ParaConnect will perform the appropriate number of calls to 
        /// retrieve all the data matching your request. Please note that the "PageSize" property will be ignored 
        /// since Paraconnect will manage the size of the page to call.
        /// CAUTION: This property might call a large amount of data and therefore be slow to respond, in addition to the pressure it 
        /// puts on Parature servers.
        /// </summary>        
        public bool RetrieveAllRecords
        {
            get { return _retrieveAllRecords; }
            set
            {
                if (value == true)
                {
                    PageSize = 500;
                }
                _retrieveAllRecords = value;
            }
        }
        private bool _retrieveAllRecords = false;


        /// <summary>
        /// The format you would like to get the results in. Leave as native for most cases, unless you specifically
        /// need an html or RSS format.
        /// </summary>
        public Paraenums.OutputFormat OutputFormat
        {
            get { return _outputFormat; }
            set
            {
                _outputFormat = value;
            }
        }

        /// <summary>
        /// Add a sort order to the Query, based on a static field.
        /// </summary>
        /// <param name="fieldName">
        /// the field name to passe would be the exact name of the field in the object properties.
        /// For example, if you have a property "Ticket.Date_Created", you will need to pass "Date_Created".
        /// </param>              
        public bool AddSortOrder(string fieldName, Paraenums.QuerySortBy SortDirection)
        {
            if (_SortByFields.Count < 5)
            {
                _SortByFields.Add(fieldName + "_" + SortDirection.ToString().ToLower() + "_");
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// When loading records under a minimalistic request, only field options that are selected are loaded. Other information (like "enable") is not parsed.
        /// </summary>        
        public bool MinimalisticLoad = false;

        /// <summary>
        /// Adds a static field based filter to the query. 
        /// Static field filters are actually general properties that will be independant from static fields.
        /// You can use them this filter by passing the Read Only Static Property of the object you are using.
        /// You will find all these properties in ModuleQuery>ObjectQuery>ObjectStaticFields, where object is
        /// the name of the module you are accessing.
        /// </summary>
        /// <param name="StaticFieldProperty">
        /// these properties in ModuleQuery>ObjectQuery>ObjectStaticFields, where object is
        /// the name of the module you are accessing.
        /// </param>
        /// <param name="Criteria">
        /// The criteria you would like to apply to this static field.
        /// </param>
        /// <param name="value">
        /// The value you would like the static field to have, for this filter.
        /// </param>
        public void AddStaticFieldFilter(string StaticFieldProperty, Paraenums.QueryCriteria Criteria, string value)
        {
            QueryFilterAdd(StaticFieldProperty, Criteria, ProcessEncoding(value));
        }


        /// <summary>
        /// Adds a static field based filter to the query. 
        /// Static field filters are actually general properties that will be independant from static fields.
        /// You can use them this filter by passing the Read Only Static Property of the object you are using.
        /// You will find all these properties in ModuleQuery>ObjectQuery>ObjectStaticFields, where object is
        /// the name of the module you are accessing.
        /// </summary>
        /// <param name="StaticFieldProperty">
        /// these properties in ModuleQuery>ObjectQuery>ObjectStaticFields, where object is
        /// the name of the module you are accessing.
        /// </param>
        /// <param name="Criteria">
        /// The criteria you would like to apply to this static field.
        /// </param>
        /// <param name="value">
        /// The DateTime value you would like the static field to have, for this filter.
        /// </param>
        public void AddStaticFieldFilter(string StaticFieldProperty, Paraenums.QueryCriteria Criteria, DateTime value)
        {
            QueryFilterAdd(StaticFieldProperty, Criteria, value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }

        /// <summary>
        /// Adds a static field based filter to the query. Use this method only if you are dealing with a bool custom field (like a checkbox)
        /// Static field filters are actually general properties that will be independant from static fields.
        /// You can use them this filter by passing the Read Only Static Property of the object you are using.
        /// You will find all these properties in ModuleQuery>ObjectQuery>ObjectStaticFields, where object is
        /// the name of the module you are accessing.
        /// </summary>
        /// <param name="StaticFieldProperty">
        /// these properties in ModuleQuery>ObjectQuery>ObjectStaticFields, where object is
        /// the name of the module you are accessing.
        /// </param>
        /// <param name="Criteria">
        /// The criteria you would like to apply to this static field.
        /// </param>
        /// <param name="value">
        /// The bool value you would like the static field to have, for this filter.
        /// </param>
        public void AddStaticFieldFilter(string StaticFieldProperty, Paraenums.QueryCriteria Criteria, bool value)
        {
            string filter = "0";
            if (value)
            {
                filter = "1";
            }
            else
            {
                filter = "0";
            }

            QueryFilterAdd(StaticFieldProperty, Criteria, filter);
        }

        /// <summary>
        /// Adds a static field based filter to the query. Use this method only if you are dealing with a bool custom field (like a checkbox)
        /// Static field filters are actually general properties that will be independant from static fields.
        /// You can use them this filter by passing the Read Only Static Property of the object you are using.
        /// You will find all these properties in ModuleQuery>ObjectQuery>ObjectStaticFields, where object is
        /// the name of the module you are accessing.
        /// </summary>
        /// <param name="StaticFieldProperty">
        /// these properties in ModuleQuery>ObjectQuery>ObjectStaticFields, where object is
        /// the name of the module you are accessing.
        /// </param>
        /// <param name="Criteria">
        /// The criteria you would like to apply to this static field.
        /// </param>
        /// <param name="value">
        /// The Date you would like to base your filter off. ParaConnect will manage the date formatting part.
        /// </param>        
        public void AddCustomFieldFilter(string StaticFieldProperty, Paraenums.QueryCriteria Criteria, DateTime value)
        {
            QueryFilterAdd(StaticFieldProperty, Criteria, value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }

        protected void QueryFilterAdd(string field, Paraenums.QueryCriteria Criteria, string value)
        {
            string internalCrit = "";
            switch (Criteria)
            {
                case Paraenums.QueryCriteria.Equal:
                    internalCrit = "=";
                    break;
                case Paraenums.QueryCriteria.LessThan:
                    internalCrit = "_max_=";
                    break;
                case Paraenums.QueryCriteria.Like:
                    internalCrit = "_like_=";
                    break;
                case Paraenums.QueryCriteria.MoreThan:
                    internalCrit = "_min_=";
                    break;
            }
            QueryElement qe = new QueryElement();
            qe.QueryName = field;
            qe.QueryFilter = internalCrit;
            qe.QueryValue = value;
            QueryElementsRemoveDuplicate(qe);
            QElements.Add(qe);
        }

        /// <summary>
        /// This method allows you to inject an extra query parameter in the URL being called by our APIs.
        /// Using this method implies a very good knowledge of the underlying Parature API structure, as well as ParaConnect's inner workings and might break the API call.
        /// </summary>       
        public void AddCustomFilter(string Filter)
        {
            if (string.IsNullOrEmpty(Filter) == false)
            {
                _CustomFilters.Add(Filter);
            }

        }

        public ArrayList BuildQueryArguments()
        {
            _QueryFilters = new ArrayList();
            return BuildParaQueryArguments();
        }

        /// <summary>
        /// Provides the string array of all dynamic filtering and fields to include that will be further processed
        /// by the module specific object passed to the APIs, to include statis filtering.
        /// </summary>
        /// <summary>
        /// Builds the query arguments.
        /// </summary>
        protected ArrayList BuildParaQueryArguments()
        {
            if (_SortByFields != null)
            {
                if (_SortByFields.Count > 0)
                {
                    string fieldsSort = "_order_=";
                    for (int j = 0; j < _SortByFields.Count; j++)
                    {
                        if (j < _SortByFields.Count - 1)
                        {
                            fieldsSort = fieldsSort + ",";
                        }

                        fieldsSort = fieldsSort + _SortByFields[j].ToString();

                    }
                    _QueryFilters.Add(fieldsSort);
                }
            }

            buildModuleSpecificFilter();

            // Include all regular queries
            foreach (QueryElement qe in QElements)
            {
                _QueryFilters.Add(qe.QueryName + qe.QueryFilter + qe.QueryValue);
            }

            // Include any custom filters strings.
            foreach (string s in _CustomFilters)
            {
                _QueryFilters.Add(s);
            }


            if (TotalOnly == true)
            {
                //QueryFilterCheckAndDeleteRecord("_total_=true");
                //QueryFilterCheckAndDeleteRecord("_total_=false");
                _QueryFilters.Add("_total_=true");
                RetrieveAllRecords = false;

            }
            else
            {
                //if (RetrieveAllRecords == true)
                //{
                //    // forcing the page size to a 1000, since we are querying all the records.    
                //    PageSize = 1000;
                //}

                //QueryFilterCheckAndDeleteRecord("_startPage_=" + (PageNumber - 1));
                //QueryFilterCheckAndDeleteRecord("_startPage_=" + PageNumber);
                _QueryFilters.Add("_startPage_=" + PageNumber);


                //QueryFilterCheckAndDeleteRecord("_pageSize_=" + PageSize);
                _QueryFilters.Add("_pageSize_=" + PageSize);

            }
            if (OutputFormat != Paraenums.OutputFormat.native)
            {
                //QueryFilterCheckAndDeleteRecord("_output_=" + OutputFormat.ToString());
                _QueryFilters.Add("_output_=" + OutputFormat.ToString());
            }

            return _QueryFilters;
        }

        /// <summary>
        /// Before adding a query element, making sure that no duplicates is there.
        /// </summary>
        /// <param name="QueryName"></param>
        protected void QueryElementsRemoveDuplicate(QueryElement qe)
        {
            foreach (QueryElement qes in QElements)
            {
                if (string.Compare(qes.QueryName, qe.QueryName, true) == 0 && string.Compare(qes.QueryFilter, qe.QueryFilter) == 0)
                {
                    QElements.Remove(qe);
                    return;
                }
            }
        }

        /// <summary>
        /// Checking if a record exists, and deleting it if it did.
        /// </summary>
        //protected void OldQueryFilterCheckAndDeleteRecord(string nameValue)
        //{
        //    ArrayCheckAndDeleteRecord(_QueryFilters, nameValue);
        //}
        protected void ArrayCheckAndDeleteRecord(ArrayList arr, string nameValue)
        {
            if (arr.IndexOf(nameValue).ToString() != "-1")
            {
                arr.Remove(nameValue);
            }
        }


        protected abstract void buildModuleSpecificFilter();

    }

    public abstract class ParaQueryModuleWithCustomField : ParaQuery
    {


        /// <summary>
        /// If you set this property to "True", ParaConnect will perform a schema call, determine the custom fields, then will make a call including all the of the objects custom fields.
        /// Caution: do not use the "IncludeCustomField" methods if you are setting this one to true, as the "IncludeCustomField" methods will be ignored.
        /// </summary>        
        public bool IncludeAllCustomFields
        {
            get { return _includeAllCustomFields; }
            set
            {
                _includeAllCustomFields = value;
            }
        }
        private bool _includeAllCustomFields = false;

        /// <summary>
        /// Adds a custom field based filter to the query. Use this method for Custom Fields that are date based. 
        /// </summary>
        /// <param name="CustomFieldID">
        /// The id of the custom field you would like to filter your query on.
        /// </param>
        /// <param name="Criteria">
        /// The criteria you would like to apply to this custom field
        /// </param>
        /// <param name="value">
        /// The Date you would like to base your filter off.
        /// </param>        
        public void AddCustomFieldFilter(Int64 CustomFieldID, Paraenums.QueryCriteria Criteria, DateTime value)
        {
            QueryFilterAdd("FID" + CustomFieldID.ToString(), Criteria, value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }

        /// <summary>
        /// Adds a custom field based filter to the query. Use this method for Custom Fields that are NOT multi values. 
        /// </summary>
        /// <param name="CustomFieldID">
        /// The id of the custom field you would like to filter your query on.
        /// </param>
        /// <param name="Criteria">
        /// The criteria you would like to apply to this custom field
        /// </param>
        /// <param name="value">
        /// The value you would like the custom field to have, for this filter.
        /// </param>
        public void AddCustomFieldFilter(Int64 CustomFieldID, Paraenums.QueryCriteria Criteria, string value)
        {
            //QueryFilterAdd("FID" + CustomFieldID, Criteria, HttpUtility.UrlEncode(value));          
            QueryFilterAdd("FID" + CustomFieldID, Criteria, ProcessEncoding(value));
        }

        /// <summary>
        /// Adds a custom field based filter to the query. Use this method for Custom Fields that are NOT multi values. 
        /// </summary>
        /// <param name="CustomFieldID">
        /// The id of the custom field you would like to filter your query on.
        /// </param>
        /// <param name="Criteria">
        /// The criteria you would like to apply to this custom field
        /// </param>
        /// <param name="value">
        /// The value you would like the custom field to have, for this filter.
        /// </param>
        public void AddCustomFieldFilter(Int64 CustomFieldID, Paraenums.QueryCriteria Criteria, bool value)
        {
            string filter = "0";
            if (value)
            {
                filter = "1";
            }
            else
            {
                filter = "0";
            }
            QueryFilterAdd("FID" + CustomFieldID, Criteria, filter);
        }

        /// <summary>
        /// Adds a custom field based filter to the query. Use this method for Custom Fields that are multi values (dropdown, radio buttons, etc). 
        /// </summary>
        /// <param name="CustomFieldID">
        /// The id of the multi value custom field you would like to filter your query on.
        /// </param>
        /// <param name="Criteria">
        /// The criteria you would like to apply to this custom field
        /// </param>
        /// <param name="CustomFieldOptionID">
        /// The list of all custom field options (for the customFieldID you specified) that need to be selected for an item to qualify to be returned when you run your query.
        /// </param>
        public void AddCustomFieldFilter(Int64 CustomFieldID, Paraenums.QueryCriteria Criteria, Int64[] CustomFieldOptionID)
        {
            if (CustomFieldOptionID.Length > 0)
            {
                int i = 0;
                string filtering = "";
                string separator = "";

                for (i = 0; i < CustomFieldOptionID.Length; i++)
                {
                    separator = ",";
                    if (i == 0)
                    {
                        if (CustomFieldOptionID.Length > 1)
                        {
                            separator = "";
                        }
                    }
                    filtering = filtering + separator + CustomFieldOptionID[i].ToString();
                }

                QueryFilterAdd("FID" + CustomFieldID, Criteria, filtering);
            }
        }

        /// <summary>
        /// Adds a custom field based filter to the query. Use this method for Custom Fields that are multi values (dropdown, radio buttons, etc).
        /// </summary>
        /// <param name="CustomFieldID">
        /// The id of the multi value custom field you would like to filter your query on.
        /// </param>
        /// <param name="Criteria">
        /// The criteria you would like to apply to this custom field
        /// </param>
        /// <param name="CustomFieldOptionID">
        /// The custom field option (for the customFieldID you specified) that need to be selected for an item to qualify to be returned when you run your query.
        /// </param>
        public void AddCustomFieldFilter(Int64 CustomFieldID, Paraenums.QueryCriteria Criteria, Int64 CustomFieldOptionID)
        {
            QueryFilterAdd("FID" + CustomFieldID, Criteria, CustomFieldOptionID.ToString());
        }

        /// <summary>
        /// Add a custom field to the query returned returned 
        /// </summary>
        public void IncludeCustomField(Int64 CustomFieldid)
        {
            IncludedFieldsCheckAndDeleteRecord(CustomFieldid.ToString());
            _IncludedFields.Add(CustomFieldid.ToString());
        }

        /// <summary>
        /// Include all the custom fields, included in the collection passed to this method, 
        /// to the api call. These custom fields will be returned with the objects receiveds from the APIs.
        /// This is very useful if you have a schema objects and would like to query with all custom fields returned.
        /// </summary>
        public void IncludeCustomField(List<ParaObjects.CustomField> CustomFields)
        {
            foreach (ParaObjects.CustomField cf in CustomFields)
            {
                IncludeCustomField(cf.CustomFieldID);
            }

        }

        /// <summary>
        /// Checking if a record exists in the _includedFields array, and delete it if it did.
        /// </summary>
        protected void IncludedFieldsCheckAndDeleteRecord(string nameValue)
        {
            ArrayCheckAndDeleteRecord(_IncludedFields, nameValue);
        }


        /// <summary>
        /// Provides the string array of all dynamic filtering and fields to include that will be further processed
        /// by the module specific object passed to the APIs, to include statis filtering.
        /// </summary>
        new internal ArrayList BuildQueryArguments()
        {
            // The following method is called here, instead of having all the logic in "BuildQueryArguments",
            // it has been externalized so that it can be separately called from inherited classes. Certain 
            // inherited classes override buildQuaryArguments    

            _QueryFilters = new ArrayList();
            BuildCustomFieldQueryArguments();
            BuildParaQueryArguments();

            return _QueryFilters;

        }

        /// <summary>
        /// Build all related query arguments related to custom fields.
        /// </summary>
        private void BuildCustomFieldQueryArguments()
        {
            bool dontAdd = false;
            if (TotalOnly == false)
            {

                //Reset the query filters.

                if (_IncludedFields != null)
                {
                    if (_IncludedFields.Count > 0)
                    {
                        string fieldsList = "_fields_=";
                        for (int j = 0; j < _IncludedFields.Count; j++)
                        {
                            if (j > 0)
                            {
                                fieldsList = fieldsList + ",";
                            }
                            fieldsList = fieldsList + _IncludedFields[j].ToString();

                        }


                        for (int i = 0; i < _QueryFilters.Count; i++)
                        {
                            if (_QueryFilters[i].ToString() == fieldsList)
                            {
                                dontAdd = true;
                                break;
                            }
                        }

                        if (!dontAdd)
                        {
                            _QueryFilters.Add(fieldsList);
                        }
                    }
                }


            }
            else
            {
                IncludeAllCustomFields = false;
            }
        }



        /// <summary>
        /// Add a sort order to the Query, based on a custom field.
        /// </summary>
        /// <param name="CustomfieldId">The id of the custom field you would like to filter upon.</param>       
        public bool AddSortOrder(Int64 CustomfieldId, Paraenums.QuerySortBy SortDirection)
        {
            if (_SortByFields.Count < 5)
            {
                _SortByFields.Add("FID" + CustomfieldId.ToString() + "_" + SortDirection.ToString().ToLower() + "_");
                return true;
            }
            else
            {
                return false;
            }
        }


    }


    /// <summary>
    /// Use this class whenever you need to query a list of objects of a particular module. This is only used when you need to list objects.
    /// </summary>
    public class ModuleQuery
    {
        /// <summary>
        /// Instantiate this class to build all the properties needed to get a list of Assets.
        /// The properties include the number of items per page, the page number, what custom fields to include in the list,
        /// as well as any filtering you need to do.
        /// </summary>
        public partial class AssetQuery : ParaQueryModuleWithCustomField
        {


            protected override void buildModuleSpecificFilter()
            {
                //_QueryFilters.Add()

            }
        }

        /// <summary>
        /// Instantiate this class to build all the properties needed to get a list of Accounts.
        /// The properties include the number of items per page, the page number, what custom fields to include in the list,
        /// as well as any filtering you need to do.
        /// </summary>
        public partial class AccountQuery : ParaQueryModuleWithCustomField
        {
            private Int64 _view = 0;

            /// <summary>
            /// The ID of the view
            /// </summary>
            public Int64 View
            {
                get { return _view; }
                set
                {
                    _view = value;
                }
            }

            protected override void buildModuleSpecificFilter()
            {
                // Checking if this is a view request.
                if (View > 0)
                {
                    _QueryFilters.Add("_view_=" + View.ToString());
                }
            }


            /// <summary>
            /// Contains all the static properties you will need when filtering by static fields.
            /// </summary>
            public static partial class AccountStaticFields
            {
                public readonly static string Accountname = "Account_Name";

                /// <summary>
                /// The criteria for this property can only be a SlaID
                /// </summary>
                public readonly static string AccountSlaID = "sla_id_";

                /// <summary>
                /// The criteria for this property can only be a RoleID
                /// </summary>
                public readonly static string DefaultCustomerRoleID = "Default_Customer_Role_id_";

                /// <summary>
                /// The criteria for this property can only be a CsrID
                /// </summary>
                public readonly static string OwnedByCsrID = "Owned_By_id_";

                /// <summary>
                /// The criteria for this property can only be a CsrID
                /// </summary>
                public readonly static string ModifiedByCsrID = "Modified_By_id_";


                public readonly static string Date_Created = "Date_Created";
                public readonly static string Date_Updated = "Date_Updated";

                /// <summary>
                /// In certain configurations, Accounts might be associated to products.
                /// In that case, you can filter by certain product IDs.
                /// </summary>
                public readonly static string AccountProducts = "Am_Product";

                /// <summary>
                /// In certain configurations, certain customers from an account can see tickets belonging
                /// to other accounts. You can filter by this field by entering the 
                /// account(s) id(s) (comma separated ids, in case you are entering multiple ids).
                /// </summary>
                public readonly static string ViewableAccountid = "shown_accounts";

            }
        }

        public partial class CsrQuery : ParaQuery
        {
            protected override void buildModuleSpecificFilter()
            {

            }

            public static partial class CsrStaticFields
            {
                /// <summary>
                /// Use this property to filter by the date created property of an Article.
                /// </summary>

                public readonly static string DateCreated = "Date_Created";
                public readonly static string DateUpdated = "Date_Format";
                public readonly static string Email = "Email";
                public readonly static string Fax = "Fax";
                public readonly static string Full_Name = "Full_Name";
                public readonly static string Phone1 = "Phone1";
                public readonly static string Phone2 = "Phone2";
                public readonly static string Screen_Name = "Screen_Name";
                public readonly static string ModifiedBy = "Modified_By_id_";
                public readonly static string Role = "CsrRole_id_";
                public readonly static string Status = "Status_id_";
                public readonly static string Timezone = "Timezone_id_";
            }


        }


        /// <summary>
        /// Instantiate this class to build all the properties needed to get a list of Customers.
        /// The properties include the number of items per page, the page number, what custom fields to include in the list,
        /// as well as any filtering you need to do.
        /// </summary>
        public partial class CustomerQuery : ParaQueryModuleWithCustomField
        {

            private Int64 _view = 0;

            /// <summary>
            /// The ID of the view
            /// </summary>
            public Int64 View
            {
                get { return _view; }
                set
                {
                    _view = value;
                }
            }

            protected override void buildModuleSpecificFilter()
            {
                // Checking if this is a view request.
                if (View > 0)
                {
                    _QueryFilters.Add("_view_=" + View.ToString());
                }
            }

            /// <summary>
            /// Contains all the static properties you will need when filtering by static fields.
            /// </summary>
            public static partial class CustomerStaticFields
            {
                /// <summary>
                /// Use this property to filter by the date created property of a customer.
                /// </summary>
                public readonly static string DateCreated = "Date_Created";

                /// <summary>
                /// Use this property to filter by the date updated property of a customer.
                /// </summary>
                public readonly static string DateUpdated = "Date_Updated";

                /// <summary>
                /// Use this property to filter by the date created property of a customer.Visited
                /// </summary>
                public readonly static string DateVisited = "Date_Visited";

                /// <summary>
                /// To search by customer User Name.
                /// </summary>
                public readonly static string User_Name = "User_Name";

                /// <summary>
                /// To search by customer Email.
                /// </summary>
                public readonly static string CustomerEmail = "Email";

                /// <summary>
                /// Search by customer First Name
                /// </summary>
                public readonly static string FirstName = "First_Name";

                /// <summary>
                /// Search by customer Last Name
                /// </summary>
                public readonly static string LastName = "Last_Name";

                /// <summary>
                /// Filter by the accountid of the customers being returned. 
                /// </summary>
                public readonly static string AccountID = "Account_id_";

                /// <summary>
                /// To filter by Customers' SLAs
                /// </summary>
                public readonly static string CustomerSla = "Sla_id_";

                /// <summary>
                /// To filter by Customers' Roles
                /// </summary>
                public readonly static string CustomerRoleID = "Customer_Role_id_";

                /// <summary>
                /// To filter by the status field. 
                /// </summary>
                public readonly static string Status = "Status_id_";

                /// <summary>
                /// In certain configurations, customers might be associated to products.
                /// In that case, you can filter by certain product IDs.
                /// </summary>
                public readonly static string CustomerProducts = "Cust_Product_id_";

                /// <summary>
                /// In certain configurations, customers might have to accept the terms of use.
                /// If you have that configuration activated, you will be able to filter by this field.
                /// </summary>
                public readonly static string AcceptUseOfTerm = "";

            }



        }

        /// <summary>
        /// Instantiate this class to build all the properties needed to get a list of Tickets.
        /// The properties include the number of items per page, the page number, what custom fields to include in the list,
        /// as well as any filtering you need to do.
        /// </summary>
        public partial class TicketQuery : ParaQueryModuleWithCustomField
        {
            // Status Type to filter tickets by.
            private Paraenums.TicketStatusType _status_Type = Paraenums.TicketStatusType.All;

            /// <summary>
            /// If there is any need to limit the ticket returned to a certain status type. Default is 
            /// to return all.
            /// </summary>
            public Paraenums.TicketStatusType Status_Type
            {
                get { return _status_Type; }
                set
                {
                    _status_Type = value;
                }
            }


            private Boolean _myTickets = false;

            /// <summary>
            /// Whether to show only tickets assigned to the CSR associated with the token you are using or not.
            /// </summary>
            public Boolean MyTickets
            {
                get { return _myTickets; }
                set
                {
                    _myTickets = value;
                }
            }


            private Int64 _view = 0;

            /// <summary>
            /// The ID of the view
            /// </summary>
            public Int64 View
            {
                get { return _view; }
                set
                {
                    _view = value;
                }
            }

            protected override void buildModuleSpecificFilter()
            {

                // need to filter if requesting by status type
                if (Status_Type != Paraenums.TicketStatusType.All)
                {
                    _QueryFilters.Add("_status_type_=" + Status_Type.ToString().ToLower());
                }

                if (MyTickets == true)
                {
                    _QueryFilters.Add("_my_tickets_=true");
                }

                // Checking if this is a view request.
                if (View > 0)
                {
                    _QueryFilters.Add("_view_=" + View.ToString());
                }
            }

            /// <summary>
            /// Contains all the static properties you will need when filtering by static fields.
            /// </summary>
            public static partial class TicketStaticFields
            {
                /// <summary>
                /// Use this property to filter by the date created property of a ticket.
                /// </summary>
                public readonly static string Date_Created = "Date_Created";

                /// <summary>
                /// Allows for filtering through the Account of the Customer that currently owns the ticket.
                /// </summary>
                public readonly static string Ticket_Account = "Ticket_Account_id_";

                /// <summary>
                /// Provide filtering with the department id of the tickets you would like to list.
                /// </summary>
                public readonly static string Ticket_Department = "Department_id_";

                /// <summary>
                /// Use this property to filter by the date created property of a ticket.
                /// </summary>
                public readonly static string Date_Updated = "Date_Updated";

                /// <summary>
                /// Filter by the "Assigned To" Csr ID. 
                /// </summary>
                public readonly static string Assigned_To = "Assigned_To_id_";

                /// <summary>
                /// Filter by the "additional_contact_account_id_". 
                /// </summary>
                public readonly static string additional_contact_account_id = "additional_contact_account_id_";

                /// <summary>
                /// Filter by the "additional_contact_id_". 
                /// </summary>
                public readonly static string additional_contact_id = "additional_contact_id_";

                /// <summary>
                /// Filter by the "Additional Contact's Account" ID. 
                /// </summary>
                public readonly static string Additional_Contact_Account = "Additional_Contact_Account_id_";

                /// <summary>
                /// 
                /// </summary>
                public readonly static string Cc_Csr = "Cc_Csr";

                /// <summary>
                /// 
                /// </summary>
                public readonly static string CC_Customer = "Cc_Customer";

                /// <summary>
                /// 
                /// </summary>
                public readonly static string Email_Notification = "Email_Notification";

                /// <summary>
                /// 
                /// </summary>
                public readonly static string Hide_From_Customer = "Hide_From_Customer";

                /// <summary>
                /// 
                /// </summary>
                public readonly static string Customer = "Ticket_Customer_id_";

                /// <summary>
                /// 
                /// </summary>
                public readonly static string Additional_Contact = "Additional_Contact_id_";

                /// <summary>
                /// 
                /// </summary>
                public readonly static string Ticket_Queue = "Ticket_Queue_id_";

                /// <summary>
                /// Filter by the created by CSR id.
                /// </summary>
                public readonly static string Entered_By = "Entered_By_id_";

                /// <summary>
                /// Tickets that have a specific ticket parentid.
                /// </summary>
                public readonly static string Ticket_Parent = "Ticket_Parent_id_";

                /// <summary>
                /// Tickets that have a specific ticket status.
                /// </summary>
                public readonly static string Status = "ticket_status_id_";

                /// <summary>
                /// Tickets owned by Customers who belong to the specific Account
                /// </summary>
                public readonly static string Account = "Ticket_Account_id_";

                /// <summary>
                /// Tickets associated to a specific Asset
                /// </summary>
                public readonly static string Asset = "Ticket_Product_id_";

            }
        }

        /// <summary>
        /// Instantiate this class to build all the properties needed to get a list of Downloads.
        /// The properties include the number of items per page, the page number, what custom fields to include in the list,
        /// as well as any filtering you need to do.
        /// </summary>
        public partial class DownloadQuery : ParaQuery
        {
            protected override void buildModuleSpecificFilter()
            {

            }
            /// <summary>
            /// Contains all the static properties you will need when filtering by static fields.
            /// </summary>
            public static partial class DownloadStaticFields
            {
                /// <summary>
                /// Use this property to filter by the date created property of a Download.
                /// </summary>
                public readonly static string DateCreated = "Date_Created";

                /// <summary>
                /// Use this property to filter by the Date Modified property of the Download.
                /// </summary>
                public readonly static string DateUpdated = "Date_Updated";
                /// <summary>
                /// Use this property to filter by the description of the download.
                /// </summary>
                public readonly static string Description = "Description";
                /// <summary>
                /// Use this property to filter by the Eula ID of the Download, in case your configs have this feature activated.
                /// </summary>
                public readonly static string Eula = "Eula_id_";
                /// <summary>
                /// Use this property to filter by the external link of the download.
                /// </summary>
                public readonly static string ExternalLink = "External_Link";
                /// <summary>
                /// Use this property to filter by the id of the folder the download belongs to.
                /// </summary>
                public readonly static string Folder = "Folder_id_";
                /// <summary>
                /// Use this property to filter by the guid of the download file.
                /// </summary>
                public readonly static string Guid = "Guid";
                /// <summary>
                /// Use this property to filter by the Name of the download.
                /// </summary>
                public readonly static string name = "Name";
                /// <summary>
                /// Use this property to filter by one or many SLAs that are specified for this download.
                /// </summary>
                public readonly static string Permissions = "Permissions_id_";
                /// <summary>
                /// Use this property to filter by the Product id of the download.
                /// </summary>
                public readonly static string Products = "Products_id_";
                /// <summary>
                /// Use this property to filter by the published state of the download.
                /// </summary>
                public readonly static string Published = "Published";

                /// <summary>
                /// Use this property to filter by the Title of the download.
                /// </summary>
                public readonly static string Title = "Title";

                /// <summary>
                /// Use this property to filter by the Visibility state of the download.
                /// </summary>
                public readonly static string Visible = "Visible";
            }
        }

        /// <summary>
        /// Instantiate this class to build all the properties needed to get a list of Articles.
        /// The properties include the number of items per page, the page number, what custom fields to include in the list,
        /// as well as any filtering you need to do.
        /// </summary>
        public partial class ArticleQuery : ParaQuery
        {
            protected override void buildModuleSpecificFilter()
            {

            }
            /// <summary>
            /// Contains all the static properties you will need when filtering by static fields.
            /// </summary>
            public static partial class ArticleStaticFields
            {
                /// <summary>
                /// Use this property to filter by the date created property of an Article.
                /// </summary>

                public readonly static string DateCreated = "Date_Created";
                public readonly static string DateUpdated = "Date_Updated";
                public readonly static string Answer = "Answer";
                public readonly static string ExpirationDate = "Expiration_Date";
                public readonly static string Published = "Published";
                public readonly static string Question = "Question";
                public readonly static string Rating = "Rating";
                public readonly static string TimesViewed = "Times_Viewed";
                public readonly static string ModifiedBy = "Modified_By_id_";
                public readonly static string CreatedBy = "Created_By_id_";
                public readonly static string Folders = "Folders_id_";
                public readonly static string Permissions = "Permissions_id_";
                public readonly static string Products = "Products_id_";
            }
        }

        /// <summary>
        /// Instantiate this class to build all the properties needed to get a list of Chats.
        /// The properties include the number of items per page, the page number, what custom fields to include in the list,
        /// as well as any filtering you need to do.
        /// </summary>
        public partial class ChatQuery : ParaQueryModuleWithCustomField
        {
            protected override void buildModuleSpecificFilter()
            {
                // Lloyd  2703484446
                // Art 8597790481
                // 
            }
            /// <summary>
            /// Contains all the static properties you will need when filtering by static fields.
            /// </summary>
            public static partial class ChatStaticFields
            {
                /// <summary>
                /// Use this property to filter by the date created property of an Article.
                /// </summary>

                public readonly static string Browser_Language = "Browser_Language";
                public readonly static string Browser_Type = "Browser_Type";
                public readonly static string Browser_Version = "Browser_Version";
                public readonly static string Chat_Number = "Chat_Number";
                public readonly static string Customer = "Customer_id_";
                public readonly static string  Date_Created = "Date_Created";
                public readonly static string  Date_Ended= "Date_Ended";
                public readonly static string  Ip_Address= "Ip_Address";
                public readonly static string  Is_Anonymous = "Is_Anonymous";
                public readonly static string Referrer_Url  = "Referrer_Url ";
                public readonly static string Related_Tickets = "Related_Tickets_id_";
                public readonly static string Status = "Status_id_";
                public readonly static string Summary  = "Summary";
                public readonly static string  User_Agent= "User_Agent";
                public readonly static string Email = "Email";
            }
        }



        /// <summary>
        /// Instantiate this class to build all the properties needed to get a list of Products.
        /// The properties include the number of items per page, the page number, what custom fields to include in the list,
        /// as well as any filtering you need to do.
        /// </summary>
        public partial class ProductQuery : ParaQueryModuleWithCustomField
        {
            protected override void buildModuleSpecificFilter()
            {

            }
            /// <summary>
            /// Contains all the static properties you will need when filtering by static fields.
            /// </summary>
            public static partial class ProductStaticFields
            {
                public readonly static string Name = "Name";
                public readonly static string Date_Created = "Date_Created";
                public readonly static string Date_Updated = "Date_Updated";
                public readonly static string Folder = "Folder_id_";
                public readonly static string Visible  = "Visible ";
                public readonly static string Instock  = "Instock ";
                public readonly static string Sku = "Sku";
                public readonly static string Price  = "Price ";
                public readonly static string Shortdesc  = "Shortdesc ";
                public readonly static string Longdesc  = "Longdesc ";
            }
        }
    }


    /// <summary>
    /// Use this class whenever you need to query a list of entities. This is only used when you need to list enities.
    /// </summary>
    public class EntityQuery
    {

        public partial class TimezoneQuery : ParaQuery
        {
            protected override void buildModuleSpecificFilter()
            {
                PageSize = 250;
            }
        }

        public partial class CustomerStatusQuery : ParaQuery
        {
            protected override void buildModuleSpecificFilter()
            {
                PageSize = 250;
            }
        }

        public partial class CsrStatusQuery : ParaQuery
        {
            protected override void buildModuleSpecificFilter()
            {
                PageSize = 250;
            }
        }

        public partial class StatusQuery : ParaQuery
        {
            protected override void buildModuleSpecificFilter()
            {
                PageSize = 250;
            }
        }

        public partial class TicketStatusQuery : ParaQuery
        {
            protected override void buildModuleSpecificFilter()
            {
                PageSize = 250;
            }
        }

        public partial class DepartmentQuery : ParaQuery
        {
            protected override void buildModuleSpecificFilter()
            {

            }
        }

        public partial class QueueQuery : ParaQuery
        {
            protected override void buildModuleSpecificFilter()
            {

            }
        }

        public partial class ViewQuery : ParaQuery
        {
            protected override void buildModuleSpecificFilter()
            {

            }
        }

        public partial class SlaQuery : ParaQuery
        {
            protected override void buildModuleSpecificFilter()
            {

            }

        }

        public partial class RoleQuery : ParaQuery
        {
            protected override void buildModuleSpecificFilter()
            {
                PageSize = 250;
            }
        }

        public partial class DownloadFolderQuery : ParaQuery
        {
            protected override void buildModuleSpecificFilter()
            {

            }
            /// <summary>
            /// Contains all the static properties you will need when filtering by static fields.
            /// </summary>
            public static partial class DownloadFolderStaticFields
            {
                /// <summary>
                /// Use this property to filter by the Date Modified property of the DownloadFolder.
                /// </summary>
                public readonly static string DateUpdated = "Date_Updated";
                public readonly static string IsPrivate = "Is_Private";
                public readonly static string Name = "Name";
                public readonly static string Description = "Description";
                /// <summary>
                /// The id of the parent folder.
                /// </summary>
                public readonly static string ParentFolder = "Parent_Folder_id_";

            }
        }

        public partial class ArticleFolderQuery : ParaQuery
        {
            protected override void buildModuleSpecificFilter()
            {

            }
            /// <summary>
            /// Contains all the static properties you will need when filtering by static fields.
            /// </summary>
            public static partial class ArticleFolderStaticFields
            {
                public readonly static string Name = "Name";
                public readonly static string ParentFolder = "Parent_Folder_id_";
            }
        }

        public partial class ProductFolderQuery : ParaQuery
        {
            protected override void buildModuleSpecificFilter()
            {

            }
            /// <summary>
            /// Contains all the static properties you will need when filtering by static fields.
            /// </summary>
            public static partial class ProductFolderStaticFields
            {
                /// <summary>
                /// Use this property to filter by the Date Modified property of the ProductFolder.
                /// </summary>
                public readonly static string DateUpdated = "Date_Updated";
                public readonly static string IsPrivate = "Is_Private";
                public readonly static string Description = "Description";
                /// <summary>
                /// The id of the parent folder.
                /// </summary>
                public readonly static string ParentFolder = "Parent_Folder_id_";






            }
        }
    }
}

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Microsoft.VisualBasic;
using ParatureSDK.Fields;
using ParatureSDK.ParaHelper;
using ParatureSDK.ParaObjects;
using Attachment = System.Net.Mail.Attachment;

namespace ParatureSDK
{
    /// <summary>
    /// The APICallFactory manages all calls made to the APIs server. No API calls should be made outside of this class.
    /// </summary>    
    static internal class ApiCallFactory
    {
        /// <summary>
        /// This method will create/update an Object in Parature.
        /// </summary>
        /// <param name="paracredentials">
        ///The credentials to be used for making the API call. 
        ///Value Type: <see cref="ParaCredentials" />   (ParaConnect.ParaCredentials)
        ///</param>
        /// <param name="module">
        ///The name of the module to create or update. Choose from the ParatureModule enum list. 
        ///Value Type: <see cref="ParaEnums.ParatureModule" />   (ParatureModule)
        ///</param>
        /// <param name="objectid">
        ///Provides the ID of the object being inserted or updated. 
        ///Value Type: <see cref="Int64" />   (System.Int64)
        ///</param>
        /// <param name="fileToPost">
        ///When creating or updating an object, you will need to pass the properly formatted XML document to be sent to the server.
        ///Value Type: <see cref="String" />   (System.String)
        ///</param>
        public static ApiCallResponse ObjectCreateUpdate(ParaCredentials paracredentials, ParaEnums.ParatureModule module, XmlDocument fileToPost, Int64 objectid)
        {
            // Calling the next method that manages the call.
            return ObjectCreateUpdate(paracredentials, module, fileToPost, objectid, null);
        }

        public static ApiCallResponse ObjectCreateUpdate<T>(ParaCredentials paracredentials, XmlDocument fileToPost, Int64 objectid)
        {
            // Calling the next method that manages the call.
            return ObjectCreateUpdate<T>(paracredentials, fileToPost, objectid, null);
        }

        ///  <summary>
        ///  This method will create/update an Object in Parature.
        ///  </summary>
        ///  <param name="paracredentials">
        /// The credentials to be used for making the API call. 
        /// Value Type: <see cref="ParaCredentials" />   (ParaConnect.ParaCredentials)
        /// </param>
        ///  <param name="module">
        /// The name of the module to create or update. Choose from the ParatureModule enum list. 
        /// Value Type: <see cref="ParaEnums.ParatureModule" />   (ParatureModule)
        /// </param>
        ///  <param name="fileToPost">
        /// When creating or updating an object, you will need to pass the properly formatted XML document to be sent to the server.
        /// Value Type: <see cref="String" />   (System.String)
        ///  </param>
        ///  <param name="objectid">
        /// Provides the ID of the object being inserted or updated. 
        /// Value Type: <see cref="Int64" />   (System.Int64)
        /// </param>
        /// <param name="arguments"></param>
        public static ApiCallResponse ObjectCreateUpdate(ParaCredentials paracredentials, ParaEnums.ParatureModule module, XmlDocument fileToPost, Int64 objectid, ArrayList arguments)
        {
            if (arguments == null)
            {
                arguments = new ArrayList();
            }
            switch (module)
            {
                case ParaEnums.ParatureModule.Ticket:
                case ParaEnums.ParatureModule.Account:
                case ParaEnums.ParatureModule.Customer:
                case ParaEnums.ParatureModule.Product:
                case ParaEnums.ParatureModule.Asset:
                    if (paracredentials.EnforceRequiredFields == false)
                    {
                        arguments.Add("_enforceRequiredFields_=" + paracredentials.EnforceRequiredFields.ToString().ToLower());
                    }
                    break;
            }
            // Getting the standard API URL to call.
            var apiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, module, objectid, arguments);

            // To set up the call method, we check if this is a create (the objectid=0 in that case)
            // or an update (when we received an objectid>0)
            var apicallhttpmethod = (objectid == 0) 
                ? ParaEnums.ApiCallHttpMethod.Post 
                : ParaEnums.ApiCallHttpMethod.Update;

            // Calling the next method that manages the call.
            return ApiMakeTheCall(apiCallUrl, apicallhttpmethod, fileToPost, paracredentials.Instanceid, paracredentials);
        }

        public static ApiCallResponse ObjectCreateUpdate<T>(ParaCredentials paracredentials, XmlDocument fileToPost, Int64 objectid, ArrayList arguments)
        {
            var entityType = typeof (T).ToString();
            if (arguments == null)
            {
                arguments = new ArrayList();
            }
            switch (entityType)
            {
                case "Ticket":
                case "Account":
                case "Customer":
                case "Product":
                case "Asset":
                    if (paracredentials.EnforceRequiredFields == false)
                    {
                        arguments.Add("_enforceRequiredFields_=" + paracredentials.EnforceRequiredFields.ToString().ToLower());
                    }
                    break;
            }
            // Getting the standard API URL to call.
            var apiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, entityType, objectid, arguments);

            // To set up the call method, we check if this is a create (the objectid=0 in that case)
            // or an update (when we received an objectid>0)
            var apicallhttpmethod = (objectid == 0)
                ? ParaEnums.ApiCallHttpMethod.Post
                : ParaEnums.ApiCallHttpMethod.Update;

            // Calling the next method that manages the call.
            return ApiMakeTheCall(apiCallUrl, apicallhttpmethod, fileToPost, paracredentials.Instanceid, paracredentials);
        }

        ///  <summary>
        ///  This method will create/update an Object in Parature.
        ///  </summary>
        ///  <param name="paracredentials">
        /// The credentials to be used for making the API call. 
        /// Value Type: <see cref="ParaCredentials" />   (ParaConnect.ParaCredentials)
        /// </param>
        /// <param name="objectid">
        /// Provides the ID of the object being inserted or updated. 
        /// Value Type: <see cref="Int64" />   (System.Int64)
        /// </param>
        /// <param name="entity"></param>
        /// <param name="fileToPost">
        /// When creating or updating an object, you will need to pass the properly formatted XML document to be sent to the server.
        /// Value Type: <see cref="String" />   (System.String)
        /// </param>
        public static ApiCallResponse EntityCreateUpdate(ParaCredentials paracredentials, ParaEnums.ParatureEntity entity, XmlDocument fileToPost, Int64 objectid)
        {
            // Getting the standard API URL to call.
            var apiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, entity, objectid, false);

            // To set up the call method, we check if this is a create (the objectid=0 in that case)
            // or an update (when we received an objectid>0)
            var apicallhttpmethod = (objectid == 0) 
                ? ParaEnums.ApiCallHttpMethod.Post 
                : ParaEnums.ApiCallHttpMethod.Update;

            // Calling the next method that manages the call.
            return ApiMakeTheCall(apiCallUrl, apicallhttpmethod, fileToPost, paracredentials.Instanceid, paracredentials);
        }

        /// <summary>
        /// Use this method to delete the object passed to it.
        /// </summary>
        /// <param name="paracredentials">
        ///The credentials to be used for making the API call. 
        ///Value Type: <see cref="ParaCredentials" />   (ParaConnect.ParaCredentials)
        ///</param>
        /// <param name="module">
        ///The name of the module to create or update. Choose from the ParatureModule enum list. 
        ///Value Type: <see cref="ParaEnums.ParatureModule" />   (Paraenums.ParatureModule)
        ///</param>
        /// <param name="objectid">
        ///The id of the object to create or update. 
        ///Value Type: <see cref="Int64" />   (System.int64)
        ///</param>
        /// <param name="purge">
        ///Indicates whether this a Purge (permanent deletion), or just a deletion (move to trash bin). Indicate TRUE for a purge, FALSE for a delete
        ///Value Type: <see cref="bool" />   (System.bool)
        ///</param>
        public static ApiCallResponse ObjectDelete(ParaCredentials paracredentials, ParaEnums.ParatureModule module, Int64 objectid, bool purge)
        {
            string apiCallUrl;

            if (purge)
            {
                var arguments = new ArrayList {"_purge_=true"};
                apiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, module, objectid, arguments);
            }
            else
            {
                apiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, module.ToString(), objectid, false);
            }


            return ApiMakeTheCall(apiCallUrl, ParaEnums.ApiCallHttpMethod.Delete, paracredentials.Instanceid, paracredentials);
        }

        public static ApiCallResponse ObjectDelete<T>(ParaCredentials paracredentials, Int64 objectid, bool purge)
        {
            var entityType = typeof (T).ToString();
            string apiCallUrl;

            if (purge)
            {
                var arguments = new ArrayList { "_purge_=true" };
                apiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, entityType, objectid, arguments);
            }
            else
            {
                apiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, entityType, objectid, false);
            }


            return ApiMakeTheCall(apiCallUrl, ParaEnums.ApiCallHttpMethod.Delete, paracredentials.Instanceid, paracredentials);
        }

        /// <summary>
        /// Use this method to delete the entity passed to it.
        /// </summary>
        /// <param name="paracredentials">
        ///The credentials to be used for making the API call. 
        ///Value Type: <see cref="ParaCredentials" />   (ParaConnect.ParaCredentials)
        ///</param>
        /// <param name="entity">
        ///The name of the entity to delete. Choose from the ParatureEntity enum list. 
        ///Value Type: <see cref="ParaEnums.ParatureEntity" />   (Paraenums.ParatureEntity)
        /// </param>
        /// <param name="entityid">
        ///The id of the entity to delete. 
        ///Value Type: <see cref="Int64" />   (System.int64)
        /// </param>
        public static ApiCallResponse EntityDelete(ParaCredentials paracredentials, ParaEnums.ParatureEntity entity, Int64 entityid)
        {
            var arguments = new ArrayList {"_purge_=true"};
            var apiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, entity, entityid, arguments);

            return ApiMakeTheCall(apiCallUrl, ParaEnums.ApiCallHttpMethod.Delete, paracredentials.Instanceid, paracredentials);
        }

        /// <summary>
        /// Use this method to get the details of an object that you plan to fill.
        /// </summary>
        /// <param name="paracredentials">
        ///The credentials to be used for making the API call. 
        ///Value Type: <see cref="ParaCredentials" />   (ParaConnect.ParaCredentials)
        ///</param>
        /// <param name="module">
        ///The name of the module to create or update. Choose from the ParatureModule enum list. 
        ///Value Type: <see cref="ParaEnums.ParatureModule" />   (Paraenums.ParatureModule)
        ///</param>
        /// <param name="objectid">
        ///The id of the object to create or update. 
        ///Value Type: <see cref="Int64" />   (System.int64)
        ///</param>
        public static ApiCallResponse ObjectGetDetail<T>(ParaCredentials paracredentials, ParaEnums.ParatureModule module, Int64 objectid) where T: ParaEntity
        {
            var entityName = typeof (T).Name;
            var apiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, entityName, objectid, false);

            return ApiMakeTheCall(apiCallUrl, ParaEnums.ApiCallHttpMethod.Get, paracredentials.Instanceid, paracredentials);
        }

        ///  <summary>
        ///  Use this method to get the details of an Entity that you plan to fill.
        ///  </summary>
        ///  <param name="paracredentials">
        /// The credentials to be used for making the API call. 
        /// Value Type: <see cref="ParaCredentials" />   (ParaConnect.ParaCredentials)
        /// </param>
        ///  <param name="module">
        /// The name of the entity to create or update. Choose from the ParatureEntity enum list. 
        /// Value Type: <see cref="ParaEnums.ParatureEntity" />   (Paraenums.ParatureEntity)
        /// </param>
        /// <param name="entity"></param>
        /// <param name="objectid">
        /// The id of the object to create or update. 
        /// Value Type: <see cref="Int64" />   (System.int64)
        /// </param>
        public static ApiCallResponse ObjectGetDetail(ParaCredentials paracredentials, ParaEnums.ParatureEntity entity, Int64 objectid)
        {
            var apiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, entity, objectid, false);

            return ApiMakeTheCall(apiCallUrl, ParaEnums.ApiCallHttpMethod.Get, paracredentials.Instanceid, paracredentials);
        }

        /// <summary>
        /// Use this method to get the details of an object that you plan to fill.
        /// </summary>
        /// <param name="paracredentials">
        ///The credentials to be used for making the API call. 
        ///Value Type: <see cref="ParaCredentials" />   (ParaConnect.ParaCredentials)
        ///</param>
        /// <param name="module">
        ///The name of the module to create or update. Choose from the ParatureModule enum list. 
        ///Value Type: <see cref="ParaEnums.ParatureModule" />   (ParatureModule)
        ///</param>
        /// <param name="objectid">
        ///The id of the object to create or update. 
        ///Value Type: <see cref="Int64" />   (System.int64)
        ///</param>
        /// <param name="arguments">
        ///The list of extra optional arguments you need to include in the call. For example, for a ticket, we might want to get the action history.
        ///Value Type: <see cref="ArrayList" />   (System.String[])
        ///</param>
        public static ApiCallResponse ObjectGetDetail(ParaCredentials paracredentials, ParaEnums.ParatureModule module, Int64 objectid, ArrayList arguments)
        {
            var apiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, module, objectid, arguments);

            return ApiMakeTheCall(apiCallUrl, ParaEnums.ApiCallHttpMethod.Get, paracredentials.Instanceid, paracredentials);
        }

        /// <summary>
        /// Use this method to get a list of objects that you plan to fill.
        /// </summary>
        /// <param name="paracredentials">
        ///The credentials to be used for making the API call. 
        ///Value Type: <see cref="ParaCredentials" />   (ParaConnect.ParaCredentials)
        ///</param>
        /// <param name="module">
        ///The name of the module to create or update. Choose from the ParatureModule enum list. 
        ///</param>
        /// <param name="arguments">
        ///The list of extra optional arguments you need to include in the call. For example, any fields filtering, any custom fields to include, etc.
        ///Value Type: <see cref="ArrayList" />   
        ///</param>
        public static ApiCallResponse ObjectGetList(ParaCredentials paracredentials, ParaEnums.ParatureModule module, ArrayList arguments)
        {
            var apiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, module, 0, arguments);
            return ApiMakeTheCall(apiCallUrl, ParaEnums.ApiCallHttpMethod.Get, paracredentials.Instanceid, paracredentials);
        }

        /// <summary>
        /// Use this method to get a list of objects that you plan to fill.
        /// </summary>
        /// <param name="paracredentials">
        ///The credentials to be used for making the API call. 
        ///Value Type: <see cref="ParaCredentials" />   (ParaConnect.ParaCredentials)
        ///</param>
        /// <param name="module">
        ///The name of the module to create or update. Choose from the ParatureModule enum list. 
        ///</param>
        /// <param name="arguments">
        ///The list of extra optional arguments you need to include in the call. For example, any fields filtering, any custom fields to include, etc.
        ///Value Type: <see cref="ArrayList" />   
        ///</param>
        public static ApiCallResponse ObjectGetList<T>(ParaCredentials paracredentials, ArrayList arguments)
        {
            var entityType = typeof (T).ToString();
            var apiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, entityType, 0, arguments);
            return ApiMakeTheCall(apiCallUrl, ParaEnums.ApiCallHttpMethod.Get, paracredentials.Instanceid, paracredentials);
        }

        /// <summary>
        /// Use this method to get a list of objects that you plan to fill.
        /// </summary>
        /// <param name="paracredentials">
        ///The credentials to be used for making the API call. 
        ///Value Type: <see cref="ParaCredentials" />   (ParaConnect.ParaCredentials)
        ///</param>
        /// <param name="entity">
        ///The name of the entity to list. Choose from the ParatureEntity enum list. 
        ///</param>      
        public static ApiCallResponse ObjectGetList(ParaCredentials paracredentials, ParaEnums.ParatureEntity entity, ArrayList arguments)
        {
            var apiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, entity, 0, arguments);
            return ApiMakeTheCall(apiCallUrl, ParaEnums.ApiCallHttpMethod.Get, paracredentials.Instanceid, paracredentials);
        }


        public static ApiCallResponse ObjectSecondLevelGetList(ParaCredentials paracredentials, ParaEnums.ParatureModule module, ParaEnums.ParatureEntity entity, ArrayList arguments)
        {
            string ApiCallUrl;
            ApiCallUrl = ApiUrlBuilder.ApiObjectCustomUrl(paracredentials, module, entity, arguments);
            return ApiMakeTheCall(ApiCallUrl, ParaEnums.ApiCallHttpMethod.Get, paracredentials.Instanceid, paracredentials);
        }

        /// <summary>
        /// Use this method to get the Schema XML of an object.
        /// </summary>
        public static ApiCallResponse ObjectGetSchema(ParaCredentials paracredentials, ParaEnums.ParatureModule module)
        {
            var apiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, module.ToString(), 0, true);

            return ApiMakeTheCall(apiCallUrl, ParaEnums.ApiCallHttpMethod.Get, paracredentials.Instanceid, paracredentials);
        }

        /// <summary>
        /// Use this method to get the Schema XML of an object.
        /// </summary>
        public static ApiCallResponse ObjectGetSchema<T>(ParaCredentials paracredentials, ParaEnums.ParatureModule module)
        {
            var entityType = typeof (T).ToString();
            var apiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, entityType, 0, true);

            return ApiMakeTheCall(apiCallUrl, ParaEnums.ApiCallHttpMethod.Get, paracredentials.Instanceid, paracredentials);
        }

        /// <summary>
        /// Use this method to determine if any custom fields have custom validation
        /// </summary>
        public static ParaEntity ObjectCheckCustomFieldTypes(ParaCredentials paracredentials, ParaEnums.ParatureModule module, ParaEntity baseObject)
        {
            string ApiCallUrl;
            paracredentials.EnforceRequiredFields = false;
            ApiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, module.ToString(), 0, true);

            if (baseObject.CustomFields != null)
            {
                foreach (CustomField cf in baseObject.CustomFields)
                {
                    if (cf.FieldType == "string")
                    {
                        cf.Value = "a";
                    }
                }
            }

            var doc = new XmlDocument();
            switch (module)
            {
                case ParaEnums.ParatureModule.Account:
                    doc = XmlGenerator.GenerateXml((Account)baseObject);
                    break;
                case ParaEnums.ParatureModule.Customer:
                    doc = XmlGenerator.GenerateXml((Customer)baseObject);
                    break;
                case ParaEnums.ParatureModule.Product:
                    doc = XmlGenerator.GenerateXml((Product)baseObject);
                    break;
                case ParaEnums.ParatureModule.Asset:
                    doc = XmlGenerator.GenerateXml((Asset)baseObject);
                    break;
                case ParaEnums.ParatureModule.Ticket:
                    doc = XmlGenerator.GenerateXml((Ticket)baseObject);
                    break;
                default:
                    break;
            }

            ApiCallResponse ar = ApiMakeTheCall(ApiCallUrl, ParaEnums.ApiCallHttpMethod.Post, doc, paracredentials.Instanceid, paracredentials);

            if (ar.HasException)
            {
                string errors = ar.ExceptionDetails;

                string[] errorLines = errors.Split(';');

                foreach (string line in errorLines)
                {

                    //added below line, since the validation message is changed for the API call
                    string tempLine = line.Contains("Short Name") ? line.Remove(line.IndexOf(", Short Name"), line.IndexOf("]") - line.IndexOf(", Short Name")) : line;

                    tempLine = tempLine.ToLower().Trim();


                    string id = null;

                    if (tempLine.StartsWith("invalid field validation message"))
                    {

                        if (line.ToLower().Contains("us phone number"))
                        {
                            Match m = Regex.Match(line, @"Invalid Field Validation Message : (.+?) is not a valid US phone number");
                            id = m.Groups[1].Value.Trim();

                            foreach (var cf in baseObject.CustomFields)
                            {
                                if (cf.Id == long.Parse(id))
                                {
                                    cf.FieldType = "usphone";
                                }
                            }
                        }
                        else if (tempLine.Contains("email address"))
                        {
                            Match m = Regex.Match(line, @"Invalid Field Validation Message : The Email Address '(.+?)' is not valid.");
                            id = m.Groups[1].Value.Trim();

                            foreach (var cf in baseObject.CustomFields)
                            {
                                if (cf.Id == long.Parse(id))
                                {
                                    cf.FieldType = "email";
                                }
                            }
                        }
                        else if (tempLine.Contains("international phone number"))
                        {
                            Match m = Regex.Match(line, @"Invalid Field Validation Message : (.+?) is not a valid international phone number");
                            id = m.Groups[1].Value.Trim();

                            foreach (CustomField cf in baseObject.CustomFields)
                            {
                                if (cf.Id == long.Parse(id))
                                {
                                    cf.FieldType = "internationalphone";
                                }
                            }
                        }
                        else if (tempLine.Contains("url"))
                        {
                            Match m = Regex.Match(line, @"Invalid Field Validation Message : (.+?) is an invalid URL.");
                            id = m.Groups[1].Value.Trim();
                            if (Information.IsNumeric(id.Trim()))
                            {
                            foreach (CustomField cf in baseObject.CustomFields)
                            {
                                if (cf.Id == long.Parse(id.Trim()))
                                {
                                    cf.FieldType = "url";
                                }
                            }
                            }
                        }
                    }
                }
            }
            else
            {
                // The call was successfull, deleting the item
                ApiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, module.ToString(), ar.Id, false);
                ar = ApiMakeTheCall(ApiCallUrl, ParaEnums.ApiCallHttpMethod.Delete, paracredentials.Instanceid, paracredentials);

                // purging the item
                ArrayList arguments = new ArrayList();
                arguments.Add("_purge_=true");
                ApiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, module, ar.Id, arguments);
                ar = ApiMakeTheCall(ApiCallUrl, ParaEnums.ApiCallHttpMethod.Delete, paracredentials.Instanceid, paracredentials);
            }

            return baseObject;
        }

        /// <summary>
        /// Use this method to determine if any custom fields have custom validation
        /// </summary>
        public static T ObjectCheckCustomFieldTypes<T>(ParaCredentials paracredentials, ParaEntity baseObject) where T: ParaEntity, new()
        {
            var entityType = typeof (T).ToString();
            paracredentials.EnforceRequiredFields = false;
            var apiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, entityType, 0, true);

            if (baseObject.CustomFields != null)
            {
                foreach (var cf in baseObject.CustomFields
                    .Where(cf => cf.FieldType == "string"))
                {
                    cf.Value = "a";
                }
            }

            var doc = new XmlDocument();
            switch (entityType)
            {
                case "Account":
                    doc = XmlGenerator.GenerateXml((Account)baseObject);
                    break;
                case "Customer":
                    doc = XmlGenerator.GenerateXml((Customer)baseObject);
                    break;
                case "Product":
                    doc = XmlGenerator.GenerateXml((Product)baseObject);
                    break;
                case "Asset":
                    doc = XmlGenerator.GenerateXml((Asset)baseObject);
                    break;
                case "Ticket":
                    doc = XmlGenerator.GenerateXml((Ticket)baseObject);
                    break;
                default:
                    break;
            }

            var ar = ApiMakeTheCall(apiCallUrl, ParaEnums.ApiCallHttpMethod.Post, doc, paracredentials.Instanceid, paracredentials);

            if (ar.HasException)
            {
                string errors = ar.ExceptionDetails;

                string[] errorLines = errors.Split(';');

                foreach (string line in errorLines)
                {

                    //added below line, since the validation message is changed for the API call
                    var tempLine = line.Contains("Short Name") 
                        ? line.Remove(line.IndexOf(", Short Name"), line.IndexOf("]") - line.IndexOf(", Short Name")) 
                        : line;

                    tempLine = tempLine.ToLower().Trim();

                    string id;

                    if (tempLine.StartsWith("invalid field validation message"))
                    {

                        if (line.ToLower().Contains("us phone number"))
                        {
                            var m = Regex.Match(line, @"Invalid Field Validation Message : (.+?) is not a valid US phone number");
                            id = m.Groups[1].Value.Trim();

                            foreach (CustomField cf in baseObject.CustomFields)
                            {
                                if (cf.Id == long.Parse(id))
                                {
                                    cf.FieldType = "usdate";
                                }
                            }
                        }
                        else if (tempLine.Contains("email address"))
                        {
                            Match m = Regex.Match(line, @"Invalid Field Validation Message : The Email Address '(.+?)' is not valid.");
                            id = m.Groups[1].Value.Trim();

                            foreach (CustomField cf in baseObject.CustomFields)
                            {
                                if (cf.Id == long.Parse(id))
                                {
                                    cf.FieldType = "email";
                                }
                            }
                        }
                        else if (tempLine.Contains("international phone number"))
                        {
                            Match m = Regex.Match(line, @"Invalid Field Validation Message : (.+?) is not a valid international phone number");
                            id = m.Groups[1].Value.Trim();

                            foreach (CustomField cf in baseObject.CustomFields)
                            {
                                if (cf.Id == long.Parse(id))
                                {
                                    cf.FieldType = "internationalphone";
                                }
                            }
                        }
                        else if (tempLine.Contains("url"))
                        {
                            Match m = Regex.Match(line, @"Invalid Field Validation Message : (.+?) is an invalid URL.");
                            id = m.Groups[1].Value.Trim();
                            if (Information.IsNumeric(id.Trim()))
                            {
                                foreach (CustomField cf in baseObject.CustomFields)
                                {
                                    if (cf.Id == long.Parse(id.Trim()))
                                    {
                                        cf.FieldType = "url";
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // The call was successfull, deleting the item
                apiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, entityType, ar.Id, false);
                ar = ApiMakeTheCall(apiCallUrl, ParaEnums.ApiCallHttpMethod.Delete, paracredentials.Instanceid, paracredentials);

                // purging the item
                ArrayList arguments = new ArrayList();
                arguments.Add("_purge_=true");
                apiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, entityType, ar.Id, arguments);
                ar = ApiMakeTheCall(apiCallUrl, ParaEnums.ApiCallHttpMethod.Delete, paracredentials.Instanceid, paracredentials);
            }

            return baseObject as T;
        }

        /// <summary>
        /// Use this method to get the Schema XML of an object.
        /// </summary>
        /// <param name="paracredentials"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static ApiCallResponse EntityGetSchema(ParaCredentials paracredentials, ParaEnums.ParatureEntity entity)
        {
            string ApiCallUrl;

            ApiCallUrl = ApiUrlBuilder.ApiObjectUrl(paracredentials, entity, 0, true);

            return ApiMakeTheCall(ApiCallUrl, ParaEnums.ApiCallHttpMethod.Get, paracredentials.Instanceid, paracredentials);
        }

        public static ApiCallResponse FileUploadGetUrl(ParaCredentials paracredentials, ParaEnums.ParatureModule module)
        {
            var resp = ApiMakeTheCall(ApiUrlBuilder.ApiObjectCustomUrl(paracredentials, module, "upload"), ParaEnums.ApiCallHttpMethod.Get, paracredentials.Instanceid, paracredentials);
            return resp;
        }

        public static ApiCallResponse FilePerformUpload(string postUrl, Attachment attachment, int accountId, ParaCredentials paraCredentials)
        {
            return ApiMakeTheCall(postUrl, ParaEnums.ApiCallHttpMethod.Post, attachment, accountId, paraCredentials);
        }

        public static ApiCallResponse FilePerformUpload(string postUrl, Byte[] attachment, string contentType, string fileName, int accountId, ParaCredentials paraCredentials)
        {
            return ApiMakeTheCall(postUrl, ParaEnums.ApiCallHttpMethod.Post, attachment, contentType, fileName, accountId, paraCredentials);
        }

        private static ApiCallResponse ApiMakeTheCall(string apiCallUrl, ParaEnums.ApiCallHttpMethod httpMethod, int accountId, ParaCredentials creds)
        {
            var ac = new ApiCallResponse();
            var uriAddress = new Uri(apiCallUrl);
            var req = WebRequest.Create(uriAddress) as HttpWebRequest;

            req.Method = ParaEnumProvider.ApiHttpPostProvider(httpMethod);
            ac.HttpCallMethod = req.Method;
            req.KeepAlive = false;

            //2 minutes request timeout
            req.Timeout = 120 * 1000;

            ac.XmlSent = null;

            ac.HasException = false;
            ac.CalledUrl = apiCallUrl;
            return ApiHttpRequestProcessor(ac, req, accountId, creds);
        }

        /// <summary>
        /// The true call is being made by this method.
        /// </summary>
        static ApiCallResponse ApiMakeTheCall(string callurl, ParaEnums.ApiCallHttpMethod httpMethod, XmlDocument xmlPosted, int accountId, ParaCredentials paraCredentials)
        {
            var ac = new ApiCallResponse();
            var uriAddress = new Uri(callurl);
            var req = WebRequest.Create(uriAddress) as HttpWebRequest;

            req.Method = ParaEnumProvider.ApiHttpPostProvider(httpMethod);
            ac.HttpCallMethod = req.Method;
            req.KeepAlive = false;
            
            //2 minutes request timeout
            req.Timeout = 120 * 1000;

            if (xmlPosted != null)
            {
                req.ContentType = "application/x-www-form-urlencoded";

                // Create a byte array of the data we want to send
                var bytedata = Encoding.UTF8.GetBytes(xmlPosted.OuterXml);

                // Set the content length in the request headers   
                req.ContentLength = bytedata.Length;

                // Write data   
                using (Stream postStream = req.GetRequestStream())
                {
                    postStream.Write(bytedata, 0, bytedata.Length);
                }
                ac.XmlSent = xmlPosted;
            }
            else
            {
                ac.XmlSent = null;
            }

            ac.HasException = false;
            ac.CalledUrl = callurl;
            return ApiHttpRequestProcessor(ac, req, accountId, paraCredentials);

        }

        /// <summary>
        /// The call, with passing a binary file.
        /// </summary>
        static ApiCallResponse ApiMakeTheCall(string callurl, ParaEnums.ApiCallHttpMethod httpMethod, Attachment att, int accountId, ParaCredentials paraCredentials)
        {

            string Boundary = "--ParaBoundary";
            string LineBreak = "\r\n";
            string ContentDisposition = string.Format("Content-Disposition: {0}; name=\"{1}\"; filename=\"{1}\"", att.ContentType.MediaType, att.ContentType.Name);
            ApiCallResponse ac = new ApiCallResponse();
            Uri uriAddress = new Uri(callurl);

            HttpWebRequest req = WebRequest.Create(uriAddress) as HttpWebRequest;
            req.Method = ParaEnumProvider.ApiHttpPostProvider(httpMethod);
            req.KeepAlive = false;
            ac.HttpCallMethod = req.Method;

            req.AllowWriteStreamBuffering = true;
            req.ReadWriteTimeout = 10 * 60 * 1000;
            req.Timeout = -1;

            req.ContentType = att.ContentType.MediaType + "; boundary:" + Boundary; ;

            byte[] Filebytes = new byte[Convert.ToInt32(att.ContentStream.Length - 1) + 1];
            att.ContentStream.Read(Filebytes, 0, Filebytes.Length);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Boundary);
            sb.AppendLine(ContentDisposition);
            sb.AppendLine("Content-Type: " + att.ContentType.MediaType);
            sb.AppendLine("");
            // sb.AppendLine(Boundary + "--");
            string header = sb.ToString();
            string endboundary = LineBreak + Boundary + "--";
            //int CurrOffset=0;
            byte[] FooterBytes = Encoding.ASCII.GetBytes(endboundary);
            byte[] HeadBytes = Encoding.ASCII.GetBytes(header);
            //req.ContentLength = header.Length +  Filebytes.Length + endboundary.Length;
            req.ContentLength = HeadBytes.Length + Filebytes.Length + FooterBytes.Length;
            Stream reqStreamTest = req.GetRequestStream();
            // String to Byte Array
            byte[] TotalRequest = new byte[HeadBytes.Length + Filebytes.Length + FooterBytes.Length];
            HeadBytes.CopyTo(TotalRequest, 0);
            Filebytes.CopyTo(TotalRequest, HeadBytes.Length);
            FooterBytes.CopyTo(TotalRequest, HeadBytes.Length + Filebytes.Length);
            reqStreamTest.Write(TotalRequest, 0, TotalRequest.Length);

            reqStreamTest.Close();

            ac.HasException = false;
            ac.CalledUrl = callurl;


            return ApiHttpRequestProcessor(ac, req, accountId, paraCredentials);

        }

        /// <summary>
        /// The call, with passing a binary file.
        /// </summary>
        static ApiCallResponse ApiMakeTheCall(string callurl, ParaEnums.ApiCallHttpMethod httpMethod, Byte[] attachment, string contentType, string fileName, int accountId, ParaCredentials paraCredentials)
        {

            string Boundary = "--ParaBoundary";
            string LineBreak = "\r\n";
            string ContentDisposition = string.Format("Content-Disposition: {0}; name=\"{1}\"; filename=\"{1}\"", contentType, fileName);
            ApiCallResponse ac = new ApiCallResponse();
            Uri uriAddress = new Uri(callurl);

            HttpWebRequest req = WebRequest.Create(uriAddress) as HttpWebRequest;
            req.Method = ParaEnumProvider.ApiHttpPostProvider(httpMethod);
            req.KeepAlive = false;
            ac.HttpCallMethod = req.Method;

            req.AllowWriteStreamBuffering = true;
            req.ReadWriteTimeout = 10 * 60 * 1000;
            req.Timeout = -1;

            req.ContentType = contentType + "; boundary:" + Boundary; ;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Boundary);
            sb.AppendLine(ContentDisposition);
            sb.AppendLine("Content-Type: " + contentType);
            sb.AppendLine("");

            string header = sb.ToString();
            string endboundary = LineBreak + Boundary + "--";

            byte[] FooterBytes = Encoding.ASCII.GetBytes(endboundary);
            byte[] HeadBytes = Encoding.ASCII.GetBytes(header);

            req.ContentLength = HeadBytes.Length + attachment.Length + FooterBytes.Length;
            Stream reqStreamTest = req.GetRequestStream();
            // String to Byte Array
            byte[] TotalRequest = new byte[HeadBytes.Length + attachment.Length + FooterBytes.Length];
            HeadBytes.CopyTo(TotalRequest, 0);
            attachment.CopyTo(TotalRequest, HeadBytes.Length);
            FooterBytes.CopyTo(TotalRequest, HeadBytes.Length + attachment.Length);
            reqStreamTest.Write(TotalRequest, 0, TotalRequest.Length);

            reqStreamTest.Close();

            ac.HasException = false;
            ac.CalledUrl = callurl;

            return ApiHttpRequestProcessor(ac, req, accountId, paraCredentials);
        }

        /// <summary>
        /// Performs the http web request for all ApiMakeCall methods.
        /// </summary>
        /// <param name="ac">
        /// Api Call response, this object is partially filled in the ApiMakeCall methods, this method will just be adding 
        /// certain data to it and return it.
        /// </param>
        /// <param name="req">
        /// The http web Request object. Each ApiMakeCall method will have its own http webrequest information. This method will make 
        /// the http call with the request object passed to it.
        /// </param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        static ApiCallResponse ApiHttpRequestProcessor(ApiCallResponse ac, HttpWebRequest req, int accountId, ParaCredentials pc)
        {
            string responseFromServer = "";

            try
            {
                using (var httpWResp = req.GetResponse() as HttpWebResponse)
                {
                    try
                    {
                        ac.HttpResponseCode = (int)httpWResp.StatusCode;
                    }
                    catch (Exception exRespCode)
                    {
                        ac.HttpResponseCode = -1;
                    }

                    var reader = new StreamReader(httpWResp.GetResponseStream());

                    responseFromServer = reader.ReadToEnd();

                    reader.Close();

                    try
                    {
                        ac.XmlReceived.LoadXml(responseFromServer);
                    }
                    catch (Exception ex)
                    {
                        ac.XmlReceived = null;
                    }

                    try
                    {
                        ac.HttpResponseCode = (int)httpWResp.StatusCode;
                        if (ac.HttpResponseCode == 201)
                        {
                            try
                            {
                                ac.Id = Int64.Parse(ac.XmlReceived.DocumentElement.Attributes["id"].Value);
                            }
                            catch (Exception exx)
                            {
                                ac.Id = 0;
                            }
                        }
                    }
                    catch (Exception exx)
                    {
                        ac.HttpResponseCode = -1;
                    }

                    ac.HasException = false;
                    ac.ExceptionDetails = "";
                }

            }
            catch (WebException ex)
            {
                try
                {
                    ac.HttpResponseCode = (int)((((HttpWebResponse)ex.Response).StatusCode));
                    ac.ExceptionDetails = ex.Message;
                }
                catch
                {}
                ac.HasException = true;

                if (string.IsNullOrEmpty(ac.ExceptionDetails) == true)
                {
                    ac.ExceptionDetails = ex.ToString();
                }

                if (string.IsNullOrEmpty(responseFromServer) == false)
                {
                    ac.ExceptionDetails = "Response from server: " + responseFromServer;
                }


                string exresponseFromServer = "";
                try
                {
                    var exreader = new StreamReader(ex.Response.GetResponseStream());
                    exresponseFromServer = exreader.ReadToEnd().ToString();
                    exreader.Close();

                    if (string.IsNullOrEmpty(exresponseFromServer) == false)
                    {
                        ac.ExceptionDetails = ac.ExceptionDetails + Environment.NewLine + "Exception response:" + exresponseFromServer;
                    }

                }
                catch (Exception exread)
                {
                    if (ac.HttpResponseCode == 0)
                    {
                        ac.HttpResponseCode = 503;
                    }
                }

                if (string.IsNullOrEmpty(exresponseFromServer) == false)
                {
                    try
                    {
                        ac.XmlReceived.LoadXml(exresponseFromServer);

                        XmlNode mainnode = ac.XmlReceived.DocumentElement;
                        if (mainnode.LocalName.ToLower() == "error")
                        {
                            if (mainnode.Attributes["code"].InnerText.ToLower() != "")
                            {
                                ac.HttpResponseCode = int.Parse(mainnode.Attributes["code"].InnerText.ToString());
                            }
                            if (mainnode.Attributes["message"].InnerText.ToLower() != "")
                            {
                                ac.ExceptionDetails = mainnode.Attributes["message"].InnerText.ToString();
                            }
                        }
                    }
                    catch (Exception exp)
                    {
                        ac.XmlReceived = null;
                    }
                }
                else
                {
                    ac.XmlReceived = null;
                }
            }
            finally
            {
                // xml sent and xml received cleanup
                // TEMP FIX
                if (ac.XmlReceived != null && string.IsNullOrEmpty(ac.XmlReceived.InnerXml))
                {
                    ac.XmlReceived = null;
                }
                if (ac.XmlSent != null && string.IsNullOrEmpty(ac.XmlSent.InnerXml))
                {
                    ac.XmlSent = null;
                }
            }

            return ac;
        }   
    }
}

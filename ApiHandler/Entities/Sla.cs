using System;
using System.Xml;
using ParatureAPI.ParaObjects;

namespace ParatureAPI.ApiHandler.Entities
{
    public class Sla
    {
        /// <summary>
        /// Returns an SLA object with all of its properties filled.
        /// </summary>
        /// <param name="slaId">
        ///The SLA number that you would like to get the details of. 
        ///Value Type: <see cref="Int64" />   (System.Int64)
        ///</param>
        /// <param name="paraCredentials">
        /// The Parature Credentials class is used to hold the standard login information. It is very useful to have it instantiated only once, with the proper information, and then pass this class to the different methods that need it.
        /// </param>               
        public static ParaObjects.Sla SLAGetDetails(Int64 slaId, ParaCredentials paraCredentials)
        {
            ParaObjects.Sla Sla = new ParaObjects.Sla();
            Sla = SlaFillDetails(slaId, paraCredentials);
            return Sla;
        }

        /// <summary>
        /// Returns an sla object from a XML Document. No calls to the APIs are made when calling this method.
        /// </summary>
        /// <param name="slaXml">
        /// The Sla XML, is should follow the exact template of the XML returned by the Parature APIs.
        /// </param>
        public static ParaObjects.Sla SLAGetDetails(XmlDocument slaXml)
        {
            ParaObjects.Sla sla = new ParaObjects.Sla();
            sla = XmlToObjectParser.SlaParser.SlaFill(slaXml);

            return sla;
        }

        /// <summary>
        /// Get the list of SLAs from within your Parature license.
        /// </summary>
        public static SlasList SLAsGetList(ParaCredentials paraCredentials)
        {
            return SlaFillList(paraCredentials, new EntityQuery.SlaQuery());
        }

        /// <summary>
        /// Get the list of SLAs from within your Parature license.
        /// </summary>
        public static SlasList SLAsGetList(ParaCredentials paraCredentials, EntityQuery.SlaQuery query)
        {
            return SlaFillList(paraCredentials, query);
        }

        /// <summary>
        /// Returns an sla list object from a XML Document. No calls to the APIs are made when calling this method.
        /// </summary>
        /// <param name="slaListXml">
        /// The Sla List XML, is should follow the exact template of the XML returned by the Parature APIs.
        /// </param>
        public static SlasList SLAsGetList(XmlDocument slaListXml)
        {
            SlasList slasList = new SlasList();
            slasList = XmlToObjectParser.SlaParser.SlasFillList(slaListXml);

            slasList.ApiCallResponse.xmlReceived = slaListXml;

            return slasList;
        }

        /// <summary>
        /// Fills a Sla list object.
        /// </summary>
        private static SlasList SlaFillList(ParaCredentials paraCredentials, EntityQuery.SlaQuery query)
        {

            SlasList SlasList = new SlasList();
            ApiCallResponse ar = new ApiCallResponse();
            ar = ApiCallFactory.ObjectGetList(paraCredentials, ParaEnums.ParatureEntity.Sla, query.BuildQueryArguments());
            if (ar.HasException == false)
            {
                SlasList = XmlToObjectParser.SlaParser.SlasFillList(ar.xmlReceived);
            }
            SlasList.ApiCallResponse = ar;

            // Checking if the system needs to recursively call all of the data returned.
            if (query.RetrieveAllRecords)
            {
                bool continueCalling = true;
                while (continueCalling)
                {
                    SlasList objectlist = new SlasList();

                    if (SlasList.TotalItems > SlasList.Slas.Count)
                    {
                        // We still need to pull data

                        // Getting next page's data
                        query.PageNumber = query.PageNumber + 1;

                        ar = ApiCallFactory.ObjectGetList(paraCredentials, ParaEnums.ParatureEntity.Sla, query.BuildQueryArguments());

                        objectlist = XmlToObjectParser.SlaParser.SlasFillList(ar.xmlReceived);

                        if (objectlist.Slas.Count == 0)
                        {
                            continueCalling = false;
                        }

                        SlasList.Slas.AddRange(objectlist.Slas);
                        SlasList.ResultsReturned = SlasList.Slas.Count;
                        SlasList.PageNumber = query.PageNumber;
                    }
                    else
                    {
                        // That is it, pulled all the items.
                        continueCalling = false;
                        SlasList.ApiCallResponse = ar;
                    }
                }
            }


            return SlasList;
        }

        private static ParaObjects.Sla SlaFillDetails(Int64 slaId, ParaCredentials paraCredentials)
        {
            ParaObjects.Sla Sla = new ParaObjects.Sla();
            ApiCallResponse ar = new ApiCallResponse();
            ar = ApiCallFactory.ObjectGetDetail(paraCredentials, ParaEnums.ParatureEntity.Sla, slaId);
            if (ar.HasException == false)
            {
                Sla = XmlToObjectParser.SlaParser.SlaFill(ar.xmlReceived);
            }
            else
            {

                Sla.SlaID = 0;
            }

            //Sla.ApiCallResponse = ar;
            return Sla;
        }
    }
}
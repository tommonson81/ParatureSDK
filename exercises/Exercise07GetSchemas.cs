﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParatureSDK;
using ParatureSDK.ParaObjects;

namespace Exercises
{
    class Exercise07GetSchemas
    {
        public static Customer CustomerSchema()
        {
            var customer = ParatureSDK.ApiHandler.Customer.Schema(CredentialProvider.Creds);

            return customer;
        }

        public static Account AccountSchema()
        {
            var account = ParatureSDK.ApiHandler.Account.Schema(CredentialProvider.Creds);

            return account;
        }

        public static Article ArticleSchema()
        {
            var article = ParatureSDK.ApiHandler.Article.Schema(CredentialProvider.Creds);

            return article;
        }

        /// <summary>
        /// Get an empty ArticleFolder
        /// </summary>
        /// <returns></returns>
        public static ArticleFolder ArticleFolderSchema()
        {
            //There is an API call to retrieve an article folder schema, but there aren't any custom fields so we are not providing a method fo schema retrieval
            var articleFoler = new ArticleFolder();

            return articleFoler;
        }

    }
}
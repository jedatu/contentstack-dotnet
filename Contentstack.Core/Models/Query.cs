﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Contentstack.Core.Internals;
using Contentstack.Core.Configuration;


namespace Contentstack.Core.Models
{
    /// <summary>
    /// A class that defines a query that is used to query for Entry instance.
    /// </summary>
    public class Query
    {
        #region Private Variables

        internal Dictionary<string, object> _FormHeaders = new Dictionary<string, object>();
        private Dictionary<string, object> _Headers = new Dictionary<string, object>();
        private Dictionary<string, object> UrlQueries = new Dictionary<string, object>();
        private Dictionary<string, object> QueryValueJson = new Dictionary<string, object>();
        private string _ResultJson = string.Empty;
        private Entry[] _Result;
        private List<Dictionary<string, object>> _Schema;
        private CachePolicy _CachePolicy;
        private int _TotalCount;
        private ContentType ContentTypeInstance { get; set; }
        private bool _IsCachePolicySet;

        private string _Url
        {
            get
            {

                Config config = this.ContentTypeInstance.StackInstance.config;
                String contentTypeName = this.ContentTypeInstance.ContentTypeName;
                //String queryParam = String.Join("&",
                //UrlQueries.Select(kvp =>{
                //    return String.Format("{0}={1}", kvp.Key, kvp.Value);

                //}));
                return String.Format("{0}/content_types/{1}/entries",
                                     config.BaseUrl,
                                     contentTypeName);

            }
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Content type uid.
        /// </summary>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///         var result = queryResult.Result.ContentTypeName;
        ///     });
        /// </code>
        /// </example>
        public string ContentTypeName
        {
            get;
            set;
        }

        /// <summary>
        /// It gives json string of result.
        /// </summary>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///         var result = queryResult.Result.ResultJson;
        ///     });
        /// </code>
        /// </example>
        public string ResultJson
        {
            get
            {
                return this._ResultJson;
            }
            set
            {
                this._ResultJson = value;
            }
        }

        /// <summary>
        /// It gives the collection of retrived Entries.
        /// </summary>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///         var result = queryResult.Result.Result;
        ///     });
        /// </code>
        /// </example>
        public Entry[] Result
        {
            get
            {
                return this._Result;
            }
        }

        /// <summary>
        /// It gives total count of entries in a contentstack.io content type
        /// </summary>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///         var result = queryResult.Result.TotalCount;
        ///     });
        /// </code>
        /// </example>
        public int TotalCount
        {
            get
            {
                return this._TotalCount;
            }
        }

        /// <summary>
        /// Contains schema structure of current query result if IncludeSchema is set on Query instance.
        /// </summary>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///         var result = queryResult.Result.Schema;
        ///     });
        /// </code>
        /// </example>
        public List<Dictionary<string, object>> Schema
        {
            get
            {
                return this._Schema;
            }
        }
        #endregion

        #region Internal Constructors
        internal Query()
        {

        }
        internal Query(string contentTypeName)
        {
            this.ContentTypeName = contentTypeName;
        }
        #endregion

        #region Internal Functions
        internal static ContentstackError GetContentstackError(Exception ex)
        {
            Int32 errorCode = 0;
            string errorMessage = string.Empty;
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            ContentstackError contentstackError = new ContentstackError(ex);
            Dictionary<string, object> errors = null;
            //ContentstackError.OtherErrors errors = null;

            try
            {
                System.Net.WebException webEx = (System.Net.WebException)ex;

                using (var exResp = webEx.Response)
                using (var stream = exResp.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    errorMessage = reader.ReadToEnd();
                    JObject data = JObject.Parse(errorMessage.Replace("\r\n", ""));
                    //errorCode = ContentstackConvert.ToInt32(data.Property("error_code").Value);
                    //errorMessage = ContentstackConvert.ToString(data.Property("error_message").Value);

                    JToken token = data["error_code"];
                    if (token != null)
                        errorCode = token.Value<int>();

                    token = data["error_message"];
                    if (token != null)
                        errorMessage = token.Value<string>();

                    token = data["errors"];
                    if (token != null)
                        errors = token.ToObject<Dictionary<string, object>>();

                    var response = exResp as HttpWebResponse;
                    if (response != null)
                        statusCode = response.StatusCode;
                }
            }
            catch
            {
                errorMessage = ex.Message;
            }

            contentstackError = new ContentstackError()
            {
                ErrorCode = errorCode,
                ErrorMessage = errorMessage,
                StatusCode = statusCode,
                Errors = errors
            };

            return contentstackError;
        }
        internal void SetContentTypeInstance(ContentType contentTypeInstance)
        {
            this.ContentTypeInstance = contentTypeInstance;
            SetLanguage(Language.ENGLISH_UNITED_STATES);
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Set Language instance
        /// </summary>
        /// <param name="language">Language value</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     csQuery.SetLanguage(Language.ENGLISH_UNITED_STATES);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query SetLanguage(Language language)
        {

            try
            {
                Language languageName = language;
                int localeValue = (int)languageName;
                LanguageCode[] languageCodeValues = Enum.GetValues(typeof(LanguageCode)).Cast<LanguageCode>().ToArray();
                string localeCode = languageCodeValues[localeValue].ToString();
                localeCode = localeCode.Replace("_", "-");

                if (QueryValueJson != null && !QueryValueJson.ContainsKey("locale"))
                {
                    UrlQueries.Remove("locale");
                    UrlQueries.Add("locale", localeCode);
                } else {
                    UrlQueries["locale"] = localeCode;
                }

            }
            catch (Exception e)
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
            }


            return this;
        }

        /// <summary>
        /// To set headers for Contentstack.io Contentstack rest calls.
        /// </summary>
        /// <param name="key">header name.</param>
        /// <param name="value">header value against given header name.</param>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     csQuery.SetHeader(&quot;custom_key&quot;, &quot;custom_value&quot;);
        /// </code>
        /// </example>
        public void SetHeader(String key, String value)
        {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                _Headers.Add(key, value);
            }
        }

        /// <summary>
        /// Remove header key
        /// </summary>
        /// <param name="key">header name.</param>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     csQuery.RemoveHeader(&quot;custom_key&quot;);
        /// </code>
        /// </example>
        public void RemoveHeader(String key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                _Headers.Remove(key);
            }
        }

        /// <summary>
        /// Add a constraint to fetch all entries that contains given value against specified  key.
        /// </summary>
        /// <param name="key">field uid.</param>
        /// <param name="value">field value which get included from the response.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     csQuery.Where(&quot;uid&quot;, &quot;bltf4fbsample851db&quot;);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query Where(String key, Object value)
        {
            try
            {
                if (key != null && value != null)
                {
                    QueryValueJson.Add(key, value);
                }
                else
                {
                    throw new Exception(StackConstants.ErrorMessage_QueryFilterException, null);
                }
            }
            catch (Exception e)
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
            }

            return this;
        }

        /// <summary>
        /// Add a custom query against specified key.
        /// </summary>
        /// <param name="key">field uid.</param>
        /// <param name="value">field value.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     csQuery.AddQuery(&quot;query_param_key&quot;, &quot;query_param_value&quot;);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query AddQuery(String key, String value)
        {
            try
            {
                if (key != null && value != null)
                {
                    QueryValueJson.Add(key, value);
                }
                else
                {
                    throw new Exception(StackConstants.ErrorMessage_QueryFilterException, null);
                }
                          }
            catch (Exception e)
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
            }
            return this;
        }

        /// <summary>
        /// Remove provided query key from custom query if exist.
        /// </summary>
        /// <param name="key">Query name to remove.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     csQuery.RemoveQuery(&quot;Query_Key&quot;);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query RemoveQuery(String key)
        {
            try
            {
                if (QueryValueJson.ContainsKey(key))
                {
                    QueryValueJson.Remove(key);
                }
            }
            catch (Exception e)
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
            }
            return this;
        }

        /// <summary>
        /// Combines all the queries together using AND operator
        /// </summary>
        /// <param name="queryObjects">list of Query instances on which AND query executes.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     ContentType contentTypeObj = stack.ContentType(&quot;contentType_name&quot;);
        ///     Query csQuery = contentTypeObj.Query();
        ///     
        ///     Query query1 = contentTypeObj.Query();
        ///     query1.Where(&quot;username&quot;,&quot;something&quot;);
        ///     
        ///     Query query2 = contentTypeObj.Query();
        ///     query2.Where(&quot;email_address&quot;,&quot;something@email.com&quot;);
        ///     
        ///     List&lt;Query&gt; queryList = new List&lt;Query&gt;();
        ///     queryList.Add(query1);
        ///     queryList.Add(query2);
        ///     
        ///     csQuery.And(queryList);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query And(List<Query> queryObjects)
        {
            if (queryObjects != null && queryObjects.Count > 0)
            {
                try
                {
                    List<Dictionary<string, object>> andValueJson = new List<Dictionary<string, object>>();
                    int count = queryObjects.Count;

                    for (int i = 0; i < count; i++) {
                        andValueJson.Add(queryObjects[i].QueryValueJson);
                        //if(queryObjects[i].QueryValueJson.ContainsKey("locale")){
                        //    queryObjects[i].QueryValueJson.Remove("locale");
                        //}
                    }
                    if (QueryValueJson.ContainsKey(StackConstants.And)) {
                        QueryValueJson.Remove(StackConstants.And);
                        QueryValueJson.Add(StackConstants.And, andValueJson);
                    } else {
                        QueryValueJson.Add(StackConstants.And, andValueJson);
                    }

                }
                catch (Exception e)
                {
                    throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
                }
            }
            else
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, null);
            }

            return this;
        }

        /// <summary>
        /// Add a constraint to fetch all entries which satisfy any queries.
        /// </summary>
        /// <param name="queryObjects">list of Query instances on which OR query executes.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     ContentType contentTypeObj = stack.ContentType(&quot;contentType_name&quot;);
        ///     Query csQuery = contentTypeObj.Query();
        ///     
        ///     Query query1 = contentTypeObj.Query();
        ///     query1.Where(&quot;username",&quot;something&quot;);
        ///     
        ///     Query query2 = contentTypeObj.Query();
        ///     query2.Where(&quot;email_address&quot;,&quot;something@email.com&quot;);
        ///     
        ///     List&lt;Query&gt; queryList = new List&lt;Query&gt;();
        ///     queryList.Add(query1);
        ///     queryList.Add(query2);
        ///     
        ///     csQuery.Or(queryList);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query Or(List<Query> queryObjects)
        {
            if (queryObjects != null && queryObjects.Count > 0)
            {
                try
                {
                    List<Dictionary<string, object>> orValueJson = new List<Dictionary<string, object>>();
                    int count = queryObjects.Count;

                    for (int i = 0; i < count; i++)
                    {
                        var l = queryObjects[i].QueryValueJson;
                        orValueJson.Add(queryObjects[i].QueryValueJson);
                        //if (queryObjects[i].QueryValueJson.ContainsKey("locale")) {
                        //    queryObjects[i].QueryValueJson.Remove("locale");
                        //}
                    }

                    if (QueryValueJson.ContainsKey(StackConstants.Or)) {
                        QueryValueJson.Remove(StackConstants.Or);
                        QueryValueJson.Add(StackConstants.Or, orValueJson);
                    } else {
                        QueryValueJson.Add(StackConstants.Or, orValueJson);
                    }

                }
                catch (Exception e)
                {
                    throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
                }
            }
            else
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, null);
            }

            return this;
        }

        /// <summary>
        /// Add a constraint to the query that requires a particular key entry to be less than the provided value.
        /// </summary>
        /// <param name="key">the key to be constrained.</param>
        /// <param name="value">the value that provides an upper bound.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.LessThan(&quot;due_date&quot;, &quot;2013-06-25T00:00:00+05:30&quot;);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query LessThan(String key, Object value)
        {
            if (key != null && value != null)
            {
                try
                {
                    Dictionary<string, object> queryValue = new Dictionary<string, object>();
                    //if (QueryValueJson.ContainsKey(key))
                    //{
                    queryValue.Add("$lt", value);
                    QueryValueJson.Add(key, queryValue);
                    //}

                }
                catch (Exception e)
                {
                    throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
                }
            }
            else
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, null);
            }

            return this;
        }

        /// <summary>
        /// Add a constraint to the query that requires a particular key entry to be less than or equal to the provided value.
        /// </summary>
        /// <param name="key">the key to be constrained.</param>
        /// <param name="value">the value that provides an upper bound or equal.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.LessThanOrEqualTo(&quot;due_date&quot;, &quot;2013-06-25T00:00:00+05:30&quot;);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query LessThanOrEqualTo(String key, Object value)
        {

            if (key != null && value != null)
            {

                try
                {
                    Dictionary<string, object> queryValue = new Dictionary<string, object>();
                    //if (QueryValueJson.ContainsKey(key))
                    //{
                    queryValue.Add("$lte", value);
                    QueryValueJson.Add(key, queryValue);
                    //}
                }
                catch (Exception e)
                {
                    throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
                }
            }
            else
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, null);
            }
            return this;
        }

        /// <summary>
        /// Add a constraint to the query that requires a particular key entry to be greater than the provided value.
        /// </summary>
        /// <param name="key">the key to be constrained.</param>
        /// <param name="value">The value that provides an lower bound.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.GreaterThan(&quot;due_date&quot;, &quot;2013-06-25T00:00:00+05:30&quot;);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query GreaterThan(String key, Object value)
        {

            if (key != null && value != null)
            {
                try
                {
                    Dictionary<string, object> queryValue = new Dictionary<string, object>();
                    //if (QueryValueJson.ContainsKey(key))
                    //{
                    queryValue.Add("$gt", value);
                    QueryValueJson.Add(key, queryValue);
                    //}
                }
                catch (Exception e)
                {
                    throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
                }
            }
            else
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, null);
            }

            return this;
        }

        /// <summary>
        /// Add a constraint to the query that requires a particular key entry to be greater than or equal to the provided value.
        /// </summary>
        /// <param name="key">the key to be constrained.</param>
        /// <param name="value">The value that provides an lower bound or equal</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.GreaterThanOrEqualTo(&quot;due_date&quot;, &quot;2013-06-25T00:00:00+05:30&quot;);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query GreaterThanOrEqualTo(String key, Object value)
        {

            if (key != null && value != null)
            {
                try
                {
                    Dictionary<string, object> queryValue = new Dictionary<string, object>();
                    //if (QueryValueJson.ContainsKey(key))
                    //{
                    queryValue.Add("$gte", value);
                    QueryValueJson.Add(key, queryValue);
                    //}
                }
                catch (Exception e)
                {
                    throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
                }
            }
            else
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, null);
            }

            return this;
        }

        /// <summary>
        /// Add a constraint to the query that requires a particular key&#39;s entry to be not equal to the provided value.
        /// </summary>
        /// <param name="key">the key to be constrained.</param>
        /// <param name="value">The object that must not be equaled.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.NotEqualTo(&quot;due_date&quot;, &quot;2013-06-25T00:00:00+05:30&quot;);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query NotEqualTo(String key, Object value)
        {

            if (key != null && value != null)
            {

                try
                {
                    Dictionary<string, object> queryValue = new Dictionary<string, object>();
                    //if (QueryValueJson.ContainsKey(key))
                    //{
                    queryValue.Add("$ne", value);
                    QueryValueJson.Add(key, queryValue);
                    //}
                }
                catch (Exception e)
                {
                    throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
                }

            }
            else
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, null);
            }

            return this;

        }

        /// <summary>
        /// Add a constraint to the query that requires a particular key&#39;s entry to be contained in the provided array.
        /// </summary>
        /// <param name="key">the key to be constrained.</param>
        /// <param name="values">The possible values for the key&#39;s object.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.ContainedIn(&quot;severity&quot;, new Object[]{&quot;Show Stopper&quot;, &quot;Critical&quot;});
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query ContainedIn(String key, Object[] values)
        {

            if (key != null && values != null)
            {
                try
                {
                    List<object> valuesArray = new List<object>();
                    int count = values.Length;

                    for (int i = 0; i < count; i++)
                    {
                        valuesArray.Add(values[i]);
                    }
                    Dictionary<string, object> queryValue = new Dictionary<string, object>();
                    queryValue.Add("$in", valuesArray);
                    QueryValueJson.Add(key, queryValue);

                }
                catch (Exception e)
                {
                    throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
                }
            }
            else
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, null);
            }

            return this;
        }

        /// <summary>
        /// Add a constraint to the query that requires a particular key entry&#39;s value not be contained in the provided array.
        /// </summary>
        /// <param name="key">the key to be constrained.</param>
        /// <param name="values">The list of values the key object should not be.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.NotContainedIn(&quot;severity&quot;, new Object[]{&quot;Show Stopper&quot;, &quot;Critical&quot;});
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query NotContainedIn(String key, Object[] values)
        {

            if (key != null && values != null)
            {
                try
                {
                    List<object> valuesArray = new List<object>();
                    int count = values.Length;

                    for (int i = 0; i < count; i++)
                    {
                        valuesArray.Add(values[i]);
                    }
                    Dictionary<string, object> queryValue = new Dictionary<string, object>();
                    queryValue.Add("$nin", valuesArray);
                    QueryValueJson.Add(key, queryValue);
                }
                catch (Exception e)
                {
                    throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
                }
            }
            else
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, null);
            }

            return this;

        }

        /// <summary>
        /// Add a constraint that requires, a specified key exists in response.
        /// </summary>
        /// <param name="key">The key to be constrained.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.Exists(&quot;severity&quot;);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query Exists(String key)
        {

            if (key != null)
            {
                try
                {
                    Dictionary<string, object> queryValue = new Dictionary<string, object>();
                    queryValue.Add("$exists", true);
                    QueryValueJson.Add(key, queryValue);

                }
                catch (Exception e)
                {
                    throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
                }
            }
            else
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, null);
            }

            return this;
        }

        /// <summary>
        /// Add a constraint that requires, a specified key does not exists in response.
        /// </summary>
        /// <param name="key">The key to be constrained.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.NotExists(&quot;severity&quot;);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query NotExists(String key)
        {

            if (key != null)
            {
                try
                {

                    Dictionary<string, object> queryValue = new Dictionary<string, object>();
                    queryValue.Add("$exists", false);
                    QueryValueJson.Add(key, queryValue);
                }
                catch (Exception e)
                {
                    throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
                }
            }
            else
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, null);
            }

            return this;
        }

        /// <summary>
        /// Add a constraint that requires a particular reference key details.
        /// </summary>
        /// <param name="filed_uid">The key to be constrained.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.IncludeReference(&quot;for_bug&quot;);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query IncludeReference(String filed_uid)
        {

            if (filed_uid != null && filed_uid.Length > 0)
            {
                UrlQueries.Add("include[]", filed_uid);
            }
            return this;
        }


        /// <summary>
        /// Add a constraint that requires a particular reference key details.
        /// </summary>
        /// <param name="filed_uids">array keys that to be constrained.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.IncludeReference(new String[]{&quot;for_bug&quot;, &quot;assignee&quot;});
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query IncludeReference(String[] filed_uids)
        {
            if (filed_uids != null && filed_uids.Length > 0)
            {
                UrlQueries.Add("include[]", filed_uids);
            }
            return this;
        }


        /// <summary>
        /// Include tags with which to search entries.
        /// </summary>
        /// <param name="tags">Comma separated array of tags with which to search entries.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.Tags(new String[]{&quot;tag1&quot;, &quot;tag2&quot;});
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        private Query Tags(String[] tags) {
            try {
                if (tags != null) {

                    String tagsvalue = null;
                    int count = tags.Length;
                    for (int i = 0; i < count; i++) {
                        tagsvalue = tagsvalue + "," + tags[i];
                    }
                    QueryValueJson.Add("tags", tagsvalue);
                } else {
                    throw new Exception(StackConstants.ErrorMessage_QueryFilterException, null);
                }
            } catch (Exception e) {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
            }
            return this;
        }

        /// <summary>
        /// Sort the results in ascending order with the given key.
        /// </summary>
        /// <param name="key">The key to order by.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.Ascending(&quot;name&quot;);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query Ascending(String key)
        {
            if (key != null) {
                try {
                    UrlQueries.Add("asc", key);
                } catch (Exception e) {
                    throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
                }
            } else {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, null);
            }
            return this;
        }

        /// <summary>
        /// Sort the results in descending order with the given key.
        /// </summary>
        /// <param name="key">The key to order by.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.Descending(&quot;name&quot;);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query Descending(String key)
        {
            if (key != null)
            {
                try
                {
                    UrlQueries.Add("desc", key);
                }
                catch (Exception e)
                {
                    throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
                }
            }
            else
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, null);
            }
            return this;
        }

        /// <summary>
        /// Retrieve only count of entries in result.
        /// </summary>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.Count();
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query Count()
        {
            try
            {
                UrlQueries.Add("count", "true");
            }
            catch (Exception e)
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
            }
            return this;
        }

        /// <summary>
        /// Retrieve count and data of objects in result.
        /// </summary>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.IncludeCount();
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query IncludeCount()
        {
            try
            {
                UrlQueries.Add("include_count", "true");
            }
            catch (Exception e)
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
            }
            return this;
        }

        /// <summary>
        /// Specifies an array of &#39;only&#39; keys in BASE object that would be &#39;included&#39; in the response.
        /// </summary>
        /// <param name="fieldUid">Array of the &#39;only&#39; keys to be included in response.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.Only(new String[]{&quot;name&quot;, &quot;description&quot;});
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query Only(String[] fieldUid) {
            try {
                if (fieldUid != null && fieldUid.Length > 0)
                {
                    UrlQueries.Add("only[BASE][]", fieldUid);
                }
            } catch {
                //CSAppUtils.showLog(TAG, "--include Reference-catch|" + e);
            }

            return this;
        }

        /// <summary>
        /// Specifies list of field uids that would be excluded from the response.
        /// </summary>
        /// <param name="fieldUids">field uid  which get excluded from the response.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.Except(new String[]{&quot;name&quot;, &quot;description&quot;});
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query Except(String[] fieldUids) {
            try {
                List<string> objectUidForExcept = new List<string>();
                Dictionary<string, object> exceptValueJson = new Dictionary<string, object>();
                if (fieldUids != null && fieldUids.Length > 0) {
                    UrlQueries.Add("except[BASE][]", fieldUids);
                }
            } catch  {
                //CSAppUtils.showLog(TAG, "--include Reference-catch|" + e);
            }
            return this;
        }

        /// <summary>
        /// Include schemas of all returned objects along with objects themselves.
        /// </summary>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.IncludeSchema();
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query IncludeSchema()
        {
            try {
                UrlQueries.Add("include_schema", true);
            } catch (Exception e) {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
            }
            return this;
        }

        
        /// <summary>
        /// Include object owner&#39;s profile in the objects data.
        /// </summary>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.IncludeOwner();
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        private Query IncludeOwner()
        {
            try
            {
                UrlQueries.Add("include_owner", true);
            }
            catch (Exception e)
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
            }
            return this;
        }
        

       
        private Query IncludeDraft()
        {
            try
            {
                UrlQueries.Add("include_draft", true);
            }
            catch (Exception e)
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
            }
            return this;
        }

        /// <summary>
        /// The number of objects to skip before returning any.
        /// </summary>
        /// <param name="number">No of objects to skip from returned objects.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.Skip(2);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query Skip(int number)
        {
            try
            {
                UrlQueries.Add("skip", number);
            }
            catch (Exception e)
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
            }
            return this;
        }

        /// <summary>
        /// A limit on the number of objects to return.
        /// </summary>
        /// <param name="number">No of objects to limit.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.Limit(2);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query Limit(int number)
        {
            try
            {
                UrlQueries.Add("limit", number);
            }
            catch (Exception e)
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
            }
            return this;
        }

        /// <summary>
        /// Add a regular expression constraint for finding string values that match the provided regular expression.
        /// </summary>
        /// <param name="key">The key to be constrained.</param>
        /// <param name="regex">The regular expression pattern to match.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.Regex(&quot;name&quot;, &quot;^browser&quot;);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query Regex(String key, String regex)
        {
            if (key != null && regex != null)
            {

                try
                {
                    Dictionary<string, object> queryValue = new Dictionary<string, object>();
                    queryValue.Add("$regex", regex);

                    QueryValueJson.Add(key, queryValue);
                }
                catch (Exception e)
                {
                    throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
                }
            }
            else
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, null);
            }
            return this;
        }

        /// <summary>
        /// Add a regular expression constraint for finding string values that match the provided regular expression.
        /// </summary>
        /// <param name="key">The key to be constrained.</param>
        /// <param name="regex">The regular expression pattern to match.</param>
        /// <param name="modifiers">Any of the following supported Regular expression modifiers.
        /// 				&lt;li&gt;use&lt;b&gt; i &lt;/b&gt; for case-insensitive matching.&lt;/li&gt;
        ///					&lt;li&gt;use&lt;b&gt; m &lt;/b&gt; for making dot match newlines.&lt;/li&gt;
        ///					&lt;li&gt;use&lt;b&gt; x &lt;/b&gt; for ignoring whitespace in regex&lt;/li&gt;</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.Regex(&quot;name&quot;, &quot;^browser&quot;, &quot;i&quot;);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query Regex(String key, String regex, String modifiers)
        {
            if (key != null && regex != null)
            {

                try
                {
                    Dictionary<string, object> queryValue = new Dictionary<string, object>();
                    queryValue.Add("$regex", regex);
                    if (modifiers != null)
                    {
                        queryValue.Add("$options", modifiers);
                    }
                    QueryValueJson.Add(key, queryValue);

                }
                catch (Exception e)
                {
                    throw new Exception(StackConstants.ErrorMessage_QueryFilterException, e);
                }
            }
            else
            {
                throw new Exception(StackConstants.ErrorMessage_QueryFilterException, null);
            }
            return this;
        }

        /// <summary>
        /// Include tags with which to search entries.
        /// </summary>
        /// <param name="tags">Comma separated array of tags with which to search entries.</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.WhereTags(new String[]{&quot;tag1&quot;, &quot;tag2&quot;});
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query WhereTags(string[] tags)
        {
            if (tags != null && tags.Length > 0)
            {

                UrlQueries.Add("tags", tags);
            }

            return this;
        }



        /// <summary>
        /// To set cache policy using query instance.
        /// </summary>
        /// <param name="cachePolicy">CachePolicy instance</param>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.SetCachePolicy(CachePolicy.CacheElseNetwork);
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public Query SetCachePolicy(CachePolicy cachePolicy)
        {
            this._CachePolicy = cachePolicy;
            this._IsCachePolicySet = true;
            return this;
        }

        /// <summary>
        /// Execute a Query and Caches its result (Optional)
        /// </summary>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.Find().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public async Task<Query> Find()
        {
            await Exec();
            return this;
        }

        /// <summary>
        /// Execute a Query and Caches its result (Optional)
        /// </summary>
        /// <returns>Current instance of Query, this will be useful for a chaining calls.</returns>
        /// <example>
        /// <code>
        ///     //&quot;blt5d4sample2633b&quot; is a dummy Stack API key
        ///     //&quot;blt6d0240b5sample254090d&quot; is dummy access token.
        ///     ContentstackClient stack = new ContentstackClinet(&quot;blt5d4sample2633b&quot;, &quot;blt6d0240b5sample254090d&quot;, &quot;stag&quot;);
        ///     Query csQuery = stack.ContentType(&quot;contentType_name&quot;).Query();
        ///     
        ///     csQuery.FindOne().ContinueWith((queryResult) =&gt; {
        ///         //Your callback code.
        ///     });
        /// </code>
        /// </example>
        public async Task<Query> FindOne()
        {
            if (UrlQueries != null && UrlQueries.ContainsKey("limit"))
            {
                UrlQueries["limit"] = 1;
            }
            else
            {
                UrlQueries.Add("limit", 1);
            }
            await Exec();
            return this;
        }

        #endregion
        private async Task<Query> Exec()
        {
            Dictionary<String, Object> headers = GetHeader(_Headers);
            Dictionary<String, object> headerAll = new Dictionary<string, object>();
            Dictionary<string, object> mainJson = new Dictionary<string, object>();

            //Dictionary<string, object> urlQueries = new Dictionary<string, object>();
            if (headers != null && headers.Count() > 0) {
                foreach (var header in headers) {
                    headerAll.Add(header.Key, (String)header.Value);
                }

                if (headers.ContainsKey("environment")) {
                    UrlQueries.Add("environment", headers["environment"]);
                } else if (headers.ContainsKey("environment_uid")) {
                    UrlQueries.Add("environment_uid", headers["environment_uid"]);
                } else {
                    mainJson.Add("environment",this.ContentTypeInstance.StackInstance.config.Environment);
                }
            }

            if (QueryValueJson != null && QueryValueJson.Count > 0)
                mainJson.Add("query", QueryValueJson);

            foreach (var kvp in UrlQueries) {
                mainJson.Add(kvp.Key, kvp.Value);
            }


            try {
                //String mainStringForMD5 = Url + JsonConvert.SerializeObject(mainJson) + JsonConvert.SerializeObject(headers);
                //String md5Value = ContentstackConvert.GetMD5FromString(mainStringForMD5.Trim());

                //string CacheFilePath = Path.Combine(ContentstackConstants.Instance.CacheFolderName, md5Value);
                CachePolicy cachePolicy = CachePolicy.NetworkOnly;

                if (_IsCachePolicySet) {
                    cachePolicy = _CachePolicy;
                }

                switch (cachePolicy) {
                    //case CachePolicy.IgnoreCache:

                        //HTTPRequestHandler contentstackRequestHandler = new HTTPRequestHandler();
                        //var result = await contentstackRequestHandler.ProcessRequest(Url, headers, mainJson);
                        ////if (IsValidJson(result))
                        ////{
                        //StackOutput contentstackOutput = new StackOutput(ContentstackConvert.ToString(result, "{}"));
                        //await GetOutputAsync(contentstackOutput);
                        ////Console.WriteLine(contentstackOutput);
                        //break;
                    //}

                    case CachePolicy.NetworkOnly:
                        HTTPRequestHandler requestHandler = new HTTPRequestHandler();
                        var output = await requestHandler.ProcessRequest(_Url, headers, mainJson);
                        StackOutput stackOutput = new StackOutput(ContentstackConvert.ToString(output, "{}"));
                        await GetOutputAsync(stackOutput);
                        break;

                    //case CachePolicy.CacheOnly:
                    //    var outputResult = FetchFromCache(CacheFilePath);
                    //    StackOutput stackOutputResult = new StackOutput(ContentstackConvert.ToString(outputResult, "{}"));
                    //    await GetOutputAsync(stackOutputResult);
                    //    break;

                    //case CachePolicy.CacheElseNetwork:
                    //    if (File.Exists(CacheFilePath))
                    //    {
                    //        var cacheResult = FetchFromCache(CacheFilePath);
                    //        StackOutput stackOutputCacheResult = new StackOutput(ContentstackConvert.ToString(cacheResult, "{}"));
                    //        Dictionary<string, object> getOutPutdictionary = (Dictionary<string, object>)stackOutputCacheResult.Output;
                    //        if (ContentstackConvert.GetResponseTimeFromCacheFile(CacheFilePath, (long)getOutPutdictionary["timestamp"], TimeSpan.TicksPerHour * 24))
                    //        {
                    //            HTTPRequestHandler cacherequestHandler = new HTTPRequestHandler();
                    //            var CacheOutputResult = await cacherequestHandler.ProcessRequest(Url, headers, mainJson, CacheFilePath);
                    //            StackOutput CacheStackOutput = new StackOutput(ContentstackConvert.ToString(CacheOutputResult, "{}"));
                    //            //Entry resultObject = new Entry();
                    //            await GetOutputAsync(CacheStackOutput);
                    //        }
                    //        else
                    //        {
                    //            await GetOutputAsync(stackOutputCacheResult);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        HTTPRequestHandler request_handler = new HTTPRequestHandler();
                    //        var output_result = await request_handler.ProcessRequest(Url, headers, mainJson, CacheFilePath);
                    //        StackOutput stack_output = new StackOutput(ContentstackConvert.ToString(output_result, "{}"));
                    //        await GetOutputAsync(stack_output);
                    //        //Console.WriteLine(stack_output);
                    //    }
                    //    break;
                    //case CachePolicy.NetworkElseCache:
                    //    if (ContentstackConvert.IsNetworkAvailable())
                    //    {
                    //        HTTPRequestHandler request_handler = new HTTPRequestHandler();
                    //        var output_result = await request_handler.ProcessRequest(Url, headers, mainJson, CacheFilePath);
                    //        StackOutput stack_output = new StackOutput(ContentstackConvert.ToString(output_result, "{}"));
                    //        await GetOutputAsync(stack_output);
                    //        //Console.WriteLine(stack_output);
                    //    }
                    //    else
                    //    {
                    //        var outputresult = FetchFromCache(CacheFilePath);
                    //        StackOutput stackoutputResult = new StackOutput(ContentstackConvert.ToString(outputresult, "{}"));
                    //        await GetOutputAsync(stackoutputResult);
                    //    }
                    //    break;

                    //case CachePolicy.CacheThenNetwork:
                        //if (File.Exists(CacheFilePath))
                        //{
                        //    var outputresult = FetchFromCache(CacheFilePath);
                        //    StackOutput stackoutputResult = new StackOutput(ContentstackConvert.ToString(outputresult, "{}"));
                        //    await GetOutputAsync(stackoutputResult);
                        //}
                        //HTTPRequestHandler request = new HTTPRequestHandler();
                        //var _result = await request.ProcessRequest(Url, headers, mainJson, CacheFilePath);
                        //StackOutput _output = new StackOutput(ContentstackConvert.ToString(_result, "{}"));
                        //await GetOutputAsync(_output);
                        ////Console.WriteLine(_output);
                        //break;

                    default:
                        break;
                }
                return this;
            }
            catch (Exception ex)
            {
                throw GetContentstackError(ex);
            }

        }

        #region Private Functions
        private Dictionary<string, object> GetHeader(Dictionary<string, object> localHeader)
        {
            Dictionary<string, object> mainHeader = _FormHeaders;
            Dictionary<string, object> classHeaders = new Dictionary<string, object>();

            if (localHeader != null && localHeader.Count > 0)
            {
                if (mainHeader != null && mainHeader.Count > 0)
                {
                    foreach (var entry in localHeader)
                    {
                        String key = entry.Key;
                        classHeaders.Add(key, entry.Value);
                    }

                    foreach (var entry in mainHeader)
                    {
                        String key = entry.Key;
                        if (!classHeaders.ContainsKey(key))
                        {
                            classHeaders.Add(key, entry.Value);
                        }
                    }

                    return classHeaders;

                }
                else
                {
                    return localHeader;
                }

            }
            else
            {
                return _FormHeaders;
            }
        }
        //private string FetchFromCache(string CacheFileName)
        //{
        //    if (File.Exists(CacheFileName))
        //    {
        //        var cacheResult = ContentstackConvert.GetJsonFromCacheFile(CacheFileName);
        //        //var deserializeHson = JsonConvert.DeserializeObject(cacheResult);

        //        StackOutput stackOutputCacheResult = new StackOutput(ContentstackConvert.ToString(cacheResult, "{}"));
        //        Dictionary<string, object> getOutPutdictionary = (Dictionary<string, object>)stackOutputCacheResult.Output;
        //        if (ContentstackConvert.GetResponseTimeFromCacheFile(CacheFileName, (long)getOutPutdictionary["timestamp"], TimeSpan.TicksPerHour * 24))
        //        {
        //            throw new Exception(StackConstants.ErrorMessage_EntryNotFoundInCache);
        //        }
        //        else
        //        {
        //            // await GetOutputAsync(stackOutputCacheResult);
        //            return cacheResult;
        //        }


        //    }
        //    else
        //    {
        //        throw new Exception(StackConstants.ErrorMessage_EntryNotFoundInCache);
        //    }
        //}
        
        private Task<Query> GetOutputAsync(StackOutput output)
        {
            try
            {
                return Task<Query>.Run(() =>
                {


                    List<Entry> lstEntryObject = new List<Entry>();
                    object[] result = (object[])output.Objects;
                    object[] schema = (object[])output.Schema;
                    List<Dictionary<string, object>> lstSchema = new List<Dictionary<string, object>>();
                    if (result != null && result.Count() > 0)
                    {
                        foreach (var item in result)
                        {
                            Entry entry = new Entry();
                            lstEntryObject.Add(entry.ParseObject((Dictionary<string, object>)item,this._Url));
                        }
                        this._Result = lstEntryObject.ToArray();
                    }
                    this._TotalCount = output.TotalCount;
                    if (schema != null && schema.Count() > 0)
                    {
                        foreach (var item in schema)
                        {
                            lstSchema.Add((Dictionary<string, object>)item);
                        }
                        this._Schema = lstSchema;
                    }

                    this.ResultJson = output.Json;

                    return this;
                });
            }
            catch (Exception ex)
            {
                throw new ContentstackError(ex);
            }
        }
        #endregion
    }
}



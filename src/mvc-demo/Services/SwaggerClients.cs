﻿//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v5.2.6075.31152 (http://NSwag.org)
// </auto-generated>
//----------------------

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace mvc_demo.Services
{
#pragma warning disable // Disable all warnings

    [GeneratedCode("NSwag", "5.2.6075.31152")]
    public partial class Client
    {
        public Client() : this("") { }

        public Client(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        partial void PrepareRequest(HttpClient request, ref string url);

        partial void ProcessResponse(HttpClient request, HttpResponseMessage response);

        public string BaseUrl { get; set; }

        /// <returns>Success</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        public Task<ObservableCollection<Person>> ApiPeopleGetAsync()
        {
            return ApiPeopleGetAsync(CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        public async Task<ObservableCollection<Person>> ApiPeopleGetAsync(CancellationToken cancellationToken)
        {
            var url_ = string.Format("{0}/{1}?", BaseUrl, "api/People");

            var client_ = new HttpClient();
            PrepareRequest(client_, ref url_);

            var response_ = await client_.GetAsync(url_, cancellationToken).ConfigureAwait(false);
            ProcessResponse(client_, response_);

            var responseData_ = await response_.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var status_ = ((int)response_.StatusCode).ToString();

            if (status_ == "200")
            {
                var result_ = default(ObservableCollection<Person>);
                try
                {
                    if (responseData_.Length > 0)
                        result_ = JsonConvert.DeserializeObject<ObservableCollection<Person>>(Encoding.UTF8.GetString(responseData_, 0, responseData_.Length));
                    return result_;
                }
                catch (Exception exception)
                {
                    throw new SwaggerException("Could not deserialize the response body.", status_, responseData_, exception);
                }
            }
            else
            {
            }

            throw new SwaggerException("The HTTP status code of the response was not expected (" + (int)response_.StatusCode + ").", status_, responseData_, null);
        }

        /// <returns>Success</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        public Task<ObservableCollection<string>> ApiValuesGetAsync()
        {
            return ApiValuesGetAsync(CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        public async Task<ObservableCollection<string>> ApiValuesGetAsync(CancellationToken cancellationToken)
        {
            var url_ = string.Format("{0}/{1}?", BaseUrl, "api/Values");

            var client_ = new HttpClient();
            PrepareRequest(client_, ref url_);

            var response_ = await client_.GetAsync(url_, cancellationToken).ConfigureAwait(false);
            ProcessResponse(client_, response_);

            var responseData_ = await response_.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var status_ = ((int)response_.StatusCode).ToString();

            if (status_ == "200")
            {
                var result_ = default(ObservableCollection<string>);
                try
                {
                    if (responseData_.Length > 0)
                        result_ = JsonConvert.DeserializeObject<ObservableCollection<string>>(Encoding.UTF8.GetString(responseData_, 0, responseData_.Length));
                    return result_;
                }
                catch (Exception exception)
                {
                    throw new SwaggerException("Could not deserialize the response body.", status_, responseData_, exception);
                }
            }
            else
            {
            }

            throw new SwaggerException("The HTTP status code of the response was not expected (" + (int)response_.StatusCode + ").", status_, responseData_, null);
        }

        /// <returns>Success</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        public Task ApiValuesPostAsync(string value)
        {
            return ApiValuesPostAsync(value, CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        public async Task ApiValuesPostAsync(string value, CancellationToken cancellationToken)
        {
            var url_ = string.Format("{0}/{1}?", BaseUrl, "api/Values");

            var client_ = new HttpClient();
            PrepareRequest(client_, ref url_);

            var content_ = new StringContent(JsonConvert.SerializeObject(value));
            content_.Headers.ContentType.MediaType = "application/json";

            var response_ = await client_.PostAsync(url_, content_, cancellationToken).ConfigureAwait(false);
            ProcessResponse(client_, response_);

            var responseData_ = await response_.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var status_ = ((int)response_.StatusCode).ToString();

            if (status_ == "200")
            {
                return;

            }
            else
            {
            }

            throw new SwaggerException("The HTTP status code of the response was not expected (" + (int)response_.StatusCode + ").", status_, responseData_, null);
        }

        /// <returns>Success</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        public Task<string> ApiValuesByIdGetAsync(int id)
        {
            return ApiValuesByIdGetAsync(id, CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        public async Task<string> ApiValuesByIdGetAsync(int id, CancellationToken cancellationToken)
        {
            var url_ = string.Format("{0}/{1}?", BaseUrl, "api/Values/{id}");

            if (id == null)
                throw new ArgumentNullException("id");
            url_ = url_.Replace("{id}", Uri.EscapeUriString(id.ToString()));

            var client_ = new HttpClient();
            PrepareRequest(client_, ref url_);

            var response_ = await client_.GetAsync(url_, cancellationToken).ConfigureAwait(false);
            ProcessResponse(client_, response_);

            var responseData_ = await response_.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var status_ = ((int)response_.StatusCode).ToString();

            if (status_ == "200")
            {
                var result_ = default(string);
                try
                {
                    if (responseData_.Length > 0)
                        result_ = JsonConvert.DeserializeObject<string>(Encoding.UTF8.GetString(responseData_, 0, responseData_.Length));
                    return result_;
                }
                catch (Exception exception)
                {
                    throw new SwaggerException("Could not deserialize the response body.", status_, responseData_, exception);
                }
            }
            else
            {
            }

            throw new SwaggerException("The HTTP status code of the response was not expected (" + (int)response_.StatusCode + ").", status_, responseData_, null);
        }

        /// <returns>Success</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        public Task ApiValuesByIdPutAsync(int id, string value)
        {
            return ApiValuesByIdPutAsync(id, value, CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        public async Task ApiValuesByIdPutAsync(int id, string value, CancellationToken cancellationToken)
        {
            var url_ = string.Format("{0}/{1}?", BaseUrl, "api/Values/{id}");

            if (id == null)
                throw new ArgumentNullException("id");
            url_ = url_.Replace("{id}", Uri.EscapeUriString(id.ToString()));

            var client_ = new HttpClient();
            PrepareRequest(client_, ref url_);

            var content_ = new StringContent(JsonConvert.SerializeObject(value));
            content_.Headers.ContentType.MediaType = "application/json";

            var response_ = await client_.PutAsync(url_, content_, cancellationToken).ConfigureAwait(false);
            ProcessResponse(client_, response_);

            var responseData_ = await response_.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var status_ = ((int)response_.StatusCode).ToString();

            if (status_ == "200")
            {
                return;

            }
            else
            {
            }

            throw new SwaggerException("The HTTP status code of the response was not expected (" + (int)response_.StatusCode + ").", status_, responseData_, null);
        }

        /// <returns>Success</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        public Task ApiValuesByIdDeleteAsync(int id)
        {
            return ApiValuesByIdDeleteAsync(id, CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        public async Task ApiValuesByIdDeleteAsync(int id, CancellationToken cancellationToken)
        {
            var url_ = string.Format("{0}/{1}?", BaseUrl, "api/Values/{id}");

            if (id == null)
                throw new ArgumentNullException("id");
            url_ = url_.Replace("{id}", Uri.EscapeUriString(id.ToString()));

            var client_ = new HttpClient();
            PrepareRequest(client_, ref url_);

            var response_ = await client_.DeleteAsync(url_, cancellationToken).ConfigureAwait(false);
            ProcessResponse(client_, response_);

            var responseData_ = await response_.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var status_ = ((int)response_.StatusCode).ToString();

            if (status_ == "200")
            {
                return;

            }
            else
            {
            }

            throw new SwaggerException("The HTTP status code of the response was not expected (" + (int)response_.StatusCode + ").", status_, responseData_, null);
        }

    }



    [JsonObject(MemberSerialization.OptIn)]
    [GeneratedCode("NJsonSchema", "4.1.6075.30950")]
    public partial class Person : INotifyPropertyChanged
    {
        private string _firstName;
        private int? _id;
        private string _lastName;

        [JsonProperty("firstName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (_firstName != value)
                {
                    _firstName = value;
                    RaisePropertyChanged();
                }
            }
        }

        [JsonProperty("id", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    RaisePropertyChanged();
                }
            }
        }

        [JsonProperty("lastName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (_lastName != value)
                {
                    _lastName = value;
                    RaisePropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Person FromJson(string data)
        {
            return JsonConvert.DeserializeObject<Person>(data);
        }

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [GeneratedCode("NSwag", "5.2.6075.31152")]
    public class SwaggerException : Exception
    {
        public string StatusCode { get; private set; }

        public byte[] ResponseData { get; private set; }

        public SwaggerException(string message, string statusCode, byte[] responseData, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ResponseData = responseData;
        }

        public override string ToString()
        {
            return string.Format("HTTP Response: n{0}n{1}", Encoding.UTF8.GetString(ResponseData), base.ToString());
        }
    }

    [GeneratedCode("NSwag", "5.2.6075.31152")]
    public class SwaggerException<TResponse> : SwaggerException
    {
        public TResponse Response { get; private set; }

        public SwaggerException(string message, string statusCode, byte[] responseData, TResponse response, Exception innerException)
            : base(message, statusCode, responseData, innerException)
        {
            Response = response;
        }
    }

}
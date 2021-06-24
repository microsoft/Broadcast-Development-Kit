using Application.Exceptions;
using Application.Exceptions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Core.Common.Extensions
{
    public static class HttpExtensions
    {
        /// <summary>
        /// Creates the request message asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Http Request Message.</returns>
        public static HttpRequestMessage CreateRequestMessage(this HttpRequest request)
        {
            var displayUri = request.GetDisplayUrl();
            var httpRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(displayUri),
                Method = new HttpMethod(request.Method),
            };

            if (request.ContentLength.HasValue && request.ContentLength.Value > 0)
            {
                httpRequest.Content = new StreamContent(request.Body);
            }

            // Copy headers
            foreach (var header in request.Headers)
            {
                httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            return httpRequest;
        }

        /// <summary>
        /// Creates the HTTP response.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="httpResponse">The HTTP response.</param>
        /// <returns>The populated <see cref="HttpResponse"/>.</returns>
        public static async Task<HttpResponse> CreateHttpResponseAsync(this HttpResponseMessage response, HttpResponse httpResponse)
        {
            httpResponse.StatusCode = (int)response.StatusCode;

            // Copy headers
            foreach (var header in response.Headers)
            {
                response.Headers.Add(header.Key, header.Value);
            }

            if (response.Content != null)
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                await httpResponse.WriteAsync(content).ConfigureAwait(false);
            }

            return httpResponse;
        }

        public static async Task<T> PostAsync<T>(this HttpClient client, Uri requestUri, IDictionary<string, string> headers, object content = null)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                var stringContent = content != null ? JsonConvert.SerializeObject(content) : null;
                var response = await RequestAsync(client, httpRequestMessage, headers, stringContent);

                var responseStringContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var objectResult = JsonConvert.DeserializeObject<ObjectResult>(responseStringContent);

                    var exceptionDetails = objectResult != null ? ActionResultExtensions.GetExceptionDetails(objectResult) : new ExceptionDetails();
                    throw new NotSuccessfulRequestException(response.StatusCode, exceptionDetails);
                }

                return JsonConvert.DeserializeObject<T>(responseStringContent);
            }
        }

        public static async Task<HttpResponseMessage> PostAsync(this HttpClient client, Uri requestUri, IDictionary<string, string> headers, object content = null)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                var stringContent = content != null ? JsonConvert.SerializeObject(content) : null;
                var response = await RequestAsync(client, httpRequestMessage, headers, stringContent);

                var responseStringContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var objectResult = JsonConvert.DeserializeObject<ObjectResult>(responseStringContent);

                    var exceptionDetails = objectResult != null ? ActionResultExtensions.GetExceptionDetails(objectResult) : new ExceptionDetails();
                    throw new NotSuccessfulRequestException(response.StatusCode, exceptionDetails);
                }

                return response;
            }
        }

        public static async Task<string> PostUrlEncodedAsync(this HttpClient client, Uri requestUri, IDictionary<string, string> content)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                var response = await RequestUrlEncodedAsync(client, httpRequestMessage, null, content);

                var responseStringContent = await response.Content?.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var objectResult = JsonConvert.DeserializeObject<ObjectResult>(responseStringContent);

                    var exceptionDetails = objectResult != null ? ActionResultExtensions.GetExceptionDetails(objectResult) : new ExceptionDetails();
                    throw new NotSuccessfulRequestException(response.StatusCode, exceptionDetails);
                }

                return responseStringContent;
            }
        }

        public static async Task<T> DeleteAsync<T>(this HttpClient client, Uri requestUri, object content = null)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, requestUri))
            {
                var stringContent = content != null ? JsonConvert.SerializeObject(content) : null;
                var response = await RequestAsync(client, httpRequestMessage, null, stringContent);

                var responseStringContent = await response.Content?.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var objectResult = JsonConvert.DeserializeObject<ObjectResult>(responseStringContent);

                    var exceptionDetails = objectResult != null ? ActionResultExtensions.GetExceptionDetails(objectResult) : new ExceptionDetails();
                    throw new NotSuccessfulRequestException(response.StatusCode, exceptionDetails);
                }

                return JsonConvert.DeserializeObject<T>(responseStringContent);
            }
        }

        public static async Task<HttpResponseMessage> DeleteAsync(this HttpClient client, Uri requestUri, object content = null)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, requestUri))
            {
                var stringContent = content != null ? JsonConvert.SerializeObject(content) : null;
                var response = await RequestAsync(client, httpRequestMessage, null, stringContent);

                var responseStringContent = await response.Content?.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var objectResult = JsonConvert.DeserializeObject<ObjectResult>(responseStringContent);

                    var exceptionDetails = objectResult != null ? ActionResultExtensions.GetExceptionDetails(objectResult) : new ExceptionDetails();
                    throw new NotSuccessfulRequestException(response.StatusCode, exceptionDetails);
                }

                return response;
            }
        }
        public static async Task<T> GetAsync<T>(this HttpClient client, Uri requestUri, IDictionary<string, string> headers)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                var response = await RequestAsync(client, httpRequestMessage, headers);

                var responseStringContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var objectResult = JsonConvert.DeserializeObject<ObjectResult>(responseStringContent);

                    var exceptionDetails = objectResult != null ? ActionResultExtensions.GetExceptionDetails(objectResult) : new ExceptionDetails();
                    throw new NotSuccessfulRequestException(response.StatusCode, exceptionDetails);
                }

                return JsonConvert.DeserializeObject<T>(responseStringContent);
            }
        }
        private static async Task<HttpResponseMessage> RequestAsync(HttpClient client, HttpRequestMessage httpRequestMessage, IDictionary<string, string> headers = null, string content = null)
        {
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    httpRequestMessage.Headers.Add(item.Key, item.Value);
                }
            }

            if (content != null)
            {
                httpRequestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");
            }

            var response = await client.SendAsync(httpRequestMessage);

            return response;
        }

        private static async Task<HttpResponseMessage> RequestUrlEncodedAsync(HttpClient client, HttpRequestMessage httpRequestMessage, IDictionary<string, string> headers = null, IDictionary<string, string> content = null)
        {
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    httpRequestMessage.Headers.Add(item.Key, item.Value);
                }
            }

            if (content != null)
            {
                httpRequestMessage.Content = new FormUrlEncodedContent(content);
            }

            return await client.SendAsync(httpRequestMessage);
        }
    }
}

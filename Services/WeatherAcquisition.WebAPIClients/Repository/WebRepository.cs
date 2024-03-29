﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using WeatherAcquisition.Interfaces.Base.Entities;
using WeatherAcquisition.Interfaces.Base.Repositories;

namespace WeatherAcquisition.WebAPIClients.Repository
{
    public class WebRepository<T> : IRepository<T> where T : IEntity, new()
    {
        private readonly HttpClient _client;

        public WebRepository(HttpClient client) => _client = client;

        public async Task<int> GetCount(CancellationToken cancellationToken = default) =>
            await _client.GetFromJsonAsync<int>("count", cancellationToken)
                .ConfigureAwait(false);

        public async Task<bool> ContainsId(int id, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _client.GetAsync($"exist/id/{id}", 
                    cancellationToken).ConfigureAwait(false);
            return response.StatusCode != HttpStatusCode.NotFound && response.IsSuccessStatusCode;
        }

        public async Task<bool> Contains(T item, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _client.PostAsJsonAsync($"exist", item,
                    cancellationToken).ConfigureAwait(false);
            return response.StatusCode != HttpStatusCode.NotFound && response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<T>> GetAll(CancellationToken cancellationToken = default) =>
            await _client.GetFromJsonAsync<IEnumerable<T>>("", cancellationToken)
                .ConfigureAwait(false);

        public async Task<IEnumerable<T>> Get(int skip, int count,
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _client.GetAsync($"items[{skip}:{count}]",
                cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Enumerable.Empty<T>();
            }

            return await response
                .Content
                .ReadFromJsonAsync<IEnumerable<T>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<T> GetById(int id, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _client.GetAsync($"{id}", cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new T();
            }

            return await response
                .Content
                .ReadFromJsonAsync<T>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> GetFirsts(int count, 
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _client.GetAsync($"firsts[{count}]",
                cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Enumerable.Empty<T>();
            }

            return await response
                .Content
                .ReadFromJsonAsync<IEnumerable<T>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> GetLasts(int count, 
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _client.GetAsync($"lasts[{count}]",
                cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Enumerable.Empty<T>();
            }

            return await response
                .Content
                .ReadFromJsonAsync<IEnumerable<T>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IPage<T>> GetPage(int pageIndex, int pageSize, 
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _client.GetAsync($"page[{pageIndex}/{pageSize}]", 
                cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new PageItems
                {
                    Items = Enumerable.Empty<T>(),
                    TotalItemCount = 0,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalPageCount = 0
                };
            }

            return await response
                .EnsureSuccessStatusCode()
                .Content
                .ReadFromJsonAsync<PageItems>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        private class PageItems : IPage<T>
        {
            public IEnumerable<T> Items { get; set; }

            public int TotalItemCount { get; set; }

            public int PageIndex { get; set; }

            public int PageSize { get; set; }

            public int TotalPageCount { get; set; }
        }

        public async Task<T> Add(T item, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _client.PostAsJsonAsync("", item, 
                cancellationToken).ConfigureAwait(false);

            T result = await response
                .EnsureSuccessStatusCode()
                .Content
                .ReadFromJsonAsync<T>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return result;
        }

        public async Task<T> Update(T item, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _client.PutAsJsonAsync("", item,
                cancellationToken).ConfigureAwait(false);

            T result = await response
                .EnsureSuccessStatusCode()
                .Content
                .ReadFromJsonAsync<T>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return result;
        }

        public async Task<T> Delete(T item, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, "")
            {
                Content = JsonContent.Create(item)
            };

            HttpResponseMessage response = await _client.SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            T result = await response
                .EnsureSuccessStatusCode()
                .Content
                .ReadFromJsonAsync<T>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return result;
        }

        public async Task<T> DeleteById(int id, CancellationToken cancellationToken = default)
        {
            T item = await GetById(id, cancellationToken).ConfigureAwait(false);

            if (item is null)
            {
                return default;
            }

            return await Delete(item, cancellationToken).ConfigureAwait(false);
        }
    }
}

﻿using System.Threading.Tasks;

namespace Essentials.Services
{
    public interface IRequestService
    {
        Task<TResult> DeleteAsync<TResult>(string uri, string token = "");

        Task<TResult> GetAsync<TResult>(string uri, string token = "");

        Task<TResult> PostAsync<TResult>(string uri, TResult data, string token = "");

        Task<TResult> PostAsync<TRequest, TResult>(string uri, TRequest data, string token = "");

        Task<TResult> PutAsync<TResult>(string uri, TResult data, string token = "");

        Task<TResult> PutAsync<TRequest, TResult>(string uri, TRequest data, string token = "");
    }
}
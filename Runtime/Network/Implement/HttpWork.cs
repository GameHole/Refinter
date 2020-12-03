using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Net;
using System.Net.Http;
namespace Refinter.Net
{
    public class HttpWork :IHttp
    {
        IWaittingUI waitting;
        HttpClient client = new HttpClient();
        public async Task<byte[]> GetBytes(string url)
        {
            waitting?.Show(true);
            var resp = await client.GetAsync(url);
            waitting?.Show(false);
            return await resp.Content.ReadAsByteArrayAsync();
        }

        public async Task<string> GetStr(string url)
        {
            waitting?.Show(true);
            var resp = await client.GetAsync(url);
            waitting?.Show(false);
            return await resp.Content.ReadAsStringAsync();
        }

        public async Task<byte[]> PostBytes(string url, string data)
        {
            waitting?.Show(true);
            var resp = await client.PostAsync(url, new StringContent(data));
            waitting?.Show(false);
            return await resp.Content.ReadAsByteArrayAsync();
        }

        public async Task<string> PostStr(string url, string data)
        {
            waitting?.Show(true);
            var m = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
            var resp = await client.PostAsync(url, m);
            waitting?.Show(false);
            return await resp.Content.ReadAsStringAsync();
        }
    }
}


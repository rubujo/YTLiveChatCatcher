using System.Net;
using System.Text;

namespace Rubujo.YouTube.Utility.Tests;

/// <summary>
/// 依請求 URL（含 HTTP 方法）比對，回傳預先準備好的內容的假 HttpMessageHandler，
/// 讓測試可以完全在本機執行，不需要真的連線到 YouTube。
/// </summary>
public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly List<(Func<HttpRequestMessage, bool> Match, Func<HttpRequestMessage, string> ContentFactory)> _routes = [];

    /// <summary>
    /// 記錄所有實際發出的請求，供測試斷言使用。
    /// </summary>
    public List<HttpRequestMessage> Requests { get; } = [];

    public FakeHttpMessageHandler When(HttpMethod method, string urlContains, string responseContent)
    {
        _routes.Add((
            request => request.Method == method && (request.RequestUri?.ToString().Contains(urlContains) ?? false),
            _ => responseContent));

        return this;
    }

    public FakeHttpMessageHandler When(HttpMethod method, string urlContains, Func<HttpRequestMessage, string> responseFactory)
    {
        _routes.Add((
            request => request.Method == method && (request.RequestUri?.ToString().Contains(urlContains) ?? false),
            responseFactory));

        return this;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);

        (Func<HttpRequestMessage, bool> Match, Func<HttpRequestMessage, string> ContentFactory) route = _routes
            .FirstOrDefault(r => r.Match(request));

        if (route.ContentFactory == null)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent(string.Empty)
            });
        }

        HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent(route.ContentFactory(request), Encoding.UTF8, "text/html")
        };

        return Task.FromResult(response);
    }
}

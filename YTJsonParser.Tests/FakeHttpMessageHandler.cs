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
    private readonly Dictionary<string, Queue<Func<HttpResponseMessage>>> _sequencedResponsesByUrlContains = [];

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

    /// <summary>
    /// 針對 URL 符合 urlContains 的請求，依序套用這些回應工廠（用完後才會退回一般的 When 路由比對）。
    /// 用於模擬「前幾次失敗（例如拋出網路例外、回傳 429）、之後才成功」的重試情境；
    /// 工廠可以直接 <c>throw</c> 來模擬 SendAsync 本身失敗（例如 <see cref="HttpRequestException"/>）。
    /// </summary>
    public FakeHttpMessageHandler WhenSequence(string urlContains, params Func<HttpResponseMessage>[] responseFactories)
    {
        _sequencedResponsesByUrlContains[urlContains] = new Queue<Func<HttpResponseMessage>>(responseFactories);

        return this;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);

        string url = request.RequestUri?.ToString() ?? string.Empty;

        foreach (KeyValuePair<string, Queue<Func<HttpResponseMessage>>> entry in _sequencedResponsesByUrlContains)
        {
            if (url.Contains(entry.Key) && entry.Value.Count > 0)
            {
                return Task.FromResult(entry.Value.Dequeue()());
            }
        }

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

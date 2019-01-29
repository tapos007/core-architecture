using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Solution.API.Middlewares
{
    public class MyIPRateMiddleware 
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<IpRateLimitMiddleware> _logger;
        private readonly IIpAddressParser _ipParser;
        private readonly IpRateLimitProcessor _processor;
        private readonly IpRateLimitOptions _options;

        public MyIPRateMiddleware(
          RequestDelegate next,
          IOptions<IpRateLimitOptions> options,
          IRateLimitCounterStore counterStore,
          IIpPolicyStore policyStore,
          ILogger<IpRateLimitMiddleware> logger,
          IIpAddressParser ipParser = null)
        {
            this._next = next;
            this._options = options.Value;
            this._logger = logger;
            this._ipParser = ipParser != null ? ipParser : (IIpAddressParser)new ReversProxyIpParser(this._options.RealIpHeader);
            this._processor = new IpRateLimitProcessor(this._options, counterStore, policyStore, this._ipParser);
        }

        public async Task Invoke(HttpContext httpContext)
        {
            MyIPRateMiddleware rateLimitMiddleware = this;
            if (rateLimitMiddleware._options == null)
            {
                await rateLimitMiddleware._next(httpContext);
            }
            else
            {
                ClientRequestIdentity identity = rateLimitMiddleware.SetIdentity(httpContext);
                if (rateLimitMiddleware._processor.IsWhitelisted(identity))
                {
                    await rateLimitMiddleware._next(httpContext);
                }
                else
                {
                    List<RateLimitRule> rules = rateLimitMiddleware._processor.GetMatchingRules(identity);
                    foreach (RateLimitRule rule in rules)
                    {
                        if (rule.Limit > 0L)
                        {
                            RateLimitCounter counter = rateLimitMiddleware._processor.ProcessRequest(identity, rule);
                            if (!(counter.Timestamp + rule.PeriodTimespan.Value < DateTime.UtcNow) && counter.TotalRequests > rule.Limit)
                            {
                                string retryAfter = rateLimitMiddleware._processor.RetryAfterFrom(counter.Timestamp, rule);
                                rateLimitMiddleware.LogBlockedRequest(httpContext, identity, counter, rule);
                                await rateLimitMiddleware.ReturnQuotaExceededResponse(httpContext, rule, retryAfter);
                                return;
                            }
                        }
                    }
                    if (rules.Any<RateLimitRule>() && !rateLimitMiddleware._options.DisableRateLimitHeaders)
                    {
                        RateLimitRule rule = rules.OrderByDescending<RateLimitRule, TimeSpan>((Func<RateLimitRule, TimeSpan>)(x => x.PeriodTimespan.Value)).First<RateLimitRule>();
                        RateLimitHeaders rateLimitHeaders = rateLimitMiddleware._processor.GetRateLimitHeaders(identity, rule);
                        rateLimitHeaders.Context = httpContext;
                        httpContext.Response.OnStarting(new Func<object, Task>(rateLimitMiddleware.SetRateLimitHeaders), (object)rateLimitHeaders);
                    }
                    await rateLimitMiddleware._next(httpContext);
                }
            }
        }

        public virtual ClientRequestIdentity SetIdentity(HttpContext httpContext)
        {
            string str1 = "anon";
            if (((IDictionary<string, StringValues>)httpContext.Request.Headers).Keys.Contains<string>(this._options.ClientIdHeader, (IEqualityComparer<string>)StringComparer.CurrentCultureIgnoreCase))
                str1 = ((IEnumerable<string>)httpContext.Request.Headers[this._options.ClientIdHeader]).First<string>();
            string empty = string.Empty;
            string str2;
            try
            {
                IPAddress clientIp = this._ipParser.GetClientIp(httpContext);
                if (clientIp == null)
                    throw new Exception("IpRateLimitMiddleware can't parse caller IP");
                str2 = clientIp.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("IpRateLimitMiddleware can't parse caller IP", ex);
            }
            return new ClientRequestIdentity()
            {
                ClientIp = str2,
                Path = httpContext.Request.Path.ToString().ToLowerInvariant(),
                HttpVerb = httpContext.Request.Method.ToLowerInvariant(),
                ClientId = str1
            };
        }

        public virtual Task ReturnQuotaExceededResponse(
          HttpContext httpContext,
          RateLimitRule rule,
          string retryAfter)
        {
            string str = string.IsNullOrEmpty(this._options.QuotaExceededMessage) ? string.Format("API calls quota exceeded! maximum admitted {0} per {1}.", (object)rule.Limit, (object)rule.Period) : this._options.QuotaExceededMessage;
            if (!this._options.DisableRateLimitHeaders)
                httpContext.Response.Headers["Retry-After"] = (StringValues)retryAfter;
            httpContext.Response.StatusCode = this._options.HttpStatusCode;

            var result = JsonConvert.SerializeObject(new { error = str });
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = this._options.HttpStatusCode;
            return httpContext.Response.WriteAsync(result);
            
        }

        public virtual void LogBlockedRequest(
          HttpContext httpContext,
          ClientRequestIdentity identity,
          RateLimitCounter counter,
          RateLimitRule rule)
        {
            this._logger.LogInformation(string.Format("Request {0}:{1} from IP {2} has been blocked, quota {3}/{4} exceeded by {5}. Blocked by rule {6}, TraceIdentifier {7}.", (object)identity.HttpVerb, (object)identity.Path, (object)identity.ClientIp, (object)rule.Limit, (object)rule.Period, (object)counter.TotalRequests, (object)rule.Endpoint, (object)httpContext.TraceIdentifier));
        }

        private Task SetRateLimitHeaders(object rateLimitHeaders)
        {
            RateLimitHeaders rateLimitHeaders1 = (RateLimitHeaders)rateLimitHeaders;
            rateLimitHeaders1.Context.Response.Headers["X-Rate-Limit-Limit"] = (StringValues)rateLimitHeaders1.Limit;
            rateLimitHeaders1.Context.Response.Headers["X-Rate-Limit-Remaining"] = (StringValues)rateLimitHeaders1.Remaining;
            rateLimitHeaders1.Context.Response.Headers["X-Rate-Limit-Reset"] = (StringValues)rateLimitHeaders1.Reset;
            return Task.CompletedTask;
        }
    }
}

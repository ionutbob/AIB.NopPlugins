using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Data;
using Nop.Plugin.Misc.BlockMaliciousRequests.Domain;
using Nop.Plugin.Misc.BlockMaliciousRequests.Models;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.BlockMaliciousRequests.Framework
{
    /// <summary>
    /// This class should receive all requests before any other middleware and block malicious requests as filters are set up.
    /// </summary>
    public class FilterMaliciousRequestsMiddleware
    {
        private readonly RequestDelegate _next;

        private static bool _filtersLoaded = false;

        private static List<MaliciousRequestFilterRecord> _urlAndCountryFilters;

        private static bool _settingsLoaded = false;

        private static BlockMaliciousRequestsSettings _filterSettings;

        public FilterMaliciousRequestsMiddleware(RequestDelegate next)
        {
            _next = next; 
        }

        public async Task Invoke(HttpContext context,
            IWebHelper webHelper,
            IPermissionService permissionService,
            IRepository<MaliciousRequestFilterRecord> filtersRepository,
            IRepository<RequestLogRecord> logsRepository,
            ISettingService settingService,
            IGeoLookupService geoLookupService)
        {           
            // TODO: First, check wether the request is not a nopcommerce task or a resource request, and let the request go if so
            var requestedUrl = webHelper.GetRawUrl(context.Request); // it will be used for logging, anyways

            if (requestedUrl.StartsWith("/scheduletask/") || requestedUrl.StartsWith("/keepalive") || requestedUrl.StartsWith("/eucookie")
                || requestedUrl.Contains("images/") || requestedUrl.Contains(".css") || requestedUrl.Contains(".js") || requestedUrl.Contains(".ico")
                || requestedUrl.Contains(".png") || requestedUrl.Contains(".gif"))
            {
                // let the request go, do not log anything            
                await _next(context);
                return;
            }

            if (requestedUrl.Contains("Admin") && permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
            {
                // let the request go, do not log anything            
                await _next(context);
                return;
            }
           
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            // Load settings and check wether there is any
            LoadSettings(settingService);

            var ip = webHelper.GetCurrentIpAddress(); // it will be used for logging, anyways
            var logRecord = new RequestLogRecord { 
                Ip = ip, 
                Country = string.Empty, 
                RequestedUrl = requestedUrl, 
                RequestTime = DateTime.Now, 
                BlockedBy = BlockedBy.None };

            if (_filterSettings.BlockBlankIpRequests && string.IsNullOrWhiteSpace(ip?.Trim()))
            {
                // return forbidden
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden");
                LogRequest(logRecord, stopWatch, logsRepository);
                return;
            }                   

            if (_filterSettings?.EnableRequestUrlFiltering == true)
            {
                // check url filters
                LoadFilters(filtersRepository);

                foreach (var filter in _urlAndCountryFilters.Where(f => f.IsUrlRequestFilter))
                {
                    if (requestedUrl.Contains(filter.Value))
                    {
                        // return forbidden
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsync("Forbidden");

                        logRecord.ResponseStatus = 403;
                        logRecord.BlockedBy = BlockedBy.Url;
                        LogRequest(logRecord, stopWatch, logsRepository);
                        return;
                    }
                }
            }

            var country = string.IsNullOrWhiteSpace(ip?.Trim()) ? string.Empty : geoLookupService.LookupCountryName(ip);
            logRecord.Country = country;

            if (_filterSettings?.EnableCountryFiltering == true && !string.IsNullOrWhiteSpace(country?.Trim()))
            {
                // check country filters
                LoadFilters(filtersRepository);   

                foreach (var filter in _urlAndCountryFilters.Where(f => f.IsCountryFilter))
                {                    
                    if (filter.Value?.ToLower() == country.ToLower())
                    {
                        // return forbidden
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsync("Forbidden");

                        logRecord.ResponseStatus = 403;
                        LogRequest(logRecord, stopWatch, logsRepository);
                        logRecord.BlockedBy = BlockedBy.Country;
                        return;
                    }
                }
            }

            if (_filterSettings?.EnableIpFiltering == true)
            {
                // check IP filters

                var blockedIp = filtersRepository.Table.FirstOrDefault(f => f.Value == ip && (f.BlockedUntil == null || f.BlockedUntil > DateTime.Now));

                if (blockedIp != null)
                {
                    // return forbidden
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Forbidden");
                    logRecord.ResponseStatus = 403;
                    logRecord.BlockedBy = BlockedBy.Ip;
                    LogRequest(logRecord, stopWatch, logsRepository);
                    return;
                }
            }

            if (_filterSettings?.EnableDynamicRequestFiltering == true)
            {
                // TODO: implement dynamic filtering
                var numberOfRequestsOnLastMinute = logsRepository.Table.Count(l => l.Ip == ip && l.RequestTime > DateTime.Now.AddMinutes(-1));
                if (numberOfRequestsOnLastMinute > _filterSettings.MaximumNumberOfRequestsPerMinute)
                {
                    // Add ip filter blocking and return forbidden
                    filtersRepository.Insert(
                        new MaliciousRequestFilterRecord 
                        { 
                            IsIpFilter = true, 
                            Value = ip, 
                            BlockedUntil = 
                            DateTime.Now.AddHours(48), 
                            IsUrlRequestFilter = false, 
                            IsCountryFilter = false 
                        });

                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Forbidden");
                    logRecord.ResponseStatus = 403;
                    logRecord.BlockedBy = BlockedBy.Dynamically;
                    LogRequest(logRecord, stopWatch, logsRepository);
                    return;
                }

                var numberOfRequestsBlockedByUrl = logsRepository.Table.Count(
                    l => l.Ip == ip && l.RequestTime > DateTime.Now.AddMinutes(-5) && l.BlockedBy == BlockedBy.Url);
                if (numberOfRequestsBlockedByUrl > 10)
                {
                    // Add ip filter blocking and return forbidden
                    filtersRepository.Insert(
                        new MaliciousRequestFilterRecord
                        {
                            IsIpFilter = true,
                            Value = ip,
                            BlockedUntil =
                            DateTime.Now.AddDays(7),
                            IsUrlRequestFilter = false,
                            IsCountryFilter = false
                        });

                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Forbidden");
                    logRecord.ResponseStatus = 403;
                    logRecord.BlockedBy = BlockedBy.Dynamically;
                    LogRequest(logRecord, stopWatch, logsRepository);
                    return;
                }
            }

            // continue the request pipeline
            await _next(context);

            if (_filterSettings.LogAllRequests)
            {
                logRecord.ResponseStatus = context.Response.StatusCode;
                LogRequest(logRecord, stopWatch, logsRepository);
            }
            else
            {
                stopWatch.Stop();                
            }
        }

        private void LogRequest(RequestLogRecord logRecord, Stopwatch stopWatch, IRepository<RequestLogRecord> logsRepository)
        {
            stopWatch.Stop();
            logRecord.RequestDuration = stopWatch.Elapsed;
            logsRepository.Insert(logRecord);
        }

        private void LoadSettings(ISettingService settingsService)
        {
            if (_settingsLoaded)
                return;

            _filterSettings = settingsService.LoadSetting<BlockMaliciousRequestsSettings>();
        }

        /// <summary>
        /// Load all filters
        /// </summary>
        /// <param name="filtersRepository"></param>
        private static void LoadFilters(IRepository<MaliciousRequestFilterRecord> filtersRepository) 
        {
            if (_filtersLoaded)
                return;            

            _urlAndCountryFilters = filtersRepository.Table.Where(item => item.IsUrlRequestFilter || item.IsCountryFilter).ToList();
            _filtersLoaded = true;
        }

        public static void Reset() 
        {
            _filtersLoaded = false;
            _settingsLoaded = false;
        }
    }
}

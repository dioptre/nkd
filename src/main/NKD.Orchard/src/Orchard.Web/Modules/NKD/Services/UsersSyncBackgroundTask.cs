using JetBrains.Annotations;
using Orchard.Logging;
using Orchard.Tasks;
using Orchard;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.Services;
using NKD.Models;
using System.Linq;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Caching;
using System;

namespace NKD.Services {
    /// <summary>
    /// Regularly fires user sync events
    /// </summary>
    [UsedImplicitly]
    public class UsersSyncBackgroundTask : IBackgroundTask
    {
        public const string NEXT_UPDATE_CACHE_KEY = "NKD.UsersSyncBackgroundTask.NextUpdate.Changed";
        private readonly ICacheManager _cache;
        private readonly ISignals _signals;
        private readonly IUsersService _userService;
        private readonly IClock _clock;
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        

        public UsersSyncBackgroundTask(IOrchardServices services, IUsersService userService, IClock clock, ICacheManager cache, ISignals signals)
        {
            _signals = signals;
            _cache = cache;
            Services = services;
            _userService = userService;
            _clock = clock;
            Logger = NullLogger.Instance;
        }


        public void Sweep()
        {
            try
            {
                DateTime? nextUpdate = _cache.Get<string, DateTime?>(NEXT_UPDATE_CACHE_KEY, ctx =>
                {
                    ctx.Monitor(_signals.When(NEXT_UPDATE_CACHE_KEY));
                    return _clock.UtcNow.AddMinutes(45);
                });

                if (_clock.UtcNow > nextUpdate.GetValueOrDefault())
                {
                    _signals.Trigger(NEXT_UPDATE_CACHE_KEY);
                    _userService.SyncUsers();
                    Logger.Information(string.Format("Successfully updated users (synchronisation completed).", DateTime.UtcNow.ToLongDateString()));

                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in UserSyncBackgroundTask");
            }
        }
    }
}

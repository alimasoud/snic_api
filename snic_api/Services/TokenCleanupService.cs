using snic_api.Services;

namespace snic_api.Services
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1); // Run every hour

        public TokenCleanupService(IServiceProvider serviceProvider, ILogger<TokenCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var tokenBlacklistService = scope.ServiceProvider.GetRequiredService<TokenBlacklistService>();
                        await tokenBlacklistService.CleanupExpiredTokensAsync();
                        _logger.LogInformation("Token cleanup completed at {Time}", DateTime.UtcNow);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during token cleanup at {Time}", DateTime.UtcNow);
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }
    }
} 
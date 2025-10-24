using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication;

namespace TableOrder_Hust.Services
{
    public interface IGoogleCalendarService
    {
        Task CreateEventAsync(string summary, string description, DateTime start, DateTime end);
    }

    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GoogleCalendarService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task CreateEventAsync(string summary, string description, DateTime startTime, DateTime endTime)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var accessToken = await httpContext.GetTokenAsync("access_token");

            if (string.IsNullOrEmpty(accessToken))
            {
                throw new InvalidOperationException("User is not authenticated or access token is missing.");
            }

            var credential = GoogleCredential.FromAccessToken(accessToken);
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "TableOrder HUST",
            });

            var newEvent = new Event()
            {
                Summary = summary,
                Description = description,
                Start = new EventDateTime()
                {
                    DateTime = startTime,
                    TimeZone = "Asia/Bangkok", // Lấy từ config
                },
                End = new EventDateTime()
                {
                    DateTime = endTime,
                    TimeZone = "Asia/Bangkok", // Lấy từ config
                },
            };

            var request = service.Events.Insert(newEvent, "primary"); // "primary" là calendar chính của user
            await request.ExecuteAsync();
        }
    }
}

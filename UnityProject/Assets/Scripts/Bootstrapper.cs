using UnityEngine;
using Util;
using System.Collections.Generic;

public class Bootstrapper : MonoBehaviour
{
    private void Awake()
    {
        // Register services 
        // Assign our user to a cohort of Variant A, B, or C
        CohortManager.AssignRandomCohort();
        
        // Bind IConfigService and fetch configuration
        var configService = new LocalConfigService();
        ServiceLocator.Bind<IConfigService>(configService);
        
        // Bind IAnalyticsService
        var analyticsService = new LocalAnalyticsService();
        ServiceLocator.Bind<IAnalyticsService>(analyticsService);
        
        // Fetch configuration on startup
        configService.FetchConfig(() => {
            Debug.Log("Configuration loaded successfully in Bootstrapper");
            
            // Register user with analytics after config is loaded
            RegisterUserWithAnalytics(analyticsService);
        });
    }

    private void RegisterUserWithAnalytics(IAnalyticsService analyticsService)
    {
        var userParameters = new Dictionary<string, object>
        {
            ["registration_timestamp"] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            ["session_id"] = System.Guid.NewGuid().ToString(),
            ["platform"] = Application.platform.ToString()
        };

        analyticsService.Register(userParameters);
        
        // Track app launch event
        analyticsService.Track("app_launched", new Dictionary<string, object>
        {
            ["launch_timestamp"] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }
}

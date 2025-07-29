using UnityEngine;
using Util;

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
        
        // Fetch configuration on startup
        configService.FetchConfig(() => {
            Debug.Log("Configuration loaded successfully in Bootstrapper");
        });
        
        //TODO:
        //ServiceLocator.Bind<IAnalyticsService>(new LocalAnalyticsService());
    }
}

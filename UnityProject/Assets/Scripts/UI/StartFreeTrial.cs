using System;
using UnityEngine;
using UnityEngine.UI;
using Util;
using System.Collections.Generic;

public class StartFreeTrial : UIScreenBase
{
    [SerializeField] private GameObject _nextUI;
    private ToggleGroup _toggleGroup;
    private IAnalyticsService _analyticsService;
    
    private void Start()
    {
        _analyticsService = ServiceLocator.Get<IAnalyticsService>();
        
        _analyticsService?.Screen(this, new Dictionary<string, object>
        {
            ["entry_timestamp"] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            ["cohort"] = CohortManager.GetUserCohort()
        });
        
        Debug.Log("StartFreeTrial: Free trial screen initialized for VariantA user");
    }

    public override void Next()
    {
        _analyticsService?.Track("free_trial_started", new Dictionary<string, object>
        {
            ["start_timestamp"] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            ["cohort"] = CohortManager.GetUserCohort()
        });
        _analyticsService?.Flush();
        
        if (_nextUI != null)
        {
            _nextUI?.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}

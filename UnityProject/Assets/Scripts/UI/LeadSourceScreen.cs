using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;
using System.Collections.Generic;

public class LeadSourceScreen : UIScreenBase
{
    [SerializeField] private GameObject _freeTrialUI;
    [SerializeField] private GameObject _paywallUI;
    private ToggleGroup _toggleGroup;
    private Toggle _activeToggle;
    private IAnalyticsService _analyticsService;

    private void Awake()
    {
        _toggleGroup = GetComponentInChildren<ToggleGroup>();
    }

    private void Start()
    {
        _analyticsService = ServiceLocator.Get<IAnalyticsService>();
        
        // Track screen view
        _analyticsService?.Screen(this, new Dictionary<string, object>
        {
            ["entry_timestamp"] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }

    public override void Next()
    {
        string selectedLeadSource = _activeToggle != null ? 
            _activeToggle.GetComponentInChildren<TMP_Text>().text : "None";
        
        _analyticsService?.Track("lead_source_next_clicked", new Dictionary<string, object>
        {
            ["selected_lead_source"] = selectedLeadSource,
            ["click_timestamp"] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        });

        // Based on user's cohort, load the appropriate UI
        string userCohort = CohortManager.GetUserCohort();
        
        if (userCohort == "VariantA")
        {
            if (_freeTrialUI != null)
            {
                Instantiate(_freeTrialUI, transform.parent);
                
                _analyticsService?.Track("ui_transition", new Dictionary<string, object>
                {
                    ["from_screen"] = "LeadSourceScreen",
                    ["to_screen"] = "StartFreeTrial",
                    ["cohort"] = userCohort
                });
            }
            else
            {
                Debug.LogError("LeadSourceScreen: Free Trial UI prefab not assigned!");
            }
        }
        else if (userCohort == "VariantB" || userCohort == "VariantC")
        {
            // VariantB and VariantC get Paywall UI
            if (_paywallUI != null)
            {
                Instantiate(_paywallUI, transform.parent);
                
                // Track UI transition
                _analyticsService?.Track("ui_transition", new Dictionary<string, object>
                {
                    ["from_screen"] = "LeadSourceScreen",
                    ["to_screen"] = "Paywall",
                    ["cohort"] = userCohort
                });
            }
            else
            {
                Debug.LogError("LeadSourceScreen: Paywall UI prefab not assigned!");
            }
        }
        
        // Hide this screen
        Destroy(gameObject);
    }

    public void OnToggleClicked()
    {
        _activeToggle = _toggleGroup
            .ActiveToggles()
            .FirstOrDefault();

        if (!_activeToggle) return;
        
        var toggleText = _activeToggle.GetComponentInChildren<TMP_Text>().text;
        // Debug.Log($"Lead Source is: {toggleText}");
        
        // Track lead source selection
        _analyticsService?.Track("lead_source_selected", new Dictionary<string, object>
        {
            ["selected_source"] = toggleText,
            ["selection_timestamp"] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }
}

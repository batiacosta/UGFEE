using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Util;
using System.Collections.Generic;

public class Paywall : UIScreenBase
{
    [SerializeField]
    private ToggleValues initialToggleValues;
    [SerializeField] private ToggleValues selectedToggleValues;
    [SerializeField] private TMP_Text annualPriceText;
    [SerializeField] private TMP_Text monthlyPriceText;
    
    private ToggleGroup _toggleGroup;
    private Toggle _activeToggle;
    private IConfigService _configService;
    private IAnalyticsService _analyticsService;
    
    private void Awake()
    {
        _toggleGroup = GetComponentInChildren<ToggleGroup>();
    }

    private void Start()
    {
        // Get services
        _configService = ServiceLocator.Get<IConfigService>();
        _analyticsService = ServiceLocator.Get<IAnalyticsService>();
        
        // Track screen view
        _analyticsService?.Screen(this, new Dictionary<string, object>
        {
            ["entry_timestamp"] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            ["cohort"] = CohortManager.GetUserCohort()
        });
        
        LoadPricesFromConfig();
    }

    private void OnEnable()
    {
        _toggleGroup.GetComponentInChildren<Toggle>(true).isOn = true;
        LoadPricesFromConfig();
    }

    private void LoadPricesFromConfig()
    {
        if (_configService == null)
        {
            Debug.LogWarning("Paywall: ConfigService not available, using default prices");
            return;
        }

        // Get user's cohort to determine which prices to show
        string userCohort = CohortManager.GetUserCohort();
        
        // Load prices based on cohort
        float priceA = _configService.GetValue<float>($"{userCohort}.priceOptionA", 99.99f);
        float priceB = _configService.GetValue<float>($"{userCohort}.priceOptionB", 12.99f);
        
        // Update UI text elements with prices
        if (annualPriceText != null)
        {
            annualPriceText.text = $"${priceA:F2}/year";
        }
        
        if (monthlyPriceText != null)
        {
            monthlyPriceText.text = $"${priceB:F2}/month";
        }
        
        // Track price display
        _analyticsService?.Track("prices_displayed", new Dictionary<string, object>
        {
            ["annual_price"] = priceA,
            ["monthly_price"] = priceB,
            ["cohort"] = userCohort
        });
        
        Debug.Log($"Paywall: Loaded prices for {userCohort} - Annual: ${priceA:F2}, Monthly: ${priceB:F2}");
    }

    public void OnToggleClicked()
    {
        if (_toggleGroup == null) return;
        
        var newSelectedToggle = _toggleGroup
            .ActiveToggles()
            .FirstOrDefault();

        if (newSelectedToggle != _activeToggle)
        {
            ToggleVisualsSetup(newSelectedToggle, selectedToggleValues);
            
            ToggleVisualsSetup(_activeToggle, initialToggleValues);
            _activeToggle = newSelectedToggle;

            void ToggleVisualsSetup(Toggle toggle, ToggleValues toggleValues)
            {
                if (toggle == null) return;
                var title = newSelectedToggle.GetComponentInChildren<TMP_Text>();
                if (toggleValues.titleBold)
                {
                    title.text = "<b>" + title.text + "</b>";
                }
                else
                {
                    title.text.Replace("<b>", "");
                    title.text.Replace("</b>", "");
                }
                var texts = toggle.gameObject.GetComponentsInChildren<TMP_Text>();
                foreach (var text in texts)
                {
                    text.color = toggleValues.textColor;
                    text.spriteAsset?.material.SetColor("_Color", toggleValues.textColor);
                }
                
                var background = toggle.gameObject.GetComponentInChildren<Image>();
                background.color = toggleValues.backgroundColor;
                var edge = background.gameObject.GetComponent<Outline>();
                edge.enabled = !toggleValues.titleBold;
            }
        }

        if (!_activeToggle) return;
        
        var toggleText = _activeToggle.GetComponentInChildren<TMP_Text>().text;
        Debug.Log($"Subscription option selected: {toggleText}");
        
        // Track subscription option selection
        _analyticsService?.Track("subscription_option_selected", new Dictionary<string, object>
        {
            ["selected_option"] = toggleText,
            ["selection_timestamp"] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            ["cohort"] = CohortManager.GetUserCohort()
        });
    }
    
    [Serializable]
    private struct ToggleValues
    {
        public Color32 backgroundColor;
        public Color32 textColor;
        public bool titleBold;
    }

    public override void Next()
    {
        string selectedOption = _activeToggle?.GetComponentInChildren<TMP_Text>().text ?? "None";
        
        _analyticsService?.Track("subscription_purchase_attempted", new Dictionary<string, object>
        {
            ["selected_option"] = selectedOption,
            ["attempt_timestamp"] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            ["cohort"] = CohortManager.GetUserCohort()
        });
        _analyticsService?.Flush();
        
        Debug.Log($"Subscription purchase attempted: {selectedOption}");
    }
}

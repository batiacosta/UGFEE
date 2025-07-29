using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Util;

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
    
    private void Awake()
    {
        _toggleGroup = GetComponentInChildren<ToggleGroup>();
    }

    private void Start()
    {
        // Get config service and load prices
        _configService = ServiceLocator.Get<IConfigService>();
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
            annualPriceText.text = $"${priceA:F2}";
        }
        
        if (monthlyPriceText != null)
        {
            monthlyPriceText.text = $"${priceB:F2}";
        }
        
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
        Debug.Log($"Lead Source is: {toggleText}");
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
        // Here send data
        Debug.Log($"Data sending to be implemented {_activeToggle.GetComponentInChildren<TMP_Text>().text}");
    }
}

using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Paywall : UIScreenBase
{
    [SerializeField]
    private ToggleValues initialToggleValues;
    [SerializeField] private ToggleValues selectedToggleValues;
    private ToggleGroup _toggleGroup;
    private Toggle _activeToggle;
    private void Awake()
    {
        _toggleGroup = GetComponentInChildren<ToggleGroup>();
    }

    private void OnEnable()
    {
        _toggleGroup.GetComponentInChildren<Toggle>(true).isOn = true;
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

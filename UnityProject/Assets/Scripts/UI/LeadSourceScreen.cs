using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class LeadSourceScreen : UIScreenBase
{
    [SerializeField] private GameObject _freeTrialUI;
    [SerializeField] private GameObject _paywallUI;
    private ToggleGroup _toggleGroup;
    private Toggle _activeToggle;

    private void Awake()
    {
        _toggleGroup = GetComponentInChildren<ToggleGroup>();
    }

    public override void Next()
    {
        // Based on user's cohort, load the appropriate UI
        string userCohort = CohortManager.GetUserCohort();
        
        if (userCohort == "VariantA")
        {
            // VariantA gets Free Trial UI
            if (_freeTrialUI != null)
            {
                Instantiate(_freeTrialUI, transform.parent);
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
            }
            else
            {
                Debug.LogError("LeadSourceScreen: Paywall UI prefab not assigned!");
            }
        }
        
        // Hide this screen
        Destroy(gameObject);
        
        //TODO: Gather Analytics
    }

    public void OnToggleClicked()
    {
        _activeToggle = _toggleGroup
            .ActiveToggles()
            .FirstOrDefault();

        if (!_activeToggle) return;
        
        var toggleText = _activeToggle.GetComponentInChildren<TMP_Text>().text;
        Debug.Log($"Lead Source is: {toggleText}");
    }
}

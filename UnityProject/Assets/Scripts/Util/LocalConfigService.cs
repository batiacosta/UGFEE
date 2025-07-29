using System;
using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    public class LocalConfigService: IConfigService
    {   
        private Dictionary<string, object> _configData;
        private bool _isConfigLoaded = false;

        public void CleanupService()
        {
            _configData?.Clear();
            _configData = null;
            _isConfigLoaded = false;
        }

        public void FetchConfig(Action onComplete)
        {
            try
            {
                // Load the configuration.json file from Resources folder
                TextAsset configFile = Resources.Load<TextAsset>("configuration");
                
                if (configFile == null)
                {
                    Debug.LogError("LocalConfigService: configuration.json file not found in Resources folder");
                    _configData = new Dictionary<string, object>();
                    _isConfigLoaded = true;
                    onComplete?.Invoke();
                    return;
                }

                // Deserialize JSON into Dictionary
                _configData = JsonUtility.FromJson<ConfigData>(configFile.text).ToDictionary();
                _isConfigLoaded = true;
                
                Debug.Log("LocalConfigService: Configuration loaded successfully");
                onComplete?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"LocalConfigService: Error loading configuration - {ex.Message}");
                _configData = new Dictionary<string, object>();
                _isConfigLoaded = true;
                onComplete?.Invoke();
            }
        }

        public T GetValue<T>(string key, T defaultValue)
        {
            if (!_isConfigLoaded || _configData == null)
            {
                Debug.LogWarning("LocalConfigService: Configuration not loaded yet. Returning default value.");
                return defaultValue;
            }

            try
            {
                // Support dot notation for nested keys (e.g., "VariantA.prefabToLoad")
                string[] keyParts = key.Split('.');
                object currentValue = _configData;

                foreach (string keyPart in keyParts)
                {
                    if (currentValue is Dictionary<string, object> dict && dict.ContainsKey(keyPart))
                    {
                        currentValue = dict[keyPart];
                    }
                    else
                    {
                        Debug.LogWarning($"LocalConfigService: Key '{key}' not found. Returning default value.");
                        return defaultValue;
                    }
                }

                // Convert the value to the requested type
                if (currentValue is T directValue)
                {
                    return directValue;
                }
                
                // Try to convert the value
                return (T)Convert.ChangeType(currentValue, typeof(T));
            }
            catch (Exception ex)
            {
                Debug.LogError($"LocalConfigService: Error retrieving value for key '{key}' - {ex.Message}");
                return defaultValue;
            }
        }

        // Helper classes for JSON deserialization
        [System.Serializable]
        private class ConfigData
        {
            public VariantData VariantA;
            public VariantData VariantB;
            public VariantData VariantC;

            public Dictionary<string, object> ToDictionary()
            {
                var result = new Dictionary<string, object>();
                
                if (VariantA != null)
                    result["VariantA"] = VariantA.ToDictionary();
                if (VariantB != null)
                    result["VariantB"] = VariantB.ToDictionary();
                if (VariantC != null)
                    result["VariantC"] = VariantC.ToDictionary();
                
                return result;
            }
        }

        [System.Serializable]
        private class VariantData
        {
            public string prefabToLoad;
            public float priceOptionA;
            public float priceOptionB;

            public Dictionary<string, object> ToDictionary()
            {
                var result = new Dictionary<string, object>();
                
                if (!string.IsNullOrEmpty(prefabToLoad))
                    result["prefabToLoad"] = prefabToLoad;
                if (priceOptionA > 0)
                    result["priceOptionA"] = priceOptionA;
                if (priceOptionB > 0)
                    result["priceOptionB"] = priceOptionB;
                
                return result;
            }
        }
    }
}

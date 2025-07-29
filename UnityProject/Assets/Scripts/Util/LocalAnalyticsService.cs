using System.Collections.Generic;
using System;
using UnityEngine;

namespace Util
{
    public class LocalAnalyticsService : IAnalyticsService
    {
        private const string ANALYTICS_KEY = "LocalAnalyticsData";
        private List<AnalyticsEvent> _analyticsEvents;
        private Dictionary<string, object> _userProperties;
        private bool _isRegistered = false;

        public LocalAnalyticsService()
        {
            _analyticsEvents = new List<AnalyticsEvent>();
            _userProperties = new Dictionary<string, object>();
            LoadStoredAnalytics();
        }

        public void CleanupService()
        {
            SaveAnalytics();
            _analyticsEvents?.Clear();
            _userProperties?.Clear();
            _isRegistered = false;
        }

        public void Register(Dictionary<string, object> parameters = null)
        {
            _isRegistered = true;
            
            // Store user registration data
            var registrationEvent = new AnalyticsEvent
            {
                EventName = "user_registered",
                Timestamp = DateTime.Now,
                Parameters = new Dictionary<string, object>()
            };

            // Add default user properties
            registrationEvent.Parameters["user_id"] = SystemInfo.deviceUniqueIdentifier;
            registrationEvent.Parameters["cohort"] = CohortManager.GetUserCohort();
            registrationEvent.Parameters["device_model"] = SystemInfo.deviceModel;
            registrationEvent.Parameters["operating_system"] = SystemInfo.operatingSystem;
            registrationEvent.Parameters["app_version"] = Application.version;

            // Add any additional parameters
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    registrationEvent.Parameters[param.Key] = param.Value;
                    _userProperties[param.Key] = param.Value; // Store as user property
                }
            }

            _analyticsEvents.Add(registrationEvent);
            SaveAnalytics();

            Debug.Log($"LocalAnalyticsService: User registered with cohort {CohortManager.GetUserCohort()}");
        }

        public void Track(string eventName, Dictionary<string, object> parameters = null)
        {
            var analyticsEvent = new AnalyticsEvent
            {
                EventName = eventName,
                Timestamp = DateTime.Now,
                Parameters = new Dictionary<string, object>()
            };

            // Always include user context
            analyticsEvent.Parameters["user_id"] = SystemInfo.deviceUniqueIdentifier;
            analyticsEvent.Parameters["cohort"] = CohortManager.GetUserCohort();

            // Add custom parameters
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    analyticsEvent.Parameters[param.Key] = param.Value;
                }
            }

            _analyticsEvents.Add(analyticsEvent);
            SaveAnalytics();

            Debug.Log($"LocalAnalyticsService: Tracked event '{eventName}' with {analyticsEvent.Parameters.Count} parameters");
        }

        public void Screen(UIScreenBase screen, Dictionary<string, object> parameters = null)
        {
            var screenEvent = new AnalyticsEvent
            {
                EventName = "screen_viewed",
                Timestamp = DateTime.Now,
                Parameters = new Dictionary<string, object>()
            };

            // Add screen information
            screenEvent.Parameters["screen_name"] = screen.ScreenName;
            screenEvent.Parameters["user_id"] = SystemInfo.deviceUniqueIdentifier;
            screenEvent.Parameters["cohort"] = CohortManager.GetUserCohort();

            // Add custom parameters
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    screenEvent.Parameters[param.Key] = param.Value;
                }
            }

            _analyticsEvents.Add(screenEvent);
            SaveAnalytics();

            // Debug.Log($"LocalAnalyticsService: Screen viewed - {screen.ScreenName}");
        }

        public void Flush()
        {
            Debug.Log("=== ANALYTICS FLUSH - COMPLETE DATA DUMP ===");
            Debug.Log($"Total Events Collected: {_analyticsEvents.Count}");
            Debug.Log($"User Registered: {_isRegistered}");
            Debug.Log($"User Cohort: {CohortManager.GetUserCohort()}");
            
            if (_userProperties.Count > 0)
            {
                Debug.Log("=== USER PROPERTIES ===");
                foreach (var prop in _userProperties)
                {
                    Debug.Log($"  {prop.Key}: {prop.Value}");
                }
            }

            Debug.Log("=== ALL EVENTS ===");
            for (int i = 0; i < _analyticsEvents.Count; i++)
            {
                var evt = _analyticsEvents[i];
                Debug.Log($"Event {i + 1}: {evt.EventName} at {evt.Timestamp:yyyy-MM-dd HH:mm:ss}");
            }

            Debug.Log("=== END ANALYTICS FLUSH ===");

            // In a real implementation, this would send data to the MixPanel
        }

        private void SaveAnalytics()
        {
            try
            {
                var analyticsData = new AnalyticsData
                {
                    Events = _analyticsEvents,
                    UserProperties = _userProperties,
                    IsRegistered = _isRegistered
                };

                string jsonData = JsonUtility.ToJson(analyticsData);
                PlayerPrefs.SetString(ANALYTICS_KEY, jsonData);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogError($"LocalAnalyticsService: Failed to save analytics - {ex.Message}");
            }
        }

        private void LoadStoredAnalytics()
        {
            try
            {
                if (PlayerPrefs.HasKey(ANALYTICS_KEY))
                {
                    string jsonData = PlayerPrefs.GetString(ANALYTICS_KEY);
                    var analyticsData = JsonUtility.FromJson<AnalyticsData>(jsonData);
                    
                    _analyticsEvents = analyticsData.Events ?? new List<AnalyticsEvent>();
                    _userProperties = analyticsData.UserProperties ?? new Dictionary<string, object>();
                    _isRegistered = analyticsData.IsRegistered;

                    Debug.Log($"LocalAnalyticsService: Loaded {_analyticsEvents.Count} stored events");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"LocalAnalyticsService: Failed to load analytics - {ex.Message}");
                _analyticsEvents = new List<AnalyticsEvent>();
                _userProperties = new Dictionary<string, object>();
            }
        }

        [System.Serializable]
        private class AnalyticsData
        {
            public List<AnalyticsEvent> Events;
            public Dictionary<string, object> UserProperties;
            public bool IsRegistered;
        }

        [System.Serializable]
        private class AnalyticsEvent
        {
            public string EventName;
            public DateTime Timestamp;
            public Dictionary<string, object> Parameters;
        }
    }
}

using UnityEngine;

namespace Util
{
    public static class CohortManager
    {
        private static string _userCohort;
        private const string COHORT_KEY = "UserCohort";

        public enum Cohort
        {
            VariantA,
            VariantB,
            VariantC
        }

        /// <summary>
        /// Assigns a random cohort to the user if not already assigned
        /// </summary>
        public static void AssignRandomCohort()
        {
            if (string.IsNullOrEmpty(_userCohort))
            {
                // Check if user already has a cohort saved
                if (PlayerPrefs.HasKey(COHORT_KEY))
                {
                    _userCohort = PlayerPrefs.GetString(COHORT_KEY);
                    Debug.Log($"CohortManager: Existing cohort loaded - {_userCohort}");
                }
                else
                {
                    // Randomly assign one of the three cohorts
                    Cohort[] cohorts = { Cohort.VariantA, Cohort.VariantB, Cohort.VariantC };
                    Cohort selectedCohort = cohorts[Random.Range(0, cohorts.Length)];
                    _userCohort = selectedCohort.ToString();
                    
                    // Save the cohort for future sessions
                    PlayerPrefs.SetString(COHORT_KEY, _userCohort);
                    PlayerPrefs.Save();
                    
                    Debug.Log($"CohortManager: New cohort assigned - {_userCohort}");
                }
            }
        }

        /// <summary>
        /// Gets the current user's cohort
        /// </summary>
        public static string GetUserCohort()
        {
            if (string.IsNullOrEmpty(_userCohort))
            {
                AssignRandomCohort();
            }
            return _userCohort;
        }

        /// <summary>
        /// Gets the current user's cohort as enum
        /// </summary>
        public static Cohort GetUserCohortEnum()
        {
            string cohortString = GetUserCohort();
            if (System.Enum.TryParse(cohortString, out Cohort cohort))
            {
                return cohort;
            }
            return Cohort.VariantA; // Default fallback
        }

        /// <summary>
        /// Forces a specific cohort (for testing purposes)
        /// </summary>
        public static void SetCohort(Cohort cohort)
        {
            _userCohort = cohort.ToString();
            PlayerPrefs.SetString(COHORT_KEY, _userCohort);
            PlayerPrefs.Save();
            Debug.Log($"CohortManager: Cohort manually set to - {_userCohort}");
        }

        /// <summary>
        /// Clears the assigned cohort (for testing purposes)
        /// </summary>
        public static void ClearCohort()
        {
            _userCohort = null;
            PlayerPrefs.DeleteKey(COHORT_KEY);
            Debug.Log("CohortManager: Cohort cleared");
        }
    }
}

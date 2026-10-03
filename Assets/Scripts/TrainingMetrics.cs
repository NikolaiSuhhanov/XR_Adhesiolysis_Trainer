using UnityEngine;
using TMPro;

public class TrainingMetrics : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text metricsText;

    private float startTime;
    private float completionTime;

    private int excessiveTractionEvents;
    private int invalidCutAttempts;

    private bool trainingStarted;
    private bool trainingCompleted;

    public void StartTraining()
    {
        startTime = Time.time;

        completionTime = 0f;
        excessiveTractionEvents = 0;
        invalidCutAttempts = 0;

        trainingStarted = true;
        trainingCompleted = false;
    }

    public void RegisterExcessiveTraction()
    {
        if (!trainingStarted || trainingCompleted)
            return;

        excessiveTractionEvents++;
    }

    public void RegisterInvalidCutAttempt()
    {
        if (!trainingStarted || trainingCompleted)
            return;

        invalidCutAttempts++;
    }

    public void CompleteTraining()
    {
        if (!trainingStarted || trainingCompleted)
            return;

        completionTime = Time.time - startTime;
        trainingCompleted = true;

        UpdateResultsUI();
    }

    private void UpdateResultsUI()
    {
        if (metricsText == null)
            return;

        int minutes = Mathf.FloorToInt(completionTime / 60f);
        int seconds = Mathf.FloorToInt(completionTime % 60f);

        metricsText.text =
            $"Completion time: {minutes:00}:{seconds:00}\n" +
            $"Excessive traction events: {excessiveTractionEvents}\n" +
            $"Invalid cut attempts: {invalidCutAttempts}";
    }
}
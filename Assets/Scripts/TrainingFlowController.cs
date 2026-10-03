using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainingFlowController : MonoBehaviour
{
    public enum TrainingState
    {
        Intro,
        Training,
        Completed
    }

    [Header("Controls Lock")]
    [SerializeField] private Behaviour[] controlsToLock;

    [Header("Adhesions")]
    public CuttableAdhesion adhesion1;
    public CuttableAdhesion adhesion2;

    [Header("UI")]
    public GameObject introPanel;
    public GameObject trainingPanel;
    public GameObject completionPanel;

    public TrainingState CurrentState { get; private set; }

    private void Start()
    {
        ShowIntro();
        SetControlsEnabled(false);
    }


    private void Update()
    {
        if (CurrentState != TrainingState.Training)
            return;

        CheckCompletion();
    }

    public void StartTraining()
    {
        CurrentState = TrainingState.Training;

        if (introPanel != null)
            introPanel.SetActive(false);

        if (trainingPanel != null)
            trainingPanel.SetActive(true);

        if (completionPanel != null)
            completionPanel.SetActive(false);
        
        SetControlsEnabled(true);

        Debug.Log("Training started.");
    }

    private void CheckCompletion()
    {
        if (adhesion1 == null || adhesion2 == null)
            return;

        if (adhesion1.isCut && adhesion2.isCut)
        {
            CompleteTraining();
        }
    }

    private void CompleteTraining()
    {
        CurrentState = TrainingState.Completed;

        if (trainingPanel != null)
            trainingPanel.SetActive(false);

        if (completionPanel != null)
            completionPanel.SetActive(true);

        SetControlsEnabled(false);

        Debug.Log("Training completed.");
    }

    private void ShowIntro()
    {
        CurrentState = TrainingState.Intro;

        if (introPanel != null)
            introPanel.SetActive(true);

        if (trainingPanel != null)
            trainingPanel.SetActive(false);

        if (completionPanel != null)
            completionPanel.SetActive(false);
    }
    private void SetControlsEnabled(bool state)
    {
        if (controlsToLock == null)
            return;

        foreach (Behaviour control in controlsToLock)
        {
            if (control != null)
                control.enabled = state;
        }
    }
    public void RestartTraining()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}
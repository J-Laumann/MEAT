using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    /// <summary>
    /// Checks to see if the game is paused or not
    /// </summary>
    public bool isPaused;

    public GameObject pauseMenu;

    // Start is called before the first frame update
    void Start()
    {
        // Starts the game not paused
        isPaused = false;
        Time.timeScale = 1;
        pauseMenu.SetActive(false);

        // Options menu is not active at the beginning of the game
        //optionsMenu.SetActive(false);

        UpdateQualityLabel();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            pauseMenu.SetActive(true);

            // If false becomes true or if true becomes false
            isPaused = !isPaused;

            // If isPaused is true, freeze things on screen
            if (isPaused)
            {
                Time.timeScale = 0;
            }
            // If isPaused is false, time is normal
            else
            {
                Time.timeScale = 1;
            }

            pauseMenu.SetActive(isPaused);
        }


        
    }
    /// <summary>
    /// When the game is not paused, the pause menu is not visible and
    /// the game speed is normal
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    /// <summary>
    /// Restarts the game
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    /// <summary>
    /// Allows the quality of the game to be edited in the options menu
    /// </summary>
    private void UpdateQualityLabel()
    {
        int currentQuality = QualitySettings.GetQualityLevel();

        string qualityName = QualitySettings.names[currentQuality];

        //Transform qualityChild = optionsMenu.transform.Find("Quality Level");

        //Text qualityText = qualityChild.GetComponent<Text>();

        //qualityText.text = "Quality Level: " + qualityName;
    }

    /// <summary>
    /// Increases the quality of the game
    /// </summary>
    public void IncreaseQuality()
    {
        QualitySettings.IncreaseLevel();
        UpdateQualityLabel();
    }

    /// <summary>
    /// Decreases the quality of the game
    /// </summary>
    public void DecreaseQuality()
    {
        QualitySettings.DecreaseLevel();
        UpdateQualityLabel();
    }

    /*
    public void ConfirmMenu()
    {
        quitMenu.SetActive(true);

        pauseMenu.SetActive(false);
    }

    /// <summary>
    /// If no is clicked, turn the menu off
    /// </summary>
    
    public void NoQuit()
    {
        quitMenu.SetActive(false);

        pauseMenu.SetActive(true);
    }*/
}
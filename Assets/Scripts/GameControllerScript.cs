using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameControllerScript : MonoBehaviour
{
    [SerializeField]
    GameObject PuzzlePanel;
    [SerializeField]
    GameObject SettingsPanel;
    [SerializeField]
    GameObject WinPanel;
    [SerializeField]
    GameObject LosePanel;
    private float elapsedTime;
    private float levelTime;
    private int secondsElapsed = 0;

    [SerializeField]
    private Board board;
    public int score = 0;
    public Text timerDisplay;
    public Text ScoreDisplay;

    [SerializeField]
    private bool isInGame;
    [SerializeField]
    private Text labelText;

    private int currLevel;
    private int oneStarScore;
    private int twoStarScore;
    private int threeStarScore;
    [SerializeField]
    List<GameObject> winStars;
    [SerializeField] Text winScoreLabel;
    [SerializeField] Text loseScoreLabel;
    [SerializeField] Slider _volumeSlider;
    [SerializeField] AudioSource _musicSource;
    [SerializeField] AudioSource _soundSource;
    [SerializeField] AudioSource _winSoundSource;
    [SerializeField] AudioSource _loseSoundSource;
    [SerializeField] Button _nextLevelButton;
    [SerializeField] AudioSource matchSoundSource;
    private void Start()
    {
        currLevel = PlayerPrefs.GetInt("currLevel", 1);
        labelText.text += currLevel.ToString();
        float volume = PlayerPrefs.GetFloat("volume", 1f);
        _volumeSlider.value = _musicSource.volume = _soundSource.volume = _winSoundSource.volume = _loseSoundSource.volume = matchSoundSource.volume = volume;
        _musicSource.volume = volume - 0.2f;
        // Calculate time and score thresholds for the current level
        levelTime = 155 + (currLevel - 1) * 10; // Level 1 starts with 2m 35s (155 seconds) and adds 10s for each level
        oneStarScore = 200 * currLevel;
        twoStarScore = 500 * currLevel;
        threeStarScore = 1000 * currLevel;

        isInGame = true;
        StartCoroutine(LevelTimer());
    }
   public void changeVolume()
    {
        PlayerPrefs.SetFloat("volume",_volumeSlider.value);
        _musicSource.volume = _soundSource.volume = _winSoundSource.volume = _loseSoundSource.volume = matchSoundSource.volume = _volumeSlider.value;
        _musicSource.volume = _soundSource.volume - 0.2f;
    }  
    public void muteVolume()
    {
        PlayerPrefs.SetFloat("volume", 0);
        _musicSource.volume = _soundSource.volume = _volumeSlider.value= _winSoundSource.volume = _loseSoundSource.volume = matchSoundSource.volume =  0;
    } 
    public void unmuteVolume()
    {
        PlayerPrefs.SetFloat("volume",1f);
        _musicSource.volume = _soundSource.volume = _volumeSlider.value = _winSoundSource.volume = _loseSoundSource.volume = matchSoundSource.volume = 1;
        _musicSource.volume = 0.6f;
    }

    void UpdateTimerDisplay()
    {
        int minutes = (int)(levelTime - elapsedTime) / 60;
        int displaySeconds = (int)(levelTime - elapsedTime) % 60;
        timerDisplay.text = string.Format("{0:00}:{1:00}", minutes, displaySeconds);
    }

    private IEnumerator LevelTimer()
    {
        while (elapsedTime < levelTime && isInGame)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
            yield return null;
        }
        WinHandler();
    }

    public void BackButtonHandler()
    {
        clickSound();
        SceneManager.LoadScene("MainMenuScene");
    }

    public void SettingButtonHundler()
    {
        clickSound();
        PuzzlePanel.SetActive(false);
        SettingsPanel.SetActive(false);
    }
    public void nextLevel()
    {
        clickSound();
        PlayerPrefs.SetInt("currLevel", currLevel + 1);
        SceneManager.LoadScene("GameScene");
    }  
    public void repeatLevel()
    {
        clickSound();
        SceneManager.LoadScene("GameScene");
    }
    void clickSound()
    {
        _soundSource.Play();
    }
    public void WinHandler()
    {
        if (!isInGame) return;
        bool won = false;
        isInGame = false;
        score = board._score;
        ScoreDisplay.text = score.ToString();
        PuzzlePanel.SetActive(false);
        WinPanel.SetActive(true);

        int stars = 0;
        if (score >= threeStarScore)
        {
            stars = 3;
            winStars[0].SetActive(true);
            winStars[2].SetActive(true);
            winStars[1].SetActive(true);
            won = true;
        }
        else if (score >= twoStarScore)
        {
            stars = 2;
            winStars[1].SetActive(true);
            winStars[0].SetActive(true);
            won = true;
        }
        else if (score >= oneStarScore)
        {
            stars = 1;
            winStars[0].SetActive(true);
            won = true;
        }
        
        PlayerPrefs.SetInt($"starsLvl{currLevel}",stars);
        Debug.Log($"Player won with {stars} star(s).");
        // Display
        if (won)
        {
            _musicSource.Stop();
            _winSoundSource.Play();
            int UnlockedLevel = PlayerPrefs.GetInt("UnlockedLevels", 1);
            if ((currLevel == UnlockedLevel) && (UnlockedLevel < 24))
                PlayerPrefs.SetInt("UnlockedLevels", currLevel + 1);
            _nextLevelButton.interactable = !(currLevel == 24);
            winScoreLabel.text += score;
            WinPanel.SetActive(true);
            
        } else {
            _musicSource.Stop();
            _loseSoundSource.Play();
            loseScoreLabel.text += score;
            LosePanel.SetActive(true);
        }

    }
}

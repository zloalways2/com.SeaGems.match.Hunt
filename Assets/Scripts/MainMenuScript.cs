using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MainMenuScript : MonoBehaviour
{
    [SerializeField]
    GameObject PrivacyPanel;
    [SerializeField]
    GameObject loadingPanel;
    [SerializeField]
    GameObject MenuPanel;
    [SerializeField]
    GameObject levelsPanel;
    [SerializeField]
    GameObject SettingsPanel;
    private bool _privacyAccepted;
    private bool isInGame;
    [SerializeField]
    private Slider _loadingSlider;
    [SerializeField] Slider _volumeSlider;
    [SerializeField] AudioSource _musicSource;
    [SerializeField] AudioSource _soundSource;
    void Start()
    {
        float volume = PlayerPrefs.GetFloat("volume", 1f);
        _volumeSlider.value = _musicSource.volume = _soundSource.volume = volume;
        _musicSource.volume = volume-0.2f;
        StartCoroutine(loadingAnimation());
    }
    public void clickSound()
    {
        _soundSource.Play();
    }
    public void changeVolume()
    {
        PlayerPrefs.SetFloat("volume", _volumeSlider.value);
        _musicSource.volume = _soundSource.volume = _volumeSlider.value;
        _musicSource.volume = _soundSource.volume - 0.2f;
    }
    public void muteVolume()
    {
        PlayerPrefs.SetFloat("volume", 0);
        _musicSource.volume = _soundSource.volume = _volumeSlider.value = 0;
    }
    public void unmuteVolume()
    {
        PlayerPrefs.SetFloat("volume", 1f);
        _musicSource.volume = _soundSource.volume = _volumeSlider.value = 1;
        _musicSource.volume = 0.6f;
    }
    IEnumerator loadingAnimation()
    {
        var privacyAgreed = PlayerPrefs.GetInt("agreedOnPolicy", 0);
        yield return new WaitForSeconds(0.25f);
        _loadingSlider.value = 0.55f;
        yield return new WaitForSeconds(0.13f);
        _loadingSlider.value = 0.77f;
        yield return new WaitForSeconds(0.19f);
        _loadingSlider.value = 0.91f;
        if (privacyAgreed == 1)
        {
            _loadingSlider.value = 1;
            loadingPanel.SetActive(false);
            MenuPanel.SetActive(true);
        }
        else
        {
            _loadingSlider.value = 1;
            loadingPanel.SetActive(false);
            PrivacyPanel.SetActive(true);
        }
    }

    public void AcceptPolicy()
    {
        _privacyAccepted = true;
        PlayerPrefs.SetInt("agreedOnPolicy", 1);
    }

   

    public void PrivacyOkButtonClicked()
    {
        AcceptPolicy();
        PrivacyPanel.SetActive(false );
        MenuPanel.SetActive(true);
    }

    public void PlayButtonClicked()
    {
        levelsPanel.SetActive(true);
        MenuPanel.SetActive(false);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}


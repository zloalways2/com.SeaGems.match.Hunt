using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectionMenuManager : MonoBehaviour
{
   

    [SerializeField]
    private Button musicOffButton;
    [SerializeField]
    private Button musicOnButton;
    public GameObject menuHandler;
   
    public LevelObject[] levelObjects;
    public List<Button> lvlButtons;
    public Sprite goldenStarSprite;
    public static int currLevel;
    public static int UnlockedLevel;

    [SerializeField]
    private Transform puzzleFields;

    [SerializeField]
    private GameObject btn;

    

    [SerializeField]
    private Sprite[] numberSprites;

    //private Text lvlNum;
    public void OnClickLevel(int levelNum)
    {
        currLevel = levelNum;
        SceneManager.LoadScene("GameLevel");
    }

    // Update is called once per frame
    void Start()
    {

     initializeLevelButtons();
    }
    public void initializeLevelButtons()
    {
        UnlockedLevel = PlayerPrefs.GetInt("UnlockedLevels", 0);
        Debug.Log($"unlocked levels : {UnlockedLevel}");    
        for (int i = 0; i < lvlButtons.Count; i++)
        {
            if (i <= UnlockedLevel)
            {
                lvlButtons[i].interactable = true;
                int stars = PlayerPrefs.GetInt("stars" + i.ToString(), 0);
                Debug.Log($"Stars [{i}] : {stars}");
                for(int j= 0; j < stars; j++)
                {
                    lvlButtons[i].GetComponent<LevelObject>().stars[j].sprite = goldenStarSprite;
                }
            }
            else
            {
                lvlButtons[i].interactable = false;
            }
        }
    }
    private void Awake()
    {
        for (int i = 0; i < 20; i++)
        {
            GameObject button = Instantiate(btn);
            button.name = "" + i;
            button.transform.SetParent(puzzleFields, false);

            Image lvlImage = button.transform.GetChild(0).GetComponent<Image>();

            if (lvlImage != null && i < numberSprites.Length)
            {
                lvlImage.sprite = numberSprites[i];
            }
            Button btnComponent = button.GetComponent<Button>();
            int levelNum = i;
            btnComponent.onClick.AddListener(() => OnClickLevel(levelNum));
            lvlButtons.Add(btnComponent);
            initializeLevelButtons();
        }
    }

   


   
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    
}

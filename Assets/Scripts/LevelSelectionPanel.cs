using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectionPanel : MonoBehaviour
{
    public GameObject menuHandler;

    public static int currLevel;
    public static int UnlockedLevel;

    [SerializeField]
    private Transform puzzleFields;

    [SerializeField]
    private GameObject btn;
    List<Button> lvlButtons;


    [SerializeField]
    private Sprite[] _stars;
    [SerializeField]
    private Sprite _lockedLevelSprite;

    //private Text lvlNum;
    public void OnClickLevel(int levelNum)
    {
        currLevel = levelNum;
        PlayerPrefs.SetInt("currLevel", currLevel);
        SceneManager.LoadScene("GameScene");
    }

    // Update is called once per frame
    void Start()
    {
        lvlButtons = new List<Button>();

        initializeLevelButtons();
    }
    void changeStars(Button btn, int stars)
    {
        if (stars == 99) btn.transform.Find("stars").gameObject.GetComponent<Image>().sprite = _lockedLevelSprite;
        else
        btn.transform.Find("stars").gameObject.GetComponent<Image>().sprite = _stars[stars];
    }
    public void initializeLevelButtons()
    {
        UnlockedLevel = PlayerPrefs.GetInt("UnlockedLevels", 1);
        Debug.Log($"unlocked levels : {UnlockedLevel}");
        for (int i = 1; i <= 24; i++)
        {
            var button = puzzleFields.Find($"btn{i}").gameObject;
            lvlButtons.Add(button.GetComponent<Button>());
            if (i <= UnlockedLevel)
            {
                lvlButtons[i-1].interactable = true;
                int stars = PlayerPrefs.GetInt($"starsLvl{i}", 0);
                Debug.Log($"Stars [{i}] : {stars}");
                changeStars(lvlButtons[i - 1], stars);
            }
            else
            {
                lvlButtons[i-1].interactable = false;
                changeStars(lvlButtons[i - 1], 99);
            }
        }
    }
    private void Awake()
    {
        //for (int i = 0; i < 24; i++)
        //{
        //    GameObject button = Instantiate(btn);
        //    button.name = "" + i;
        //    button.transform.SetParent(puzzleFields, false);

        //    Image lvlImage = button.transform.GetChild(0).GetComponent<Image>();

        //    if (lvlImage != null && i < numberSprites.Length)
        //    {
        //        lvlImage.sprite = numberSprites[i];
        //    }
        //    Button btnComponent = button.GetComponent<Button>();
        //    int levelNum = i;
        //    btnComponent.onClick.AddListener(() => OnClickLevel(levelNum));
        //    lvlButtons.Add(btnComponent);
        //    initializeLevelButtons();
        //}
    }
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}

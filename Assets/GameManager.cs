
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum ColorChoice
{
    Green,
    Red
}

public enum GameState
{
    SET,
    PLAY,
    WAIT,
}

public class GameManager : MonoBehaviour
{
    [SerializeField] public int ButtonAddedPerRound;
    public int currentRound { get; private set; } = 0;
    private int numberOfButton => currentRound * ButtonAddedPerRound;
    [SerializeField] public GameObject SubmitButton;
    [SerializeField] public GameObject GameButtonPrefab;
    [SerializeField] public GameObject GreenField;
    [SerializeField] public GameObject RedField;
    private Hashtable GreenButtonHash;
    private Hashtable RedButtonHash;
    private GameState gameState;
    private Image submitImage;
    private Button submitButton;
    [SerializeField] public int MaxLives;
    private int currentLives;
    public float ShowAnswerFor;
    private float timer = 0;

    private void Awake()
    {
        submitButton = SubmitButton.GetComponent<Button>();
        submitImage = SubmitButton.GetComponent<Image>();
    }
    private void Start()
    {
        submitButton.onClick.AddListener(SubmitButtonOnClick);
        GreenButtonHash = new Hashtable();
        RedButtonHash = new Hashtable();
        currentLives = MaxLives;
        gameState = GameState.SET;
    }

    private void Update()
    {
        switch (gameState)
        {
            case GameState.SET:
                SetUpdate();
                break;
            case GameState.PLAY:
                break;
            case GameState.WAIT:
                WaitUpdate();
                break;
            default:
                break;
        }
    }

    public void SubmitButtonOnClick()
    {
        if(gameState == GameState.PLAY)
        { 
        if (CheckAnswers())
        {
            StartCoroutine(ButtonColorPulse(submitImage, 3, Color.white, Color.green));
        }
        else
        {
            StartCoroutine(ButtonColorPulse(submitImage, 3, Color.white, Color.red));
            currentLives -= 1;
            if(currentLives <= 0)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else currentRound -= 1;
        }

        gameState = GameState.SET;
            }
    }

    private bool CheckAnswers()
    {
        foreach (DictionaryEntry button in GreenButtonHash)
        {
            if (!CheckButton((GameObject)button.Value)) return false;
            else continue;
        }

        foreach (DictionaryEntry button in RedButtonHash)
        {
            if (CheckButton((GameObject)button.Value)) return false;
            else continue;
        }

        return true;
    }

    private bool CheckButton(GameObject button)
    {
        return button.CompareTag("Green");
    }

    private void createButtons()
    {

        for (int i = 0; i < numberOfButton; i++)
        {
            GameObject button = Instantiate(GameButtonPrefab);
            button.tag = GetRandomColor();
            ColorButton(button);
            int randomNumber = AssignRandomNumber(button);
            button.GetComponent<Button>().onClick.AddListener(() => SwapSide(button));
            GreenButtonHash.Add(randomNumber, button);
            button.transform.parent = GreenField.transform;
        }
    }


    private string GetRandomColor()
    {
        Array values = typeof(ColorChoice).GetEnumValues();
        return values.GetValue(UnityEngine.Random.Range(0, values.Length)).ToString();
    }

    private void ColorButton(GameObject button)
    {
        if (button.CompareTag("Green")) button.GetComponent<Image>().color = Color.green;
        else button.GetComponent<Image>().color = Color.red;
    }

    private int AssignRandomNumber(GameObject button)
    {
        int random = UnityEngine.Random.Range(0, 9999);
        while (GreenButtonHash.ContainsKey(random))
        {
            random = UnityEngine.Random.Range(0, 9999);
        }

        button.GetComponentInChildren<Text>().text = random.ToString();

        return random;
    }

    public IEnumerator ButtonColorPulse(Image button, float duration, Color colorA, Color colorB)
    {
        float timeEndColor = Time.time + duration;
        while (Time.time < timeEndColor)
        {
            button.color = Color.Lerp(colorA, colorB, (timeEndColor - Time.time) / duration);
            yield return null;
        }
    }

    private void SwapSide(GameObject button)
    {
        if (gameState == GameState.PLAY)
        {
            int buttonKey = Int32.Parse(button.GetComponentInChildren<Text>().text);
            if (GreenButtonHash.Contains(buttonKey))
            {
                GreenButtonHash.Remove(buttonKey);
                RedButtonHash.Add(buttonKey, button);
                button.transform.parent = RedField.transform;
            }
            else if (RedButtonHash.Contains(buttonKey))
            {
                RedButtonHash.Remove(buttonKey);
                GreenButtonHash.Add(buttonKey, button);
                button.transform.parent = GreenField.transform;
            }
        }
    }

    private void SetUpdate()
    {
        CleanGame();
        currentRound += 1;
        createButtons();
        gameState = GameState.WAIT;
    }

    private void WaitUpdate()
    {
        timer += Time.deltaTime;
        if(timer >= ShowAnswerFor)
        {
            foreach (DictionaryEntry button in GreenButtonHash)
            {
                ((GameObject)button.Value).GetComponent<Image>().color = Color.white;
            }
            timer = 0;
            gameState = GameState.PLAY;
        }
    }

    private void cleanPanel(Transform panel)
    {
        foreach (Transform child in panel)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    private void CleanGame()
    {
        if (GreenButtonHash.Count > 0)
        {
            GreenButtonHash.Clear();
            cleanPanel(GreenField.transform);
        }

        if (RedButtonHash.Count > 0)
        { 
            RedButtonHash.Clear();
            cleanPanel(RedField.transform);
        }
    }
}

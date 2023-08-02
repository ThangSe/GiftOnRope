using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class UIManager: MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _fullHeartImgRef, _emptyHeartImgRef, _audioMuteRef, _audioUnMuteRef;
    [SerializeField] private Image[] _hearts;
    [SerializeField] private Text _scoreText, _gameoverScoreText, _turnPlayLeftText;
    [SerializeField] private Transform _textSpawnPos;
    [SerializeField] private Transform _backgroundSprite;
    [SerializeField] private GameObject _backgroundPanel, _homePage, _tutorialPage, _basePage, _gamePage, _gameOverPage;
    private Image _audioImage;
    private Color _defaultColor;
    private Color _flashColor;
    private float _flashTime = .1f;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioGamePlay, _audioPoint, _audioLoseHeart, _audioGameOver;

    [Header("Button")]
    [SerializeField] private Button _audioButton;

    private float _currentScore;

    private void Start()
    {
        _audioImage = _audioButton.transform.GetComponent<Image>();
        _audioImage.sprite = _audioUnMuteRef.sprite;
        ColorUtility.TryParseHtmlString("#EBEBEB", out _defaultColor);
        ColorUtility.TryParseHtmlString("#FFFFFF", out _flashColor);
        _turnPlayLeftText.text = MainScript.Instance.TurnPlayLeft().ToString();
        MainScript.Instance.OnStateChanged += MainScript_OnStateChanged;
        MainScript.Instance.OnPassCircle += MainScript_OnPassCircle;
        MainScript.Instance.OnHeartChanged += MainScript_OnHeartChanged;
        MainScript.Instance.OnToggleButtonSound += MainScript_OnToggleButtonSound;
    }

    private void MainScript_OnToggleButtonSound(object sender, MainScript.OnToggleButtonSoundEventArgs e)
    {
        if (e.isMuteSound)
        {
            _audioImage.sprite = _audioMuteRef.sprite;
        }
        if (!e.isMuteSound)
        {
            _audioImage.sprite = _audioUnMuteRef.sprite;
        }
    }

    private void MainScript_OnHeartChanged(object sender, MainScript.OnHeartChangedEventArgs e)
    {
        _audioLoseHeart.Play();
        SetUIHeart(e.currentHeart);
        CreatePopUpLoseHeart(e.newPopups, e.loseheartPopup);
    }

    private void SetUIHeart(int currentHeart)
    {
        _hearts[currentHeart].sprite = _emptyHeartImgRef.sprite;
    }

    private void MainScript_OnPassCircle(object sender, MainScript.OnPassCircleEventArgs e)
    {
        _currentScore = e.currentScore;
        _audioPoint.Play();
        _scoreText.text = _currentScore.ToString();
        CreatePopUp(e.newPopups, e.scorePopup, e.scoreReceived);
        StartCoroutine(FlashScreen(_flashTime));
    }

    private IEnumerator FlashScreen(float flashTime)
    {
        _backgroundSprite.GetComponent<SpriteRenderer>().color = _flashColor;
        yield return new WaitForSeconds(flashTime);
        _backgroundSprite.GetComponent<SpriteRenderer>().color = _defaultColor;
    }

    private void MainScript_OnStateChanged(object sender, System.EventArgs e)
    {
        if(MainScript.Instance.IsHomeState())
        {
            _gameOverPage.SetActive(false);
            _backgroundPanel.SetActive(true);
            _gamePage.SetActive(false);
            _homePage.SetActive(true);
        }
        if(MainScript.Instance.IsWaitingToStartState())
        {
            _audioGameOver.volume = 0f;
            _gameOverPage.SetActive(false);
            _backgroundPanel.SetActive(true);
            _gamePage.SetActive(false);
            _homePage.SetActive(false);
            _tutorialPage.SetActive(true);
        }
        if(MainScript.Instance.IsCountDownToStartState())
        {
            _backgroundPanel.SetActive(false);
            _tutorialPage.SetActive(false);
            _gameOverPage.SetActive(false);
            _gamePage.SetActive(true);
            foreach (Image img in _hearts)
            {
                img.sprite = _fullHeartImgRef.sprite;
            }
            _scoreText.text = "0";
        }
        if(MainScript.Instance.IsGamePlayingState())
        {
            _audioGamePlay.time = 0;
            if(!_audioGamePlay.isPlaying) _audioGamePlay.Play();
            _audioGamePlay.volume = 1f;
        }
        if(MainScript.Instance.IsGameOverState())
        {
            _backgroundPanel.SetActive(true);
            _gameOverPage.SetActive(true);
            _gamePage.SetActive(false);
            _gameoverScoreText.text = _currentScore.ToString();
            _turnPlayLeftText.text = MainScript.Instance.TurnPlayLeft().ToString(); 
            _audioGameOver.time = 0;
            if(!_audioGameOver.isPlaying) _audioGameOver.Play();
            _audioGameOver.volume = 1f;
            _audioGamePlay.volume = 0f;
        }
    }

    private void CreatePopUp(List<IItem> newPopup, GameObject scorePopup, int perfectCount)
    {
        AddPopUpToList(_textSpawnPos.position, newPopup, scorePopup, perfectCount);
    }

    private void AddPopUpToList(Vector3 pos, List<IItem> newPopup, GameObject scorePopup, int perfectCount)
    {
        bool isAdded = false;
        for (int i = 0; i < newPopup.Count; i++)
        {
            if (newPopup[i].GetActiveState() == false && newPopup[i].CompareTagGO("TextPopup"))
            {
                newPopup[i].Active(pos, Vector3.zero, perfectCount.ToString());
                isAdded = true;
                break;
            }
        }
        if (isAdded == false)
        {
            newPopup.Add(Factory.CreatePopUp(GameObject.Instantiate(scorePopup, _basePage.transform)));
            newPopup[newPopup.Count - 1].Active(pos, Vector3.zero, perfectCount.ToString());
        }
    }

    private void CreatePopUpLoseHeart(List<IItem> newPopup, GameObject loseHeartPopup)
    {
        bool isAdded = false;
        for (int i = 0; i < newPopup.Count; i++)
        {
            if (newPopup[i].GetActiveState() == false && newPopup[i].CompareTagGO("LoseHeartPopup"))
            {
                newPopup[i].Active(_textSpawnPos.position, Vector3.zero);
                isAdded = true;
                break;
            }
        }
        if (isAdded == false)
        {
            newPopup.Add(Factory.CreatePopUpLoseHeart(GameObject.Instantiate(loseHeartPopup, _basePage.transform)));
            newPopup[newPopup.Count - 1].Active(_textSpawnPos.position, Vector3.zero);
        }
    }
}


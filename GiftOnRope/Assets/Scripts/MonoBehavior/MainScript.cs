using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using UnityEngine.Experimental.AI;

public class MainScript : MonoBehaviour
{
    public static MainScript Instance { get; private set; }   
    public event EventHandler OnStateChanged;
    public event EventHandler<OnHeartChangedEventArgs> OnHeartChanged;
    public event EventHandler<OnPassCircleEventArgs> OnPassCircle;
    public event EventHandler<OnToggleButtonSoundEventArgs> OnToggleButtonSound;

    public class OnToggleButtonSoundEventArgs: EventArgs
    {
        public bool isMuteSound;
    }
    public class OnPassCircleEventArgs : EventArgs
    {
        public int currentScore;
        public List<IItem> newPopups;
        public GameObject scorePopup;
        public int scoreReceived;
    }

    public class OnHeartChangedEventArgs : EventArgs
    {
        public int currentHeart;
        public List<IItem> newPopups;
        public GameObject loseheartPopup;
    }
    [SerializeField] private PendulumGiftRefsSO _pendulumGiftRefsSO;
    [SerializeField] private Transform _textSpawnPos;
    [SerializeField] private Transform _pfCircle;
    [SerializeField] private GameObject _scorePopUp;
    [SerializeField] private GameObject _loseHeartPopup;

    [Header("Button")]
    [SerializeField] private Button _tutorialButton, _playButton, _replayButton, _audioButton;

    [Header("Player")]
    [SerializeField] private Transform _pfGift;

    private GameObject _rope, _gift, _giftPos;
    private TrailRenderer _trailRenderer;

    [Header("Physic")] 
    [SerializeField] private float _calDirFromAngle;
    [SerializeField] private float _pendulumSpeed;
    [SerializeField] private float _multiplerSpeed;
    [SerializeField] private float _powerForce;
    private Rigidbody2D _rb;
    private Vector2 _forceDir = Vector2.zero;
    private float _currentAngleByTime;
    private float _angle;

    private bool _isGameOver;
    private bool _isCut;
    private bool _isMuteSound;
    private bool _isPopup;

    private int _turnPlayLeft;
    private int _turnPlayMax = 7;

    private float _countdownToStartTimer;
    private int _currentHeart;
    private int _multiplier = 1;
    private int _currentPerfectCount;
    private int _currentScore;

    private float _worldHeight;
    private float _worldWidth;

    private List<IItem> _newPopUps;
    private enum State
    {
        Home,
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private State _state;
    private void Awake()
    {
        if(Instance != null ) {
            Destroy(this);
        } else
        {
            Instance = this;

            _worldHeight = Camera.main.orthographicSize * 2;
            _worldWidth = _worldHeight * (float)Screen.width / Screen.height;
            _rope = _pfGift.Find("Rope").gameObject;
            _gift = _pfGift.Find("Gift").gameObject;
            _giftPos = _pfGift.Find("GiftPos").gameObject;
            _calDirFromAngle = _pendulumGiftRefsSO._maxAngleDeflection - 1f;
            _rb = _gift.GetComponent<Rigidbody2D>();
            _trailRenderer = _gift.GetComponent<TrailRenderer>();
            _rb.gravityScale = _pendulumGiftRefsSO._noGravity;
            _newPopUps = new List<IItem>();
            _turnPlayLeft = _turnPlayMax;
            SetUp();
            _state = State.WaitingToStart;
        }
        CircleTrigger.ResetStaticData();   
    }
    private void Start()
    {
        _playButton.onClick.AddListener( () => {
            _state = State.CountdownToStart;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        });
        _tutorialButton.onClick.AddListener(() =>
        {
            if(TurnPlayLeft() > 0)
            {
                _state = State.WaitingToStart;
                OnStateChanged?.Invoke(this, EventArgs.Empty);
            }
        });
        _replayButton.onClick.AddListener(() =>
        {
            SetUp();
            _rope.SetActive(true);
            _gift.transform.position = _giftPos.transform.position;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            _gift.transform.localRotation = Quaternion.Euler(0, 0, 0);
            _rb.gravityScale = _pendulumGiftRefsSO._noGravity;
            _rb.velocity = Vector2.zero;
            _isCut = !_isCut;
            _isGameOver = !_isGameOver;
            
            if(TurnPlayLeft() > 0)
            {
                _state = State.WaitingToStart;
            } else
            {
                _state = State.Home;
            }
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        });
        _audioButton.onClick.AddListener(() =>
        {
            if(_isMuteSound)
            {
                AudioListener.volume = 1;
                
            }
            if(!_isMuteSound) {
                AudioListener.volume = 0;
            }
            _isMuteSound = !_isMuteSound;
            OnToggleButtonSound?.Invoke(this, new OnToggleButtonSoundEventArgs
            {
                isMuteSound = _isMuteSound,
            });
        });
        CircleTrigger.OnPassThroughCircle += CircleTrigger_OnPassThroughCircle;
    }
    private void SetUp()
    {
        Camera.main.transform.Translate(0, Mathf.Abs(Camera.main.transform.position.y), 0);
        _pfCircle.transform.position = _pendulumGiftRefsSO._defaultCirclePos[UnityEngine.Random.Range(0, _pendulumGiftRefsSO._defaultCirclePos.Length)];
        _pfGift.transform.position = _pendulumGiftRefsSO._defaultGiftPos;
        _pendulumSpeed = _pendulumGiftRefsSO._pendulumSpeedDefault;
        _currentHeart = _pendulumGiftRefsSO._maxHeart;
        _currentPerfectCount = _pendulumGiftRefsSO._defaultPerfectCount;
        _currentScore = _pendulumGiftRefsSO._defaultCurrentScore;
        _countdownToStartTimer = _pendulumGiftRefsSO._countdownToStartTimerMax;
        for(int i = 0; i < _newPopUps.Count; i++)
        {
            _newPopUps[i].Deactive();
        }
    }
    private void CircleTrigger_OnPassThroughCircle(object sender, System.EventArgs e)
    {
        _currentPerfectCount++;
        CircleTrigger circle = sender as CircleTrigger;
        _pfGift.position = circle.transform.position;
        _pendulumSpeed = _pendulumGiftRefsSO._pendulumSpeedDefault * _multiplerSpeed;
        _multiplier = PerfectToMul(_currentPerfectCount);
        SetGiftBackToRope();
        StartCoroutine(PopupTimer(1f, true));
        OnPassCircle?.Invoke(this, new OnPassCircleEventArgs
        {
            currentScore = _currentScore,
            newPopups = _newPopUps,
            scorePopup = _scorePopUp,
            scoreReceived = _currentPerfectCount * _pendulumGiftRefsSO._baseIncScore
        });
    }
    IEnumerator PopupTimer(float timer, bool isScored)
    {
        _isPopup = !_isPopup;
        if(isScored) IncScore();
        yield return new WaitForSeconds(timer);
        _isPopup = !_isPopup;
    }
    private void SetGiftBackToRope()
    {
        _isCut = !_isCut;
        _multiplier = PerfectToMul(GetPerfectCount());
        _pendulumSpeed = _pendulumGiftRefsSO._pendulumSpeedDefault;
        _gift.transform.position = _giftPos.transform.position;
        _rope.SetActive(true);
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        _gift.transform.localRotation = Quaternion.Euler(0, 0, 0);
        _rb.gravityScale = _pendulumGiftRefsSO._noGravity;
        _rb.velocity = Vector2.zero;
        // Set current rotation (not past) when setBack
        _pfGift.localRotation = Quaternion.Euler(0, 0, _pendulumGiftRefsSO._maxAngleDeflection * Mathf.Sin(Time.time * _pendulumSpeed));
    }
    private void Update()
    {
        switch (_state)
        {
            case State.WaitingToStart:
                break;

            case State.CountdownToStart:
                _countdownToStartTimer -= Time.deltaTime;
                if (_countdownToStartTimer < 0f)
                {
                    _state = State.GamePlaying;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                    _countdownToStartTimer = _pendulumGiftRefsSO._countdownToStartTimerMax;
                    _trailRenderer.enabled = true;
                }
                break;

            case State.GamePlaying:
                GetKeyDown();
                PendulumMove();
                if(_isPopup) UpdateItemAction();

                if (IsOutSide() && _isCut && HasHeart())
                {
                    _currentHeart--;
                    OnHeartChanged?.Invoke(this, new OnHeartChangedEventArgs
                    {
                        currentHeart = _currentHeart,
                        newPopups = _newPopUps,
                        loseheartPopup = _loseHeartPopup
                    });
                    StartCoroutine(PopupTimer(1f, false));
                    SetGiftBackToRope();
                    _currentPerfectCount = _pendulumGiftRefsSO._defaultPerfectCount;
                }
                if(IsOutSide() && !HasHeart())
                {
                    
                    _turnPlayLeft--;
                    _state = State.GameOver;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;

            case State.GameOver:
                if(!_isGameOver)
                {
                    _trailRenderer.enabled = false;
                    _isGameOver = !_isGameOver;                    
                }
                break;
        }
    }
    private void UpdateItemAction()
    {
        for (int i = 0; i < _newPopUps.Count; i++)
        {
            if (_newPopUps[i].GetActiveState())
            {
                _newPopUps[i].Act();
            }
        }
    }

    private void  PendulumMove()
    {
        if (!_isCut)
        {
            _rb.constraints = RigidbodyConstraints2D.None;
            _currentAngleByTime = Mathf.Sin(Time.time * _pendulumSpeed);
            _angle = _pendulumGiftRefsSO._maxAngleDeflection * _currentAngleByTime;
            _pfGift.localRotation = Quaternion.Euler(0, 0, _angle);
            _powerForce = Mathf.Abs(_currentAngleByTime);
            if (_angle > _calDirFromAngle)
            {
                _forceDir = Vector2.left;
            }
            if (_angle < -_calDirFromAngle)
            {
                _forceDir = Vector2.right;
            }
        }
    }
    private void GetKeyDown()
    {
        if ((Input.GetMouseButtonDown(0)) && !_isCut)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            _isCut = !_isCut;
            _rope.SetActive(false);
            _rb.gravityScale = _pendulumGiftRefsSO._hasGravity;
            _rb.AddForce(_forceDir * _pendulumGiftRefsSO._defaultForce * _powerForce, ForceMode2D.Impulse);

        }
    }  
    private bool IsOutSide()
    {
        if(_isCut)
        {
            if (_gift.transform.position.y < Camera.main.transform.position.y - _worldHeight / 2 || _gift.transform.position.x < -_worldWidth / 2 || _gift.transform.position.x > _worldWidth / 2)
            {
                StartCoroutine(SetTrailTimer(.5f));
                return true;
            } else
            {
                return false;
            }
        } else
        {
            return false;
        }
    }
    IEnumerator SetTrailTimer(float timer)
    {
        _trailRenderer.time = 0;
        yield return new WaitForSeconds(timer);
        _trailRenderer.time = timer;
    }
    private bool HasHeart()
    {
        if(_currentHeart > 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void IncScore()
    {
        _currentScore+= _pendulumGiftRefsSO._baseIncScore * _multiplier;  
    }
    private int PerfectToMul(int perfectCount)
    {
        if (perfectCount >= 2 && perfectCount <= 4)
        {
            _multiplerSpeed = 1.2f;
            return 2;
        }
        else if (perfectCount > 4 && perfectCount <= 8)
        {
            _multiplerSpeed = 1.4f;
            return 3;
        }
        else if (perfectCount > 8 && perfectCount <= 15)
        {
            _multiplerSpeed = 1.6f;
            return 4;
        }
        else if (perfectCount > 15 && perfectCount <= 30)
        {
            _multiplerSpeed = 1.8f;
            return 5;
        }
        else if (perfectCount > 30)
        {
            _multiplerSpeed = 2f;
            return 7;
        }
        else
        {
            _multiplerSpeed = 1;
            return 1;
        }
    }

    public int TurnPlayLeft()
    {
        return _turnPlayLeft;
    }


    private int GetPerfectCount()
    {
        return _currentPerfectCount;
    }
    public bool IsHomeState()
    {
        return _state == State.Home;
    }
    public bool IsWaitingToStartState()
    {
        return _state == State.WaitingToStart;
    }
    public bool IsCountDownToStartState()
    {
        return _state == State.CountdownToStart;
    }
    public bool IsGamePlayingState()
    {
        return _state == State.GamePlaying;
    }
    public bool IsGameOverState()
    {
        return _state == State.GameOver;
    }
}

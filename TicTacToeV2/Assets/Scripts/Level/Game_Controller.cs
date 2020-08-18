using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Game_Controller : MonoBehaviour
{
    [Header("Cell Settings")]
    [SerializeField]
    private Button[] _Cells;
    [SerializeField]
    private Sprite[] _CellSprites;

    [Header("Button Text Settings")]
    [SerializeField]
    private TextMeshProUGUI[] _buttonTextList;

    [Header("UI Settings")]
    [SerializeField]
    private GameObject _gameOverPanel;
    [SerializeField]
    private TextMeshProUGUI _gameOverText;
    [SerializeField]
    private GameObject _restartBtn;
    [SerializeField]
    private GameObject _playerXIdentifierPanel;
    [SerializeField]
    private TextMeshProUGUI _playerXCountdown;
    [SerializeField]
    private GameObject _compuetOIdentifierPanel;
    [SerializeField]
    private TextMeshProUGUI _computerOCountdown;

    [Header("Audio Settings")]
    [SerializeField]
    private AudioClip[] _audioClips;
    [SerializeField]
    private AudioClip _eraser;
    private AudioSource _audioSource;

    [Space(20)]
    public bool _playerMove; //Public so I can disable user clicks when its not their turn
    public Button _lastPlacedMarker { get; set; }
    
    //Private variables, not exposed
    //private int row = 3;
    //private int columns = 3;

    private string _playerMarker = string.Empty;
    private string _computerMarker = string.Empty;

    private int _moveCounter;
    private float _delayTimer;

    //AI
    private int randCellChoice;
    private string _difficultyLevel;

    private void Awake()
    {
        _difficultyLevel = Global_Controller._difficultyLevel;

        //Attempt to convert the current grid into a 2D array.
        /*int[,] _cells = new int[row, columns];

        for(int i = 0; i < row; i++)
        {
            for(int j = 0; j < columns; j++)
            {
                Vector2 position;
                foreach (var button in _Cells)
                {
                    float X = button.transform.position.x;
                    float Y = button.transform.position.y;

                    position = new Vector2(X, Y);
                }

                _cells[position];
            }
        }*/

        _audioSource = GameObject.Find("Canvas").GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            Debug.Log("No Audio Source Found!");
        }

        //Set up some defaults
        _playerMove = true;
        _playerMarker = "X";
        _computerMarker = "O";
        _moveCounter = 0;
        _delayTimer = 5.0f; //Set the countdown start to 5 seconds


        //Set the panels
        _gameOverPanel.SetActive(false);
        _restartBtn.SetActive(false);
        _playerXIdentifierPanel.SetActive(true);
        _compuetOIdentifierPanel.SetActive(false);

        SetGameControllerReferenceOnButtons();
        foreach (var cell in _Cells)
        {
            int randomCellBg = Random.Range(0, _CellSprites.Length);
            cell.GetComponent<Image>().sprite = _CellSprites[randomCellBg]; 
        }
    }

    // Update is called once per frame
    private void Update()
    {
        //WARNING - The AILogic is a mess to read! Don't laugh
        AILogic();

        if(_playerMove == true) 
        {
            if (CountdownTimer() <= 0.0f)
            {
                //Pick a random number from the amount of buttons available.
                randCellChoice = Random.Range(0, _buttonTextList.Length);
                //If the button selected is available
                if (_buttonTextList[randCellChoice].GetComponentInParent<Button>().interactable == true)
                {
                    //Mark the button and disable it.
                    _lastPlacedMarker = _buttonTextList[randCellChoice].GetComponentInParent<Button>();
                    _buttonTextList[randCellChoice].text = _playerMarker;
                    PlaySound();
                    _buttonTextList[randCellChoice].GetComponentInParent<Button>().interactable = false;
                    EndTurn();
                }
                //Could do with an else, due to the random occasionally picking a button already disabled.
            }
        }
    }

    //Set the timer off and chnage the countdown to red once you have 3 seconds left.
    private float CountdownTimer()
    {
        _delayTimer -= 1 * Time.deltaTime;
        if(_playerMove == true)
        {
            _playerXCountdown.text = _delayTimer.ToString("0");
            if(_delayTimer <= 3)
            {
                _playerXCountdown.color = Color.red;
            }
            else
            {
                _playerXCountdown.color = Color.black;
            }
        } 
        else
        {
            _computerOCountdown.text = _delayTimer.ToString("0");
            if (_delayTimer <= 3)
            {
                _computerOCountdown.color = Color.red;
            }
            else
            {
                _computerOCountdown.color = Color.black;
            }
        }
        return _delayTimer;
    }

    private void AILogic()
    {
        //A solid AI that is as dumb as a goldfish but it works.
        if (_difficultyLevel == "Too Easy")
        {
            if (_playerMove != true)
            {
                if (CountdownTimer() <= 0.0f)
                {
                    //Dirty method for "AI" no real logic, just a random cell is chosen.
                    RandomCellSelect();
                }
            }
        }
        else if (_difficultyLevel == "Little Harder")
        {
            //Best case scenario AI would be ---> To use a 2D array, as it allow you to know the positions for both rows & columns. Makes life easier here.
            //A slightly better AI but the above would be better/more efficient/more user friendly
            if (_playerMove != true)
            {
                if (CountdownTimer() <= 0)
                {
                    //Get the last placed marker by the player.
                    Button lastXCell = _lastPlacedMarker;//UsedButton[UsedButton.Count - 1];
                    List<Button> AllUseableButtons = new List<Button>();
                    foreach (var cellButton in _Cells)
                    {
                        //Get all the active buttons
                        if (cellButton.GetComponent<Button>().interactable == true)
                        {
                            AllUseableButtons.Add(cellButton);
                        }
                    }

                    int possibleButtonIndex = 0;
                    //A very brute force approach, this needs extensive refactoring!
                    switch (lastXCell.name)
                    {
                        case "Cell":
                            for (int i = 0; i < AllUseableButtons.Count; i++)
                            {
                                if (AllUseableButtons[i].name == "Cell (1)" || AllUseableButtons[i].name == "Cell (3)")
                                {
                                    possibleButtonIndex++;
                                }
                            }
                            int choice = Random.Range(0, possibleButtonIndex);
                            if (choice == 0 && _buttonTextList[1].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 1;
                            }
                            else if (choice == 1 && _buttonTextList[3].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 3;
                            } 
                            else
                            {
                                if(_buttonTextList[1].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 1;
                                }
                                else if (_buttonTextList[3].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 3;
                                } 
                                else
                                {
                                    RandomCellSelect();
                                }
                            }
                            _buttonTextList[choice].text = _computerMarker;
                            _buttonTextList[choice].GetComponentInParent<Button>().interactable = false;
                            EndTurn();
                            break;
                        case "Cell (1)":
                            for (int i = 0; i < AllUseableButtons.Count; i++)
                            {
                                if (AllUseableButtons[i].name == "Cell" || AllUseableButtons[i].name == "Cell (2)" || AllUseableButtons[i].name == "Cell (4)")
                                {
                                    possibleButtonIndex++;
                                }
                            }
                            choice = Random.Range(0, possibleButtonIndex);
                            if (choice == 0 && _buttonTextList[0].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 0;
                            }
                            else if (choice == 1 && _buttonTextList[2].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 2;
                            }
                            else if (choice == 2 && _buttonTextList[4].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 4;
                            }
                            else
                            {
                                if (_buttonTextList[0].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 0;
                                }
                                else if (_buttonTextList[2].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 2;
                                } 
                                else if (_buttonTextList[4].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 4;
                                }
                                else
                                {
                                    RandomCellSelect();
                                }
                            }
                            _buttonTextList[choice].text = _computerMarker;
                            _buttonTextList[choice].GetComponentInParent<Button>().interactable = false;
                            EndTurn();
                            break;
                        case "Cell (2)":
                            for (int i = 0; i < AllUseableButtons.Count; i++)
                            {
                                if (AllUseableButtons[i].name == "Cell (1)" || AllUseableButtons[i].name == "Cell (5)")
                                {
                                    possibleButtonIndex++;
                                }
                            }
                            choice = Random.Range(0, possibleButtonIndex);
                            if (choice == 0 && _buttonTextList[1].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 1;
                            }
                            else if (choice == 1 && _buttonTextList[5].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 5;
                            }
                            else
                            {
                                if (_buttonTextList[1].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 1;
                                }
                                else if (_buttonTextList[5].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 5;
                                }
                                else
                                {
                                    RandomCellSelect();
                                }
                            }
                            _buttonTextList[choice].text = _computerMarker;
                            _buttonTextList[choice].GetComponentInParent<Button>().interactable = false;
                            EndTurn();
                            break;
                        case "Cell (3)":
                            for (int i = 0; i < AllUseableButtons.Count; i++)
                            {
                                if (AllUseableButtons[i].name == "Cell" || AllUseableButtons[i].name == "Cell (4)" || AllUseableButtons[i].name == "Cell (6)")
                                {
                                    possibleButtonIndex++;
                                }
                            }
                            choice = Random.Range(0, possibleButtonIndex);
                            if (choice == 0 && _buttonTextList[0].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 0;
                            }
                            else if (choice == 1 && _buttonTextList[4].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 4;
                            }
                            else if (choice == 2 && _buttonTextList[6].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 6;
                            }
                            else
                            {
                                if (_buttonTextList[0].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 0;
                                }
                                else if (_buttonTextList[4].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 4;
                                }
                                else if (_buttonTextList[6].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 6;
                                }
                                else
                                {
                                    RandomCellSelect();
                                }
                            }
                            _buttonTextList[choice].text = _computerMarker;
                            _buttonTextList[choice].GetComponentInParent<Button>().interactable = false;
                            EndTurn();
                            break;
                        case "Cell (4)":
                            for (int i = 0; i < AllUseableButtons.Count; i++)
                            {
                                if (AllUseableButtons[i].name == "Cell (1)" || AllUseableButtons[i].name == "Cell (3)" || AllUseableButtons[i].name == "Cell (5)" || AllUseableButtons[i].name == "Cell (7)")
                                {
                                    possibleButtonIndex++;
                                }
                            }
                            choice = Random.Range(0, possibleButtonIndex);
                            if (choice == 0 && _buttonTextList[1].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 1;
                            }
                            else if (choice == 1 && _buttonTextList[3].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 3;
                            }
                            else if (choice == 2 && _buttonTextList[5].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 5;
                            }
                            else if (choice == 3 && _buttonTextList[7].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 7;
                            }
                            else
                            {
                                if (_buttonTextList[1].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 1;
                                }
                                else if (_buttonTextList[3].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 3;
                                }
                                else if (_buttonTextList[5].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 5;
                                }
                                else if (_buttonTextList[7].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 7;
                                }
                                else
                                {
                                    RandomCellSelect();
                                }
                            }
                            _buttonTextList[choice].text = _computerMarker;
                            _buttonTextList[choice].GetComponentInParent<Button>().interactable = false;
                            EndTurn();
                            break;
                        case "Cell (5)":
                            for (int i = 0; i < AllUseableButtons.Count; i++)
                            {
                                if (AllUseableButtons[i].name == "Cell (2)" || AllUseableButtons[i].name == "Cell (4)" || AllUseableButtons[i].name == "Cell (8)")
                                {
                                    possibleButtonIndex++;
                                }
                            }
                            choice = Random.Range(0, possibleButtonIndex);
                            if (choice == 0 && _buttonTextList[2].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 2;
                            }
                            else if (choice == 1 && _buttonTextList[4].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 4;
                            }
                            else if (choice == 2 && _buttonTextList[8].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 8;
                            }
                            else
                            {
                                if (_buttonTextList[2].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 2;
                                }
                                else if (_buttonTextList[4].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 4;
                                }
                                else if (_buttonTextList[8].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 8;
                                }
                                else
                                {
                                    RandomCellSelect();
                                }
                            }
                            _buttonTextList[choice].text = _computerMarker;
                            _buttonTextList[choice].GetComponentInParent<Button>().interactable = false;
                            EndTurn();
                            break;
                        case "Cell (6)":
                            for (int i = 0; i < AllUseableButtons.Count; i++)
                            {
                                if (AllUseableButtons[i].name == "Cell (3)" || AllUseableButtons[i].name == "Cell (7)")
                                {
                                    possibleButtonIndex++;
                                }
                            }
                            choice = Random.Range(0, possibleButtonIndex);
                            if (choice == 0 && _buttonTextList[3].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 3;
                            }
                            else if (choice == 1 && _buttonTextList[7].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 7;
                            }
                            else
                            {
                                if (_buttonTextList[3].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 3;
                                }
                                else if (_buttonTextList[7].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 7;
                                }
                                else
                                {
                                    RandomCellSelect();
                                }
                            }
                            _buttonTextList[choice].text = _computerMarker;
                            _buttonTextList[choice].GetComponentInParent<Button>().interactable = false;
                            EndTurn();
                            break;
                        case "Cell (7)":
                            for (int i = 0; i < AllUseableButtons.Count; i++)
                            {
                                if (AllUseableButtons[i].name == "Cell (4)" || AllUseableButtons[i].name == "Cell (6)" || AllUseableButtons[i].name == "Cell (8)")
                                {
                                    possibleButtonIndex++;
                                }
                            }
                            choice = Random.Range(0, possibleButtonIndex);
                            if (choice == 0 && _buttonTextList[4].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 4;
                            }
                            else if (choice == 1 && _buttonTextList[6].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 6;
                            }
                            else if (choice == 2 && _buttonTextList[8].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 8;
                            }
                            else
                            {
                                if (_buttonTextList[4].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 4;
                                }
                                else if (_buttonTextList[6].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 6;
                                }
                                else if (_buttonTextList[8].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 8;
                                }
                                else
                                {
                                    RandomCellSelect();
                                }
                            }
                            _buttonTextList[choice].text = _computerMarker;
                            _buttonTextList[choice].GetComponentInParent<Button>().interactable = false;
                            EndTurn();
                            break;
                        case "Cell (8)":
                            for (int i = 0; i < AllUseableButtons.Count; i++)
                            {
                                if (AllUseableButtons[i].name == "Cell (5)" || AllUseableButtons[i].name == "Cell (8)")
                                {
                                    possibleButtonIndex++;
                                }
                            }
                            choice = Random.Range(0, possibleButtonIndex);
                            if (choice == 0 && _buttonTextList[5].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 5;
                            } else if (choice == 1 && _buttonTextList[7].GetComponentInParent<Button>().interactable != false)
                            {
                                choice = 7;
                            }
                            else
                            {
                                if (_buttonTextList[5].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 5;
                                }
                                else if (_buttonTextList[7].GetComponentInParent<Button>().interactable == true)
                                {
                                    choice = 7;
                                }
                                else
                                {
                                    RandomCellSelect();
                                }
                            }
                            _buttonTextList[choice].text = _computerMarker;
                            _buttonTextList[choice].GetComponentInParent<Button>().interactable = false;
                            EndTurn();
                            break;
                        default:
                            randCellChoice = Random.Range(0, _buttonTextList.Length);
                            if (_buttonTextList[randCellChoice].GetComponentInParent<Button>().interactable == true)
                            {
                                _buttonTextList[randCellChoice].text = _computerMarker;
                                PlaySound();
                                _buttonTextList[randCellChoice].GetComponentInParent<Button>().interactable = false;
                                EndTurn();
                            }
                            break;
                    }
                    AllUseableButtons.Clear();


                    //*********************************************************/
                    //*********************************************************/
                    //         WHOLE BUNCH OF ATTEMPTS AT A BETTER AI          /
                    //*********************************************************/
                    //*********************************************************/
                    /*List<Button> UsedButton = new List<Button>();
                    foreach (var button in _Cells)
                    {
                        //Check all buttons that have been activated
                        if (button.GetComponent<Button>().interactable == false)
                        {
                            //if (button.GetComponentInChildren<TextMeshProUGUI>().text == "X")
                            //{
                            //Save those that have the player marker
                            UsedButton.Add(button);
                            //}
                        }
                    }*/

                    //int randomButtonsChoice = 0;
                    /*foreach(var activeButtons in _Cells)
                    {
                        if (activeButtons.GetComponent<Button>().interactable == true)
                        {
                            AllUseableButtons.Add(activeButtons);
                        }
                    }

                    for(int i = 0; i < AllUseableButtons.Count; i++)
                    {
                        if(AllUseableButtons[i].name == "Cell (1)")
                        {
                            randomButtonsChoice++;
                        }
                    }*/

                    /*List<Button> UseableButtons = new List<Button>();
                    List<Button> RandomButtonChoice = new List<Button>();

                    if(lastXCell.name == "Cell")
                    {
                        foreach(var button in _Cells)
                        {
                            if(button.GetComponent<Button>().interactable == true)
                            {
                                UseableButtons.Add(button);
                            }

                            foreach(var newButton in UseableButtons)
                            {
                                if (button.name == "Cell (1)")
                                {
                                    RandomButtonChoice.Add(newButton);
                                }
                                if(button.name == "Cell (3)")
                                {
                                    RandomButtonChoice.Add(newButton);
                                }
                            }
                        }

                        int randomButton = Random.Range(0, RandomButtonChoice.Count);
                        _buttonTextList[randomButton].text = _computerMarker;
                    }*/
                }
            }
        }
    }


    //Pick a random number the same as the "Too Easy" AI, but this is if the AI can't pick a close cell to the last place X marker.
    private void RandomCellSelect()
    {
        randCellChoice = Random.Range(0, _buttonTextList.Length);
        if (_buttonTextList[randCellChoice].GetComponentInParent<Button>().interactable == true)
        {
            _buttonTextList[randCellChoice].text = _computerMarker;
            PlaySound();
            _buttonTextList[randCellChoice].GetComponentInParent<Button>().interactable = false;
            EndTurn();
        }
    }

    private void SetGameControllerReferenceOnButtons()
    {
        for (int i = 0; i < _buttonTextList.Length; i++)
        {
            _buttonTextList[i].GetComponentInParent<Button_Controller>().SetGameControllerReference(this);
        }
    }

    public string GetPlayerMarker()
    {
        return _playerMarker;
    }

    public void PlaySound()
    {
        int randAudioClip = Random.Range(0, _audioClips.Length);
        AudioClip audioClip = _audioClips[randAudioClip];
        _audioSource.clip = audioClip;

        _audioSource.Play();
    }

    public void EndTurn()
    {
        _moveCounter++;

        if (IsGameWonBy(_playerMarker))
        {
            GameOver(_playerMarker);
        }
        else if (IsGameWonBy(_computerMarker))
        {
            GameOver(_computerMarker);
        }
        else if (_moveCounter >= 9)
        {
            GameOver("draw");
        }
        else
        {
            ChangeSides();
            _delayTimer = 5.0f;
        }
    }

    //Brute force approach
    private bool IsGameWonBy(string side)
    {
        if (_buttonTextList[0].text == side && _buttonTextList[1].text == side && _buttonTextList[2].text == side)
        {
            return true;
        }
        else if (_buttonTextList[3].text == side && _buttonTextList[4].text == side && _buttonTextList[5].text == side)
        {
            return true;
        }
        else if (_buttonTextList[6].text == side && _buttonTextList[7].text == side && _buttonTextList[8].text == side)
        {
            return true;
        }
        else if (_buttonTextList[0].text == side && _buttonTextList[4].text == side && _buttonTextList[8].text == side)
        {
            return true;
        }
        else if (_buttonTextList[2].text == side && _buttonTextList[4].text == side && _buttonTextList[6].text == side)
        {
            return true;
        }
        else if (_buttonTextList[0].text == side && _buttonTextList[3].text == side && _buttonTextList[6].text == side)
        {
            return true;
        }
        else if (_buttonTextList[1].text == side && _buttonTextList[4].text == side && _buttonTextList[7].text == side)
        {
            return true;
        }
        else if (_buttonTextList[2].text == side && _buttonTextList[5].text == side && _buttonTextList[8].text == side)
        {
            return true;
        }

        return false;
    }

    private void ChangeSides()
    {
        if(_playerMove == true)
        {
            _playerMove = false;
            _playerXCountdown.text = _delayTimer.ToString("0");
            _playerXIdentifierPanel.SetActive(false);
            _compuetOIdentifierPanel.SetActive(true);
        } 
        else
        {
            _playerMove = true;
            _computerOCountdown.text = _delayTimer.ToString("0");
            _playerXIdentifierPanel.SetActive(true);
            _compuetOIdentifierPanel.SetActive(false);
        }
    }

    private void GameOver(string winningPlayer)
    {
        if (winningPlayer == "draw")
        {
            SetGameOverText("It's a draw!");
        }
        else
        {
            SetGameOverText(winningPlayer + " Wins!");
        }

        _delayTimer = 5.0f;
        SetBoardInteractable(false);
        _restartBtn.SetActive(true);
        _playerXIdentifierPanel.SetActive(false);
        _compuetOIdentifierPanel.SetActive(false);
    }
    private void SetGameOverText(string value)
    {
        _gameOverPanel.SetActive(true);
        _gameOverText.text = value;
    }

    //Toggle whether the buttons are pressable or not. Handy for the end game
    private void SetBoardInteractable(bool toggle)
    {
        for (int i = 0; i < _buttonTextList.Length; i++)
        {
            _buttonTextList[i].GetComponentInParent<Button>().interactable = toggle;
        }
    }

    public void RestartGame()
    {
        _audioSource.clip = _eraser;
        _audioSource.Play();

        _playerMarker = "X";
        _moveCounter = 0;
        _gameOverPanel.SetActive(false);
        SetBoardInteractable(true);

        for (int i = 0; i < _buttonTextList.Length; i++)
        {
            _buttonTextList[i].text = "";
        }

        _delayTimer = 5.0f;
        _playerMove = true;
        _playerXIdentifierPanel.SetActive(true);
        _restartBtn.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit ();
#endif
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Button_Controller : MonoBehaviour
{
    [SerializeField]
    private Button _button;
    [SerializeField]
    private TextMeshProUGUI _btnText;
    private Game_Controller _controller;

    //Get the Game_Controller from the Game_Controller class itself. 
    //So pass it through
    public void SetGameControllerReference(Game_Controller contoller)
    {
        _controller = contoller;
    }

    //Set the cell chosen to the value of the player marker (Default will be "X")
    public void SetPlayerMarker()
    {
        if (_controller._playerMove)
        {
            _btnText.text = _controller.GetPlayerMarker();
            _button.interactable = false;
            _controller.PlaySound();
            _controller._lastPlacedMarker = _button;
            _controller.EndTurn();
        }
    }
}

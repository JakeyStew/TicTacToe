using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Menu_Controller : MonoBehaviour
{
    [SerializeField]
    private Image[] _fallObjList;
    [SerializeField]
    private Button _leftArrow;
    [SerializeField]
    private Button _rightArrow;
    [SerializeField]
    private TextMeshProUGUI _difficultyText;

    private void Awake()
    {
        StartCoroutine(FallObjects());
    }

    public void PlayGame()
    {
        Global_Controller._difficultyLevel = _difficultyText.text;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit ();
#endif
    }

    public void OnArrowClicked()
    {
        if(_difficultyText.text == "Too Easy")
        {
            _difficultyText.text = "Little Harder";
        } 
        else
        {
            _difficultyText.text = "Too Easy";
        }
    }

    //A very unneccesary coroutine to make the noughts fall on the menu after a period of time.
    //Thought it looked cool though.
    private IEnumerator FallObjects()
    {
        yield return new WaitForSeconds(2.0f);
        foreach(var img in _fallObjList)
        {
            img.GetComponent<Rigidbody2D>().isKinematic = false;
        }
    }
}

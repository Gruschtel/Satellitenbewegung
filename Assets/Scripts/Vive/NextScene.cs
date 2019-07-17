using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Wechseln einer Szene (Pausetaste!)
/// </summary>
public class NextScene : MonoBehaviour {
    public string sceneName;
   
    //Make sure to attach these Buttons in the Inspector
    public Button m_YourButton;

    void Start()
    {
        //Calls the TaskOnClick/TaskWithParameters/ButtonClicked method when you click the Button
        m_YourButton.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        //Output this to console when Button1 or Button3 is clicked
        Debug.Log("You have clicked the button!");
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName.ToString());
    }
}

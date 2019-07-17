using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Utility;

public class StartScene : MonoBehaviour
    , IColliderEventClickHandler
    , IColliderEventPressEnterHandler
    , IColliderEventPressExitHandler
{
    [SerializeField] private string NextRoom;
    //[SerializeField] private float x, y, z, rotateX, rotateY, rotateZ;
    [SerializeField] private GameObject CameraObject;
    public ControllerManagerSample controllerManager;
    [SerializeField]
    private ColliderButtonEventData.InputButton m_activeButton = ColliderButtonEventData.InputButton.Trigger;

    public Transform buttonObject;
    public Vector3 buttonDownDisplacement;

    private Vector3 buttonOriginPosition;
    private bool menuVisible = false;

    private HashSet<ColliderButtonEventData> pressingEvents = new HashSet<ColliderButtonEventData>();

    public ColliderButtonEventData.InputButton activeButton
    {
        get
        {
            return m_activeButton;
        }
        set
        {
            m_activeButton = value;
            // set all child MaterialChanger heighlightButton to value;
            var changers = ListPool<MaterialChanger>.Get();
            GetComponentsInChildren(changers);
            for (int i = changers.Count - 1; i >= 0; --i) { changers[i].heighlightButton = value; }
            ListPool<MaterialChanger>.Release(changers);
        }
    }

    private void Start()
    {
        buttonOriginPosition = buttonObject.position;
        //StartNextRoom(menuVisible);
    }

#if UNITY_EDITOR

    protected virtual void OnValidate()
    {
        activeButton = m_activeButton;
    }

#endif

    public void StartNextRoom()
    {
        if (NextRoom.Equals("Exit"))
        {
            Application.Quit();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(NextRoom.ToString());
        }
    }

    public void OnColliderEventClick(ColliderButtonEventData eventData)
    {
        if (pressingEvents.Contains(eventData) && pressingEvents.Count == 1)
        {
            StartNextRoom();
        }
    }

    public void OnColliderEventPressEnter(ColliderButtonEventData eventData)
    {
        if (eventData.button == m_activeButton && eventData.clickingHandlers.Contains(gameObject) && pressingEvents.Add(eventData) && pressingEvents.Count == 1)
        {
            buttonObject.position = buttonOriginPosition + buttonDownDisplacement;
        }
    }

    public void OnColliderEventPressExit(ColliderButtonEventData eventData)
    {
        if (pressingEvents.Remove(eventData) && pressingEvents.Count == 0)
        {
            buttonObject.position = buttonOriginPosition;
        }
    }
}

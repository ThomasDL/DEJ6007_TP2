using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private MouseLook playerLook;
    [SerializeField] private PlayerController_test playerController;

    void Awake()
    {
        if (Time.timeScale != 1f) Time.timeScale = 1f;
        //int numGameSessions = FindObjectsOfType<GameSession>().Length;
        //if (numGameSessions > 1)
        //{
        //    Destroy(gameObject);
        //}
        //else
        //{
        //    DontDestroyOnLoad(gameObject);
        //}
        //_menu.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        gameOverScreen.SetActive(false);
        //playerLook = FindObjectOfType<MouseLook>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void DisplayGameOverScreen(bool isDead)
    {
        if (isDead)
        {
            Time.timeScale = 0f;
            playerController.enabled = false;
            playerLook.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            gameOverScreen.SetActive(true);
        }
    }
}

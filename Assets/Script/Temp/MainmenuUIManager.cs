using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainmenuUIManager : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("GamePlay");
    }
}
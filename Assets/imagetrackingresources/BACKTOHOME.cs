using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToHome : MonoBehaviour
{
    public void GoHome()
    {
        SceneManager.LoadScene("HomeScreen");
    }
}
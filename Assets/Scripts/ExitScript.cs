using UnityEngine;

public class ExitScript : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}

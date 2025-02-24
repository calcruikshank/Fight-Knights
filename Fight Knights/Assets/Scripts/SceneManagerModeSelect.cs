using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerModeSelect : MonoBehaviour
{
    public void LoadLocal()
    {
        SceneManager.LoadScene(1);
    }
    public void LoadOnline()
    {
        SceneManager.LoadScene(12);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonsBehaviorScript : MonoBehaviour
{
	public void ButtonClicked()
    {
		Debug.Log("The button clicked was");
	}

    public void BtnCreateAccount()
    {
        Debug.Log("Create Account Clicked");
        SceneManager.LoadScene("createAccount", LoadSceneMode.Single);
    }

    public void BtnLogin()
    {
        Debug.Log("Login Clicked");
    }

    public void BtnMainGame()
    {
        Debug.Log("Login Clicked");
        SceneManager.LoadScene("mainGame", LoadSceneMode.Single);
    }

}

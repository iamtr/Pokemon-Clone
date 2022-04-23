using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneDetails : MonoBehaviour
{
	[SerializeField] private List<SceneDetails> connectedScenes;
	[SerializeField] public bool IsLoaded;
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "Player")
		{
			Debug.Log($"Entered {gameObject.name}");
			LoadScene();
			GameController.Instance.SetCurrentScene(this);

			foreach (var scene in connectedScenes)
			{
				scene.LoadScene();
			}	

			if (GameController.Instance.PreviousScene != null)
			{
				var previouslyLoadedScenes = GameController.Instance.PreviousScene.connectedScenes;
				foreach (var scene in previouslyLoadedScenes)
				{
					if (!connectedScenes.Contains(scene) && scene != this)
					{
						scene.UnloadScene();
						
					}
				}
			}
		}
	}

	private void LoadScene()
	{
		if (!IsLoaded)
		{
			SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
			IsLoaded = true;
			Debug.Log(gameObject.name + " "+IsLoaded);
		}
	}

	private void UnloadScene()
	{
		if (IsLoaded)
		{
			SceneManager.UnloadSceneAsync(gameObject.name);	
			IsLoaded = false;
			Debug.Log(gameObject.name + " " + IsLoaded);
		}
	}

}

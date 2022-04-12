using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogBox;
    [SerializeField] private Text dialogText;
    [SerializeField] private int lettersPerSecond;

	public event Action OnShowDialog;
	public event Action OnCloseDialog;

	private Dialog dialog;
	private int currentLine;
	private bool isTyping;

	public static DialogManager Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	public void HandleUpdate()
	{
		if (Input.GetKeyDown(KeyCode.Z) && !isTyping)
		{
			++currentLine;
			if (currentLine < dialog.Lines.Count)
			{
				StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
			}
			else
			{
				currentLine = 0;
				OnCloseDialog?.Invoke();
				dialogBox.SetActive(false);
			}
		}
	}

	public IEnumerator ShowDialog(Dialog dialog)
	{
		yield return new WaitForEndOfFrame();

		OnShowDialog?.Invoke();

		this.dialog = dialog;
		dialogBox.SetActive(true);
		StartCoroutine(TypeDialog(dialog.Lines[0]));



	}

	public IEnumerator TypeDialog(string dialog)
	{
		isTyping = true;
		dialogText.text = "";
		foreach (var letter in dialog.ToCharArray())
		{
			dialogText.text += letter;
			yield return new WaitForSeconds(1 / lettersPerSecond);
		}
		isTyping = false;
	}

}

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameMenuManagment : MonoBehaviour 
{
	public GameObject[] firstSelectedObjects;

	public Button StartGame_Btn;

	public int sceneIndex;
	public int teamColorIndex;

	[SerializeField]
	private Sprite[] screenShots;
	[SerializeField]
	private EventSystem eventSystem;
	[SerializeField]
	private Text SceneName;
	[SerializeField]
	private GameObject colorSelectionImage;

	private GameObject sceneScreenShot;
	private GameObject lastSelectedObj;
	private Text colorImageText;
	private Text sceneScreenShotText;

	private void Awake()
	{
		SetUpInAwake();
	}

    private void Start()
    {
		SetUpInStart();
    }

    private void Update()
    {
		ReturnToMainMenu();
    }

	private void SetUpInAwake()
    {
		sceneScreenShot = firstSelectedObjects[1];
		sceneScreenShot.GetComponent<Image>().sprite = screenShots[0];
		colorImageText = colorSelectionImage.GetComponentInChildren<Text>();
		sceneScreenShotText = sceneScreenShot.GetComponentInChildren<Text>();
		lastSelectedObj = new GameObject();

		SetUpBtn();
	}

	private void SetUpInStart()
    {
		eventSystem.SetSelectedGameObject(firstSelectedObjects[0]);
		//LoadGameSettings();
		//ShowCameraSettingValues(4);
	}

	private void ReturnToMainMenu()
    {

	}









    private void SetUpBtn()
	{

	}

	

	private void SaveGameSettings()
    {

    }



	public void ChangeFirstSelectedButton(int index)
    {
		StartCoroutine(WaitForAFrame(index));
	}

	IEnumerator WaitForAFrame(int index)
    {
		eventSystem.SetSelectedGameObject(null);
		yield return null;
		eventSystem.SetSelectedGameObject(firstSelectedObjects[index]);
	}
}

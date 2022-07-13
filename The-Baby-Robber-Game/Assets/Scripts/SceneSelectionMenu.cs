using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SceneSelectionMenu : UIMenu
{
	[SerializeField]
	private GameObject[] selectableObjects;
	[SerializeField]
	private Sprite[] screenShots;
	[SerializeField]
	private Button startGameButton;
	[SerializeField]
	private Text colorImageText;
	[SerializeField]
	private Text sceneScreenShotText;
	[SerializeField]
	private Image sceneScreenShot;

	private int teamColorIndex;
    private int sceneIndex;
	private string[] sceneDescriptionText = { "Catch the little boy and survive I", "Catch the little boy and survive II", "Catch the little boy and survive III"
	, "Catch the little boy and kill more opponents I", "Catch the little boy and kill more opponents II", "Catch the little boy and kill more opponents III"
	, "Catch the little boy and survive IV", "Catch the little boy and survive V", "Catch the little boy and survive VI"
	, "Catch the little boy and kill more opponents IV" , "Catch the little boy and kill more opponents V", "Catch the little boy and kill more opponents VI"
	};

    public override void SetUp()
    {
        base.SetUp();
		startGameButton.onClick.AddListener(() => {
			GameManager.instance.StartBattle();
		});
		ChangeScene(true);
		ChangeTeamColor(true);
		gameObject.SetActive(false);
	}

    public override void Tick()
    {
        base.Tick();
		SceneSelection();
		TeamColorSelection();
	}

    private void SceneSelection()
	{
		if (EventSystem.current.currentSelectedGameObject == selectableObjects[0])
		{
			if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
			{
				ChangeScene(false);
			}
			else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
			{
				ChangeScene(true);
			}
		}
	}

	private void TeamColorSelection()
	{
		if (EventSystem.current.currentSelectedGameObject == selectableObjects[1])
		{
			if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
			{
				ChangeTeamColor(false);
			}
			else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
			{
				ChangeTeamColor(true);
			}
		}
	}

	private void ChangeScene(bool increase)
	{
		if (increase)
			sceneIndex++;
		else
			sceneIndex--;

		if (increase && sceneIndex > GameManager.instance.maxSceneCount || sceneIndex < 1 && !increase)
			sceneIndex = 1;

		GameManager.instance.sceneIndex = sceneIndex;
		ShowSceneScreenShot();
		ShowSceneScreenShotText();
	}

	private void ChangeTeamColor(bool increase)
	{
		if (increase)
			teamColorIndex++;
		else
			teamColorIndex--;
		if (increase && teamColorIndex >= PrefabManager.instance.GetNumberOfColor() || teamColorIndex < 1 && !increase)
			teamColorIndex = 0;

		GameManager.instance.currentPlayerSelectedTeamColor = teamColorIndex;
		ShowCurrentTeamColor();
	}

	private void ShowCurrentTeamColor()
	{
		colorImageText.text = "Colour " + PrefabManager.instance.GetAvailibleFactionName(teamColorIndex);
	}

	private void ShowSceneScreenShot()
	{
		int index = sceneIndex;
		sceneScreenShot.sprite = screenShots[index - 1];
	}

	private void ShowSceneScreenShotText()
	{
		int index = sceneIndex;
		sceneScreenShotText.text = sceneDescriptionText[index - 1];
	}
}

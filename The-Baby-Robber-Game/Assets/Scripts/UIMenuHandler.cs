using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIMenuHandler : MonoBehaviour
{
    public GameObject[] UIPanels;

	[SerializeField]
	bool closeAllUIWhenTheGameIsStart;
	[SerializeField]
	GameObject firstSelectedButton;
	GameObject lastSelectedButton;
	[SerializeField]
	UIMenu[] uiMenus;

    private void Start()
    {
		SetUp();
    }

    private void Update()
    {
		UpdateUIMenus();
		CloseCurrentUIAndReturnToFirstUIMenu();
		RefocusSelectedButtonWhenTheMouseButtonIsClicked();
    }

    private void SetUp()
    {
		SetUpUIMenus();
		ChangeFirstSelectedButton(firstSelectedButton);
		CloseAllUIPanels(closeAllUIWhenTheGameIsStart);
	}

	private void SetUpUIMenus()
    {
		uiMenus = GetComponentsInChildren<UIMenu>();

		for(int i = 0; i < uiMenus.Length; i++)
        {
			uiMenus[i].SetUp();
        }
	}

	private void UpdateUIMenus()
    {
		for(int y = 0; y < uiMenus.Length; y++)
        {
			if(uiMenus[y].gameObject.activeSelf)
            {
				uiMenus[y].Tick();
			}
        }
    }

	private void RefocusSelectedButtonWhenTheMouseButtonIsClicked()
	{
		if (EventSystem.current.currentSelectedGameObject == null)
		{
			EventSystem.current.SetSelectedGameObject(lastSelectedButton);
		}
		else
		{
			lastSelectedButton = EventSystem.current.currentSelectedGameObject;
		}
	}

	private void CloseCurrentUIAndReturnToFirstUIMenu()
	{
		for (int x = 1; x < UIPanels.Length; x++)
		{
			if (UIPanels[x].activeSelf)
			{
				if (Input.GetKeyDown(KeyCode.Escape))
				{
					UIPanels[x].SetActive(false);
					UIPanels[0].SetActive(true);
					ChangeFirstSelectedButton(firstSelectedButton);
				}
			}
		}
	}

	private void CloseAllUIPanels(bool closeAll)
    {
		int startLoop = 1;
		if (closeAll)
			startLoop = 0;

		for(int x = startLoop; x < UIPanels.Length; x++)
        {
			UIPanels[x].SetActive(false);
        }
    }
	
	public void ChangeFirstSelectedButton(GameObject newObject)
	{
		StartCoroutine(WaitForAFrame(newObject));
	}

	public void QuitGame()
    {
		Application.Quit();
    }

	IEnumerator WaitForAFrame(GameObject firstSelectedObject)
	{
		EventSystem.current.SetSelectedGameObject(null);
		yield return null;
		EventSystem.current.SetSelectedGameObject(firstSelectedObject);
	}
}

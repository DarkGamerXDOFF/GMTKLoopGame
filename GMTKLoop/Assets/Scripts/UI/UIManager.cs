using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager i;

    [SerializeField] private Menu[] menus;

    private void Awake()
    {
        if (i == null)
            i = this;
        else
            Destroy(gameObject);

        CloseAllMenus(); // Ensure all menus are closed at the start
    }

    public void OpenMenu(string menuName)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].menuName == menuName)
            {
                menus[i].Open();
            }
            else
            {
                menus[i].Close();
            }
        }
    }
    public void OpenMenu(string menuName, Action<Menu> OnOpenMenu)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].menuName == menuName)
            {
                menus[i].Open();
                OnOpenMenu?.Invoke(menus[i]);
            }
            else
            {
                menus[i].Close();
            }
        }
    }

    public void OpenMenu(Menu menu)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].open)
            {
                menus[i].Close();
            }
        }
        menu.Open();
    }

    public void CloseMenu(Menu menu)
    {
        menu.Close();
    }

    private void CloseAllMenus()
    {
        for (int i = 0; i < menus.Length; i++)
        {
            menus[i].Close();
        }
    }

    public void StartGame()
    {
        OpenMenu("Game");
        GameManager.i.StartRound();
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    public Menu GetMenu(string menuName)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].menuName == menuName)
            {
                return menus[i];
            }
        }
        Debug.LogWarning($"Menu with name {menuName} not found.");
        return null;
    }
}

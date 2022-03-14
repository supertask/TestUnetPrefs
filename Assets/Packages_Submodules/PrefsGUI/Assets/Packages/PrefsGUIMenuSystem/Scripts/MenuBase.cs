using UnityEngine;
using System.Collections;

namespace PrefsGUI
{
	public class MenuBase : MonoBehaviour
	{
		public MenuState menuState = MenuState.Close;
		public KeyCode toggleKey = KeyCode.None;

		public Color menuButtonColor = Color.white;
		public Color menuButtonBkgColor = Color.gray;

		Rect _windowRect = new Rect();
		protected virtual float MinWidth { get { return 500f; } }
		public virtual string MenuName { get { return  "";} }

		void OnGUI()
		{
			if(menuState == MenuState.Close || menuState == MenuState.OpenByParentMenu)
				return;

			_windowRect = GUILayout.Window(GetHashCode(), _windowRect, (id) =>
			{
				OnGUIInternal();
				GUI.DragWindow();
			},
			MenuName,
			GUILayout.MinWidth(MinWidth));
		}

		protected virtual void Update ()
		{
			CheckMenuToggle();
		}

		void CheckMenuToggle()
		{
			if (Input.GetKeyDown(toggleKey))
			{
				switch(menuState)
				{
					case MenuState.Close: menuState = MenuState.Open; OnMenuOpen();  break;
					case MenuState.Open: CloseMenu(); break;
				}
			}
		}

		protected virtual void OnMenuOpen()
		{

		}

		public virtual void OnGUIInternal()
		{
			OnGUIInteralAddCloseButton();
		}

		protected void OnGUIInteralAddCloseButton()
		{
			GUILayout.Label("");
			GUILayout.BeginHorizontal();
			
			if (GUILayout.Button( string.Format("SAVE changes in:\n{0}",PrefsGUI.Prefs.GetFileLocation() ) ) )
			{ 
				//PrefsGUI.Prefs.SaveInAllLocations();
				PrefsGUI.Prefs.Save();
			}
			//GUILayout.Label("");
			//if (GUILayout.Button( "SAVE changes in ALL Locations") )
			//{ 
			//	//PrefsGUI.Prefs.SaveInAllLocations();
			//	PrefsGUI.Prefs.Save();
			//}
			GUILayout.Label("");
			if (GUILayout.Button( menuState == MenuState.OpenByParentMenu ? "BACK to previous menu\n(NO SAVE)" :  "CLOSE Menu\n(NO SAVE)"))
			{ 
				CloseMenu();
			}
			GUILayout.EndHorizontal();
		}

		public virtual void CloseMenu()
		{
			menuState = MenuState.Close;
		}
	}
}
using UnityEngine;
using System.Collections;
using PrefsGUI;

namespace PrefsGUI
{

	public class PrefsGUISettings : PrefsGUI.MenuBase //MonoBehaviour
	{
		[Tooltip("Default File Save/Load location")]
		public Prefs.FileLocation fileLocation = Prefs.FileLocation.PersistantData;
		[SerializeField]
		[Tooltip("By Default, the same path prefix is used for all 'FileLocations'. However per 'FileLocation' path prefixs can be set via code.")]
		private string pathPrefix = "";
		[SerializeField]
		private string hardedCodedPath = "c:/Unity/PrefsGUI/";
		private Prefs.FileLocation fileLastLoadedFrom = Prefs.FileLocation.PersistantData;

		protected override float MinWidth => 700;

		// if necessary this function can be overridden to return seperate paths based on the fileLocation type.
		public virtual string GetPathPrefix( Prefs.FileLocation fileLocation ) { return pathPrefix; }

		public void SetFileLocation( Prefs.FileLocation fileLocation)
		{
			PrefsGUI.Prefs.SetFileLocation( fileLocation );
			//PrefsGUI.Prefs.SetFilePathPrefix( fileLocation, GetPathPrefix( fileLocation ) );
		}

		//[System.Serializable]
		//public class PrefsEnum : PrefsParam<Prefs.FileLocation>
		//{
		//	public PrefsEnum( string key, Prefs.FileLocation defaultValue = default( Prefs.FileLocation ) ) : base( key, defaultValue ) { }
		//}
		//public PrefsEnum _prefsEnum;

			
		protected virtual void Awake()
		{
			if(this.isActiveAndEnabled == false)
				return;
			SetFileLocation( fileLocation );
			for(Prefs.FileLocation i = 0; i < Prefs.FileLocation.NumLocations; ++i)
			{
				Prefs.SetFilePathPrefix(i, GetPathPrefix(i) );
			}
			PrefsGUI.Prefs.SetFileLocationHardCodedPath( hardedCodedPath );
			/*
			// work in progress
		//#if (UNITY_EDITOR == false)
			// if file location is streaming assets, change it to persistant data
			if (fileLocation == Prefs.FileLocation.StreamingAssets)
			{
				System.DateTime defaultFileDT = PrefsGUI.Prefs.GetFileTimeStamp();

				// if fileLocationno persistant data file exists, copy latest streaming assets file to persistant data
				// if persistant data file is older than streaming assets file, copy latest streaming assets file to persistant data
			}
		//#endif
		*/

			// reload file after changing path location
			PrefsGUILoad();

			//_prefsEnum = new PrefsEnum("Save_Load_Location", fileLocation);
		}

		//public void OnGUIInternal()
		public override void OnGUIInternal()
		{
			// pring defaut save location

			GUILayout.Label(string.Format("Default Save/Load Location:\t{0}", PrefsGUI.Prefs.GetFileLocation()), GUILayout.MinWidth(200f));
			ChangeDefaultLocationGUI();
			
			GUILayout.Label("");
			GUILayout.Label(string.Format("File Last Loaded From:\t{0}", fileLastLoadedFrom));
			GUILayout.Label("");

			ShowAllSaveLocationPathsGUI();

			// save to Streaming Assets
			if (GUILayout.Button("SAVE in Streaming Assets & ALL LOCATIONS"))
			{
				PrefsGUI.Prefs.SaveInAllLocations();
				//PrefsGUI.Prefs.Save();
			}

			// offer options to load settings files from other locations than the default location
			for (Prefs.FileLocation i = 0; i < Prefs.FileLocation.NumLocations; ++i)
			{
				if (i != fileLocation)
				{ 
					// provide option to load from stream assets
					// load from Streaming Assets
					if (GUILayout.Button(string.Format("LOAD from {0}", i) ))
					{
						SetFileLocation(i);
						PrefsGUILoad();
						SetFileLocation(fileLocation);
					}
				}
			}

			GUILayout.Label("Settings File Timestamps:");
			for (Prefs.FileLocation i = 0; i < Prefs.FileLocation.NumLocations; ++i)
			{
				SetFileLocation(i);
				System.DateTime dt = PrefsGUI.Prefs.GetFileTimeStamp();
				bool validSettingFile = dt.Equals(System.DateTime.MinValue) == false;

				GUILayout.Label(string.Format("     {0}:\t {1}", i, validSettingFile ? dt.ToString() : "No File / Unknown"));
			}
			SetFileLocation(fileLocation);


			base.OnGUIInternal();
		}

		protected virtual void ChangeDefaultLocationGUI()
		{
			GUILayout.Label("\t(To change 'default Location', change in Unity Editor)"
				#if (UNITY_EDITOR == false)
				+ " and rebuild .exe"
				#endif
				);
		}

		void ShowAllSaveLocationPathsGUI()
		{
			GUILayout.Label("File Paths:");
			PrefsGUI.Prefs.FileLocation backup = PrefsGUI.Prefs.GetFileLocation();


			for(int i = 0; i < (int)PrefsGUI.Prefs.FileLocation.NumLocations; ++i)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("     " + ((PrefsGUI.Prefs.FileLocation)i).ToString(), GUILayout.MinWidth(150f) );

				PrefsGUI.Prefs.FileLocation f = (PrefsGUI.Prefs.FileLocation)i; 
				SetFileLocation( f );
				GUILayout.Label( PrefsGUI.Prefs.GetFileNameAndPath() );
				GUILayout.FlexibleSpace();

				GUILayout.EndHorizontal();
			}

			// restore origional location
			SetFileLocation(backup);
		}


		private void PrefsGUILoad()
		{
			PrefsGUI.Prefs.Load();
			fileLastLoadedFrom = PrefsGUI.Prefs.GetFileLocation();
		}
	} // end class
} // end namespace
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrefsGUI;

public class TestScript : MonoBehaviour
{

    public class PrefsGUIData
    {
        public int arrayNum;
        public PrefsVector4[] vec4s = null;
        public PrefsGUIData()
        {
            //バグらない
            //arrayNum = 999;
            arrayNum = 3;
            vec4s = new PrefsVector4[arrayNum];
            for (int i = 0; i < arrayNum; i++)
            {
                vec4s[i] = new PrefsVector4("12345678910" + i);
            }

            ////バグる
            //arrayNum = 1999;
            //vec4s = new PrefsVector4[arrayNum];
            //for (int i = 0; i < arrayNum; i++)
            //{
            //    vec4s[i] = new PrefsVector4("12345678910" + i);
            //}

            ////バグる
            //arrayNum = 999;
            //vec4s = new PrefsVector4[arrayNum];
            //for (int i = 0; i < arrayNum; i++)
            //{
            //    vec4s[i] = new PrefsVector4("123456789101" + i);
            //}
        }
    }
    PrefsGUIData prefsGUIData = new PrefsGUIData();

    private void OnGUI()
    {
        for(int i = 0; i < 3; i++)
        {
            prefsGUIData.vec4s[i].OnGUI();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers instance;
    static bool init = false;

    UpdateManager updateManager = null;

    public static UpdateManager UpdateM { get { return Instance?.updateManager; } }
    public static Managers Instance
    {
        get
        {
            if (!init)
            {
                init = true;
                GameObject go = GameObject.Find("@Managers");

                if (go == null)
                {
                    go = new GameObject() { name = "@Managers" };
                    go.AddComponent<Managers>();
                }

                DontDestroyOnLoad(go);
                instance = go.GetComponent<Managers>();
                instance.updateManager = go.AddComponent<UpdateManager>();
            }

            return instance;
        }
    }
}

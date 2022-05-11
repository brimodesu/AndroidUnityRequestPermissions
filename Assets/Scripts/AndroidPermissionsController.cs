using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class AndroidPermissionsController : MonoBehaviour
{
    private Transform objMainCanvas;
    public GameObject prefabAndroidPermissionsRequestPopUp;


    private static List<AndroidPermission> required_permissions = new List<AndroidPermission>()
    {
        new AndroidPermission()
        {
            name = "Teléfono/SMS",
            reason =
                "Esta aplicación necesita poder enviar solicitudes USSD al operador para la generación de reportes.",
            key = "android.permission.CALL_PHONE"
        },
        new AndroidPermission()
        {
            name = "Almacenamiento",
            reason =
                "Esta aplicación necesita almacenar de manera local toda la información necesaria para su funcionamiento.",
            key = Permission.ExternalStorageWrite
        },
        new AndroidPermission()
        {
            name = "Cámara",
            reason =
                "Esta aplicación necesita realizar fotografías y escaneo de documentos.",
            key = Permission.Camera
        },
        new AndroidPermission()
        {
            name = "Ubicación",
            reason = "Esta aplicación obtener tu ubicación GPS para el registro en distintos procesos.",
            key = Permission.FineLocation
        },
    };

    private static PermissionCallbacks callbacks;

    private GameObject popUp;

    private void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionDeniedAndDontAskAgain");
        popUp = Instantiate(prefabAndroidPermissionsRequestPopUp, objMainCanvas);
        popUp.GetComponent<PermissionRequestPopUpController>().SetUp(true, null, () =>
        {
#if PLATFORM_ANDROID && !UNITY_EDITOR
               GoToSettings();
#elif UNITY_EDITOR
            Debug.Log("GoToSettings");
#endif
            Destroy(popUp);
        });
    }

    private void PermissionCallbacks_PermissionGranted(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionGranted");
        switch (permissionName)
        {
            case Permission.FineLocation:
                // ACCESS_BACKGROUND_LOCATION tiene que pedirse despues de fine location
                // if (AndroidVersion.SDK_INT >= 30 &&
                //     !Permission.HasUserAuthorizedPermission("android.permission.ACCESS_BACKGROUND_LOCATION"))
                // {
                //     Permission.RequestUserPermission("android.permission.ACCESS_BACKGROUND_LOCATION");
                // }

                break;
        }
    }

    private void PermissionCallbacks_PermissionDenied(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionDenied");
        popUp = Instantiate(prefabAndroidPermissionsRequestPopUp, objMainCanvas);
        popUp.GetComponent<PermissionRequestPopUpController>().SetUp(true, null, () =>
        {
#if PLATFORM_ANDROID && !UNITY_EDITOR
               GoToSettings();
#elif UNITY_EDITOR
            Debug.Log("GoToSettings");
#endif
            Destroy(popUp);
        });
    }

    public void RequestAndroidPermissions()
    {
        callbacks = new PermissionCallbacks();
        callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
        callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
        callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;

        var permissionsToRequestString = new List<string>();
        var permissionsToRequestUI = new List<AndroidPermission>();

        foreach (var permissionToRequest in required_permissions)
        {
            if (!string.IsNullOrEmpty(permissionToRequest.key) &&
                !Permission.HasUserAuthorizedPermission(permissionToRequest.key))
            {
                permissionsToRequestString.Add(permissionToRequest.key);
                permissionsToRequestUI.Add(permissionToRequest);
            }
        }

        if (permissionsToRequestUI.Count > 0)
        {
            popUp = Instantiate(prefabAndroidPermissionsRequestPopUp, objMainCanvas);
            popUp.GetComponent<PermissionRequestPopUpController>().SetUp(false, permissionsToRequestUI, () =>
            {
#if PLATFORM_ANDROID && !UNITY_EDITOR
                Permission.RequestUserPermissions(permissionsToRequestString.ToArray(), callbacks);
#elif UNITY_EDITOR
                Debug.Log("Grant");
#endif
                Destroy(popUp);
            });
        }
    }

    public static void GoToSettings()
    {
        try
        {
#if PLATFORM_ANDROID
            using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivityObject =
                   unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                string packageName = currentActivityObject.Call<string>("getPackageName");

                using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                using (AndroidJavaObject uriObject =
                       uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null))
                using (var intentObject = new AndroidJavaObject("android.content.Intent",
                           "android.settings.APPLICATION_DETAILS_SETTINGS", uriObject))
                {
                    intentObject.Call<AndroidJavaObject>("addCategory", "android.intent.category.DEFAULT");
                    intentObject.Call<AndroidJavaObject>("setFlags", 0x10000000);
                    currentActivityObject.Call("startActivity", intentObject);
                }
            }
#endif
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private void Awake()
    {
        objMainCanvas = GameObject.Find("MainCanvas").transform;
        RequestAndroidPermissions();
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class AndroidPermissionsController : MonoBehaviour
{
    private static List<AndroidPermission> required_permissions = new List<AndroidPermission>()
    {
        new AndroidPermission()
        {
            name = "Teléfono/SMS",
            reason =
                "Esta aplicación necesita poder enviar solicitudes USSD al operador para la generación de reportes y venta de productos no físicos.",
            key = "android.permission.CALL_PHONE"
        },
        new AndroidPermission()
        {
            name = "Almacenamiento",
            reason =
                "Esta aplicación necesita almacenar de manera local toda la información necesaria para su funcionamiento (ventas, inventario, fotografias, etc.).",
            key = Permission.ExternalStorageWrite
        },
        new AndroidPermission()
        {
            name = "Cámara",
            reason =
                "Esta aplicación necesita realizar fotografías y escaneo de documentos para documentar y agilizar procesos (gestiones y ventas).",
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

    private static void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionDeniedAndDontAskAgain");
        // MyMessageBox.Question("No se pudo obtener el siguiente permiso: \n" + permissionName +
        //                       "\n Este permiso es necesatio para el funcionamiento correcto de la aplicación.")
        //         .Callback +=
        //     (bool response) =>
        //     {
        //         if (response)
        //         {
        //             GoToSettings();
        //         }
        //     };
    }

    private static void PermissionCallbacks_PermissionGranted(string permissionName)
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

    private static void PermissionCallbacks_PermissionDenied(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionDenied");
        // MyMessageBox.Notification(
        //     "Para poder utilizar correctamente esta aplicación es necesario el siguiente permiso: " +
        //     permissionName);
    }

    public static void RequestAndroidPermissions()
    {
        callbacks = new PermissionCallbacks();
        callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
        callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
        callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;

        var permissionsToRequestString = new List<string>();
        
        foreach (var permissionToRequest in required_permissions)
        {
            if (!string.IsNullOrEmpty(permissionToRequest.key) &&
                !Permission.HasUserAuthorizedPermission(permissionToRequest.key))
            {
                permissionsToRequestString.Add(permissionToRequest.key);
            }
        }
        Permission.RequestUserPermissions(permissionsToRequestString.ToArray(), callbacks);
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
}
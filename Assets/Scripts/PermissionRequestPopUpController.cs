using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PermissionRequestPopUpController : MonoBehaviour
{
    public ScrollRect permissionScrollView;

    public TMP_Text explanation;
    public TMP_Text callToAction;

    public GameObject prefabPermissionDetail;

    private Action grantCallback;

    public void SetUp(bool hasDenied, List<AndroidPermission> permissionsToRequestUI, Action callback)
    {
        grantCallback = callback;


        if (hasDenied)
        {
            explanation.text = "No se ha obtenido el permiso de uno o mas permisos";
            callToAction.text = "Ir a configuraciones";
            permissionScrollView.gameObject.SetActive(false);
        }
        else
        {
            explanation.text = "Esta APP necesita acceder a los siguientes permisos para su funcionamiento:";
            callToAction.text = "Otorgar";
            foreach (var permission in permissionsToRequestUI)
            {
                var permissionDetail = Instantiate(prefabPermissionDetail, permissionScrollView.content);
                permissionDetail.GetComponent<PermissionDetailController>().SetUp(permission);
            }
        }
    }

    public void GrantPermissions()
    {
        grantCallback();
    }
}
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

public class PermissionDetailController : MonoBehaviour
{
    public TMP_Text permissionName;
    public TMP_Text permissionReason;

    public void SetUp(AndroidPermission permission)
    {
        permissionName.text = permission.name;
        permissionReason.text = permission.reason;
    }
}
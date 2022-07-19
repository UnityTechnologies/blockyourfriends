using UnityEngine;

public class ATTManager : MonoBehaviour
{
    [SerializeField]
    private GameObject ATTCheckerPrefab;

    private void Awake()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            Instantiate(ATTCheckerPrefab, transform);
        }
    }
}
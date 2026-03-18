using TMPro;
using UnityEngine;

public class DisplayUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI quotaText;

    [SerializeField] private UpdateQuota updateQuota;
    void Update()
    {
        quotaText.text = "Quota: " + updateQuota.totalQuota + " / " + QuotaManager.Instance.GetQuotaValue();
        roundText.text = "Current Round: " + RoundManager.Instance.ReturnRound(); 
    }
}

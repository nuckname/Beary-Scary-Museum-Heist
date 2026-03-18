using UnityEngine;

public class QuotaManager : MonoBehaviour
{
    public static QuotaManager Instance { get; private set; }

    [SerializeField] private int quotaValue = 0;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
    
    public int GetQuotaValue()
    {
        int currentRound = RoundManager.Instance.ReturnRound();
        switch (currentRound)
        {
            case 1:
                quotaValue = 3;
                break;
            case 2:
                quotaValue = 4;
                break;
            case 3:
                quotaValue = 5;
                break;
            case >4:
                quotaValue = 6; 
                break;
        }
        return quotaValue;
    }
}
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    //State Manager is good here
    public static RoundManager Instance { get; private set; }

    [SerializeField] private UpdateQuota updateQuota;
    
    [SerializeField] private int currentRound = 1;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Start is called once before the first execution of Update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public int ReturnRound()
    {
        return currentRound;
    }
    
    public void NextRound()
    {
        updateQuota.totalQuota = 0;
        currentRound++;
    }
}
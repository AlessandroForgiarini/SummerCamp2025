using UnityEngine;

public class CrystalHandler : MonoBehaviour
{
    [SerializeField] private CrystalDataSO crystalData;

    public bool IsFree { get; private set; }
    public int Score => crystalData.score;

    public Bounds Bounds => GetAABBVolume();

    private Transform _crystalsContainer;
    private Vector3 _startingScale;
    
    private void Start()
    {
        GameObject containerGO = GameObject.Find("Crystals");
        Transform container = containerGO == null ? transform.parent : containerGO.transform;

        _crystalsContainer = container;
        IsFree = true;
    }

    public void ResetParent()
    {
        transform.SetParent(_crystalsContainer);
        
        IsFree = true;
        if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 5))
        {
            transform.position = hit.point;
            transform.up = Vector3.up;
        }

        transform.localScale = _startingScale;
    }

    public void SetBackPackParent(Transform parent)
    {
        IsFree = false;
        _startingScale = transform.localScale;
        transform.localScale = _startingScale / 2;
        transform.position = parent.position;
        
        transform.SetParent(parent);
    }


    Bounds GetAABBVolume()
    {
        return transform.GetComponentInChildren<MeshRenderer>().bounds;
    }
}
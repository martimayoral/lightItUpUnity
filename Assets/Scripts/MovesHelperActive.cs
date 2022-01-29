using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MovesHelperActive : MonoBehaviour
{
    public bool active = false;


    [SerializeField] TextMeshProUGUI moves;
    [SerializeField] TextMeshProUGUI movesToClone;
    [SerializeField] TextMeshProUGUI nextMoves;
    [SerializeField] TextMeshProUGUI nextMovesToClone;
    [SerializeField] Image stars;
    [SerializeField] Image starsToClone;

    // Start is called before the first frame update
    void Start()
    {
        if (movesToClone == null)
            Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            moves.text = movesToClone.text;
            nextMoves.text = nextMovesToClone.text;
            nextMoves.color = nextMovesToClone.color;
            stars.sprite = starsToClone.sprite;
        }
    }
}

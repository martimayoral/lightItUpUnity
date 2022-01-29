using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    Button btn;
    [SerializeField] AudioManager.eSound sound = AudioManager.eSound.Select;

    // Start is called before the first frame update
    void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(() => AudioManager.Instance.PlaySound(sound));
    }

}

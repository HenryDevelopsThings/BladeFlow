using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JuiceBar : MonoBehaviour
{
    [SerializeField] public Image juiceBar;
    private void Start() {

    }

    public void UpdateJuice(float fraction) {
        juiceBar.fillAmount = fraction;
    }
}

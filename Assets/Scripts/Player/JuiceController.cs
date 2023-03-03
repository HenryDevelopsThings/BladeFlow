using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JuiceController : MonoBehaviour
{

    [Header("Juice Bar")]
    [SerializeField] private int maxJuice;
    [SerializeField] private JuiceBar juiceBar;
    private int currentJuice;

    private void Start() {
        currentJuice = maxJuice;
    }

    public void TakeJuice(int amount) {
        currentJuice -= amount;
        juiceBar.UpdateJuice((float) currentJuice / (float) maxJuice);
    }

    public int GetJuice() {
        return currentJuice;
    }
}

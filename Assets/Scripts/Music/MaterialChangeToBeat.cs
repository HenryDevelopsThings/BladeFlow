using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChangeToBeat : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material red;
    [SerializeField] private Material orange;
    [SerializeField] private Material green;

    private Renderer render;

    void Start()
    {
        render = GetComponent<Renderer>();
        render.enabled = true;
    }

    public void changeMaterial() {
        if (render != null) {
            // Change material 0.4 seconds before and after to orange and on beat green then reset to red
            StartCoroutine(ChangeMaterialCoroutine());
        }
    }

    private IEnumerator ChangeMaterialCoroutine() {
        render.material = orange;
        yield return new WaitForSeconds(1f);
        render.material = green;
        yield return new WaitForSeconds(1.5f);
        render.material = orange;
        yield return new WaitForSeconds(1f);
        render.material = red;
    }

}

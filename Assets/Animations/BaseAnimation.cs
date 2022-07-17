using System.Collections;
using UnityEngine;

public class BaseAnimation : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(Animation());
    }

    public int Count => sprites.Length;
    [SerializeField] Sprite[] sprites;
    IEnumerator Animation()
    {
        var sr = GetComponent<SpriteRenderer>();
        foreach (var s in sprites)
        {
            sr.sprite = s;
            yield return new WaitForEndOfFrame();
        }
        gameObject.SetActive(false);
    }
}
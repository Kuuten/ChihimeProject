using UnityEngine;
using DG.Tweening;

public class ButtonScaling : MonoBehaviour {

    public void OnOver() {
        transform.localScale = new Vector3(1f,1f,1f);
        transform.DOScale(1.3f,0.3f)
            .SetEase(Ease.OutElastic);
            
    }

    public void OnAway() {
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(1.0f, 0.2f))
            .SetEase(Ease.OutElastic);
        //seq.OnComplete(() => DestroyWindow());
        //seq.Play();
    }

    void DestroyWindow() {
        Destroy(gameObject);
    }
}

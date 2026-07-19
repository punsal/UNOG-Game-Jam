using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace VFX
{
    [RequireComponent(typeof(Image))]
    public class LogoMarkVFX : MonoBehaviour
    {
        private Image _image;
        
        [SerializeField, Range(0f, 2f)] private float duration = 0.65f;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void Start()
        {
            if (_image == null)
            {
                return;
            }
            _image.DOColor(Color.white, duration).SetEase(Ease.OutSine).SetLoops(-1, LoopType.Yoyo);
        }
    }
}

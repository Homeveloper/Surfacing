using TMPro;
using UnityEngine;

public class CoinCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counter;
    [SerializeField] private Transform coin;

    private int _coinCount = 0;
    private float _rotationDuration = 0.5f;
    private bool _isRotating = false;

    void Start()
    {
        UpdateCoinText();
    }

    public void AddCoin()
    {
        _coinCount++;
        UpdateCoinText();

        if (!_isRotating)
            StartCoroutine(RotateCoinIcon());
    }

    private void UpdateCoinText()
    {
        if (counter != null)
            counter.text = _coinCount.ToString();
    }

    private System.Collections.IEnumerator RotateCoinIcon()
    {
        _isRotating = true;

        float elapsed = 0f;
        float startAngle = coin.localEulerAngles.y;
        float endAngle = startAngle + 360f;

        while (elapsed < _rotationDuration)
        {
            float t = elapsed / _rotationDuration;
            float currentAngle = Mathf.Lerp(startAngle, endAngle, t);

            Vector3 angles = coin.localEulerAngles;
            angles.y = currentAngle;
            coin.localEulerAngles = angles;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Vector3 finalAngles = coin.localEulerAngles;
        finalAngles.y = endAngle % 360f;
        coin.localEulerAngles = finalAngles;

        _isRotating = false;
    }
}

using System.Collections;
using UnityEngine;

public class RewardSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform[] flyWaypoints;
    [SerializeField] private GameObject[] rewardPrefabs;
    [SerializeField] private CoinCounter coinCounter;
    [SerializeField] private Light spotLight;

    public float floatHeight = 2f;
    public float moveToCameraDuration = 0.8f;
    public float disappearDuration = 0.5f;
    public float flyWaypointDuration = 0.6f;

    private GameObject _currentReward;
    private bool _isAnimating = false;
    private Vector3 _originalScale;
    private float spawnCooldown = 1f;

    public bool IsAnimating() => _isAnimating;
       
    public GameObject SpawnReward()
    {
        if (rewardPrefabs.Length == 0 || spawnPoint == null || _isAnimating)
            return null;

        if (_currentReward != null)
            Destroy(_currentReward);

        GameObject prefab = rewardPrefabs[Random.Range(0, rewardPrefabs.Length)];
        _currentReward = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        _originalScale = _currentReward.transform.localScale;
        _isAnimating = false;

        return _currentReward;
    }

    public void AnimateRewardAndDestroy()
    {
        if (_currentReward != null && !_isAnimating)
            StartCoroutine(AnimateReward(_currentReward));
    }

    private IEnumerator AnimateReward(GameObject reward)
    {
        _isAnimating = true;

        Vector3 startPos = reward.transform.position;
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 toCamera = cameraForward.normalized * 6f;
        Vector3 targetPos = Camera.main.transform.position + toCamera;

        float elapsed = 0f;
        while (elapsed < moveToCameraDuration)
        {
            float t = elapsed / moveToCameraDuration;
            reward.transform.position = Vector3.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        reward.transform.position = targetPos;

        if (spotLight != null)
            spotLight.enabled = true;

        float rotationDuration = 1.5f;
        elapsed = 0f;
        float startAngle = reward.transform.eulerAngles.y;
        float endAngle = startAngle + 360f;
        while (elapsed < rotationDuration)
        {
            float t = elapsed / rotationDuration;
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            Vector3 rot = reward.transform.eulerAngles;
            rot.y = angle;
            reward.transform.eulerAngles = rot;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Vector3 prev = targetPos;
        if (flyWaypoints != null && flyWaypoints.Length > 0)
        {
            foreach (var waypoint in flyWaypoints)
            {
                Vector3 next = waypoint.position;
                elapsed = 0f;
                while (elapsed < flyWaypointDuration)
                {
                    float t = elapsed / flyWaypointDuration;
                    float curvedT = t * t; 
                    reward.transform.position = Vector3.LerpUnclamped(prev, next, curvedT);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                reward.transform.position = next;
                prev = next;
            }
        }

        elapsed = 0f;
        Vector3 originalScale = reward.transform.localScale;
        Renderer renderer = reward.GetComponentInChildren<Renderer>();
        Material material = renderer != null ? renderer.material : null;
        Color originalColor = material != null && material.HasProperty("_Color") ? material.color : Color.white;

        while (elapsed < disappearDuration)
        {
            float t = elapsed / disappearDuration;
            reward.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            if (material != null && material.HasProperty("_Color"))
            {
                Color c = originalColor;
                c.a = Mathf.Lerp(originalColor.a, 0, t);
                material.color = c;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (spotLight != null) spotLight.enabled = false;

        Destroy(reward);
        _currentReward = null;
        _isAnimating = false;

        if (coinCounter != null)
            coinCounter.AddCoin();

        yield return new WaitForSeconds(spawnCooldown);
    }

    public void SetRewardProgress(float normalizedProgress)
    {
        if (_currentReward != null && !_isAnimating)
        {
            Vector3 startPos = spawnPoint.position;
            _currentReward.transform.position = startPos + Vector3.up * (floatHeight * Mathf.Clamp01(normalizedProgress));
        }
    }

    public void HideReward()
    {
        if (_currentReward != null && !_isAnimating)
        {
            Destroy(_currentReward);
            _currentReward = null;
        }
    }
}

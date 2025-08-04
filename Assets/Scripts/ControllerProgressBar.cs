using UnityEngine;
using UnityEngine.UI;

public class ControllerProgressBar : MonoBehaviour
{
    [SerializeField] private Image _progressBar;
    [SerializeField] private RewardSpawner _rewardSpawner;
    [SerializeField] private ParticleSystem _swampBubbles;
    [SerializeField] private ParticleSystem _ringEffect;
    [SerializeField] private ParticleSystem _fireworkEffect;


    public float progressSpeed = 0.2f;

    private float targetProgress = 0f;
    private bool particlesActive = false;
    private float maxEmissionRate = 30f;
    private float emissionLerpSpeed = 5f;

    private ParticleSystem.EmissionModule _emission;
    private ParticleSystem.MainModule _main;

    private bool ringEffectPlaying = false;


    void Start()
    {
        _main = _swampBubbles.main;
        _emission = _swampBubbles.emission;

        if (_ringEffect != null)
        {
            _ringEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void Update()
    {
        if (_rewardSpawner.IsAnimating())
            return;

        float v = progressSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.UpArrow))
            ControlUpdate(v);
        else if (Input.GetKey(KeyCode.DownArrow))
            ControlUpdate(-v);
    }

    private void ControlUpdate(float delta)
    {
        float prevProgress = targetProgress;
        targetProgress += delta;
        targetProgress = Mathf.Clamp01(targetProgress);

        _progressBar.fillAmount = targetProgress;
        _main.simulationSpeed = Mathf.Lerp(0f, 5f, targetProgress);

        if (targetProgress > 0f && prevProgress <= 0f)
            _rewardSpawner.SpawnReward();

        if (targetProgress < 1f)
        {
            _rewardSpawner.SetRewardProgress(targetProgress);
        }
        else if (prevProgress < 1f && targetProgress >= 1f)
        {
            _progressBar.fillAmount = 0f;
            targetProgress = 0f;
            _rewardSpawner.SetRewardProgress(1f);
            _rewardSpawner.AnimateRewardAndDestroy();

            if (_fireworkEffect != null)
            {
                Invoke(nameof(PlayFireworkEffect), 0.85f);
            }
        }


        if (targetProgress <= 0f && prevProgress > 0f)
        {
            _rewardSpawner.HideReward();
        }

        if (particlesActive)
        {
            float currentRate = _emission.rateOverTime.constant;
            float targetRate = Mathf.Lerp(0f, maxEmissionRate, targetProgress);
            float newRate = Mathf.Lerp(currentRate, targetRate, Time.deltaTime * emissionLerpSpeed);

            var rate = _emission.rateOverTime;
            rate.constant = newRate;
            _emission.rateOverTime = rate;
        }

        ParticleOnAndOff();
        HandleRingEffect();
    }

    private void ParticleOnAndOff()
    {
        if (targetProgress <= 0f && particlesActive)
        {
            _swampBubbles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particlesActive = false;
        }
        else if (targetProgress > 0f && !particlesActive)
        {
            _swampBubbles.Play();
            var rate = _emission.rateOverTime;
            rate.constant = 0f;
            _emission.rateOverTime = rate;
            particlesActive = true;
        }
    }
    private void PlayFireworkEffect()
    {
        _fireworkEffect.Play();
    }

    private void HandleRingEffect()
    {
        if (_ringEffect == null)
            return;

        if (targetProgress > 0f && targetProgress < 1f)
        {
            if (!_ringEffect.isPlaying && !ringEffectPlaying)
            {
                _ringEffect.Play();
                ringEffectPlaying = true;
            }
        }
        else
        {
            if (_ringEffect.isPlaying)
            {
                _ringEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ringEffectPlaying = false;
            }
        }
    }
}
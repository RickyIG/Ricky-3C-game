using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource _footstepSfx;
    [SerializeField]
    private AudioSource _landingSfx;
    [SerializeField]
    private AudioSource _punchSfx;
    [SerializeField]
    private AudioSource _glideSfx;
    [SerializeField]
    private AudioSource _trampolineSfx;
    [SerializeField]
    private AudioSource _destroyableSfx;
    [SerializeField]
    private AudioSource _respawnableDestroyableSFX;

    public void PlayGlideSfx()
    {
        _glideSfx.Play();
    }

    public void StopGlideSfx()
    {
        _glideSfx.Stop();
    }

    private void PlayFootstepSfx()
    {
        _footstepSfx.volume = Random.Range(0.8f, 1f);
        _footstepSfx.pitch = Random.Range(0.8f, 1.5f);
        _footstepSfx.Play();
    }

    private void PlayLandingSfx()
    {
        _landingSfx.Play();
    }

    private void PlayPunchSfx()
    {
        _punchSfx.volume = Random.Range(0.8f, 1f);
        _punchSfx.pitch = Random.Range(0.8f, 1.5f);
        _punchSfx.Play();
    }

    public void PlayTrampolineSfx()
    {
        _trampolineSfx.Play();
    }

    public void PlayDestroyableSFX()
    {
        _destroyableSfx.Play();
    }

    public void PlayRespawnableDestroyableSFX()
    {
        _respawnableDestroyableSFX.Play();
    }
}

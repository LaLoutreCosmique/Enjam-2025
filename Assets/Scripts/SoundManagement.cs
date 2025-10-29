using UnityEngine;
using UnityEngine.Audio;

public class SoundManagement : MonoBehaviour
{
    public AudioSource audiosource;
    public AudioClip playButton;
    public AudioClip turnAround1;
    public AudioClip turnAround2;
    public AudioClip vinVerser1;
    public AudioClip vinVerser2;

    public void ClickButton()
    {
        audiosource.clip = playButton;
        audiosource.Play();
    }

    public void VerserVin()
    {
        int x = Random.Range(0, 2);
        if (x == 0)
        {
            audiosource.clip = vinVerser1;
        }
        else { audiosource.clip = vinVerser2; }
        audiosource.Play();
    }

    public void TournerTete()
    {
        int x = Random.Range(0, 2);
        if (x == 0)
        {
            audiosource.clip = turnAround1;
        }
        else { audiosource.clip = turnAround2; }
        audiosource.Play();
    }
}

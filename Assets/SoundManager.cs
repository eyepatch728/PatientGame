using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Audio Sources")]
     public AudioSource sfxSource;
     public AudioSource musicSource;
     public AudioSource voiceSource;

    [Header("SFX Clips")]
    public AudioClip ambulance, blabla, click, CorrectMatch, counter, crySniff, cutting, dentalCleaning, done,
                     drop, earSound1, earSound2, MedalShining, objectPlaced, Pouring, splash, transition, warning,
                     whoosh, wiping, writing, xray, zzz, laser;

    [Header("Voice Clips")]
    public AudioClip checkPatSight, ChooseFrame, ChooseOutFit, ComputerScan, CuttLens, EyeInfection,
                     getRidOfTheBactiera, GivePill, giveVacciene, GiveWater, GreatChoiceAfterOutfit, Happy,
                     InvitePatients, Lens, ListenLungWithStethoScope, ListenToTheTummyWithStethoScope,
                     LooksPerfectAfterOutfit, MatchTools, MeasureBloodPressure, MeasureingPatHeightandWeight,
                     MumSignForm, OhNoYouHaveAllergy, OintmentOnTheCutt, PatCanSeeBetter, PatFeelsBetter,
                     patHealthyAndStrong, PlaceBandage, PlaceTheStickersOnTheHand, PourSyrp, PutBlankter,
                     PutDripInPat, PutOnMedicalGoun, RollTheBandage, SignToFinishCheckUp, TemperatureHigh,
                     TempIsGood, TestHeaing, ThermoMeter, TissueInTrash, TissueWiping, ToolToRemoveBacteria,
                     Useointment, usePappeits, Virus, Welcome, WellDone, WowAfterOutFit, XrayDrag;

    [Header("Music Clips")]
    public AudioClip mainMenuMusic, ScenesMusic, WaitingRoom;

    private float sfxVolume, musicVolume, voiceVolume;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
      
        sfxVolume = Prefs.SoundVolume;
        musicVolume = 0.3f;
        voiceVolume = 1;

        sfxSource.volume = sfxVolume;
        musicSource.volume = musicVolume;
        voiceSource.volume = voiceVolume;
    }

    // --- Play Methods ---
    public void PlaySFX(AudioClip clip, bool loop = false)
    {
        sfxSource.loop = false;
        sfxSource.Stop();

        if (clip != null)
        {
            sfxSource.clip = clip;
            sfxSource.loop = loop;
            sfxSource.volume = sfxVolume;
            sfxSource.Play();
        }
    }


    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    public void PlayVoice(AudioClip clip)
    {
        if (clip != null)
            voiceSource.PlayOneShot(clip, voiceVolume);
    }

    // --- Stop Methods ---
    public void StopSFX() => sfxSource.Stop();
    public void StopMusic() => musicSource.Stop();
    public void StopVoice() => voiceSource.Stop();

    // --- Volume Controls ---
    public void SetSFXVolume(float vol)
    {
        sfxVolume = Mathf.Clamp01(vol);
        Prefs.SoundVolume = sfxVolume;
        sfxSource.volume = sfxVolume;
    }

    public void SetMusicVolume(float vol)
    {
        musicVolume = Mathf.Clamp01(vol);
        Prefs.MusicVolume = musicVolume;
        musicSource.volume = musicVolume;
    }

    public void SetVoiceVolume(float vol)
    {
        voiceVolume = Mathf.Clamp01(vol);
        Prefs.VoiceVolume = voiceVolume;
        voiceSource.volume = voiceVolume;
    }

    // --- Helper Methods for Quick Access ---
    public void PlayAmbulance() => PlaySFX(ambulance);
    public void PlayBlabla() => PlaySFX(blabla);
    public void PlayClick() => PlaySFX(click);
    public void PlayCorrectMatch() => PlaySFX(CorrectMatch);
    public void PlayCounter() => PlaySFX(counter);
    public void PlayCrySniff() => PlaySFX(crySniff);
    public void PlayCutting() => PlaySFX(cutting);
    public void PlayDentalCleaning() => PlaySFX(dentalCleaning , true);
    public void PlayDone() => PlaySFX(done);
    public void PlayDrop() => PlaySFX(drop);
    public void PlayEarSound1() => PlaySFX(earSound1);
    public void PlayEarSound2() => PlaySFX(earSound2);
    public void PlayMedalShining() => PlaySFX(MedalShining);
    public void PlayObjectPlaced() => PlaySFX(objectPlaced);
    public void PlayPouring() => PlaySFX(Pouring);
    public void PlaySplash() => PlaySFX(splash);
    public void PlayTransition() => PlaySFX(transition);
    public void PlayWarning() => PlaySFX(warning);
    public void PlayWhoosh() => PlaySFX(whoosh);
    public void PlayWiping() => PlaySFX(wiping);
    public void PlayWriting() => PlaySFX(writing);
    public void PlayXray() => PlaySFX(xray);
    public void PlayZzz() => PlaySFX(zzz , true);
    public void PlayLaser() => PlaySFX(laser , true);


    // Voice Clips
    public void PlayCheckPatSight() => PlayVoice(checkPatSight);
    public void PlayChooseFrame() => PlayVoice(ChooseFrame);
    public void PlayChooseOutFit() => PlayVoice(ChooseOutFit);
    public void PlayComputerScan() => PlayVoice(ComputerScan);
    public void PlayCuttLens() => PlayVoice(CuttLens);
    public void PlayEyeInfection() => PlayVoice(EyeInfection);
    public void PlayGetRidOfTheBacteria() => PlayVoice(getRidOfTheBactiera);
    public void PlayGivePill() => PlayVoice(GivePill);
    public void PlayGiveVacciene() => PlayVoice(giveVacciene);
    public void PlayGiveWater() => PlayVoice(GiveWater);
    public void PlayGreatChoiceAfterOutfit() => PlayVoice(GreatChoiceAfterOutfit);
    public void PlayHappy() => PlayVoice(Happy);
    public void PlayInvitePatients() => PlayVoice(InvitePatients);
    public void PlayLens() => PlayVoice(Lens);
    public void PlayListenLungWithStethoScope() => PlayVoice(ListenLungWithStethoScope);
    public void PlayListenToTheTummyWithStethoScope() => PlayVoice(ListenToTheTummyWithStethoScope);
    public void PlayLooksPerfectAfterOutfit() => PlayVoice(LooksPerfectAfterOutfit);
    public void PlayMatchTools() => PlayVoice(MatchTools);
    public void PlayMeasureBloodPressure() => PlayVoice(MeasureBloodPressure);
    public void PlayMeasuringPatHeightAndWeight() => PlayVoice(MeasureingPatHeightandWeight);
    public void PlayMumSignForm() => PlayVoice(MumSignForm);
    public void PlayOhNoYouHaveAllergy() => PlayVoice(OhNoYouHaveAllergy);
    public void PlayOintmentOnTheCutt() => PlayVoice(OintmentOnTheCutt);
    public void PlayPatCanSeeBetter() => PlayVoice(PatCanSeeBetter);
    public void PlayPatFeelsBetter() => PlayVoice(PatFeelsBetter);
    public void PlayPatHealthyAndStrong() => PlayVoice(patHealthyAndStrong);
    public void PlayPlaceBandage() => PlayVoice(PlaceBandage);
    public void PlayPlaceTheStickersOnTheHand() => PlayVoice(PlaceTheStickersOnTheHand);
    public void PlayPourSyrp() => PlayVoice(PourSyrp);
    public void PlayPutBlanket() => PlayVoice(PutBlankter);
    public void PlayPutDripInPat() => PlayVoice(PutDripInPat);
    public void PlayPutOnMedicalGown() => PlayVoice(PutOnMedicalGoun);
    public void PlayRollTheBandage() => PlayVoice(RollTheBandage);
    public void PlaySignToFinishCheckUp() => PlayVoice(SignToFinishCheckUp);
    public void PlayTemperatureHigh() => PlayVoice(TemperatureHigh);
    public void PlayTempIsGood() => PlayVoice(TempIsGood);
    public void PlayTestHearing() => PlayVoice(TestHeaing);
    public void PlayThermometer() => PlayVoice(ThermoMeter);
    public void PlayTissueInTrash() => PlayVoice(TissueInTrash);
    public void PlayTissueWiping() => PlayVoice(TissueWiping);
    public void PlayToolToRemoveBacteria() => PlayVoice(ToolToRemoveBacteria);
    public void PlayUseOintment() => PlayVoice(Useointment);
    public void PlayUsePappeits() => PlayVoice(usePappeits);
    public void PlayVirus() => PlayVoice(Virus);
    public void PlayWelcome() => PlayVoice(Welcome);
    public void PlayWellDone() => PlayVoice(WellDone);
    public void PlayWowAfterOutfit() => PlayVoice(WowAfterOutFit);
    public void PlayXrayDrag() => PlayVoice(XrayDrag);

    // Music Clips
    public void PlayMainMenuMusic() => PlayMusic(mainMenuMusic);
    public void PlayScenesMusic() => PlayMusic(ScenesMusic);
    public void PlayWaitingRoomMusic() => PlayMusic(WaitingRoom);

    public void StopAmbulance() => StopSFX();
    public void StopClick() => StopSFX();
    public void StopCorrectMatch() => StopSFX();
    public void StopCounter() => StopSFX();
    public void StopCrySniff() => StopSFX();
    public void StopCutting() => StopSFX();
    public void StopDentalCleaning() => StopSFX();
    public void StopDone() => StopSFX();
    public void StopDrop() => StopSFX();
    public void StopEarSound1() => StopSFX();
    public void StopEarSound2() => StopSFX();
    public void StopMedalShining() => StopSFX();
    public void StopObjectPlaced() => StopSFX();
    public void StopPouring() => StopSFX();
    public void StopSplash() => StopSFX();
    public void StopTransition() => StopSFX();
    public void StopWarning() => StopSFX();
    public void StopWhoosh() => StopSFX();
    public void StopWiping() => StopSFX();
    public void StopWriting() => StopSFX();
    public void StopXray() => StopSFX();
    public void StopZzz() => StopSFX();
    public void StopLaser() => StopSFX();

    public void StopCheckPatSight() => StopVoice();
    public void StopChooseFrame() => StopVoice();
    public void StopChooseOutFit() => StopVoice();
    public void StopComputerScan() => StopVoice();
    public void StopCuttLens() => StopVoice();
    public void StopEyeInfection() => StopVoice();
    public void StopGetRidOfTheBacteria() => StopVoice();
    public void StopGivePill() => StopVoice();
    public void StopGiveVacciene() => StopVoice();
    public void StopGiveWater() => StopVoice();
    public void StopGreatChoiceAfterOutfit() => StopVoice();
    public void StopHappy() => StopVoice();
    public void StopInvitePatients() => StopVoice();
    public void StopLens() => StopVoice();
    public void StopListenLungWithStethoScope() => StopVoice();
    public void StopListenToTheTummyWithStethoScope() => StopVoice();
    public void StopLooksPerfectAfterOutfit() => StopVoice();
    public void StopMatchTools() => StopVoice();
    public void StopMeasureBloodPressure() => StopVoice();
    public void StopMeasuringPatHeightAndWeight() => StopVoice();
    public void StopMumSignForm() => StopVoice();
    public void StopOhNoYouHaveAllergy() => StopVoice();
    public void StopOintmentOnTheCutt() => StopVoice();
    public void StopPatCanSeeBetter() => StopVoice();
    public void StopPatFeelsBetter() => StopVoice();
    public void StopPatHealthyAndStrong() => StopVoice();
    public void StopPlaceBandage() => StopVoice();
    public void StopPlaceTheStickersOnTheHand() => StopVoice();
    public void StopPourSyrp() => StopVoice();
    public void StopPutBlanket() => StopVoice();
    public void StopPutDripInPat() => StopVoice();
    public void StopPutOnMedicalGown() => StopVoice();
    public void StopRollTheBandage() => StopVoice();
    public void StopSignToFinishCheckUp() => StopVoice();
    public void StopTemperatureHigh() => StopVoice();
    public void StopTempIsGood() => StopVoice();
    public void StopTestHearing() => StopVoice();
    public void StopThermometer() => StopVoice();
    public void StopTissueInTrash() => StopVoice();
    public void StopTissueWiping() => StopVoice();
    public void StopToolToRemoveBacteria() => StopVoice();
    public void StopUseOintment() => StopVoice();
    public void StopUsePappeits() => StopVoice();
    public void StopVirus() => StopVoice();
    public void StopWelcome() => StopVoice();
    public void StopWellDone() => StopVoice();
    public void StopWowAfterOutfit() => StopVoice();
    public void StopXrayDrag() => StopVoice();

}
public static class Prefs
{
    public static float SoundVolume
    {
        get => PlayerPrefs.GetFloat("SoundVolume", 1f);
        set => PlayerPrefs.SetFloat("SoundVolume", Mathf.Clamp01(value));
    }

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat("MusicVolume", 1f);
        set => PlayerPrefs.SetFloat("MusicVolume", Mathf.Clamp01(value));
    }

    public static float VoiceVolume
    {
        get => PlayerPrefs.GetFloat("VoiceVolume", 1f);
        set => PlayerPrefs.SetFloat("VoiceVolume", Mathf.Clamp01(value));
    }
}
`Legato` is a toolset for integrating an adaptive music system into games, based on cinematic techniques. With the addition of a single Unity component, a game object can be granted one or more musical elements, which can be cued into the soundtrack with a single public method call. 

 All of the scripts provided in the package (with the exception of the ones that are used exclusively in the demo scenes) are contained within the namespace \textsf{Legato}.

The tool consists of two main parts: one to facilitate the rendering of the required samples in Reaper and another to allow the configuration and execution of the tool in Unity.

Unity version: 2022.3.9f1

# Installation

The Legato package is available from: https://github.com/Legato-Audio/Legato

For the tool to function correctly, it is also required to download \textit{extOSC}: https://assetstore.unity.com/packages/tools/input-management/extosc-open-sound-control-72005

Both of these packages must be included in the \textsf{Assets} directory of the Unity project.

# Reaper Configuration

The first step is to import the Reaper configuration into the project. 
- Open ReaperProject.rpp, located in the directory Assets/Legato.
- In the menu bar, go to Options->Preferences. In the General section, click on ``Import configuration...'' and select legato.ReaperConfigZip (also located in Assets/Legato).

The project is now ready to be used, but must be set up in the following format:

- The first tracks to appear must correspond one-to-one with the different instruments that will be used in the game's adaptive soundtrack. The instrument tracks must contain no MIDI or sound elements, only virtual instruments and other effects. There is no additional limitation to the sounds that can be used as instruments: any combination of effects that Reaper is able to process is valid.
- The remaining tracks must correspond one-to-one with the leitmotifs that will be used in the soundtrack. These tracks can contain effects, but these will not appear anywhere in the final renders.
- Both the instrument tracks and the motif tracks must appear in the same order that the instruments and motifs do in the configuration menu in Unity. When the project is rendered, the MIDI items in the leitmotif tracks above will be combined with the individual instrument tracks to create the final audio clips.

Some additional points:

- Calling the action directly from Reaper can cause problems, as any previous versions of the files created by the action will not be deleted. To avoid this issue, the action should only be called from the configuration menu in Unity.
- The beats per minute and beats per measure of the Reaper project will be overwritten.
- Reaper uses a render queue to store all individual renders before producing them all. Since this action flushes the queue, any previous renders that may still be in the queue will also be processed.
- The action saves the Reaper project before doing anything else, but does not save it after it finishes. This means that Reaper will detect the project as dirty (containing unsaved changes) even if there are no net changes relative to the previous state of the project. This also means that if the user wishes to recover anything that was overwritten by the action (such as the project BPM) they can simply close Reaper and discard the unsaved changes.
- Selecting a track name before the custom action is called can cause issues during rendering. To avoid this, before rendering, click anywhere in the area to the right, highlighted in green in the following screenshot:

# Legato Configuration

Legato is configured from Unity, in the LegatoConfiguration pop-up that can be found in Window->Legato within the editor.

The first fields of the window are for listing and naming the instruments, leitmotifs and tempos that will be used in the soundtradk. The number of instruments and leitmotifs set in this window must be the same as the number of tracks in the Reaper project.

The next section contains two matrices to choose the combinations of motif/instrument and motif/tempo to render.

At the bottom of the window there are buttons to save changes, render and regenerate assets. This last button should only be clicked if any issue ocurred during the creation of the Unity assets (after rendering) or if the user decided to manually create their audio files (not using this tool). 

In case the user created their files, these must be located in Assets/Resources/Legato/RenderedSamples, and their names must be formatted as: motif.<motif name>.<instrument name>.<tempo>.wav (all of this before clicking the regenerate assets button).

For example, the clip for the motif "chords A" with the instrument "piano" and the tempo 120 should be called motif.chords A.piano.120.wav

# Legato_Emitter

`public class Legato_Emitter : MonoBehaviour`

## Description
The Legato_Emitter class manages the audio playback for different sound scenes, allowing for detailed control over volume, spatial blend, and more.

Legato_Emitter is the main Unity component of the toolset; the source of the adaptive soundtrack. It can be found in the Assets/Legato/Components directory.

The emitter is a singleton with a publicly accessible instance. The user can add the component to a game object in the scene or instantiate the prefab found in Assets/Legato/Prefabs.

## Serialized Properties

- `bool debug`
  - Activates informative message logs about the music and events being played during runtime.
- `float volume`
  - Controls the global volume of the music that is played.
- `SoundScene[] soundScenes`
  - `float spatialBlend`
    - How affected by 3D spatialisation the scene is.
  - `SoundSettings3D soundSettings3D`
    - Controller for the AurioRollofMode, minDistance and maxDistance attributes of a 3D scene.
  - `Channel[] channels`
    - `Fragment[] fragments`
      - Array of musical fragments that will be played by default.
    - `SortingAlgorithms algorithm`
      - The algorithm that is used to arrange the fragments.
    - `float volume`
      - Controls the channel's volume.
    - `Instrument instrument`
      - The instrument that the channel will be played in at the start.
    - `Tempo tempo`
      - The tempo that the channel will be played in at the start.
    - ` int beatsPerMeasure`
      - Number of beats per measure, used for synchronising channels and changing between fragments at the right time.
    - `bool playOnStart`
      - Whether the channel will begin playing at the start or not.

## Public Properties

- `AudioMixerGroup output`
    - Set whether the audio should play through an Audio Mixer first or directly through the Audio Listener.
- `const float loadingBuffer = 0.4f`
    - Time (in seconds) that is allowed for audio clips to load between measures.


## Public Methods

- `public static Legato_Emitter GetInstance()`
    - Returns the singleton instance of the `Legato_Emitter`.

- `public void SetVolume(int c, float v)`
    - Sets the volume of a specific channel of the current scene.
    - **Parameters:**
        - `int c`: The channel to set the volume to.
        - `float v`: The volume of the channel (0.0 to 1.0).

- `public void SetGlobalVolume(float v)`
    - Sets the global volume of the emitter.
    - **Parameters:**
        - `float v`: The volume of the emitter (0.0 to 1.0).

- `public void SetScene(int s)`
    - Sets the current scene.
    - **Parameters:**
        - `int s`: The scene to set as current.

- `public void SetSpatialBlend(float spatialBlend)`
    - Changes the spatial blend attribute of the current scene.
    - **Parameters:**
        - `float spatialBlend`: The value of spatial blend, where 0.0 is fully 2D and 1.0 is fully 3D.

- `public void SetSpatialBlend(int s, float spatialBlend)`
    - Changes the spatial blend attribute of the specified scene.
    - **Parameters:**
        - `int s`: The scene to change the spatial blend to.
        - `float spatialBlend`: The value of spatial blend, where 0.0 is fully 2D and 1.0 is fully 3D.

- `public void Set3DRolloffMode(AudioRolloffMode rolloffMode)`
    - Sets the rolloff mode of the current scene's 3D audio source.
    - **Parameters:**
        - `AudioRolloffMode rolloffMode`: The new rolloff mode.

- `public void Set3DMinDistance(float minDistance)`
    - Sets the minimum distance of the current scene's 3D audio source.
    - **Parameters:**
        - `float minDistance`: The new value of minDistance.

- `public void Set3DMaxDistance(float maxDistance)`
    - Sets the maximum distance of the current scene's 3D audio source.
    - **Parameters:**
        - `float maxDistance`: The new value of maxDistance.

- `public void PlayMotif(int c, string motif, bool interrupt = false, float priority = 1f, float priorityOverTime = 0f)`
    - Queues a motif to be played in a specific channel.
    - **Parameters:**
        - `int c`: The channel to queue the motif in.
        - `string motif`: The name of the motif to add to the queue.
        - `bool interrupt`: Whether to interrupt the current motif.
        - `float priority`: The initial priority of the motif.
        - `float priorityOverTime`: The rate at which the motif's priority increases or decreases over time.

- `public void PlayMotif(int c, Fragment motif, bool interrupt = false, float priority = 1f, float priorityOverTime = 0f)`
    - Queues a motif to be played in a specific channel.
    - **Parameters:**
        - `int c`: The channel to queue the motif in.
        - `Fragment motif`: The motif to add to the queue.
        - `bool interrupt`: Whether to interrupt the current motif.
        - `float priority`: The initial priority of the motif.
        - `float priorityOverTime`: The rate at which the motif's priority increases or decreases over time.

- `public void CancelMotif(int c, string motif)`
    - Removes all instances of a given motif from a specific channel's motif queue.
    - **Parameters:**
        - `int c`: The channel to look for the motif.
        - `string motif`: The name of the motif to cancel.

- `public void CancelMotif(int c, Fragment motif)`
    - Removes all instances of a given motif from a specific channel's motif queue.
    - **Parameters:**
        - `int c`: The channel to look for the motif.
        - `Fragment motif`: The motif to cancel.

- `public void Resume(int c)`
    - Resumes a specific channel of the current scene.
    - **Parameters:**
        - `int c`: The channel to resume.

- `public void Stop(int c)`
    - Stops a specific channel of the current scene.
    - **Parameters:**
        - `int c`: The channel to stop.

- `public void StopAll()`
    - Stops all the channels of the current scene.

- `public void ResumeAll(bool onlyPreviouslyPlaying = true)`
    - Resumes play on all the channels of the current scene.
    - **Parameters:**
        - `bool onlyPreviouslyPlaying`: If true, only channels that were playing before `StopAll` was called will be resumed.

- `public void SetInstrument(int c, string instrument, bool changeCurrent = false, float fadeDuration = 0f)`
    - Sets the instrument of a specific channel.
    - **Parameters:**
        - `int c`: The channel to set the instrument to.
        - `string instrument`: The name of the instrument to set.
        - `bool changeCurrent`: Change the current fragment's instrument immediately or wait until the next fragment.
        - `float fadeDuration`: The length of the crossfade in seconds when changing the instrument.

- `public void SetInstrument(int c, Instrument instrument, bool changeCurrent = false, float fadeDuration = 0f)`
    - Sets the instrument of a specific channel.
    - **Parameters:**
        - `int c`: The channel to set the instrument to.
        - `Instrument instrument`: The instrument to set.
        - `bool changeCurrent`: Change the current fragment's instrument immediately or wait until the next fragment.
        - `float fadeDuration`: The length of the crossfade in seconds when changing the instrument.

- `public void SetTempo(int c, int tempo)`
    - Sets the tempo of a specific channel of the current scene.
    - **Parameters:**
        - `int c`: The channel to set the tempo to.
        - `int tempo`: The tempo in bpm to set.

- `public void SetTempo(int c, Tempo tempo)`
    - Sets the tempo of a specific channel of the current scene.
    - **Parameters:**
        - `int c`: The channel to set the tempo to.
        - `Tempo tempo`: The tempo to set.

- `public void SetTempo(int tempo)`
    - Sets the tempo in all channels of the current scene.
    - **Parameters:**
        - `int tempo`: The tempo in bpm to set.

- `public void SetTempo(Tempo tempo)`
    - Sets the tempo in all channels of the current scene.
    - **Parameters:**
        - `Tempo tempo`: The tempo to set.


# Legato_Event 

**Description:**
The `Legato_Event` class represents an abstract event in the Legato system that can be triggered to perform specific actions related to audio playback.

## Protected Properties

- `Legato_Emitter emitter`  
  The Legato emitter instance.

- `int channel`  
  The audio channel that the event will trigger on.

## Public Methods

- `public abstract void Trigger()`  
  Abstract method to trigger the event. Subclasses must implement this method.


# Legato_ResumeEvent 

**Description:**
The `Legato_ResumeEvent` class represents an event that triggers the resuming of audio playback in the Legato system. It inherits from the `Legato_Event` class.

## Public Properties

- `bool applyToAll`  
  If true, triggering this event will resume all channels at once.
  
- `bool onlyResumePreviouslyPlaying`  
  If true, only channels that were playing before `StopAll` was called will be resumed.

## Public Methods

- `public override void Trigger()`  
  Triggers the event. If `applyToAll` is true, resumes all channels. Otherwise, resumes the specific channel.


# Legato_StopEvent 

**Description:**
The `Legato_StopEvent` class represents an event that triggers the stopping of audio playback in the Legato system. It inherits from the `Legato_Event` class.

## Public Properties

- `bool applyToAll`  
  If true, triggering this event will stop all channels at once.

## Public Methods

- `public override void Trigger()`  
  Triggers the event. If `applyToAll` is true, stops all channels. Otherwise, stops the specific channel.


# Legato_MotifEvent 

**Description:**
The `Legato_MotifEvent` class represents an event that triggers the playing of a motif in the Legato system. It inherits from the `Legato_Event` class.

## Public Properties

- `Fragment motif`  
  The motif that will play on triggering the event.

- `bool interrupt`  
  If true, the motif will be played in the selected channel at the next possible bar. If false, it will be added to the dynamic priority queue.

- `float priority`  
  Triggered motifs are added to a queue. When the current fragment ends, the highest priority motif will play next.

- `float priorityOverTime`  
  Rate that the motif's queued priority changes over time (in units/second). Can be positive or negative. If the motif's priority is below 0, the motif is removed from the queue.

## Public Methods

- `public override void Trigger()`  
  Triggers the event, playing the motif on the specified channel with optional interruption and priority settings.


# Legato_InstrumentEvent

**Description:**
The `Legato_InstrumentEvent` class represents an event that triggers the setting of a new instrument in the Legato system. It inherits from the `Legato_Event` class.

## Public Properties

- `Instrument instrument`  
  The instrument that will be set on triggering the event.

- `bool changeCurrent`  
  If true, any music currently playing be crossfade to the instrument on event trigger. If false, the current fragment will finish playing with its current instrument and the next one in the channel will use the new instrument.

- `float fadeDuration`  
  If greater than 0, triggering the event will cause the channel to crossfade its instrument over fadeDuration seconds.

## Public Methods

- `public override void Trigger()`  
  Triggers the event, setting the instrument on the specified channel with optional crossfade.


# Legato_TempoEvent 

**Description:**
The `Legato_TempoEvent` class represents an event that triggers the setting of a new tempo in the Legato system. It inherits from the `Legato_Event` class.

## Public Properties

- `Tempo tempo`  
  The tempo that will be set on triggering the event.

- `bool applyToAll`  
  If true, changes in tempo will apply to all channels at once, regardless of selected channel.

## Public Methods

- `public override void Trigger()`  
  Triggers the event, setting the tempo on the specified channel or globally with optional application to all channels.


# 2024_11_27_IsInWindowNativeGameLocker

This feature allows toggling functionality based solely on whether a specific game is active. It ensures that remote controls are disabled outside the game to prevent misuse.

I am developing a Twitch Plays game. However, Twitch often involves trolls who may attempt to quit the game maliciously. To counter this:

1. The game needs to relaunch automatically if it is closed.
2. Keyboard, XInput, and other control features must deactivate within **0.1 seconds** or less when the game exits. If not, trolls could exploit this to shut down or hack the computer.

The code need to be in Unity to be fast to toggle the feature that are compute with Job System and Computer Shader in Unity3D.

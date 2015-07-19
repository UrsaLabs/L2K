# L2K
Launchpad2Keyboard - Map your Novation Launchpad to keyboard presses.

This project has been developed to allow the control of Twitch streaming software (Xsplit, OBS) using a Novation Launchpad. 
It can however be used to bind any of the launchpad buttons to any keyboard keystroke which can then be captured by a number
of applications that subscribe to the Windows hotkey API.

This project uses the Launchpad Library by IntelOrca [https://github.com/IntelOrca/launchpad]
In order to compile and develop you will need:
- midi-dot-net ( running Install-Package midi-dot-net in Nugget PC will do)
- InputSimulator (running Install-Package InputSimulator will do)
- NotifyIcon.Wpf (running Install-Package Hardcodet.NotifyIcon.Wpf will do)

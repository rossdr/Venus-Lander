In 1982, one Stephen "Steve" Sullivan of Kansas City, MO wrote a lander program in Microsoft BASIC, which he floated to Color Computer News. In September, they published it.

The Lander, in Sullivan's original code, was a "Lunar" lander. But as you will see, at heart it is not. There is sound, which few known moons should carry; but also, we get sky (the land is black against it) and terminal-velocity. Sullivan's colorset had green as the background. He could have used red like Mars but chose against it; he also wasn't keen on blue. So he named his game "Venus Lander" on submission to the magazine.

This conversion uses a custom parser to handle the "Logo" pattern of Color Computer's DRAW command. One could just as well run the code in the Xroar emulator, saving the bitmaps. But at 256x192, I didn't think it mattered.

I did not however use a music parser. Some such parsers are floating around, e.g. for the Commodore; but I have not yet found a good one for CoCo. I ran those sounds in Xroar and held a microphone to the speaker; I then resaved that noise as WAV. In my opinion Xroar should allow some native output to FLAC; in my further opinion, Visual Studio should support FLAC as-well-as-but-preferably-instead-of WAV. Pardon the rant.

Besides that, everything is pretty much as Sullivan left it. The keys are: left and right to thrust, which does not use fuel; up arrow to thrust, which does. You land on a field chosen at (pseudo)random: a city, a landscape, and a landscape with a tower. Each has three possible platforms, flat and wide enough to land upon. You have to land the full lander on it, at a slow speed.

<img width="286" height="238" alt="Lander_City" src="https://github.com/user-attachments/assets/a28b60a3-8118-4c20-ae36-9b6d00b47d18" />


Sullivan displayed the score and remaining fuel in a popup separate from the game field proper. He was writing in 16K RAM which made painful to write text in PMODE. I carry this design forward. (For my part this is my first solo Git push, so I'm starting slow.)

The logic is turn-based upon the Timer "Tick" event, so fairly seamless in this low resolution. Your main adversary here is Isaac Newton "the deadliest S.O.B. in space", here employing his gravitational acceleration. The constants are kludged just to make a game of it, not a simulator. This event keeps track of whether fuel is present to handle the "Keydown" event for thrust anymore. And it detects collisions based upon the background landscape bitmap.



David Ross

30 August 2025


# [Netrve's DeepStorage GUI (Continued)]()

![Image](https://i.imgur.com/buuPQel.png)

Update of Netrves mod https://steamcommunity.com/sharedfiles/filedetails/?id=2169841018

![Image](https://i.imgur.com/pufA0kM.png)
	
![Image](https://i.imgur.com/Z4GOv8H.png)

# Notice

Due to focus on other project and my lack of interest in RimWorld for now, I'm not maintaining my mods for the time being. They are all open source, so feel free to fork them!

Thanks to Owlchemist's awesome work, there is an update for 1.4 out.

# Netrve's DeepStorage GUI

A Mod-Mod for [LittleWhiteMouse's DeepStorage](https://steamcommunity.com/sharedfiles/filedetails/?id=1617282896) that overhauls the user interface it offers.

Currently, changes both the Storage Tab and the Right Click Orders menu.

Both come with options for sorting, customizable appearance, and search functionality. Aside from offering some Quality Of Life improvements, I have also paid a lot of attention to optimizing the inner workings as best as I could without compromising compatibility.

Requires LWM DeepStorage.

# Mod Support

As long as a mod makes use of DeepStorage's elements and definitions, my menu should take over. This includes Little Storage 1 &amp; 2 for example.

Some mods do however define their own menus or assign a different one than expected. Those would need patches, but should continue to work normally just without my menu.

# Incompatibilities
 
In Vanilla the list of orders when you right click on a thing with a pawn actively selected is called FloatMenu, so to address the issues with that menu for storage units I have to intercept the TryMakeFloatMenu call to reroute that call to my function (if applies). 

If other mods did their implementation well, this shouldn't be an issue as I let calls pass through should the target not be covered by my mod. Some do however manipulate it in ways that I can't forsee, so should you run into an issue with no menu showing please post a HugsLib log (Ctrl+F12) into the Feedback topic so I can narrow it down.

# Known Issues

- Simply Storage - Refrigeration not using the new UI

# Source

The full source code, licensed under MPL 2.0, can be found here: https://github.com/Dakraid/RW_DSGUI

Current Version: v1.5.0

![Image](https://i.imgur.com/PwoNOj4.png)



-  See if the the error persists if you just have this mod and its requirements active.
-  If not, try adding your other mods until it happens again.
-  Post your error-log using [HugsLib](https://steamcommunity.com/workshop/filedetails/?id=818773962) or the standalone [Uploader](https://steamcommunity.com/sharedfiles/filedetails/?id=2873415404) and command Ctrl+F12
-  For best support, please use the Discord-channel for error-reporting.
-  Do not report errors by making a discussion-thread, I get no notification of that.
-  If you have the solution for a problem, please post it to the GitHub repository.
-  Use [RimSort](https://github.com/RimSort/RimSort/releases/latest) to sort your mods



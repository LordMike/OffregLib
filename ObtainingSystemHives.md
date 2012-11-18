# Obtaining system hives
A small walkthrough on how to export system hives.

## Purpose

Having a library to work with Registry Hives is fun and all, but what good does it do without any data to work with?
I have found that the most effective method is to export your own computers hive(s). 

## Overview

* Locate the system hives
* Find that they're locked
* Bypass locking, and export them anyways
* Load and recurse them in OffregLib

## Details & Examples

### Locate the system hives

Microsoft stores the Registry in multiple hives. There are some hives pr. user, a hive for the system, a hive for software and so on. Most of them are stored in this folder:

    C:\Windows\System32\config

This folder contains a lot of critical files, such as the SAM database (Windows authentication database), multiple Registry Hives and backups of these. Other hives are stord in various locations, such as:

    %UserProfile%\AppData\Local\Microsoft\Windows\Usrclass.dat

And

    %LocalAppData%\Microsoft\Windows\Usrclass.dat

See more details at the [Wikipedia page](http://en.wikipedia.org/wiki/Windows_Registry#Windows_NT-based_operating_systems "Registry Hive locations") about the topic.

### Find that they're locked

However, if we try at all to open, edit, delete or otherwise manipulate these hive files, we'll discover that they're all locked by System. This is a security feature in Windows, where the file can be exclusively opened by one process, thus rendering reading by other parties (such as us or virus'es) impossible.

Except - if we're doing a backup. 

Microsoft introduced [Volume Shadow Copy](http://en.wikipedia.org/wiki/Shadow_Copy "Volume Shadow Copy") in XP and forward, which allows a process to access locked and hidden files (such as disk volume information, registry hives and so on). The purpose here is making a backup, we can use this to our advantage.

### Bypass locking, and export them anyways

If you have Shadow Copies enabled (also named System Restore), you'll probably already have a snapshot of C:\ available. I use a handy tool, [Shadow Explorer](http://www.shadowexplorer.com/downloads.html "Shadow Explorers download page"), to explore these shadow copies. It allows you to browse the snapshot, and export any file (f.ex. the Registry Hives). You can then load them into the OffregLib as files, and work with them all you'd like.

If there are no snapshots available, follow the guide here:
<iframe class="imgur-album" width="100%" height="550" frameborder="0" src="http://imgur.com/a/jKQ4l/embed"></iframe>

Once that's done, you can return to Shadow Explorer (re-start the program if it hasn't registered the new snapshot), and then browse to any of the locations mentioned above - select a hive (the SOFTWARE hive is by far the largest on all systems I've seen), right click and export it.

### Load and recurse them in OffregLib

You should now have a Registry Hive file filled with data, open it in OffregLib (follow the example in the Test Application or in the Readme)
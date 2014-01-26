LSKYSmoothStreamPlayer
======================

LSKYSmoothStreamPlayer is a very simple Silverlight client for IIS7's Smooth Streaming.

There are two versions of the player in this repo:
 * _Live is designed for live streams, and does not have a "scrub bar". It will always skip ahead to the most current point in the video and stay there. This was created because the default Silverlight player for IIS Smooth Streaming starts at the beginning of the footage, which is obviously not ideal for a live event.
 * _PreRecorded includes the scrub bar, and is intended for playback of the recorded stream after the event has ended. It starts playback at the beginning, and behaves as any other streaming video player would. It is being phased out in favor of an HTML5 player, so it may not get updated much.

This player was designed for use in the Living Sky School Division, and as such includes our logos and branding (which can be easily removed).

How to compile
==============

To compile this you will need:
 * Visual Studio (tested on 2010 and 2012 - Express editions should work too)
 * Silverlight 4 SDK  or Silverlight 5 SDK
 * IIS Smooth Streaming Client 1.5 - Download should be available from http://www.microsoft.com/en-ca/download/details.aspx?id=8227. Do not use a newer version (such as 2.0), because they will not work

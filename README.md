# Blurift GLControl

This is work based on JFMajor and Nick Gravelyn's posts

http://blogs.msdn.com/b/nicgrave/archive/2011/03/25/wpf-hosting-for-xna-game-studio-4-0.aspx
http://www.jfmajor.com/post/38447851593/monogame-in-wpf

## Supported Platforms

* This has only been tested on windows.

## Quick Start

Well for started this uses the opengl branch of monogame, some things will need to be refenced in your C# project on visual studio to also get this working. To use this there is an included edited version of the Monogame .dll files which have some slight differences in how the GraphicsDevice is created so if you do not use this dll you wpf app will no compile.

I do plan on making a example app also.

## Limitations

Currently only one graphics control can be created and used at once. I may look in to this at a later date.

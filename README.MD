# SkyEditor.Core
SkyEditor.Core is a plugin framework with file-editing features.

## Notable Features
These descriptions are not comprehensive, but give a general overview of what this project can do.

### The PluginManager class
The centerpiece of this repository is the Plugin Manager, which provides a framework for dynamically loading assembly plugins in a way that those plugins can have their own plugins.

Applications without plugin support can still use this for constructor-based dependency injection with its ability to create instances of types.

### Extension Management (Extensions namespace)
Manages collections of files, the meaning of which can be defined through plugins. One pre-defined extension collection type allows plugins to be dynamically loaded through the plugin manager.

### File Plugins (IO namespace)
Framework built on top of the plugin manager that can open files on disk as classes defined by plugins.

This is built on top of the GenericFile class, which allows editing any file on a byte level. The most notable feature of this class is the ability to load a file either as a stream or a byte array, which is mostly transparent to callers. (Pay attention to the IsThreadSafe property before performing any multi-threaded read or write operations.)

Thanks to the I/O abstraction (IIOProvider interface), most classes built on top of this framework can load data from fake filesystems, whether it be simulated in memory or from an archive file. The possibilities are wide open: one could write a class to model a proprietary zip file, and other classes can load their data from one of the contained files as if it was a file system.

### UI Framework (UI namespace)
Plugin framework for UI controls that can interact with view models that can interact with arbitrary classes (such as those that represent files loaded using the IO namespace).

### Projects (Projects namespace)
Files that allow for custom projects and solutions (resembling projects as seen in IDE's).

### Application View Model
View model for a file editor window making use (directly or indirectly) of all of the above. Notable features:
* Opening and closing open files
* Choosing view models for those files
* Opening one solution at a time
* Showing or hiding logical menu items according to the selected file
* Managing the loading displays of background tasks

### Console Shell (ConsoleCommands namespace)
A framework allowing classes to be written resembling console application Main functions. Commands are defined through plugins, and some commands are included with the end goal of providing a console interface for all of the above.

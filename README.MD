# SkyEditor.Core
SkyEditor.Core is a plugin framework with file-editing features.

Projects:

- SkyEditor.Core - The plugin and file editing framework
- SkyEditor.Core.PluginLoaderCore - .Net Standard project that can load plugins.  This is separate because System.Runtime.Loader is incompatible with the regular .Net Framework.
- SkyEditor.Core.FrameworkCompatibility - .Net Framework that can load plugins.  This is separate because System.Reflection isn't fully supported in .Net Standard